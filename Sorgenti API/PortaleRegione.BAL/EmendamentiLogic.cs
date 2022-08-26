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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using Newtonsoft.Json;
using PortaleRegione.BAL.OpenData;
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

namespace PortaleRegione.BAL
{
    public class EmendamentiLogic : BaseLogic
    {
        private readonly FirmeLogic _logicFirme;
        private readonly PersoneLogic _logicPersone;
        private readonly UtilsLogic _logicUtil;
        private readonly IUnitOfWork _unitOfWork;

        public EmendamentiLogic(IUnitOfWork unitOfWork, FirmeLogic logicFirme, PersoneLogic logicPersone,
            UtilsLogic logicUtil)
        {
            _unitOfWork = unitOfWork;
            _logicFirme = logicFirme;
            _logicPersone = logicPersone;
            _logicUtil = logicUtil;
        }

        public async Task ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _unitOfWork.Emendamenti.ORDINA_EM_TRATTAZIONE(id);
            }
            catch (Exception e)
            {
                Log.Error("Logic - ORDINA_EM_TRATTAZIONE", e);
                throw e;
            }
        }

        public async Task ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO(Guid attoUId, PersonaDto persona)
        {
            try
            {
                var atto = await _unitOfWork.Atti.Get(attoUId);
                var ruolo_segreteria = await _unitOfWork.Ruoli.Get((int) RuoliIntEnum.Segreteria_Assemblea);
                await _logicUtil.InvioMail(new MailModel
                {
                    DA = persona.email,
                    A =
                        $"{ruolo_segreteria.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                    OGGETTO = $"[ORDINAMENTO CONCLUSO] {atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}",
                    MESSAGGIO = $"Ordinamento atto concluso da {persona.DisplayName}"
                });
            }
            catch (Exception e)
            {
                Log.Error("Logic - ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO", e);
                throw e;
            }
        }

        public async Task UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _unitOfWork.Emendamenti.UP_EM_TRATTAZIONE(id);
            }
            catch (Exception e)
            {
                Log.Error("Logic - UP_EM_TRATTAZIONE", e);
                throw e;
            }
        }

        public async Task DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _unitOfWork.Emendamenti.DOWN_EM_TRATTAZIONE(id);
            }
            catch (Exception e)
            {
                Log.Error("Logic - DOWN_EM_TRATTAZIONE", e);
                throw e;
            }
        }

        public async Task SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                await _unitOfWork.Emendamenti.SPOSTA_EM_TRATTAZIONE(id, pos);
            }
            catch (Exception e)
            {
                Log.Error("Logic - SPOSTA_EM_TRATTAZIONE", e);
                throw e;
            }
        }

        public async Task<EmendamentiFormModel> ModelloNuovoEM(ATTI atto, Guid? em_riferimentoUId, PersonaDto persona)
        {
            try
            {
                var sub_em = em_riferimentoUId != Guid.Empty;

                var result = new EmendamentiFormModel
                {
                    CurrentUser = persona
                };
                var emendamento = new EmendamentiDto();

                var isGiunta = persona.IsGiunta;

                var progressivo = persona.IsSegreteriaAssemblea
                                  || persona.IsPresidente
                    ? 1
                    : await _unitOfWork.Emendamenti.GetProgressivo(atto.UIDAtto,
                        persona.Gruppo.id_gruppo, sub_em);

                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                if (sub_em)
                {
                    var ref_em = await GetEM_DTO(em_riferimentoUId.Value, atto, persona, personeInDbLight);
                    emendamento.SubProgressivo = progressivo;
                    emendamento.Rif_UIDEM = em_riferimentoUId;
                    emendamento.IDStato = (int) StatiEnum.Bozza;
                    emendamento.IDTipo_EM = ref_em.IDTipo_EM;
                    emendamento.IDParte = ref_em.IDParte;
                    switch ((PartiEMEnum) emendamento.IDParte)
                    {
                        case PartiEMEnum.Titolo_PDL:
                            break;
                        case PartiEMEnum.Titolo:
                            emendamento.NTitolo = ref_em.NTitolo;
                            break;
                        case PartiEMEnum.Capo:
                            emendamento.NCapo = ref_em.NCapo;
                            break;
                        case PartiEMEnum.Articolo:
                            if (ref_em.UIDArticolo.HasValue) emendamento.UIDArticolo = ref_em.UIDArticolo;

                            if (ref_em.UIDComma.HasValue) emendamento.UIDComma = ref_em.UIDComma;

                            if (ref_em.UIDLettera.HasValue) emendamento.UIDLettera = ref_em.UIDLettera;

                            break;
                        case PartiEMEnum.Missione:
                            if (ref_em.NMissione.HasValue) emendamento.NMissione = ref_em.NMissione;

                            if (ref_em.NTitoloB.HasValue) emendamento.NTitoloB = ref_em.NTitoloB;

                            if (ref_em.NProgramma.HasValue) emendamento.NProgramma = ref_em.NProgramma;

                            break;
                        case PartiEMEnum.Allegato_Tabella:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    emendamento.TestoEM_originale =
                        $"L' {ref_em.N_EM.Replace("EM", "emendamento")} - {ref_em.TestoEM_originale}<br><strong>è così modificato:</strong><br>";
                    emendamento.TestoREL_originale = ref_em.TestoREL_originale;
                    emendamento.N_EM = GetNomeEM(emendamento, ref_em);
                }
                else
                {
                    emendamento.Progressivo = progressivo;
                    if (persona.IsSegreteriaAssemblea
                        || persona.IsPresidente)
                    {
                        emendamento.IDStato = (int) StatiEnum.Bozza;
                    }
                    else
                    {
                        emendamento.IDStato = (int) StatiEnum.Bozza;
                        var info_gruppo = await _unitOfWork.Gruppi.GetJoinGruppoAdmin(persona.Gruppo.id_gruppo);
                        if (info_gruppo is {AbilitaEMPrivati: true})
                            emendamento.IDStato = (int) StatiEnum.Bozza_Riservata;
                    }

                    emendamento.N_EM = GetNomeEM(emendamento, null);
                }

                if (persona.IsConsigliereRegionale ||
                    persona.IsAssessore)
                {
                    emendamento.UIDPersonaProponente = persona.UID_persona;
                    emendamento.PersonaProponente = new PersonaLightDto
                    {
                        UID_persona = persona.UID_persona,
                        cognome = persona.cognome,
                        nome = persona.nome
                    };
                }
                else
                {
                    if (persona.IsSegreteriaAssemblea
                        || persona.IsPresidente)
                    {
                        result.ListaConsiglieri =
                            await _logicPersone.GetConsiglieri();
                        result.ListaAssessori = await _logicPersone
                            .GetAssessoriRiferimento();
                    }
                    else
                    {
                        if (isGiunta)
                            result.ListaGruppo = await _logicPersone
                                .GetAssessoriRiferimento();
                        else
                            result.ListaGruppo = await _logicPersone.GetConsiglieriGruppo(persona.Gruppo.id_gruppo);
                    }
                }

                emendamento.UIDPersonaCreazione = persona.UID_persona;
                emendamento.DataCreazione = DateTime.Now;
                emendamento.idRuoloCreazione = (int) persona.CurrentRole;
                if (!persona.IsSegreteriaAssemblea
                    && !persona.IsPresidente)
                    emendamento.id_gruppo = persona.Gruppo.id_gruppo;

                emendamento.UIDAtto = atto.UIDAtto;
                emendamento.ATTI = Mapper.Map<ATTI, AttiDto>(atto);

                result.ListaPartiEmendabili = await GetPartiEM();
                result.ListaTipiEmendamento = await GetTipiEM();
                result.ListaMissioni = await GetMissioni();
                result.ListaTitoli_Missioni = await GetTitoliMissioni();
                result.ListaArticoli = await GetArticoli(atto.UIDAtto);

                result.Emendamento = emendamento;
                result.Atto = Mapper.Map<ATTI, AttiDto>(atto);

                if (persona.IsSegreteriaAssemblea)
                    result.Emendamento.TestoEM_originale = AppSettingsConfiguration.TestoEMCartaceo;

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModelloNuovoEM", e);
                throw e;
            }
        }

        public async Task<EmendamentiFormModel> ModelloModificaEM(EM emInDb, PersonaDto persona)
        {
            try
            {
                var atto = await _unitOfWork.Atti.Get(emInDb.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var em = await GetEM_DTO(emInDb, atto, persona, personeInDbLight);
                var result = new EmendamentiFormModel {Emendamento = em, Atto = em.ATTI, CurrentUser = persona};
                if (persona.CurrentRole != RuoliIntEnum.Consigliere_Regionale &&
                    persona.CurrentRole != RuoliIntEnum.Assessore_Sottosegretario_Giunta)
                {
                    if (persona.IsSegreteriaAssemblea)
                    {
                        result.ListaConsiglieri =
                            (await _unitOfWork.Persone.GetConsiglieri(
                                await _unitOfWork.Legislature.Legislatura_Attiva()))
                            .Select(Mapper.Map<View_UTENTI, PersonaDto>);
                        result.ListaAssessori = (await _unitOfWork.Persone
                                .GetAssessoriRiferimento(await _unitOfWork.Legislature.Legislatura_Attiva()))
                            .Select(Mapper.Map<View_UTENTI, PersonaDto>);
                        result.ListaAreaPolitica = Utility.GetEnumList<AreaPoliticaIntEnum>();
                    }
                    else
                    {
                        result.ListaGruppo = (await _unitOfWork.Gruppi.GetConsiglieriGruppo(
                                await _unitOfWork.Legislature.Legislatura_Attiva(), persona.Gruppo.id_gruppo))
                            .Select(Mapper.Map<View_UTENTI, PersonaDto>);
                    }
                }

                result.ListaPartiEmendabili = await GetPartiEM();
                result.ListaTipiEmendamento = await GetTipiEM();
                result.ListaMissioni = await GetMissioni();
                result.ListaTitoli_Missioni = await GetTitoliMissioni();
                result.ListaArticoli = (await _unitOfWork
                        .Articoli
                        .GetArticoli(em.UIDAtto))
                    .Select(Mapper.Map<ARTICOLI, ArticoliDto>);

                if (string.IsNullOrEmpty(em.TestoEM_Modificabile)) em.TestoEM_Modificabile = em.TestoEM_originale;

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModelloModificaEM", e);
                throw e;
            }
        }

        public async Task<EM> NuovoEmendamento(EmendamentiDto emendamentoDto, PersonaDto persona, bool isGiunta = false)
        {
            try
            {
                var progressivo =
                    await _unitOfWork.Emendamenti.GetProgressivo(emendamentoDto.UIDAtto,
                        persona.Gruppo.id_gruppo,
                        emendamentoDto.Rif_UIDEM.HasValue);
                if (emendamentoDto.Rif_UIDEM.HasValue)
                    emendamentoDto.SubProgressivo = progressivo;
                else
                    emendamentoDto.Progressivo = progressivo;

                var em = Mapper.Map<EmendamentiDto, EM>(emendamentoDto);
                em.N_EM = null;
                em.ATTI = null;
                em.UIDEM = Guid.NewGuid();
                em.UID_QRCode = Guid.NewGuid();
                em.Eliminato = false;
                em.DataCreazione = DateTime.Now;
                em.OrdinePresentazione = 0;
                em.id_gruppo = persona.Gruppo.id_gruppo;
                _unitOfWork.Emendamenti.Add(em);
                await _unitOfWork.CompleteAsync();

                if (emendamentoDto.DocAllegatoGenerico_Stream != null)
                {
                    var path = ByteArrayToFile(emendamentoDto.DocAllegatoGenerico_Stream);
                    em.PATH_AllegatoGenerico =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                }

                if (emendamentoDto.DocEffettiFinanziari_Stream != null)
                {
                    var path = ByteArrayToFile(emendamentoDto.DocEffettiFinanziari_Stream);
                    em.PATH_AllegatoTecnico =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                }

                if (em.EffettiFinanziari == 0) em.PATH_AllegatoTecnico = string.Empty;

                if (em.UIDLettera.HasValue && string.IsNullOrEmpty(em.NLettera))
                {
                    var lettera = await _unitOfWork.Lettere.GetLettera(em.UIDLettera.Value);
                    em.NLettera = lettera.Lettera;
                }

                await _unitOfWork.CompleteAsync();

                //Controllo Tags
                if (!string.IsNullOrEmpty(em.Tags))
                {
                    var tags = JsonConvert.DeserializeObject<List<TagDto>>(em.Tags);
                    foreach (var t in tags)
                    {
                        bool exists = await _unitOfWork.Emendamenti.TagExists(t.tag);
                        if (!exists)
                        {
                            _unitOfWork.Emendamenti.AddTag(t.tag);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                }

                return em;
            }
            catch (Exception e)
            {
                Log.Error("Logic - NuovoEmendamento", e);
                throw e;
            }
        }

        public async Task ModificaEmendamento(EmendamentiDto model, EM em, PersonaDto persona)
        {
            try
            {
                if (model.IDStato != (int) StatiEnum.Bozza
                    && model.IDStato != (int) StatiEnum.Bozza_Riservata)
                {
                    throw new InvalidOperationException($"Stato non valido [{model.IDStato}]");
                }

                var updateDto = Mapper.Map<EmendamentiDto, EmendamentoLightDto>(model);
                Mapper.Map(updateDto, em);

                em.Tags = model.Tags;

                var countFirme = await _unitOfWork.Firme.CountFirme(model.UIDEM);

                if (!string.IsNullOrEmpty(em.EM_Certificato) && countFirme == 1)
                {
                    //cancelliamo firme - notifiche - stampe
                    em.UIDPersonaPrimaFirma = Guid.Empty;
                    em.DataPrimaFirma = null;
                    em.EM_Certificato = string.Empty;
                    em.Hash = string.Empty;
                    await _unitOfWork.Firme.CancellaFirme(model.UIDEM);
                }

                if (em.IDStato < (int) StatiEnum.Depositato)
                {
                    em.UIDPersonaModifica = persona.UID_persona;
                    em.DataModifica = DateTime.Now;
                }

                await _unitOfWork.CompleteAsync();

                if (model.DocAllegatoGenerico_Stream != null)
                {
                    var path = ByteArrayToFile(model.DocAllegatoGenerico_Stream);
                    em.PATH_AllegatoGenerico =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                }

                if (model.DocEffettiFinanziari_Stream != null)
                {
                    var path = ByteArrayToFile(model.DocEffettiFinanziari_Stream);
                    em.PATH_AllegatoTecnico =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                }

                if (em.EffettiFinanziari == 0) em.PATH_AllegatoTecnico = string.Empty;

                if (em.UIDLettera.HasValue && string.IsNullOrEmpty(em.NLettera))
                {
                    var lettera = await _unitOfWork.Lettere.GetLettera(em.UIDLettera.Value);
                    em.NLettera = lettera.Lettera;
                }

                PuliziaMetaDati(em);

                await _unitOfWork.CompleteAsync();

                //Controllo Tags
                if (!string.IsNullOrEmpty(em.Tags))
                {
                    var tags = JsonConvert.DeserializeObject<List<TagDto>>(model.Tags);
                    foreach (var t in tags)
                    {
                        bool exists = await _unitOfWork.Emendamenti.TagExists(t.tag);
                        if (!exists)
                        {
                            _unitOfWork.Emendamenti.AddTag(t.tag);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaEmendamento", e);
                throw e;
            }
        }

        public async Task ModificaMetaDatiEmendamento(EmendamentiDto model, EM em, PersonaDto persona)
        {
            try
            {
                var updateMetaDatiDto = Mapper.Map<EmendamentiDto, MetaDatiEMDto>(model);
                var emAggiornato = Mapper.Map(updateMetaDatiDto, em);

                emAggiornato.UIDPersonaModifica = persona.UID_persona;
                emAggiornato.DataModifica = DateTime.Now;
                if (!string.IsNullOrEmpty(model.TestoEM_Modificabile))
                {
                    emAggiornato.TestoEM_Modificabile = model.TestoEM_Modificabile;
                    emAggiornato.IDStato = (int) StatiEnum.Approvato_Con_Modifiche;
                }
                else if (string.IsNullOrEmpty(model.TestoEM_Modificabile) &&
                         !string.IsNullOrEmpty(em.TestoEM_Modificabile) &&
                         em.IDStato == (int) StatiEnum.Approvato_Con_Modifiche)
                {
                    //caso in cui l'utente voglia tornare allo stato precedente all' "approvato con modifiche"
                    emAggiornato.IDStato = (int) StatiEnum.Approvato;
                }

                if (em.UIDLettera.HasValue && string.IsNullOrEmpty(em.NLettera))
                {
                    var lettera = await _unitOfWork.Lettere.GetLettera(em.UIDLettera.Value);
                    em.NLettera = lettera.Lettera;
                }

                PuliziaMetaDati(em);

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaMetaDatiEmendamento", e);
                throw e;
            }
        }

        private void PuliziaMetaDati(EM em)
        {
            switch ((PartiEMEnum) em.IDParte)
            {
                case PartiEMEnum.Titolo_PDL:
                    CleanParteArticolo(em);
                    CleanParteMissione(em);
                    CleanParteTitolo(em);
                    CleanParteCapo(em);
                    break;
                case PartiEMEnum.Titolo:
                    CleanParteArticolo(em);
                    CleanParteMissione(em);
                    CleanParteCapo(em);
                    break;
                case PartiEMEnum.Capo:
                    CleanParteArticolo(em);
                    CleanParteMissione(em);
                    CleanParteTitolo(em);
                    break;
                case PartiEMEnum.Articolo:
                    CleanParteMissione(em);
                    CleanParteTitolo(em);
                    CleanParteCapo(em);
                    break;
                case PartiEMEnum.Missione:
                    CleanParteArticolo(em);
                    CleanParteTitolo(em);
                    CleanParteCapo(em);
                    break;
                default:
                {
                    CleanParteArticolo(em);
                    CleanParteMissione(em);
                    CleanParteTitolo(em);
                    CleanParteCapo(em);
                    break;
                }
            }

            if (em.IDStato != (int) StatiEnum.Approvato_Con_Modifiche) em.TestoEM_Modificabile = string.Empty;
        }

        private void CleanParteCapo(EM em)
        {
            em.NCapo = string.Empty;
        }

        private void CleanParteTitolo(EM em)
        {
            em.NTitolo = string.Empty;
        }

        private void CleanParteMissione(EM em)
        {
            em.NMissione = null;
            em.NProgramma = null;
            em.NTitoloB = null;
        }

        private void CleanParteArticolo(EM em)
        {
            em.UIDArticolo = null;
            em.UIDComma = null;
            em.NLettera = string.Empty;
            em.UIDLettera = null;
        }

        public async Task DeleteEmendamento(EM em, PersonaDto persona)
        {
            try
            {
                em.Eliminato = true;
                em.UIDPersonaModifica = persona.UID_persona;
                em.DataModifica = DateTime.Now;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - DeleteEmendamento", e);
                throw e;
            }
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(EM em)
        {
            try
            {
                var invitati = await _unitOfWork
                    .Emendamenti
                    .GetInvitati(em.UIDEM);
                var destinatari = invitati
                    .Select(Mapper.Map<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>)
                    .ToList();
                var result = new List<DestinatariNotificaDto>();
                foreach (var destinatario in destinatari)
                {
                    destinatario.Firmato = await _unitOfWork
                        .Firme
                        .CheckFirmato(destinatario.NOTIFICHE.UIDEM, destinatario.UIDPersona);
                    result.Add(destinatario);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetInvitati", e);
                throw e;
            }
        }

        public async Task<string> GetBodyEM(EmendamentiDto emDto, IEnumerable<FirmeDto> firme, PersonaDto persona,
            TemplateTypeEnum template, bool isDeposito = false)
        {
            try
            {
                var em = await _unitOfWork.Emendamenti.Get(emDto.UIDEM);
                return await GetBodyEM(em, firme, persona, template, isDeposito);
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }

        public async Task<string> GetBodyEM(EM em, IEnumerable<FirmeDto> firme, PersonaDto persona,
            TemplateTypeEnum template, bool isDeposito = false)
        {
            try
            {
                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var emendamentoDto = await GetEM_DTO(em.UIDEM, atto, persona, personeInDbLight);
                var attoDto = Mapper.Map<ATTI, AttiDto>(atto);

                try
                {
                    var body = GetTemplate(template);

                    switch (template)
                    {
                        case TemplateTypeEnum.MAIL:
                            GetBody(emendamentoDto, attoDto, firme, persona, false, ref body);
                            break;
                        case TemplateTypeEnum.PDF:
                            GetBody(emendamentoDto, attoDto, firme, persona, true, ref body);
                            break;
                        case TemplateTypeEnum.HTML:
                            GetBody(emendamentoDto, attoDto, firme, persona, false, ref body);
                            break;
                        case TemplateTypeEnum.FIRMA:
                            GetBodyTemporaneo(emendamentoDto, attoDto, ref body);
                            break;
                        case TemplateTypeEnum.HTML_MODIFICABILE:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(template), template, null);
                    }

                    return body;
                }
                catch (Exception e)
                {
                    Log.Error("GetBodyEM", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }

        public async Task<string> GetCopertina(CopertinaModel model)
        {
            try
            {
                var countEM_Atto = await _unitOfWork.Atti.CountEM(model.Atto.UIDAtto, false, null, 0);
                var countSUBEM_Atto = await _unitOfWork.Atti.CountEM(model.Atto.UIDAtto, true, null, 0);

                var body = GetTemplate(TemplateTypeEnum.PDF_COPERTINA);
                body = body.Replace("{NumeroProgettoLegge}", model.Atto.NAtto);
                body = body.Replace("{OggettoProgettoLegge}", model.Atto.Oggetto);
                body = body.Replace("{DataOdierna}", DateTime.Now.ToString("dd/MM/yyyy"));

                if (countEM_Atto + countSUBEM_Atto != model.Totale)
                {
                    body = body.Replace("{CountEM}", $"EM/SUBEM estratti: {model.Totale}");
                    body = body.Replace("{CountSUBEM}", string.Empty);
                }
                else
                {
                    body = body.Replace("{CountEM}", $"EM: {countEM_Atto}");
                    body = body.Replace("{CountSUBEM}", $"/ SUBEM: {countSUBEM_Atto}");
                }

                body = body.Replace("{ORDINE}",
                    model.Ordinamento == OrdinamentoEnum.Votazione ? "VOTAZIONE" : "PRESENTAZIONE");

                return body;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetCopertina", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel firmaModel, PersonaDto persona,
            PinDto pin, bool firmaUfficio = false)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var counterFirme = 1;
                var firstEM = await _unitOfWork.Emendamenti.Get(firmaModel.Lista.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                var isRelatore = await _unitOfWork.Persone.IsRelatore(persona.UID_persona, firstEM.UIDAtto);
                var isAssessore = await _unitOfWork.Persone.IsAssessore(persona.UID_persona, firstEM.UIDAtto);

                var carica = await _unitOfWork.Persone.GetCarica(persona.UID_persona);

                foreach (var idGuid in firmaModel.Lista)
                {
                    if (counterFirme == Convert.ToInt32(AppSettingsConfiguration.LimiteFirmaMassivo) + 1) break;

                    var em = await GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var emDto = await GetEM_DTO(em, atto, persona, personeInDbLight);
                    var n_em = emDto.N_EM;

                    if (em.IDStato > (int) StatiEnum.Depositato)
                    {
                        results.Add(idGuid, $"ERROR: Emendamento {n_em} già votato e non è più sottoscrivibile");
                        continue;
                    }

                    var firmaCert = string.Empty;

                    if (firmaUfficio)
                    {
                        //Controllo se l'utente ha già firmato
                        if (emDto.Firma_da_ufficio)
                        {
                            results.Add(idGuid, $"ERROR: Emendamento {n_em} già firmato dall'ufficio");
                            continue;
                        }

                        firmaCert = EncryptString($"{AppSettingsConfiguration.FirmaUfficio}"
                            , AppSettingsConfiguration.masterKey);
                    }
                    else
                    {
                        //Controllo se l'utente ha già firmato
                        if (emDto.Firmato_Da_Me) continue;

                        //Controllo la firma del proponente
                        if (!emDto.Firmato_Dal_Proponente && em.UIDPersonaProponente != persona.UID_persona)
                        {
                            results.Add(idGuid, $"ERROR: Il Proponente non ha ancora firmato l'emendamento {n_em}");
                            continue;
                        }

                        if (emDto.Firma_su_invito && em.UIDPersonaProponente != persona.UID_persona)
                        {
                            var check_notifica = await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(emDto.UIDEM,
                                persona.UID_persona, false);

                            if (check_notifica == null)
                            {
                                results.Add(idGuid, $"ERROR: Emendamento {n_em} non firmabile. Il proponente ha riservato la firma dell’emendamento a un gruppo ristretto.");
                                continue;
                            }
                        }

                        var info_codice_carica_gruppo = string.Empty;
                        switch (persona.CurrentRole)
                        {
                            case RuoliIntEnum.Consigliere_Regionale:
                                info_codice_carica_gruppo = persona.Gruppo.codice_gruppo;
                                break;
                            case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                                info_codice_carica_gruppo = carica;
                                break;
                        }

                        var bodyFirmaCert =
                            $"{persona.DisplayName} ({info_codice_carica_gruppo}){(isRelatore ? " - RELATORE" : string.Empty)}{(isAssessore ? " - Ass. capofila" : string.Empty)}";
                        firmaCert = EncryptString(bodyFirmaCert
                            , AppSettingsConfiguration.masterKey);
                    }

                    var dataFirma = EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);

                    var countFirme = await _unitOfWork.Firme.CountFirme(idGuid);
                    if (countFirme == 0)
                    {
                        //Se è la prima firma dell'emendamento, questo viene cryptato e così certificato e non modificabile
                        em.Hash = firmaUfficio
                            ? EncryptString(AppSettingsConfiguration.MasterPIN, AppSettingsConfiguration.masterKey)
                            : pin.PIN;
                        em.UIDPersonaPrimaFirma = persona.UID_persona;
                        em.DataPrimaFirma = DateTime.Now;
                        var body = await GetBodyEM(em, new List<FirmeDto>
                            {
                                new FirmeDto
                                {
                                    UIDEM = idGuid,
                                    UID_persona = persona.UID_persona,
                                    FirmaCert = firmaCert,
                                    Data_firma = dataFirma,
                                    ufficio = firmaUfficio
                                }
                            }, persona,
                            TemplateTypeEnum.FIRMA);
                        var body_encrypt = EncryptString(body,
                            firmaUfficio ? AppSettingsConfiguration.MasterPIN : pin.PIN_Decrypt);

                        em.EM_Certificato = body_encrypt;
                    }

                    await _unitOfWork.Firme.Firma(idGuid, persona.UID_persona, firmaCert, dataFirma, firmaUfficio);

                    var destinatario_notifica =
                        await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(idGuid, persona.UID_persona);
                    if (destinatario_notifica != null)
                        await _unitOfWork.Notifiche_Destinatari.SetSeen_DestinatarioNotifica(destinatario_notifica,
                            persona.UID_persona);

                    results.Add(idGuid, $"{n_em} - OK");
                    counterFirme++;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - FirmaEmendamento", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> RitiroFirmaEmendamento(ComandiAzioneModel firmaModel,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                var firstEM = await _unitOfWork.Emendamenti.Get(firmaModel.Lista.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                var seduta = await _unitOfWork.Sedute.Get(atto.UIDSeduta!.Value);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                var ruoloSegreterie = await _unitOfWork.Ruoli.Get((int) RuoliIntEnum.Segreteria_Assemblea);
                var jumpMail = false;

                foreach (var idGuid in firmaModel.Lista)
                {
                    var em = await GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var emDto = await GetEM_DTO(em, atto, persona, personeInDbLight);
                    var n_em = emDto.N_EM;

                    var countFirme = await _unitOfWork.Firme.CountFirme(idGuid);
                    if (countFirme == 1)
                    {
                        if (DateTime.Now > seduta.Data_seduta)
                        {
                            results.Add(idGuid,
                                "ERROR: Non è possibile ritirare l'ultima firma, in quanto equivale al ritiro dell'emendamento: annuncia in Aula l'intenzione di ritiro della firma");
                            continue;
                        }

                        //RITIRA EM
                        em.IDStato = (int) StatiEnum.Ritirato;
                        em.UIDPersonaRitiro = persona.UID_persona;
                        em.DataRitiro = DateTime.Now;

                        if (DateTime.Now > seduta.Scadenza_presentazione.Value && DateTime.Now < seduta.Data_seduta)
                        {
                            jumpMail = true;
                            await _logicUtil.InvioMail(new MailModel
                            {
                                DA = "pem@consiglio.regione.lombardia.it",
                                A =
                                    $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                                OGGETTO =
                                    $"[RITIRATO] {n_em} del {atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}",
                                MESSAGGIO =
                                    $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato l'ultima firma presente sull'emendamento. L'EMENDAMENTO E' QUINDI DA CONSIDERARSI RITIRATO."
                            });
                        }
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _unitOfWork
                        .Firme
                        .GetFirmatari(em, FirmeTipoEnum.ATTIVI);
                    FIRME firma_utente;
                    if (persona.IsSegreteriaAssemblea)
                        firma_utente = firmeAttive.Single(f => f.ufficio);
                    else
                        firma_utente = firmeAttive.Single(f => f.UID_persona == persona.UID_persona);

                    firma_utente.Data_ritirofirma =
                        EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            AppSettingsConfiguration.masterKey);

                    if (DateTime.Now > seduta.Scadenza_presentazione.Value)
                        if (!jumpMail)
                            await _logicUtil.InvioMail(new MailModel
                            {
                                DA = "pem@consiglio.regione.lombardia.it",
                                A =
                                    $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                                OGGETTO =
                                    $"[FIRMA-RITIRATA] {n_em} nel {em.ATTI.TIPI_ATTO.Tipo_Atto} {em.ATTI.NAtto}",
                                MESSAGGIO =
                                    $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato la propria firma dall'emendamento in oggetto"
                            });

                    await _unitOfWork.CompleteAsync();
                    results.Add(idGuid, "OK");
                    jumpMail = false;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - RitiroFirmaEmendamento", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> EliminaFirmaEmendamento(ComandiAzioneModel firmaModel,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in firmaModel.Lista)
                {
                    var em = await GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var countFirme = await _unitOfWork.Firme.CountFirme(idGuid);
                    if (countFirme == 1)
                    {
                        em.EM_Certificato = string.Empty;
                        em.DataPrimaFirma = null;
                        em.UIDPersonaPrimaFirma = null;
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _logicFirme.GetFirme(em, FirmeTipoEnum.ATTIVI);
                    var firma_utente = firmeAttive.Single(f => f.UID_persona == persona.UID_persona);
                    var firma_da_ritirare = await _unitOfWork.Firme.Get(firma_utente.UIDEM, firma_utente.UID_persona);
                    _unitOfWork.Firme.Remove(firma_da_ritirare);

                    results.Add(idGuid, "OK");
                }

                await _unitOfWork.CompleteAsync();
                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - EliminaFirmaEmendamento", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> DepositaEmendamento(ComandiAzioneModel depositoModel,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                ManagerLogic.BloccaDeposito = true;
                var counterDepositi = 1;
                var firstEM = await _unitOfWork.Emendamenti.Get(depositoModel.Lista.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                foreach (var idGuid in depositoModel.Lista)
                {
                    if (counterDepositi == Convert.ToInt32(AppSettingsConfiguration.LimiteDepositoMassivo) + 1) break;

                    var em = await GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var emDto = await GetEM_DTO(em, atto, persona, personeInDbLight);
                    var n_em = emDto.N_EM;
                    if (emDto.IDStato >= (int) StatiEnum.Depositato) continue;

                    if (!emDto.Depositabile)
                    {
                        results.Add(idGuid, $"ERROR: Emendamento {n_em} non depositabile");
                        continue;
                    }

                    var etichetta_progressiva =
                        await _unitOfWork.Emendamenti.GetEtichetta(emDto.UIDAtto, emDto.Rif_UIDEM.HasValue) + 1;
                    var etichetta_encrypt =
                        EncryptString(etichetta_progressiva.ToString(), AppSettingsConfiguration.masterKey);

                    var checkProgressivo_unique =
                        await _unitOfWork.Emendamenti.CheckProgressivo(emDto.UIDAtto, etichetta_encrypt,
                            emDto.Rif_UIDEM.HasValue ? CounterEmendamentiEnum.SUB_EM : CounterEmendamentiEnum.EM);

                    if (!checkProgressivo_unique)
                    {
                        results.Add(idGuid, $"ERROR: Progressivo {n_em} occupato");
                        continue;
                    }

                    em.UIDPersonaDeposito = persona.UID_persona;
                    em.OrdinePresentazione = em.OrdineVotazione =
                        await _unitOfWork.Emendamenti.GetOrdinePresentazione(emDto.UIDAtto) + 1;
                    em.Timestamp = DateTime.Now;
                    em.DataDeposito = EncryptString(em.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                    em.IDStato = (int) StatiEnum.Depositato;
                    if (em.Rif_UIDEM.HasValue)
                        em.N_SUBEM = etichetta_encrypt;
                    else
                        em.N_EM = etichetta_encrypt;

                    var count_firme = await _unitOfWork.Firme.CountFirme(idGuid);
                    em.chkf = count_firme.ToString();

                    await _unitOfWork.CompleteAsync();

                    results.Add(idGuid, $"{n_em} - OK");

                    _unitOfWork.Stampe.Add(new STAMPE
                    {
                        UIDStampa = Guid.NewGuid(),
                        UIDUtenteRichiesta = persona.UID_persona,
                        CurrentRole = (int) persona.CurrentRole,
                        DataRichiesta = DateTime.Now,
                        UIDAtto = em.UIDAtto,
                        Da = 1,
                        A = 1,
                        Ordine = 1,
                        Notifica = true,
                        Scadenza = DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink)),
                        UIDEM = idGuid
                    });
                    await _unitOfWork.CompleteAsync();
                    counterDepositi++;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - DepositaEmendamento", e);
                throw e;
            }
        }

        public async Task RitiraEmendamento(EM em, PersonaDto persona)
        {
            try
            {
                em.IDStato = (int) StatiEnum.Ritirato;
                em.UIDPersonaRitiro = persona.UID_persona;
                em.DataRitiro = DateTime.Now;

                if (DateTime.Now > em.ATTI.SEDUTE.Scadenza_presentazione &&
                    DateTime.Now < em.ATTI.SEDUTE.Data_seduta)
                {
                    // INVIO MAIL A SEGRETERIA PER AVVISARE DEL RITIRO DELL'EM DOPO IL TERMINE DELL'ATTO
                    var nome_em = GetNomeEM(em, em.Rif_UIDEM.HasValue ? await GetEM(em.Rif_UIDEM.Value) : null);
                    var ruoloSegreterie = await _unitOfWork.Ruoli.Get(10);
                    await _logicUtil.InvioMail(new MailModel
                    {
                        DA = "pem@consiglio.regione.lombardia.it",
                        A =
                            $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                        OGGETTO = $"Ritirato {nome_em} nel {em.ATTI.TIPI_ATTO.Tipo_Atto} {em.ATTI.NAtto}",
                        MESSAGGIO = "ATTENZIONE: E' stato appena ritirato l'emendamento in oggetto"
                    });
                }
                else if (DateTime.Now > em.ATTI.SEDUTE.Data_seduta)
                {
                    throw new Exception(
                        "Non è possibile ritirare l'emendamento durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                }

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - RitiraEmendamento", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> ModificaStatoEmendamento(ModificaStatoModel model,
            PersonaDto personaDto)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                model.Lista ??= new List<Guid>();
                switch (model.All)
                {
                    case true when !model.Lista.Any():
                        model.Lista =
                            (await ScaricaEmendamenti(model.AttoUId, model.Ordine, model.Mode, personaDto,
                                personeInDbLight))
                            .Select(em => em.UIDEM).ToList();
                        break;
                    case true when model.Lista.Any():
                    {
                        var emendamentiInDb =
                            (await ScaricaEmendamenti(model.AttoUId, model.Ordine, model.Mode, personaDto,
                                personeInDbLight))
                            .Select(em => em.UIDEM).ToList();
                        emendamentiInDb.RemoveAll(em => model.Lista.Contains(em));
                        model.Lista = emendamentiInDb;
                        break;
                    }
                }

                var firstEM = await _unitOfWork.Emendamenti.Get(model.Lista.First());
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                foreach (var idGuid in model.Lista)
                {
                    var em = await GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    if (string.IsNullOrEmpty(em.DataDeposito))
                        continue;

                    em.IDStato = (int) model.Stato;
                    await _unitOfWork.CompleteAsync();
                    results.Add(idGuid, "OK");
                    try
                    {
                        //OPENDATA
                        if (AppSettingsConfiguration.AbilitaOpenData == "1")
                        {
                            var wsOD = new UpsertOpenData();
                            var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                            var firmeDto = firme.ToList();
                            var emDto = await GetEM_DTO(em, atto, null, personeInDbLight, null, null, false);
                            var resultOpenData = await GetEM_OPENDATA(emDto, atto,
                                firmeDto,
                                personeInDbLight.First(p => p.UID_persona == em.UIDPersonaProponente),
                                personeInDbLight);
                            wsOD.UpsertEM(resultOpenData, AppSettingsConfiguration.OpenData_PrivateToken);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("OpenDataEM", e);
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaStatoEmendamento", e);
                throw e;
            }
        }

        /// <summary>
        ///     Ritorna la lista delle parti testo emendabili
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PartiTestoDto>> GetPartiEM()
        {
            return (await _unitOfWork.Emendamenti.GetPartiEmendabili()).Select(Mapper.Map<PARTI_TESTO, PartiTestoDto>);
        }

        /// <summary>
        ///     Ritorna la lista dei tipi di emendamento disponibili
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Tipi_EmendamentiDto>> GetTipiEM()
        {
            return (await _unitOfWork
                    .Emendamenti
                    .GetTipiEmendamento())
                .Select(Mapper.Map<TIPI_EM, Tipi_EmendamentiDto>);
        }

        /// <summary>
        ///     Ritorna la lista delle missioni nel db
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MissioniDto>> GetMissioni()
        {
            return (await _unitOfWork
                    .Emendamenti
                    .GetMissioniEmendamento())
                .Select(Mapper.Map<MISSIONI, MissioniDto>);
        }

        /// <summary>
        ///     Ritorna la lista dei titoli missioni nel db
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TitoloMissioniDto>> GetTitoliMissioni()
        {
            return (await _unitOfWork
                    .Emendamenti
                    .GetTitoliMissioneEmendamento())
                .Select(Mapper.Map<TITOLI_MISSIONI, TitoloMissioniDto>);
        }

        /// <summary>
        ///     Ritorna la lista degli stati disponibili nel db
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StatiDto>> GetStatiEM()
        {
            return (await _unitOfWork
                    .Emendamenti
                    .GetStatiEmendamento())
                .Select(Mapper.Map<STATI_EM, StatiDto>);
        }

        /// <summary>
        ///     Ritorna la lista degli articoli emendabili disponibili per l'atto selezionato
        /// </summary>
        /// <param name="atto"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ArticoliDto>> GetArticoli(Guid atto)
        {
            return (await _unitOfWork
                    .Articoli
                    .GetArticoli(atto))
                .Select(Mapper.Map<ARTICOLI, ArticoliDto>);
        }

        public async Task EliminaEmendamento(EM em, Guid currentUId)
        {
            em.Eliminato = true;
            em.DataElimina = DateTime.Now;
            em.UIDPersonaElimina = currentUId;

            await _unitOfWork.CompleteAsync();
        }

        public async Task AssegnaNuovoProponente(EM em, AssegnaProponenteModel model)
        {
            var persona = await _logicPersone.GetPersona(model.NuovoProponente);
            em.IDStato = (int) StatiEnum.Depositato;
            em.UIDPersonaProponenteOLD = em.UIDPersonaProponente;
            em.UIDPersonaProponente = model.NuovoProponente;
            em.id_gruppo = (await _logicPersone
                    .GetGruppoAttualePersona(new List<string> {persona.GruppiAD}))
                .id_gruppo;

            await _unitOfWork.CompleteAsync();
        }

        public async Task RaggruppaEmendamento(EM em, string colore)
        {
            em.Colore = colore;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<HttpResponseMessage> Download(string path)
        {
            var complete_path = Path.Combine(
                AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                Path.GetFileName(path));
            var result = await ComposeFileResponse(complete_path);
            return result;
        }

        public async Task Proietta(EM em, Guid currentUId)
        {
            var em_in_proiezione = await _unitOfWork.Emendamenti.GetCurrentEMInProiezione(em.UIDAtto);
            if (em_in_proiezione != null) em_in_proiezione.Proietta = false;

            em.Proietta = true;
            em.DataProietta = DateTime.Now;
            em.UIDPersonaProietta = currentUId;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<ProiettaResponse> GetEM_ByProietta(Guid id, int ordine, PersonaDto persona)
        {
            var em_da_proiettare = await _unitOfWork.Emendamenti.GetEMInProiezione(id, ordine);
            if (em_da_proiettare == null) return null;

            var atto = await _unitOfWork.Atti.Get(em_da_proiettare.UIDAtto);
            var personeInDb = await _unitOfWork.Persone.GetAll();
            var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

            var proietta = new ProiettaResponse
                {EM = await GetEM_DTO(em_da_proiettare, atto, persona, personeInDbLight)};
            var em_next = await _unitOfWork.Emendamenti.GetEMInProiezione(id, ordine + 1);
            if (em_next != null) proietta.next = em_next.OrdineVotazione;

            var em_prev = await _unitOfWork.Emendamenti.GetEMInProiezione(id, ordine - 1);
            if (em_prev != null) proietta.prev = em_prev.OrdineVotazione;

            return proietta;
        }

        public async Task<EM> GetEM(Guid id)
        {
            return await _unitOfWork.Emendamenti.Get(id);
        }

        public async Task<EM> GetEM(string id)
        {
            var guidId = new Guid(id);
            return await GetEM(guidId);
        }

        public async Task<EmendamentiDto> GetEM_DTO(Guid emendamentoUId, ATTI atto, PersonaDto persona,
            List<PersonaLightDto> personeInDbLight, List<PersonaLightDto> relatori = null,
            PersonaDto presidente_regione = null, bool enable_cmd = true)
        {
            var em = await GetEM(emendamentoUId);
            return await GetEM_DTO(em, atto, persona, personeInDbLight, relatori, presidente_regione, enable_cmd);
        }

        public async Task<EmendamentiDto> GetEM_DTO(EM em, ATTI atto, PersonaDto persona,
            List<PersonaLightDto> personeInDbLight,
            List<PersonaLightDto> relatori = null, PersonaDto presidente_regione = null,
            bool enable_cmd = true)
        {
            try
            {
                em.ATTI = atto;

                var emendamentoDto = Mapper.Map<EM, EmendamentiDto>(em);

                emendamentoDto.N_EM = GetNomeEM(Mapper.Map<EM, EmendamentiDto>(em),
                    em.Rif_UIDEM.HasValue
                        ? await GetEM_DTO(em.Rif_UIDEM.Value, atto, persona, personeInDbLight)
                        : null);
                emendamentoDto.ConteggioFirme = await _logicFirme.CountFirme(emendamentoDto.UIDEM);
                if (!string.IsNullOrEmpty(emendamentoDto.DataDeposito))
                    emendamentoDto.DataDeposito = Decrypt(emendamentoDto.DataDeposito);

                if (!string.IsNullOrEmpty(emendamentoDto.EM_Certificato))
                    emendamentoDto.EM_Certificato = Decrypt(emendamentoDto.EM_Certificato, em.Hash);

                if (persona != null && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                                        persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta))
                    emendamentoDto.Firmato_Da_Me = await _unitOfWork.Firme.CheckFirmato(em.UIDEM, persona.UID_persona);

                emendamentoDto.Firma_da_ufficio = await _unitOfWork.Firme.CheckFirmatoDaUfficio(emendamentoDto.UIDEM);
                if (emendamentoDto.Firma_da_ufficio)
                {
                    var firmaUfficio = await _logicFirme.GetFirmaUfficio(emendamentoDto);
                    emendamentoDto.Firma_ufficio = firmaUfficio;
                }

                emendamentoDto.Firmato_Dal_Proponente =
                    await _unitOfWork.Firme.CheckFirmato(em.UIDEM, em.UIDPersonaProponente);
                emendamentoDto.PersonaProponente =
                    personeInDbLight.First(p => p.UID_persona == em.UIDPersonaProponente);
                emendamentoDto.PersonaCreazione = personeInDbLight.First(p => p.UID_persona == em.UIDPersonaCreazione);
                if (!string.IsNullOrEmpty(emendamentoDto.DataDeposito))
                    emendamentoDto.PersonaDeposito =
                        personeInDbLight.First(p => p.UID_persona == em.UIDPersonaDeposito);

                if (emendamentoDto.UIDPersonaModifica.HasValue)
                    emendamentoDto.PersonaModifica =
                        personeInDbLight.First(p => p.UID_persona == em.UIDPersonaModifica);

                emendamentoDto.gruppi_politici =
                    Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                        await _unitOfWork.Gruppi.Get(em.id_gruppo));

                if (persona == null) return emendamentoDto;

                emendamentoDto.AbilitaSUBEM = emendamentoDto.IDStato == (int) StatiEnum.Depositato
                                              && emendamentoDto.UIDPersonaProponente != persona.UID_persona
                                              && !emendamentoDto.ATTI.Chiuso ||
                                              persona.CurrentRole == RuoliIntEnum.Amministratore_Giunta &&
                                              persona.IsSegreteriaAssemblea;

                if (persona.IsSegreteriaAssemblea)
                    if (emendamentoDto.ConteggioFirme > 1)
                    {
                        var firme = await _logicFirme.GetFirme(emendamentoDto, FirmeTipoEnum.ATTIVI);
                        emendamentoDto.Firme = firme
                            .Where(f => f.UID_persona != emendamentoDto.UIDPersonaProponente)
                            .Select(f => f.FirmaCert)
                            .Aggregate((i, j) => i + "<br>" + j);
                    }

                if (enable_cmd)
                {
                    if (string.IsNullOrEmpty(em.DataDeposito))
                        emendamentoDto.Depositabile = _unitOfWork
                            .Emendamenti
                            .CheckIfDepositabile(emendamentoDto,
                                persona);

                    if (em.IDStato <= (int) StatiEnum.Depositato)
                        emendamentoDto.Firmabile = await _unitOfWork
                            .Firme
                            .CheckIfFirmabile(emendamentoDto,
                                persona);

                    if (!em.DataRitiro.HasValue && em.IDStato == (int) StatiEnum.Depositato)
                        emendamentoDto.Ritirabile = _unitOfWork
                            .Emendamenti
                            .CheckIfRitirabile(emendamentoDto,
                                persona);

                    if (string.IsNullOrEmpty(em.DataDeposito))
                        emendamentoDto.Eliminabile = _unitOfWork
                            .Emendamenti
                            .CheckIfEliminabile(emendamentoDto,
                                persona);

                    emendamentoDto.Modificabile = _unitOfWork
                        .Emendamenti
                        .CheckIfModificabile(emendamentoDto,
                            persona);

                    emendamentoDto.Invito_Abilitato = _unitOfWork
                        .Notifiche
                        .CheckIfNotificabile(emendamentoDto,
                            persona);
                }

                if (relatori != null)
                    if (relatori.Any(r => r.UID_persona == emendamentoDto.UIDPersonaProponente))
                        emendamentoDto.Proponente_Relatore = true;

                if (atto.UIDAssessoreRiferimento.HasValue)
                    emendamentoDto.Proponente_Assessore_Riferimento =
                        emendamentoDto.UIDPersonaProponente == atto.UIDAssessoreRiferimento;

                var presentato_oltre_termini = false;
                if (presidente_regione != null)
                    presentato_oltre_termini = IsOutdate(emendamentoDto, presidente_regione);

                emendamentoDto.PresentatoOltreITermini = presentato_oltre_termini;
                return emendamentoDto;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEM_DTO", e);
                throw e;
            }
        }

        private bool IsOutdate(EmendamentiDto emendamentoDto, PersonaDto presidente_regione)
        {
            var result = false;
            if (emendamentoDto.IDStato >= (int) StatiEnum.Depositato)
                if (Convert.ToDateTime(emendamentoDto.DataDeposito) >
                    emendamentoDto.ATTI.SEDUTE.Scadenza_presentazione)
                {
                    result = true;

                    if (emendamentoDto.IsSUBEM)
                        result = false;

                    if (emendamentoDto.Proponente_Relatore || emendamentoDto.Proponente_Assessore_Riferimento)
                        result = false;

                    if (emendamentoDto.Firmato_Dal_Proponente)
                        if (emendamentoDto.UIDPersonaProponente == presidente_regione.UID_persona)
                            result = false;

                    if (emendamentoDto.Rif_UIDEM != null) result = false;
                }

            return result;
        }

        public async Task<EmendamentiDto> GetEM_DTO_Light(Guid uidEM, ATTI atto, PersonaDto persona,
            List<PersonaLightDto> personeInDbLight)
        {
            try
            {
                var em = await _unitOfWork.Emendamenti.Get(uidEM);
                em.ATTI = atto;

                var emendamentoDto = Mapper.Map<EM, EmendamentiDto>(em);

                emendamentoDto.N_EM = GetNomeEM(Mapper.Map<EM, EmendamentiDto>(em),
                    em.Rif_UIDEM.HasValue
                        ? await GetEM_DTO_Light(em.Rif_UIDEM.Value, atto, persona, personeInDbLight)
                        : null);

                if (!string.IsNullOrEmpty(emendamentoDto.DataDeposito))
                    emendamentoDto.DataDeposito = Decrypt(emendamentoDto.DataDeposito);

                emendamentoDto.PersonaProponente =
                    personeInDbLight.First(p => p.UID_persona == em.UIDPersonaProponente);

                return emendamentoDto;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEM_DTO", e);
                throw e;
            }
        }

        public async Task<EmendamentiViewModel> GetEmendamenti(BaseRequest<EmendamentiDto> model,
            PersonaDto persona, int CLIENT_MODE, int VIEW_MODE, PersonaDto presidente_regione, Uri uri)
        {
            try
            {
                var queryFilter = new Filter<EM>();
                foreach (var filterStatement in model.filtro.Where(filterStatement =>
                    filterStatement.PropertyId == nameof(EmendamentiDto.N_EM)))
                    filterStatement.Value =
                        EncryptString(filterStatement.Value.ToString(), AppSettingsConfiguration.masterKey);

                var tags = new List<TagDto>();
                var tags_request = new FilterStatement<EmendamentiDto>();
                if (model.filtro.Any(statement => statement.PropertyId == "Tags"))
                {
                    tags_request = model.filtro.First(statement => statement.PropertyId == "Tags");
                    tags = JsonConvert.DeserializeObject<List<TagDto>>(tags_request.Value.ToString());
                    model.filtro.Remove(tags_request);
                }
                
                var firmatari = new List<Guid>();
                var firmatari_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == "Firmatario"))
                {
                    firmatari_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == "Firmatario"));
                    firmatari.AddRange(firmatari_request.Select(firmatario => new Guid(firmatario.Value.ToString())));
                    foreach (var firmatarioStatement in firmatari_request) model.filtro.Remove(firmatarioStatement);
                }
                
                var proponenti = new List<Guid>();
                var proponenti_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)))
                {
                    proponenti_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)));
                    proponenti.AddRange(proponenti_request.Select(proponente => new Guid(proponente.Value.ToString())));
                    foreach (var proponenteStatement in proponenti_request) model.filtro.Remove(proponenteStatement);
                }
                
                var gruppi = new List<int>();
                var gruppi_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.id_gruppo)))
                {
                    gruppi_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.id_gruppo)));
                    gruppi.AddRange(gruppi_request.Select(proponente => Convert.ToInt32(proponente.Value.ToString())));
                    foreach (var gruppiStatement in gruppi_request) model.filtro.Remove(gruppiStatement);
                }

                var stati = new List<int>();
                var stati_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.IDStato)))
                {
                    stati_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.IDStato)));
                    stati.AddRange(stati_request.Select(stato => Convert.ToInt32(stato.Value.ToString())));
                    foreach (var statiStatement in stati_request) model.filtro.Remove(statiStatement);
                }

                queryFilter.ImportStatements(model.filtro);

                var em_in_db = await _unitOfWork
                    .Emendamenti
                    .GetAll(persona,
                        model.ordine,
                        model.page,
                        model.size,
                        CLIENT_MODE,
                        queryFilter,
                        firmatari,
                        proponenti, 
                        gruppi,
                        stati,
                        tags);

                if (!em_in_db.Any())
                    return new EmendamentiViewModel
                    {
                        Data = new BaseResponse<EmendamentiDto>(
                            model.page,
                            model.size,
                            new List<EmendamentiDto>(),
                            model.filtro,
                            0,
                            uri),
                        Mode = (ClientModeEnum) Convert.ToInt16(CLIENT_MODE),
                        ViewMode = (ViewModeEnum) Convert.ToInt16(VIEW_MODE),
                        Ordinamento = model.ordine,
                        CurrentUser = persona
                    };

                var firstEM = await _unitOfWork.Emendamenti.Get(em_in_db.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var relatori = await _unitOfWork.Atti.GetRelatori(firstEM.UIDAtto);

                var result = new List<EmendamentiDto>();
                foreach (var em in em_in_db)
                {
                    var dto = await GetEM_DTO(em, atto, persona, personeInDbLight, relatori.ToList(),
                        presidente_regione);
                    result.Add(dto);
                }

                var total_em = await CountEM(model, persona, Convert.ToInt16(CLIENT_MODE), CounterEmendamentiEnum.NONE,
                    firmatari, proponenti, gruppi, stati, tags);
                if (firmatari_request.Any())
                    model.filtro.AddRange(firmatari_request);
                if (proponenti_request.Any())
                    model.filtro.AddRange(proponenti_request);
                if (gruppi.Any())
                    model.filtro.AddRange(gruppi_request);
                if (stati.Any())
                    model.filtro.AddRange(stati_request);

                return new EmendamentiViewModel
                {
                    Data = new BaseResponse<EmendamentiDto>(
                        model.page,
                        model.size,
                        result,
                        model.filtro,
                        total_em,
                        uri),
                    Mode = (ClientModeEnum) Convert.ToInt16(CLIENT_MODE),
                    ViewMode = (ViewModeEnum) Convert.ToInt16(VIEW_MODE),
                    Ordinamento = model.ordine,
                    CurrentUser = persona
                };
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEmendamenti", e);
                throw e;
            }
        }

        public async Task<EmendamentiViewModel> GetEmendamenti_RawChunk(BaseRequest<EmendamentiDto> model,
            PersonaDto persona, int CLIENT_MODE, Uri uri, List<PersonaLightDto> personeInDbLight,
            bool open_data_enabled = false, bool light_version = false)
        {
            try
            {
                var em_in_db = await _unitOfWork
                    .Emendamenti
                    .GetAll(persona,
                        model.ordine,
                        model.page,
                        model.size,
                        CLIENT_MODE);

                var result = new List<EmendamentiDto>();
                var totalProcessTime = 0f;
                var firstEM = await _unitOfWork.Emendamenti.Get(em_in_db.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);

                foreach (var guid in em_in_db)
                    try
                    {
                        var startTimer = DateTime.Now;
                        EmendamentiDto dto;
                        if (light_version)
                            dto = await GetEM_DTO_Light(guid, atto, persona, personeInDbLight);
                        else
                            dto = await GetEM_DTO(guid, atto, persona, personeInDbLight);

                        if (open_data_enabled)
                        {
                            var firme = await _logicFirme.GetFirme(dto, FirmeTipoEnum.TUTTE);
                            var firmeDto = firme.ToList();

                            var firmatari_opendata = "--";
                            try
                            {
                                if (firmeDto.Any(f =>
                                    f.Timestamp < Convert.ToDateTime(dto.DataDeposito)))
                                    firmatari_opendata = GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                            f.Timestamp < Convert.ToDateTime(dto.DataDeposito)),
                                        persona.CurrentRole, personeInDbLight);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            dto.Firme_OPENDATA = firmatari_opendata;
                        }

                        result.Add(dto);
                        var spentTime = Math.Round((DateTime.Now - startTimer).TotalSeconds, 2);
                        totalProcessTime += (float) spentTime;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                Log.Debug($"GetEmendamenti_RawChunk: Eseguito in {totalProcessTime} s");
                var total_em = await CountEM(model, persona, Convert.ToInt16(CLIENT_MODE));

                return new EmendamentiViewModel
                {
                    Data = new BaseResponse<EmendamentiDto>(
                        model.page,
                        model.size,
                        result,
                        model.filtro,
                        total_em,
                        uri),
                    Mode = (ClientModeEnum) Convert.ToInt16(CLIENT_MODE),
                    CurrentUser = persona
                };
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEmendamenti_RawChunk", e);
                throw e;
            }
        }

        public async Task<List<EmendamentiDto>> GetEmendamenti_RichiestaPropriaFirma(BaseRequest<EmendamentiDto> model,
            PersonaDto persona, int CLIENT_MODE)
        {
            try
            {
                var em_in_db = await _unitOfWork
                    .Emendamenti
                    .GetAll_RichiestaPropriaFirma(model.id,
                        persona,
                        model.ordine,
                        model.page,
                        model.size,
                        CLIENT_MODE);
                var result = new List<EmendamentiDto>();
                if (em_in_db == null) return result;
                var emInDb = em_in_db.ToList();
                if (!emInDb.Any()) return result;

                var atto = await _unitOfWork.Atti.Get(emInDb.First().UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                foreach (var em in emInDb)
                {
                    var dto = await GetEM_DTO(em, atto, null, personeInDbLight);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEmendamenti", e);
                throw e;
            }
        }

        public async Task<List<EmendamentiDto>> GetEmendamenti(ByQueryModel model)
        {
            try
            {
                var emendamenti = _unitOfWork
                    .Emendamenti
                    .GetAll(model);
                var result = new List<EmendamentiDto>();
                var atto = await _unitOfWork.Atti.Get(emendamenti.First().UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                foreach (var em in emendamenti)
                {
                    var dto = await GetEM_DTO(em, atto, null, personeInDbLight);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetEmendamenti", e);
                throw e;
            }
        }

        public async Task<int> CountEM(BaseRequest<EmendamentiDto> model, PersonaDto persona, int CLIENT_MODE,
            CounterEmendamentiEnum type = CounterEmendamentiEnum.NONE, List<Guid> firmatari = null,
            List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null, List<TagDto> tagDtos = null)
        {
            try
            {
                var queryFilter = new Filter<EM>();
                queryFilter.ImportStatements(model.filtro);

                return await _unitOfWork.Emendamenti.Count(model.id,
                    persona, type, CLIENT_MODE, queryFilter, firmatari, proponenti, gruppi, stati, tagDtos);
            }
            catch (Exception e)
            {
                Log.Error("Logic - CountEM", e);
                throw e;
            }
        }

        public async Task<int> CountEM(string query)
        {
            try
            {
                return await _unitOfWork.Emendamenti.Count(query);
            }
            catch (Exception e)
            {
                Log.Error("Logic - CountEM", e);
                throw e;
            }
        }

        public async Task<IEnumerable<EmendamentiDto>> ScaricaEmendamenti(Guid attoUId, OrdinamentoEnum ordine,
            ClientModeEnum mode,
            PersonaDto persona, List<PersonaLightDto> personeInDbLight, bool open_data_enabled = false,
            bool light_version = false)
        {
            var result = new List<EmendamentiDto>();
            var counter_em = await _unitOfWork.Emendamenti.Count(attoUId, persona, CounterEmendamentiEnum.NONE,
                (int) mode);

            var emList = await GetEmendamenti_RawChunk(new BaseRequest<EmendamentiDto>
                {
                    id = attoUId,
                    ordine = ordine,
                    page = 1,
                    size = counter_em
                },
                persona,
                (int) mode,
                new Uri(AppSettingsConfiguration.urlPEM),
                personeInDbLight,
                open_data_enabled,
                light_version);

            result.AddRange(emList.Data.Results);

            return result;
        }

        /// <summary>
        ///     Restituisce la stringa da aggiornare/inserire in OpenData
        /// </summary>
        /// <param name="em"></param>
        /// <param name="em_riferimento"></param>
        /// <param name="firme"></param>
        /// <param name="proponente"></param>
        /// <param name="personeInDbLight"></param>
        /// <param name="uidEM"></param>
        /// <returns></returns>
        public async Task<string> GetEM_OPENDATA(EmendamentiDto em, ATTI atto, List<FirmeDto> firme,
            PersonaLightDto proponente, List<PersonaLightDto> personeInDbLight)
        {
            var separatore = AppSettingsConfiguration.OpenData_Separatore;
            var result = string.Empty;
            try
            {
                var legislatura = await _unitOfWork.Legislature.Get(atto.SEDUTE.id_legislatura);
                var nome_em = em.N_EM;
                //Colonna IDEM
                result +=
                    $"{atto.TIPI_ATTO.Tipo_Atto}-{atto.NAtto}-{legislatura.num_legislatura}-{nome_em}{separatore}";
                //Colonna Atto
                result +=
                    $"{atto.TIPI_ATTO.Tipo_Atto}-{atto.NAtto}-{legislatura.num_legislatura}{separatore}";
                //Colonna Numero EM
                result += nome_em + separatore;
                //Colonna Data Deposito
                if (em.IDStato >= (int) StatiEnum.Depositato)
                {
                    var dataDeposito =
                        Convert.ToDateTime(em.DataDeposito);
                    result += dataDeposito.ToString("yyyy-MM-dd HH:mm") + separatore;
                }
                else
                {
                    result += "--" + separatore;
                }

                //Colonna Stato
                var statiEmendamento = await _unitOfWork.Emendamenti.GetStatiEmendamento();
                result += $"{em.IDStato}-{statiEmendamento.First(s => s.IDStato == em.IDStato).Stato}{separatore}";
                //Colonna Tipo EM
                var tipiEmendamento = await _unitOfWork.Emendamenti.GetTipiEmendamento();
                result +=
                    $"{em.IDTipo_EM}-{tipiEmendamento.First(t => t.IDTipo_EM == em.IDTipo_EM).Tipo_EM}{separatore}";
                //Colonna Parte
                var partiEmendabili = await _unitOfWork.Emendamenti.GetPartiEmendabili();
                result += $"{em.IDParte}-{partiEmendabili.First(p => p.IDParte == em.IDParte).Parte}{separatore}";
                //Colonna Articolo
                var articolo = string.Empty;
                if (em.UIDArticolo.HasValue)
                {
                    var art = await _unitOfWork.Articoli.GetArticolo(em.UIDArticolo.Value);
                    articolo = art.Articolo;
                }

                result += $"{articolo}{separatore}";

                //Colonna Comma
                var comma = string.Empty;
                if (em.UIDComma.HasValue)
                {
                    var com = await _unitOfWork.Commi.GetComma(em.UIDComma.Value);
                    comma = com.Comma;
                }

                result += $"{comma}{separatore}";
                //Colonna NTitolo
                result += $"{em.NTitolo}{separatore}";
                //Colonna NCapo
                result += $"{em.NCapo}{separatore}";
                //Colonna NMissione
                result += $"{em.NMissione}{separatore}";
                //Colonna NProgramma
                result += $"{em.NProgramma}{separatore}";
                //Colonna NTitoloB
                result += $"{em.NTitoloB}{separatore}";
                //Colonna Proponente
                result += $"{proponente.id_persona}-{proponente.DisplayName}{separatore}";
                //Colonna AreaPolitica
                if (em.AreaPolitica.HasValue)
                    switch ((AreaPoliticaIntEnum) em.AreaPolitica.Value)
                    {
                        case AreaPoliticaIntEnum.Maggioranza:
                            result += $"{AreaPoliticaEnum.Maggioranza}{separatore}";
                            break;
                        case AreaPoliticaIntEnum.Minoranza:
                            result += $"{AreaPoliticaEnum.Minoranza}{separatore}";
                            break;
                        case AreaPoliticaIntEnum.Misto_Maggioranza:
                            result += $"{AreaPoliticaEnum.Misto_Maggioranza}{separatore}";
                            break;
                        case AreaPoliticaIntEnum.Misto_Minoranza:
                            result += $"{AreaPoliticaEnum.Misto_Minoranza}{separatore}";
                            break;
                        default:
                            result += $"{separatore}";
                            break;
                    }
                else
                    result += $"{separatore}";

                //Colonna Firmatari
                if (em.IDStato >= (int) StatiEnum.Depositato)
                {
                    var firmeAnte = firme.Where(f =>
                        f.Timestamp < Convert.ToDateTime(em.DataDeposito));
                    var firmePost = firme.Where(f =>
                        f.Timestamp > Convert.ToDateTime(em.DataDeposito));

                    result +=
                        $"{GetFirmatariEM_OPENDATA(firmeAnte.ToList(), RuoliIntEnum.Amministratore_PEM, personeInDbLight)}{separatore}";
                    result +=
                        $"{GetFirmatariEM_OPENDATA(firmePost.ToList(), RuoliIntEnum.Amministratore_PEM, personeInDbLight)}{separatore}";
                }
                else
                {
                    result += "--" + separatore;
                    result += "--" + separatore;
                }

                //Colonna Link
                result += $"{AppSettingsConfiguration.urlPEM_ViewEM}{em.UID_QRCode}";

                return result;
            }
            catch (Exception e)
            {
                Log.Error("GetEM_OPENDATA", e);
                throw e;
            }
        }

        public string GetFirmatariEM_OPENDATA(IEnumerable<FirmeDto> firmeDtos, RuoliIntEnum ruolo,
            List<PersonaLightDto> personeInDbLight)
        {
            try
            {
                if (firmeDtos == null) return "--";

                if (!firmeDtos.Any()) return "--";

                var result = "";
                foreach (var firmeDto in firmeDtos)
                    if (string.IsNullOrEmpty(firmeDto.Data_ritirofirma))
                    {
                        if (ruolo == RuoliIntEnum.Amministratore_PEM)
                        {
                            var firmatario = personeInDbLight.First(p => p.UID_persona == firmeDto.UID_persona);
                            result +=
                                $"{firmatario.id_persona}-{firmeDto.FirmaCert}; ";
                        }
                        else
                        {
                            result += $"{firmeDto.FirmaCert}; ";
                        }
                    }
                    else
                    {
                        if (ruolo == RuoliIntEnum.Amministratore_PEM)
                        {
                            var firmatario = personeInDbLight.First(p => p.UID_persona == firmeDto.UID_persona);
                            result +=
                                $"{firmatario.id_persona}-{firmeDto.FirmaCert} (ritirata); ";
                        }
                        else
                        {
                            result +=
                                $"{firmeDto.FirmaCert} (ritirata); ";
                        }
                    }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatariEM_OPENDATA", e);
                throw e;
            }
        }

        public async Task<ProiettaResponse> GetEM_LiveProietta(Guid attoUId, PersonaDto persona)
        {
            var em_da_proiettare = await _unitOfWork.Emendamenti.GetCurrentEMInProiezione(attoUId);
            if (em_da_proiettare == null) return null;

            var atto = await _unitOfWork.Atti.Get(em_da_proiettare.UIDAtto);
            var personeInDb = await _unitOfWork.Persone.GetAll();
            var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
            var proietta = new ProiettaResponse
                {EM = await GetEM_DTO(em_da_proiettare, atto, persona, personeInDbLight)};
            var em_next =
                await _unitOfWork.Emendamenti.GetEMInProiezione(attoUId, em_da_proiettare.OrdineVotazione + 1);
            if (em_next != null) proietta.next = em_next.OrdineVotazione;

            var em_prev =
                await _unitOfWork.Emendamenti.GetEMInProiezione(attoUId, em_da_proiettare.OrdineVotazione - 1);
            if (em_prev != null) proietta.prev = em_prev.OrdineVotazione;

            return proietta;
        }

        public async Task<EM> GetEM_ByQR(Guid id)
        {
            return await _unitOfWork
                .Emendamenti
                .GetByQR(id);
        }

        public async Task<List<TagDto>> GetTags()
        {
            var result = await _unitOfWork.Emendamenti.GetTags();

            return result
                .Select(Mapper.Map<TAGS, TagDto>)
                .ToList();
        }
    }
}