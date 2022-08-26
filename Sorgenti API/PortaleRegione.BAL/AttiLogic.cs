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
using System.Net.Http;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AttiLogic : BaseLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttiLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<AttiDto>> GetAtti(BaseRequest<AttiDto> model, int CLIENT_MODE,
            PersonaDto currentUser,
            List<PersonaLightDto> personeInDbLight,
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

                        appoggio.Informazioni_Mancanti = listaArticoli.Any() || listaRelatori.Any() ? false : true;
                    }

                    appoggio.Relatori = await GetRelatori(appoggio.UIDAtto);
                    if (appoggio.UIDAssessoreRiferimento.HasValue)
                        appoggio.PersonaAssessore =
                            personeInDbLight.First(p => p.UID_persona == appoggio.UIDAssessoreRiferimento);


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
            try
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
            catch (Exception e)
            {
                Log.Error("Crea Articoli", e);
                throw;
            }
        }

        public async Task<ARTICOLI> GetArticolo(Guid id)
        {
            try
            {
                return await _unitOfWork.Articoli.GetArticolo(id);
            }
            catch (Exception e)
            {
                Log.Error("Get Articolo", e);
                throw;
            }
        }

        public async Task DeleteArticolo(ARTICOLI articolo)
        {
            try
            {
                _unitOfWork.Articoli.Remove(articolo);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Articolo", e);
                throw;
            }
        }

        public async Task<IEnumerable<COMMI>> GetCommi(Guid id)
        {
            try
            {
                var result = new List<COMMI>();
                var commInDb = await _unitOfWork.Commi.GetCommi(id);
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
            catch (Exception e)
            {
                Log.Error("Get Commi", e);
                throw;
            }
        }

        public async Task DeleteCommi(IEnumerable<COMMI> listCommi)
        {
            try
            {
                _unitOfWork.Commi.RemoveRange(listCommi);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Commi", e);
                throw;
            }
        }

        public async Task<IEnumerable<LETTERE>> GetLettere(Guid commaUId)
        {
            try
            {
                return await _unitOfWork.Lettere.GetLettere(commaUId);
            }
            catch (Exception e)
            {
                Log.Error("Get Lettere", e);
                throw;
            }
        }

        public async Task DeleteLettere(IEnumerable<LETTERE> listLettere)
        {
            try
            {
                _unitOfWork.Lettere.RemoveRange(listLettere);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Lettere", e);
                throw;
            }
        }

        public async Task DeleteLettere(LETTERE lettera)
        {
            try
            {
                _unitOfWork.Lettere.Remove(lettera);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Lettera", e);
                throw;
            }
        }

        public async Task CreaCommi(ARTICOLI articolo, string commi)
        {
            try
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
            }
            catch (Exception e)
            {
                Log.Error("Crea Commi", e);
                throw;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<COMMI> GetComma(Guid id)
        {
            try
            {
                return await _unitOfWork.Commi.GetComma(id);
            }
            catch (Exception e)
            {
                Log.Error("Get Comma", e);
                throw;
            }
        }

        public async Task DeleteComma(COMMI comma)
        {
            try
            {
                _unitOfWork.Commi.Remove(comma);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Comma", e);
                throw;
            }
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
            try
            {
                await _unitOfWork.Atti.SalvaRelatori(attoUId, relatori);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Salva Relatori", e);
                throw;
            }
        }

        public async Task PubblicaFascicolo(ATTI attoInDb, PubblicaFascicoloModel model, PersonaDto currentUser)
        {
            try
            {
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
            catch (Exception e)
            {
                Log.Error("Pubblica Fascicolo", e);
                throw;
            }
        }

        public async Task<HttpResponseMessage> Download(string path)
        {
            var complete_path = Path.Combine(
                AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                Path.GetFileName(path));

            Log.Debug($"Download file atto: {complete_path} [originale: {path}]");
            var result = await ComposeFileResponse(complete_path);
            return result;
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
    }
}