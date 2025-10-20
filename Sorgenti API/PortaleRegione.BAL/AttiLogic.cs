/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using AutoMapper;
using ExpressionBuilder.Generics;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AttiLogic : BaseLogic
    {
        public AttiLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEM)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEM;

            GetUsersInDb();
        }

        public async Task<BaseResponse<AttiDto>> GetAtti(BaseRequest<AttiDto> model, int CLIENT_MODE,
            PersonaDto currentUser,
            Uri url = null)
        {
            try
            {
                var queryFilter = new Filter<ATTI>();
                queryFilter.ImportStatements(model.filtro);

                var appoggioAttiDtos = (await _unitOfWork.Atti.GetAll(model.id, model.page, model.size, CLIENT_MODE,
                        currentUser, queryFilter))
                    .Select(Mapper.Map<ATTI, AttiDto>);
                var result = new List<AttiDto>();
                foreach (var appoggio in appoggioAttiDtos)
                {
                    appoggio.Conteggio_EM = await _unitOfWork.Emendamenti.Count(appoggio.UIDAtto,
                        currentUser, CounterEmendamentiEnum.EM, CLIENT_MODE);
                    appoggio.Conteggio_SubEM = await _unitOfWork.Emendamenti.Count(appoggio.UIDAtto,
                        currentUser, CounterEmendamentiEnum.SUB_EM, CLIENT_MODE);
                    appoggio.CounterODG = await _unitOfWork.DASI.CountODGByAttoPEM(appoggio.UIDAtto);
                    if (currentUser.IsSegreteriaAssemblea)
                    {
                        appoggio.CanMoveUp = _unitOfWork.Atti.CanMoveUp(appoggio.Priorita.Value);
                        appoggio.CanMoveDown =
                            await _unitOfWork.Atti.CanMoveDown(appoggio.UIDSeduta.Value, appoggio.Priorita.Value);

                        var listaArticoli = await _unitOfWork.Articoli.GetArticoli(appoggio.UIDAtto);
                        var listaRelatori = await _unitOfWork.Persone.GetRelatori(appoggio.UIDAtto);

                        appoggio.Informazioni_Mancanti = !listaArticoli.Any() && !listaRelatori.Any();
                    }

                    appoggio.Relatori = await GetRelatori(appoggio.UIDAtto);
                    if (appoggio.UIDAssessoreRiferimento.HasValue)
                        appoggio.PersonaAssessore =
                            Users.First(p => p.UID_persona == appoggio.UIDAssessoreRiferimento);

                    if (appoggio.NAtto == "$$")
                    {
                        appoggio.NAtto = "";
                    }

                    result.Add(appoggio);
                }

                return new BaseResponse<AttiDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await _unitOfWork.Atti.Count(model.id, CLIENT_MODE, currentUser, queryFilter),
                    url);
            }
            catch (Exception e)
            {
                Log.Error("Riepilogo Atti", e);
                throw e;
            }
        }

        public async Task DeleteAtto(ATTI attoInDb)
        {
            try
            {
                attoInDb.Eliminato = true;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Atto", e);
                throw e;
            }
        }

        public async Task<ATTI> NuovoAtto(AttiFormUpdateModel attoModel, PersonaDto currentUser)
        {
            try
            {
                var atto = Mapper.Map<AttiFormUpdateModel, ATTI>(attoModel);
                atto.UIDAtto = Guid.NewGuid();
                atto.Eliminato = false;
                atto.UIDPersonaCreazione = currentUser.UID_persona;
                atto.DataCreazione = DateTime.Now;
                atto.OrdinePresentazione = false;
                atto.OrdineVotazione = false;
                atto.Priorita = await _unitOfWork.Atti.PrioritaAtto(atto.UIDSeduta.Value);
                if (string.IsNullOrEmpty(attoModel.NAtto))
                {
                    atto.NAtto = "$$";
                }

                _unitOfWork.Atti.Add(atto);
                await _unitOfWork.CompleteAsync();

                if (attoModel.DocAtto_Stream != null)
                {
                    var path = ByteArrayToFile(attoModel.DocAtto_Stream);
                    atto.Path_Testo_Atto = path;
                    await _unitOfWork.CompleteAsync();
                }

                return atto;
            }
            catch (Exception e)
            {
                Log.Error("Nuovo Atto", e);
                throw e;
            }
        }

        public async Task<ATTI> SalvaAtto(ATTI attoInDb, AttiFormUpdateModel attoModel, PersonaDto currentUser)
        {
            try
            {
                attoInDb.UIDPersonaModifica = currentUser.UID_persona;
                attoInDb.DataModifica = DateTime.Now;
                Mapper.Map(attoModel, attoInDb);
                if (!attoModel.Data_chiusura.HasValue)
                {
                    attoInDb.Data_chiusura = null;
                }

                // #981
                if (!attoModel.UIDAssessoreRiferimento.HasValue)
                {
                    attoInDb.UIDAssessoreRiferimento = null;
                }

                if (!attoInDb.Legislatura.HasValue)
                {
                    if (attoInDb.UIDSeduta.HasValue)
                    {
                        var seduta = await _unitOfWork.Sedute.Get(attoInDb.UIDSeduta.Value);
                        attoInDb.Legislatura = seduta.id_legislatura;
                    }
                }

                await _unitOfWork.CompleteAsync();

                if (attoModel.DocAtto_Stream != null)
                {
                    var path = ByteArrayToFile(attoModel.DocAtto_Stream);
                    attoInDb.Path_Testo_Atto =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                    await _unitOfWork.CompleteAsync();
                }

                return attoInDb;
            }
            catch (Exception e)
            {
                Log.Error("Salva Atto", e);
                throw e;
            }
        }

        public async Task ModificaFascicoli(ATTI attoInDb, AttiDto attiDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(attiDto.LinkFascicoloPresentazione))
                {
                    attoInDb.LinkFascicoloPresentazione = attiDto.LinkFascicoloPresentazione;
                    attoInDb.DataCreazionePresentazione = DateTime.Now;
                }

                if (!string.IsNullOrEmpty(attiDto.LinkFascicoloVotazione))
                {
                    attoInDb.LinkFascicoloVotazione = attiDto.LinkFascicoloVotazione;
                    attoInDb.DataCreazioneVotazione = DateTime.Now;
                }

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Modifica Fascicoli Atto", e);
                throw e;
            }
        }

        public async Task<IEnumerable<ArticoliDto>> GetArticoli(Guid id)
        {
            return (await _unitOfWork.Articoli.GetArticoli(id)).Select(Mapper.Map<ARTICOLI, ArticoliDto>);
        }

        public async Task CreaArticoli(Guid id, string articoli)
        {
            var appo = articoli.Split(',');
            foreach (var s in appo)
            {
                if (s.Contains("-"))
                {
                    //INSERIMENTO RANGE
                    var appo_range = s.Split('-');
                    var start = int.Parse(appo_range[0]);
                    var end = int.Parse(appo_range[1]);

                    for (var i = Convert.ToInt32(start); i <= Convert.ToInt32(end); i++)
                    {
                        if (await _unitOfWork.Articoli.CheckIfArticoloExists(id, i.ToString()))
                        {
                            continue;
                        }

                        _unitOfWork.Articoli.Add(new ARTICOLI
                        {
                            Articolo = i.ToString(),
                            UIDAtto = id,
                            UIDArticolo = Guid.NewGuid(),
                            Ordine = await _unitOfWork.Articoli.OrdineArticolo(id)
                        });
                        await _unitOfWork.CompleteAsync();
                    }
                }
                else
                {
                    if (await _unitOfWork.Articoli.CheckIfArticoloExists(id, s))
                    {
                        continue;
                    }

                    _unitOfWork.Articoli.Add(new ARTICOLI
                    {
                        Articolo = s,
                        UIDAtto = id,
                        UIDArticolo = Guid.NewGuid(),
                        Ordine = await _unitOfWork.Articoli.OrdineArticolo(id)
                    });
                    await _unitOfWork.CompleteAsync();
                }
            }
        }

        public async Task<ARTICOLI> GetArticolo(Guid id)
        {
            return await _unitOfWork.Articoli.GetArticolo(id);
        }

        public async Task DeleteArticolo(ARTICOLI articolo, PersonaDto currentUser)
        {
            articolo.UIDUtenteModifica = currentUser.UID_persona;
            articolo.DataModifica = DateTime.Now;
            articolo.Eliminato = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<COMMI>> GetCommi(Guid id, bool expanded = false)
        {
            var result = new List<COMMI>();
            var commInDb = await _unitOfWork.Commi.GetCommi(id);
            if (!expanded) return commInDb;
            foreach (var comma in commInDb)
            {
                var lettere = await _unitOfWork.Lettere.GetLettere(comma.UIDComma);
                if (lettere.Count() > 0)
                {
                    comma.Comma = $"{comma.Comma} ({lettere.Select(i => i.Lettera).Aggregate((k, j) => k + "-" + j)})";
                }
                result.Add(comma);
            }

            return result;
        }

        public async Task DeleteCommi(IEnumerable<COMMI> listCommi, PersonaDto currentUser)
        {
            foreach (var commis in listCommi)
            {
                commis.UIDUtenteModifica = currentUser.UID_persona;
                commis.DataModifica = DateTime.Now;
                commis.Eliminato = true;
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<LETTERE>> GetLettere(Guid commaUId)
        {
            return await _unitOfWork.Lettere.GetLettere(commaUId);
        }

        public async Task DeleteLettere(IEnumerable<LETTERE> listLettere, PersonaDto currentUser)
        {
            foreach (var lettere in listLettere)
            {
                lettere.UIDUtenteModifica = currentUser.UID_persona;
                lettere.DataModifica = DateTime.Now;
                lettere.Eliminato = true;
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteLettere(LETTERE lettera, PersonaDto currentUser)
        {
            lettera.UIDUtenteModifica = currentUser.UID_persona;
            lettera.DataModifica = DateTime.Now;
            lettera.Eliminato = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task CreaCommi(ARTICOLI articolo, string commi)
        {
            var appo = commi.Split(',');
            foreach (var s in appo)
            {
                if (s.Contains("-"))
                {
                    //INSERIMENTO RANGE
                    var appo_range = s.Split('-');
                    var start = int.Parse(appo_range[0]);
                    var end = int.Parse(appo_range[1]);

                    for (var i = Convert.ToInt32(start); i <= Convert.ToInt32(end); i++)
                    {
                        if (await _unitOfWork.Commi.CheckIfCommiExists(articolo.UIDArticolo, i.ToString()))
                        {
                            continue;
                        }

                        _unitOfWork.Commi.Add(new COMMI
                        {
                            Comma = i.ToString(),
                            UIDComma = Guid.NewGuid(),
                            UIDAtto = articolo.UIDAtto,
                            UIDArticolo = articolo.UIDArticolo,
                            Ordine = await _unitOfWork.Commi.OrdineComma(articolo.UIDArticolo)
                        });
                        await _unitOfWork.CompleteAsync();
                    }
                }
                else
                {
                    if (await _unitOfWork.Commi.CheckIfCommiExists(articolo.UIDArticolo, s))
                    {
                        continue;
                    }

                    _unitOfWork.Commi.Add(new COMMI
                    {
                        Comma = s,
                        UIDComma = Guid.NewGuid(),
                        UIDAtto = articolo.UIDAtto,
                        UIDArticolo = articolo.UIDArticolo,
                        Ordine = await _unitOfWork.Commi.OrdineComma(articolo.UIDArticolo)
                    });
                    await _unitOfWork.CompleteAsync();
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<COMMI> GetComma(Guid id)
        {
            return await _unitOfWork.Commi.GetComma(id);
        }

        public async Task DeleteComma(COMMI comma, PersonaDto currentUser)
        {
            comma.UIDUtenteModifica = currentUser.UID_persona;
            comma.DataModifica = DateTime.Now;
            comma.Eliminato = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task CreaLettere(COMMI comma, string lettere)
        {
            var appo = lettere.Split(',');
            foreach (var s in appo)
            {
                if (s.Contains("-"))
                {
                    //INSERIMENTO RANGE
                    var appo_range = s.Split('-');
                    var start = int.Parse(appo_range[0]);
                    var end = int.Parse(appo_range[1]);

                    for (var i = Convert.ToInt32(start); i <= Convert.ToInt32(end); i++)
                    {
                        if (await _unitOfWork.Lettere.CheckIfLetteraExists(comma.UIDComma, i.ToString()))
                        {
                            continue;
                        }

                        _unitOfWork.Lettere.Add(new LETTERE
                        {
                            Lettera = i.ToString(),
                            UIDComma = comma.UIDComma,
                            UIDLettera = Guid.NewGuid(),
                            Ordine = await _unitOfWork.Lettere.OrdineLettera(comma.UIDComma)
                        });
                        await _unitOfWork.CompleteAsync();
                    }
                }
                else
                {
                    if (await _unitOfWork.Commi.CheckIfCommiExists(comma.UIDComma, s))
                    {
                        continue;
                    }

                    _unitOfWork.Lettere.Add(new LETTERE
                    {
                        Lettera = s,
                        UIDComma = comma.UIDComma,
                        UIDLettera = Guid.NewGuid(),
                        Ordine = await _unitOfWork.Lettere.OrdineLettera(comma.UIDComma)
                    });
                    await _unitOfWork.CompleteAsync();
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<LETTERE> GetLettera(Guid id)
        {
            return await _unitOfWork.Lettere.GetLettera(id);
        }

        public async Task SalvaRelatori(Guid attoUId, IEnumerable<Guid> relatori)
        {
            await _unitOfWork.Atti.SalvaRelatori(attoUId, relatori);
            await _unitOfWork.CompleteAsync();
        }

        public async Task PubblicaFascicolo(ATTI attoInDb, PubblicaFascicoloModel model, PersonaDto currentUser)
        {
            attoInDb.Fascicoli_Da_Aggiornare = false;
            switch (model.Ordinamento)
            {
                case OrdinamentoEnum.Default:
                case OrdinamentoEnum.Presentazione:
                    attoInDb.OrdinePresentazione = model.Abilita;
                    attoInDb.LinkFascicoloPresentazione = string.Empty;
                    break;
                case OrdinamentoEnum.Votazione:
                    attoInDb.OrdineVotazione = model.Abilita;
                    attoInDb.LinkFascicoloVotazione = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Ordinamento), model.Ordinamento, null);
            }

            attoInDb.UIDPersonaModifica = currentUser.UID_persona;
            attoInDb.DataModifica = DateTime.Now;

            await _unitOfWork.Atti.RimuoviFascicoliObsoleti(attoInDb.UIDAtto, model.Ordinamento);

            await _unitOfWork.CompleteAsync();
        }

        public async Task<ATTI> GetAtto(Guid id)
        {
            return await _unitOfWork.Atti.Get(id);
        }

        public async Task SPOSTA_UP(Guid attoUId)
        {
            await _unitOfWork.Atti.SPOSTA_UP(attoUId);
        }

        public async Task SPOSTA_DOWN(Guid attoUId)
        {
            await _unitOfWork.Atti.SPOSTA_DOWN(attoUId);
        }

        public async Task<IEnumerable<PersonaLightDto>> GetRelatori(Guid attoUId)
        {
            var result = await _unitOfWork.Atti.GetRelatori(attoUId);
            return result;
        }

        public IEnumerable<Tipi_AttoDto> GetTipi(bool dasi = true)
        {
            var result = new List<Tipi_AttoDto>();
            var tipi = Enum.GetValues(typeof(TipoAttoEnum));
            foreach (var tipo in tipi)
            {
                if (dasi)
                {
                    if (Utility.tipiNonVisibili.Contains((TipoAttoEnum)tipo))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!Utility.tipiNonVisibili.Contains((TipoAttoEnum)tipo))
                    {
                        continue;
                    }
                }

                result.Add(new Tipi_AttoDto
                {
                    IDTipoAtto = (int)tipo,
                    Tipo_Atto = Utility.GetText_Tipo((int)tipo)
                });
            }

            return result;
        }

        public async Task<List<ArticoliModel>> GetGrigliaTesto(Guid id, bool viewEm = false)
        {
            var result = new List<ArticoliModel>();
            var articoli = await _unitOfWork.Articoli.GetArticoli(id);
            foreach (var articolo in articoli)
            {
                var articoliModel = new ArticoliModel
                {
                    Data = Mapper.Map<ARTICOLI, ArticoliDto>(articolo)
                };

                if (viewEm)
                {
                    var em_per_articolo = await _unitOfWork.Emendamenti.GetByArticolo(articolo.UIDArticolo);

                    foreach (var em_articolo in em_per_articolo.Where(em => em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche))
                    {
                        var emArticolo = await _logicEm.GetEM_DTO_Light(em_articolo.UIDEM);
                        articoliModel.Emendamenti.Add(emArticolo);
                    }
                }

                var commi = await _unitOfWork.Commi.GetCommi(articolo.UIDArticolo);

                if (commi.Any())
                {
                    foreach (var comma in commi)
                    {
                        var commiModel = new CommiModel
                        {
                            Data = Mapper.Map<COMMI, CommiDto>(comma)
                        };

                        if (viewEm)
                        {
                            var em_per_comma = await _unitOfWork.Emendamenti.GetByComma(comma.UIDComma);
                            foreach (var em_comma in em_per_comma.Where(em => em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche))
                            {
                                var emComma = await _logicEm.GetEM_DTO_Light(em_comma.UIDEM);
                                commiModel.Emendamenti.Add(emComma);
                            }
                        }

                        var lettere = await _unitOfWork.Lettere.GetLettere(comma.UIDComma);

                        if (lettere.Any())
                        {
                            foreach (var lettera in lettere)
                            {
                                var letteraModel = new LettereModel
                                {
                                    Data = Mapper.Map<LETTERE, LettereDto>(lettera)
                                };

                                if (viewEm)
                                {
                                    var em_per_lettera = await _unitOfWork.Emendamenti.GetByLettera(lettera.UIDLettera);
                                    foreach (var em_lettera in em_per_lettera.Where(em => em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche))
                                    {
                                        var emLettera = await _logicEm.GetEM_DTO_Light(em_lettera.UIDEM);
                                        letteraModel.Emendamenti.Add(emLettera);
                                    }
                                }

                                commiModel.Lettere.Add(letteraModel);
                            }
                        }

                        articoliModel.Commi.Add(commiModel);
                    }
                }

                result.Add(articoliModel);
            }

            return result;
        }

        public async Task SalvaTesto(TestoAttoModel model)
        {
            try
            {
                var success = Guid.TryParse(model.Id, out var guid);
                if (!success)
                    throw new Exception("Identificativo non valido");

                var articolo = await _unitOfWork.Articoli.GetArticolo(guid);
                if (articolo != null)
                {
                    articolo.TestoArticolo = model.Testo;
                    await _unitOfWork.CompleteAsync();
                    return;
                }

                var comma = await _unitOfWork.Commi.GetComma(guid);
                if (comma != null)
                {
                    comma.TestoComma = model.Testo;
                    await _unitOfWork.CompleteAsync();
                    return;
                }

                var lettera = await _unitOfWork.Lettere.GetLettera(guid);
                if (lettera != null)
                {
                    lettera.TestoLettera = model.Testo;
                    await _unitOfWork.CompleteAsync();
                    return;
                }

                throw new Exception("Identificativo non valido");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<EmendamentoExtraLightDto>> GetGrigliaOrdinamento(Guid id)
        {
            var result = new List<EmendamentoExtraLightDto>();
            var em = await _unitOfWork.Emendamenti.GetGrigliaOrdinamento(id);
            foreach (var emInDb in em)
            {
                var emDto = await _logicEm.GetEM_DTO(emInDb.UIDEM);
                result.Add(emDto.toLight());
            }

            return result;
        }

        public async Task SpostaInAltraSeduta(Guid uidAtto, Guid uidSeduta)
        {
            var attoInDb = await GetAtto(uidAtto);
            var atto_clone = attoInDb.Clona();
            _unitOfWork.Atti.Add(atto_clone);
            attoInDb.UIDSeduta = uidSeduta;
            await _unitOfWork.CompleteAsync();
        }
    }
}