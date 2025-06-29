﻿/*
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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using PortaleRegione.API.Controllers;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class NotificheLogic : BaseLogic
    {
        public NotificheLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEM, PersoneLogic logicPersone,
            UtilsLogic logicUtil, FirmeLogic logicFirme, DASILogic logicDASI, AttiFirmeLogic logicFirmeDASI)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEM;
            _logicPersona = logicPersone;
            _logicUtil = logicUtil;
            _logicFirme = logicFirme;
            _logicDasi = logicDASI;
            _logicAttiFirme = logicFirmeDASI;
        }

        public async Task<RiepilogoNotificheModel> GetNotificheInviate(BaseRequest<NotificaDto> model,
            PersonaDto currentUser,
            bool Archivio, Uri uri)
        {
            try
            {
                //Log.Debug($"Logic - GetNotificheInviate - page[{model.page}], pageSize[{model.size}]");
                var queryFilter = new Filter<NOTIFICHE>();
                queryFilter.ImportStatements(model.filtro);

                var idGruppo = 0;
                if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                    || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                    || currentUser.CurrentRole == RuoliIntEnum.Segreteria_Politica
                    || currentUser.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                    idGruppo = currentUser.Gruppo.id_gruppo;

                var notifiche = (await _unitOfWork.Notifiche
                        .GetNotificheInviate(currentUser, idGruppo, Archivio, model.page, model.size, queryFilter))
                    .Select(Mapper.Map<NOTIFICHE, NotificaDto>)
                    .ToList();

                var result = new List<NotificaDto>();
                foreach (var notifica in notifiche)
                {
                    if (notifica.UIDEM == Guid.Empty)
                    {
                        var atto_dasi = await _logicDasi.GetAttoDto(notifica.UIDAtto, currentUser);
                        notifica.ATTO_DASI = atto_dasi;
                        idGruppo = notifica.ATTO_DASI.id_gruppo;
                    }
                    else
                    {
                        var atto = await _unitOfWork.Atti.Get(notifica.UIDAtto);
                        notifica.EM = await _logicEm.GetEM_DTO(notifica.UIDEM, atto, currentUser);
                        idGruppo = notifica.EM.id_gruppo;
                    }

                    notifica.UTENTI_NoCons = await _logicPersona.GetPersona(notifica.Mittente,
                        idGruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                    result.Add(notifica);
                }

                return new RiepilogoNotificheModel
                {
                    Data = new BaseResponse<NotificaDto>(
                        model.page,
                        model.size,
                        result,
                        model.filtro,
                        await CountInviate(model, currentUser, Convert.ToBoolean(Archivio)),
                        uri),
                    CurrentUser = currentUser
                };
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetNotificheInviate", e);
                throw e;
            }
        }

        public async Task<RiepilogoNotificheModel> GetNotificheRicevute(BaseRequest<NotificaDto> model,
            PersonaDto currentUser,
            bool Archivio,
            bool Solo_Non_Viste, Uri uri)
        {
            var queryFilter = new Filter<NOTIFICHE>();
            queryFilter.ImportStatements(model.filtro);

            var idGruppo = 0;
            if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                || currentUser.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                idGruppo = currentUser.Gruppo.id_gruppo;

            var notifiche = (await _unitOfWork.Notifiche
                    .GetNotificheRicevute(currentUser, idGruppo, Archivio, Solo_Non_Viste, model.page, model.size,
                        queryFilter))
                .Select(Mapper.Map<NOTIFICHE, NotificaDto>)
                .ToList();

            var result = new List<NotificaDto>();

            foreach (var notifica in notifiche)
            {
                if (notifica.UIDEM == Guid.Empty)
                {
                    var atto_dasi = await _logicDasi.GetAttoDto(notifica.UIDAtto, currentUser);
                    if (atto_dasi == null)
                        continue;
                    notifica.ATTO_DASI = atto_dasi;
                    idGruppo = notifica.ATTO_DASI.id_gruppo;
                }
                else
                {
                    var atto = await _unitOfWork.Atti.Get(notifica.UIDAtto);
                    notifica.EM = await _logicEm.GetEM_DTO(notifica.UIDEM, atto, currentUser);
                    idGruppo = notifica.EM.id_gruppo;
                }

                notifica.UTENTI_NoCons = await _logicPersona.GetPersona(notifica.Mittente,
                    idGruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                result.Add(notifica);
            }

            return new RiepilogoNotificheModel
            {
                Data = new BaseResponse<NotificaDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await CountRicevute(model, currentUser, Convert.ToBoolean(Archivio),
                        Convert.ToBoolean(Solo_Non_Viste)),
                    uri),
                CurrentUser = currentUser
            };
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetDestinatariNotifica(string notificaId)
        {
            var destinatari = await _unitOfWork
                .Notifiche
                .GetDestinatariNotifica(notificaId);

            var result = new List<DestinatariNotificaDto>();
            foreach (var destinatario in destinatari)
            {
                var dto = Mapper.Map<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>(destinatario);
                if (destinatario.NOTIFICHE.UIDEM != null)
                    dto.Firmato = await _unitOfWork
                        .Firme
                        .CheckFirmato(destinatario.NOTIFICHE.UIDEM.Value, destinatario.UIDPersona);
                else
                    dto.Firmato = await _unitOfWork
                        .Atti_Firme
                        .CheckFirmato(destinatario.NOTIFICHE.UIDAtto, destinatario.UIDPersona);


                result.Add(dto);
            }

            return result;
        }

        public async Task<Dictionary<Guid, string>> InvitaAFirmare(ComandiAzioneModel model,
            PersonaDto currentUser)
        {
            var results = new Dictionary<Guid, string>();
            var listaDestinatari = new List<PersonaDto>();
            var listaResponsabili = new List<PersonaDto>();
            var sonoPersone = Guid.TryParse(model.ListaDestinatari.First(), out var _);
            if (sonoPersone)
            {
                foreach (var destinatario in model.ListaDestinatari)
                {
                    var dest = await _logicPersona.GetPersona(new Guid(destinatario), false);
                    listaDestinatari.Add(dest);

                    //#864 Notifiche per responsabili di segreteria
                    var responsabili = await _logicPersona.GetSegreteriaPolitica(dest.Gruppo.id_gruppo, true, false);
                    foreach (var resp in responsabili)
                    {
                        if (listaResponsabili.FirstOrDefault(i => i.UID_persona == resp.UID_persona) == null)
                        {
                            listaResponsabili.Add(resp);
                        }
                    }
                }
            }
            else
            {
                var sonoGruppi = int.TryParse(model.ListaDestinatari.First(), out var _);
                if (sonoGruppi)
                    foreach (var gruppoId in model.ListaDestinatari.Select(g => Convert.ToInt32(g)))
                    {
                        listaDestinatari.AddRange(await _logicPersona.GetConsiglieriGruppo(gruppoId));

                        //#864 Notifiche per responsabili di segreteria
                        var responsabili = await _logicPersona.GetSegreteriaPolitica(gruppoId, true, false);
                        foreach (var resp in responsabili)
                        {
                            if (listaResponsabili.FirstOrDefault(i => i.UID_persona == resp.UID_persona) == null)
                            {
                                listaResponsabili.Add(resp);
                            }
                        }
                    }
            }

            if (!listaDestinatari.Any())
            {
                results.Add(Guid.Empty, "Nessun invitato a firmare");
                return results;
            }

            var bodyMail = string.Empty;
            var attachMail = new List<AllegatoMail>();

            if (model.IsDASI)
            {
                #region DASI

                foreach (var idGuid in model.Lista)
                {
                    var atto = await _logicDasi.GetAttoDto(idGuid, currentUser);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var n_atto = atto.Display;

                    var check = _unitOfWork.Notifiche.CheckIfNotificabile(atto, currentUser);
                    if (check == false)
                    {
                        results.Add(idGuid, $"ERROR: Non è possibile creare notifiche per l'atto {n_atto}");
                        continue;
                    }

                    var _guid = Guid.NewGuid();
                    var sync_guid = Guid.NewGuid();

                    var newNotifica = new NOTIFICHE
                    {
                        UIDNotifica = _guid.ToString(),
                        UIDAtto = atto.UIDAtto,
                        Mittente = currentUser.UID_persona,
                        RuoloMittente = (int)currentUser.CurrentRole,
                        IDTipo = 1,
                        Messaggio = string.Empty,
                        DataCreazione = DateTime.Now,
                        IdGruppo = atto.id_gruppo,
                        SyncGUID = sync_guid
                    };

                    _unitOfWork.Notifiche.Add(newNotifica);

                    var destinatariNotifica = new List<NOTIFICHE_DESTINATARI>();
                    foreach (var destinatario in listaDestinatari)
                    {
                        var existDestinatario =
                            await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(atto.UIDAtto,
                                destinatario.UID_persona, true);

                        if (existDestinatario == null)
                            destinatariNotifica.Add(new NOTIFICHE_DESTINATARI
                            {
                                UIDNotifica = _guid.ToString(),
                                UIDPersona = destinatario.UID_persona,
                                IdGruppo = atto.id_gruppo,
                                UID = Guid.NewGuid()
                            });
                    }

                    try
                    {
                        if (destinatariNotifica.Any()) _unitOfWork.Notifiche_Destinatari.AddRange(destinatariNotifica);

                        await _unitOfWork.CompleteAsync();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Errore salvataggio a database", e); //#984
                    }

                    // #954
                    bodyMail +=
                        $@"[{atto.Display}] (proponente {atto.PersonaProponente.DisplayName} - {atto.gruppi_politici.codice_gruppo}) - {atto.OggettoView()}
<a href='{AppSettingsConfiguration.urlDASI_ViewATTO.Replace("{{UIDATTO}}", atto.UIDAtto.ToString())}'> Collegati all’atto per procedere alla firma</a><br>";

                    var attoInDb = await _logicDasi.Get(atto.UIDAtto);
                    var content = await _logicDasi.PDFIstantaneo(attoInDb, null);
                    attachMail.Add(new AllegatoMail(content, $"{n_atto}.pdf"));
                    results.Add(idGuid, $"{n_atto} - OK");
                }

                if (attachMail.Any())
                {
                    // #954
                    bodyMail += $@"<br><br><br> <a href='{AppSettingsConfiguration.urlDASI_ViewATTO.Replace("{{UIDATTO}}", "riepilogodasi")}'>Collegati alla piattaforma per visualizzare tutti gli atti del tuo gruppo</a>";

                    await _logicUtil.InvioMail(new MailModel
                    {
                        OGGETTO = "Invito a firmare un atto",
                        DA = currentUser.email,
                        A =
                            $"{string.Join(";", listaDestinatari.Select(p => p.email))};{string.Join(";", listaResponsabili.Select(r => r.email))}",
                        MESSAGGIO = $"{currentUser.DisplayName} chiede di firmare i seguenti atti: <br> {bodyMail}",
                        ATTACHMENTS = attachMail
                    });
                }

                #endregion
            }
            else
            {
                #region EMENDAMENTI

                var firstEM = await _unitOfWork.Emendamenti.Get(model.Lista.First(), false);
                var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                foreach (var idGuid in model.Lista)
                {
                    var em = await _logicEm.GetEM_DTO(idGuid, atto, currentUser);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var n_em = em.N_EM;

                    if (em.IDStato >= (int)StatiEnum.Depositato)
                    {
                        results.Add(idGuid,
                            $"ERROR: Non è possibile creare notifiche per {n_em} essendo già stato depositato");
                        continue;
                    }

                    var check = _unitOfWork.Notifiche.CheckIfNotificabile(em, currentUser);
                    if (check == false)
                    {
                        results.Add(idGuid, $"ERROR: Non è possibile creare notifiche per {n_em}");
                        continue;
                    }

                    var _guid = Guid.NewGuid();
                    var sync_guid = Guid.NewGuid();

                    var newNotifica = new NOTIFICHE
                    {
                        UIDNotifica = _guid.ToString(),
                        UIDEM = em.UIDEM,
                        UIDAtto = em.UIDAtto,
                        Mittente = currentUser.UID_persona,
                        RuoloMittente = (int)currentUser.CurrentRole,
                        IDTipo = 1,
                        Messaggio = string.Empty,
                        DataScadenza = em.ATTI.SEDUTE.Scadenza_presentazione,
                        DataCreazione = DateTime.Now,
                        IdGruppo = em.id_gruppo,
                        SyncGUID = sync_guid
                    };

                    _unitOfWork.Notifiche.Add(newNotifica);

                    var destinatariNotifica = new List<NOTIFICHE_DESTINATARI>();
                    foreach (var destinatario in listaDestinatari)
                    {
                        var existDestinatario =
                            await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(em.UIDEM,
                                destinatario.UID_persona);

                        if (existDestinatario == null)
                            destinatariNotifica.Add(new NOTIFICHE_DESTINATARI
                            {
                                UIDNotifica = _guid.ToString(),
                                UIDPersona = destinatario.UID_persona,
                                IdGruppo = em.id_gruppo,
                                UID = Guid.NewGuid()
                            });
                    }

                    try
                    {
                        if (destinatariNotifica.Any()) _unitOfWork.Notifiche_Destinatari.AddRange(destinatariNotifica);

                        await _unitOfWork.CompleteAsync();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Errore salvataggio a database", e); //#984
                    }

                    var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                    bodyMail += await _logicEm.GetBodyEM(em, firme.ToList(), currentUser, TemplateTypeEnum.HTML);
                    bodyMail +=
                        $"<br/> <a href='{AppSettingsConfiguration.urlPEM_ViewEM}{em.UID_QRCode}'>Vedi online</a>";
                    results.Add(idGuid, $"{n_em} - OK");
                }

                if (!string.IsNullOrEmpty(bodyMail))
                    await _logicUtil.InvioMail(new MailModel
                    {
                        OGGETTO = "Invito a firmare i seguenti emendamenti",
                        DA = currentUser.email,
                        A =
                            $"{string.Join(";", listaDestinatari.Select(p => p.email))};{string.Join(";", listaResponsabili.Select(r => r.email))}",
                        MESSAGGIO = bodyMail
                    });

                #endregion
            }

            return results;
        }

        public async Task<int> CountRicevute(BaseRequest<NotificaDto> model, PersonaDto currentUser, bool Archivio,
            bool Solo_Non_Viste)
        {
            var queryFilter = new Filter<NOTIFICHE>();
            queryFilter.ImportStatements(model.filtro);
            var idGruppo = 0;
            if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta)
                idGruppo = currentUser.Gruppo.id_gruppo;

            return await _unitOfWork.Notifiche.CountRicevute(currentUser, idGruppo, Archivio, Solo_Non_Viste,
                queryFilter);
        }

        public async Task<int> CountInviate(BaseRequest<NotificaDto> model, PersonaDto currentUser, bool Archivio)
        {
            var queryFilter = new Filter<NOTIFICHE>();
            queryFilter.ImportStatements(model.filtro);
            var idGruppo = 0;
            if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta)
                idGruppo = currentUser.Gruppo.id_gruppo;

            var result = await _unitOfWork.Notifiche.CountInviate(currentUser, idGruppo, Archivio, queryFilter);
            return result;
        }

        public async Task NotificaVista(string notificaId, Guid personaUId)
        {
            try
            {
                var destinatario = await _unitOfWork
                    .Notifiche_Destinatari.Get(notificaId, personaUId);
                if (destinatario == null) return;

                destinatario.Visto = true;
                destinatario.DataVisto = DateTime.Now;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - NotificaVista", e);
                throw e;
            }
        }

        public async Task<Dictionary<string, string>> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo,
            PersonaDto persona)
        {
            var result = new Dictionary<string, string>();

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM) tipo = TipoDestinatarioNotificaEnum.TUTTI;

            switch (tipo)
            {
                case TipoDestinatarioNotificaEnum.TUTTI:
                    result = (await _logicPersona.GetProponenti())
                        .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    break;
                case TipoDestinatarioNotificaEnum.CONSIGLIERI:
                    if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                        persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                    {
                        result = (await _logicPersona.GetAssessoriRiferimento())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                             persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                    {
                        result = (await _logicPersona.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else
                    {
                        var consiglieri_In_Db = (await _logicPersona.GetConsiglieri())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName_GruppoCode);
                        foreach (var consigliere in consiglieri_In_Db)
                            result.Add(consigliere.Key, consigliere.Value);
                    }

                    break;
                case TipoDestinatarioNotificaEnum.ASSESSORI:
                    if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                        persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                    {
                        result = (await _logicPersona.GetAssessoriRiferimento())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                             persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                    {
                        result = (await _logicPersona.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else
                    {
                        var assessori_In_Db = (await _logicPersona.GetAssessoriRiferimento())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        foreach (var assessori in assessori_In_Db) result.Add(assessori.Key, assessori.Value);
                    }

                    break;
                case TipoDestinatarioNotificaEnum.GRUPPI:
                    result = (await _logicPersona.GetGruppiAttivi())
                        .ToDictionary(k => k.id.ToString(), z => z.descr);
                    break;
                case TipoDestinatarioNotificaEnum.RELATORI:
                    result = (await _logicPersona.GetRelatori(atto))
                        .ToDictionary(k => k.UID_persona.ToString(), z => z.DisplayName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            return result;
        }

        public async Task<Dictionary<string, string>> GetListaDestinatari(TipoDestinatarioNotificaEnum tipo,
            PersonaDto persona)
        {
            var result = new Dictionary<string, string>();

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM) tipo = TipoDestinatarioNotificaEnum.TUTTI;

            switch (tipo)
            {
                case TipoDestinatarioNotificaEnum.TUTTI:
                    result = (await _logicPersona.GetProponenti())
                        .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    break;
                case TipoDestinatarioNotificaEnum.CONSIGLIERI:
                    if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                        persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                    {
                        result = (await _logicPersona.GetAssessoriRiferimento())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                             persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                    {
                        result = (await _logicPersona.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                    }
                    else
                    {
                        var consiglieri_In_Db = (await _logicPersona.GetConsiglieri())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName_GruppoCode);
                        foreach (var consigliere in consiglieri_In_Db)
                            result.Add(consigliere.Key, consigliere.Value);
                    }

                    break;
                case TipoDestinatarioNotificaEnum.GRUPPI:
                    result = (await _logicPersona.GetGruppiAttivi())
                        .ToDictionary(k => k.id.ToString(), z => z.descr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            return result;
        }

        public async Task AccettaPropostaFirma(string id)
        {
            var notifica = await _unitOfWork.Notifiche.Get(id);
            notifica.Valida = true;
            notifica.Chiuso = true;
            var firma = await _unitOfWork.Atti_Firme.FindInCache(notifica.UIDAtto, notifica.Mittente);
            firma.Valida = true;

            await _unitOfWork.CompleteAsync();
        }

        public async Task AccettaRitiroFirma(string id)
        {
            var notifica = await _unitOfWork.Notifiche.Get(id);
            notifica.Chiuso = true;
            var firma = await _unitOfWork.Atti_Firme.FindInCache(notifica.UIDAtto, notifica.Mittente);
            firma.Data_ritirofirma =
                BALHelper.EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    AppSettingsConfiguration.masterKey);
            await _unitOfWork.CompleteAsync();
            var atto = await _logicDasi.GetAttoDto(notifica.UIDAtto);
            SEDUTE seduta = null;
            if (atto.UIDSeduta.HasValue) seduta = await _unitOfWork.Sedute.Get(atto.UIDSeduta.Value);
            //#755 Se l'atto non ha una seduta assegnata controllo la data di proposta
            if (!string.IsNullOrEmpty(atto.DataRichiestaIscrizioneSeduta) && seduta == null)
                seduta =
                    await _unitOfWork.Sedute.Get(Convert.ToDateTime(atto.DataRichiestaIscrizioneSeduta));

            var check_presentazione = await _logicDasi.ControlloFirmePresentazione(atto, true, seduta);
            if (!string.IsNullOrEmpty(check_presentazione))
            {
                var attoInDb = await _unitOfWork.DASI.Get(notifica.UIDAtto);
                attoInDb.IDStato = (int)StatiAttoEnum.COMPLETATO;
                attoInDb.TipoChiusuraIter = (int)TipoChiusuraIterEnum.DECADUTO;
                attoInDb.DataRitiro = DateTime.Now;

                await _unitOfWork.CompleteAsync();

                // #844
                try
                {
                    var mailModel = new MailModel
                    {
                        DA = AppSettingsConfiguration.EmailInvioDASI,
                        A = AppSettingsConfiguration.EmailInvioDASI,
                        OGGETTO = $"L'atto {atto.Display} è decaduto",
                        MESSAGGIO =
                            $"L'atto {atto.Display} è decaduto perchè non ha più il numero di firme necessario. {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception e)
                {
                    Log.Error("Invio Mail - Decaduto", e);
                }
            }
        }

        public async Task ArchiviaNotifiche(List<string> notifiche, PersonaDto user)
        {
            foreach (var id in notifiche)
            {
                var notifica = await _unitOfWork.Notifiche.Get(id);

                // #948
                if (user.IsResponsabileSegreteriaPolitica)
                {
                    if (notifica.IdGruppo.Equals(user.Gruppo.id_gruppo))
                    {
                        notifica.Chiuso = true;
                        var listaDestinatariGruppo =
                            await _unitOfWork.Notifiche_Destinatari.Get(id, user.Gruppo.id_gruppo);
                        if (listaDestinatariGruppo.Any())
                        {
                            foreach (var notificheDestinatario in listaDestinatariGruppo)
                            {
                                notificheDestinatario.Chiuso = true;
                            }
                        }

                        await _unitOfWork.CompleteAsync();
                    }
                }
                else
                {
                    if (notifica.Mittente == user.UID_persona)
                    {
                        notifica.Chiuso = true;
                    }
                    else
                    {
                        var notificheDestinatari = await _unitOfWork.Notifiche_Destinatari.Get(id, user.UID_persona);
                        if (notificheDestinatari != null) notificheDestinatari.Chiuso = true;
                    }

                    await _unitOfWork.CompleteAsync();
                }
            }
        }

        public async Task ControlloNotifiche(PersonaDto currentUser, Guid uid)
        {
            try
            {
                if (currentUser == null) return;
                if (!currentUser.IsConsigliereRegionale && !currentUser.IsAssessore) return;

                var result = await _unitOfWork.Notifiche.UpdateNotificaVista(currentUser, uid);
                if (result)
                    await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}