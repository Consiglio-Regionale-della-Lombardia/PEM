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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using PortaleRegione.API.Controllers;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class NotificheLogic : BaseLogic
    {
        private readonly DASILogic _logicDasi;
        private readonly EmendamentiLogic _logicEm;
        private readonly FirmeLogic _logicFirme;
        private readonly AttiFirmeLogic _logicFirmeDasi;
        private readonly PersoneLogic _logicPersone;
        private readonly UtilsLogic _logicUtil;
        private readonly IUnitOfWork _unitOfWork;

        #region ctor

        public NotificheLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEM, PersoneLogic logicPersone,
            UtilsLogic logicUtil, FirmeLogic logicFirme, DASILogic logicDASI, AttiFirmeLogic logicFirmeDASI)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEM;
            _logicPersone = logicPersone;
            _logicUtil = logicUtil;
            _logicFirme = logicFirme;
            _logicDasi = logicDASI;
            _logicFirmeDasi = logicFirmeDASI;
        }

        #endregion

        #region GetNotificheInviate

        public async Task<IEnumerable<NotificaDto>> GetNotificheInviate(BaseRequest<NotificaDto> model,
            PersonaDto currentUser,
            bool Archivio)
        {
            try
            {
                Log.Debug($"Logic - GetNotificheInviate - page[{model.page}], pageSize[{model.size}]");
                var queryFilter = new Filter<NOTIFICHE>();
                queryFilter.ImportStatements(model.filtro);

                var idGruppo = 0;
                if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                    || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta)
                    idGruppo = currentUser.Gruppo.id_gruppo;

                var notifiche = (await _unitOfWork.Notifiche
                        .GetNotificheInviate(currentUser, idGruppo, Archivio, model.page, model.size, queryFilter))
                    .Select(Mapper.Map<NOTIFICHE, NotificaDto>)
                    .ToList();

                if (!notifiche.Any()) return new List<NotificaDto>();

                var result = new List<NotificaDto>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                foreach (var notifica in notifiche)
                {
                    var idgruppo = 0;
                    if (notifica.UIDEM == Guid.Empty)
                    {
                        var atto_dasi = await _logicDasi.GetAttoDto(notifica.UIDAtto, currentUser, personeInDbLight);
                        notifica.ATTO_DASI = atto_dasi;
                        idGruppo = notifica.ATTO_DASI.id_gruppo;
                    }
                    else
                    {
                        var atto = await _unitOfWork.Atti.Get(notifica.UIDAtto);
                        notifica.EM = await _logicEm.GetEM_DTO(notifica.UIDEM, atto, currentUser, personeInDbLight);
                        idGruppo = notifica.EM.id_gruppo;
                    }

                    notifica.UTENTI_NoCons = await _logicPersone.GetPersona(notifica.Mittente,
                        idGruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                    result.Add(notifica);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetNotificheInviate", e);
                throw e;
            }
        }

        #endregion

        #region GetNotificheRicevute

        public async Task<IEnumerable<NotificaDto>> GetNotificheRicevute(BaseRequest<NotificaDto> model,
            PersonaDto currentUser,
            bool Archivio,
            bool Solo_Non_Viste)
        {
            try
            {
                Log.Debug($"Logic - GetNotificheRicevute - page[{model.page}], pageSize[{model.size}]");
                var queryFilter = new Filter<NOTIFICHE>();
                queryFilter.ImportStatements(model.filtro);

                var idGruppo = 0;
                if (currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                    || currentUser.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta)
                    idGruppo = currentUser.Gruppo.id_gruppo;

                var notifiche = (await _unitOfWork.Notifiche
                        .GetNotificheRicevute(currentUser, idGruppo, Archivio, Solo_Non_Viste, model.page, model.size,
                            queryFilter))
                    .Select(Mapper.Map<NOTIFICHE, NotificaDto>)
                    .ToList();

                if (!notifiche.Any()) return new List<NotificaDto>();

                var result = new List<NotificaDto>();

                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                foreach (var notifica in notifiche)
                {
                    var idgruppo = 0;
                    if (notifica.UIDEM == Guid.Empty)
                    {
                        var atto_dasi = await _logicDasi.GetAttoDto(notifica.UIDAtto, currentUser, personeInDbLight);
                        notifica.ATTO_DASI = atto_dasi;
                        idGruppo = notifica.ATTO_DASI.id_gruppo;
                    }
                    else
                    {
                        var atto = await _unitOfWork.Atti.Get(notifica.UIDAtto);
                        notifica.EM = await _logicEm.GetEM_DTO(notifica.UIDEM, atto, currentUser, personeInDbLight);
                        idGruppo = notifica.EM.id_gruppo;
                    }

                    notifica.UTENTI_NoCons = await _logicPersone.GetPersona(notifica.Mittente,
                        idGruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                    result.Add(notifica);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetNotificheRicevute", e);
                throw e;
            }
        }

        #endregion

        #region GetDestinatariNotifica

        public async Task<IEnumerable<DestinatariNotificaDto>> GetDestinatariNotifica(long notificaId)
        {
            try
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
            catch (Exception e)
            {
                Log.Error("Logic - GetDestinatariNotifica", e);
                throw e;
            }
        }

        #endregion

        #region InvitaAFirmare

        public async Task<Dictionary<Guid, string>> InvitaAFirmare(ComandiAzioneModel model,
            PersonaDto currentUser)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var listaDestinatari = new List<PersonaDto>();
                var sonoPersone = Guid.TryParse(model.ListaDestinatari.First(), out var _);
                if (sonoPersone)
                {
                    foreach (var destinatario in model.ListaDestinatari)
                        listaDestinatari.Add(await _logicPersone.GetPersona(new Guid(destinatario), false));
                }
                else
                {
                    var sonoGruppi = int.TryParse(model.ListaDestinatari.First(), out var _);
                    if (sonoGruppi)
                        foreach (var gruppoId in model.ListaDestinatari.Select(g => Convert.ToInt32(g)))
                            listaDestinatari.AddRange(await _logicPersone.GetConsiglieriGruppo(gruppoId));
                }

                if (!listaDestinatari.Any())
                {
                    results.Add(Guid.Empty, "Nessun invitato a firmare");
                    return results;
                }

                var bodyMail = string.Empty;
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                if (model.IsDASI)
                {
                    #region DASI

                    foreach (var idGuid in model.Lista)
                    {
                        var atto = await _logicDasi.GetAttoDto(idGuid, currentUser, personeInDbLight);
                        if (atto == null)
                        {
                            results.Add(idGuid, "ERROR: NON TROVATO");
                            continue;
                        }

                        var n_atto = atto.NAtto;

                        if (atto.IDStato >= (int) StatiAttoEnum.PRESENTATO)
                        {
                            results.Add(idGuid,
                                $"ERROR: Non è possibile creare notifiche per l'atto {n_atto} essendo già stato presentato");
                            continue;
                        }

                        var check = _unitOfWork.Notifiche.CheckIfNotificabile(atto, currentUser);
                        if (check == false)
                        {
                            results.Add(idGuid, $"ERROR: Non è possibile creare notifiche per l'atto {n_atto}");
                            continue;
                        }

                        var newNotifica = new NOTIFICHE
                        {
                            UIDAtto = atto.UIDAtto,
                            Mittente = currentUser.UID_persona,
                            RuoloMittente = (int) currentUser.CurrentRole,
                            IDTipo = 1,
                            Messaggio = string.Empty,
                            DataCreazione = DateTime.Now,
                            IdGruppo = atto.id_gruppo,
                            SyncGUID = Guid.NewGuid()
                        };

                        var destinatariNotifica = new List<NOTIFICHE_DESTINATARI>();
                        foreach (var destinatario in listaDestinatari)
                        {
                            var existDestinatario =
                                await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotificaDASI(atto.UIDAtto,
                                    destinatario.UID_persona);

                            if (!existDestinatario)
                                destinatariNotifica.Add(new NOTIFICHE_DESTINATARI
                                {
                                    NOTIFICHE = newNotifica,
                                    UIDPersona = destinatario.UID_persona,
                                    IdGruppo = atto.id_gruppo,
                                    UID = Guid.NewGuid()
                                });
                        }

                        if (destinatariNotifica.Any()) _unitOfWork.Notifiche_Destinatari.AddRange(destinatariNotifica);

                        await _unitOfWork.CompleteAsync();
                        var attoInDb = await _logicDasi.Get(atto.UIDAtto);
                        var firme = await _logicFirmeDasi.GetFirme(attoInDb, FirmeTipoEnum.TUTTE);
                        bodyMail += await _logicDasi.GetBodyDASI(attoInDb, firme, currentUser, TemplateTypeEnum.HTML);
                        bodyMail +=
                            $"<br/> <a href='{AppSettingsConfiguration.urlPEM_ViewEM}{atto.UID_QRCode}'>Vedi online</a>";
                        results.Add(idGuid, $"{n_atto} - OK");
                    }

                    if (!string.IsNullOrEmpty(bodyMail))
                        await _logicUtil.InvioMail(new MailModel
                        {
                            OGGETTO = "Invito a firmare i seguenti atti",
                            DA = currentUser.email,
                            A = listaDestinatari.Select(p => p.email).Aggregate((m1, m2) => $"{m1};{m2}"),
                            MESSAGGIO = bodyMail
                        });

                    #endregion
                }
                else
                {
                    #region EMENDAMENTI

                    var firstEM = await _unitOfWork.Emendamenti.Get(model.Lista.First(), false);
                    var atto = await _unitOfWork.Atti.Get(firstEM.UIDAtto);
                    foreach (var idGuid in model.Lista)
                    {
                        var em = await _logicEm.GetEM_DTO(idGuid, atto, currentUser, personeInDbLight);
                        if (em == null)
                        {
                            results.Add(idGuid, "ERROR: NON TROVATO");
                            continue;
                        }

                        var n_em = em.N_EM;

                        if (em.IDStato >= (int) StatiEnum.Depositato)
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

                        var newNotifica = new NOTIFICHE
                        {
                            UIDEM = em.UIDEM,
                            UIDAtto = em.UIDAtto,
                            Mittente = currentUser.UID_persona,
                            RuoloMittente = (int) currentUser.CurrentRole,
                            IDTipo = 1,
                            Messaggio = string.Empty,
                            DataScadenza = em.ATTI.SEDUTE.Scadenza_presentazione,
                            DataCreazione = DateTime.Now,
                            IdGruppo = em.id_gruppo,
                            SyncGUID = Guid.NewGuid()
                        };

                        var destinatariNotifica = new List<NOTIFICHE_DESTINATARI>();
                        foreach (var destinatario in listaDestinatari)
                        {
                            var existDestinatario =
                                await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(em.UIDEM,
                                    destinatario.UID_persona);

                            if (!existDestinatario)
                                destinatariNotifica.Add(new NOTIFICHE_DESTINATARI
                                {
                                    NOTIFICHE = newNotifica,
                                    UIDPersona = destinatario.UID_persona,
                                    IdGruppo = em.id_gruppo,
                                    UID = Guid.NewGuid()
                                });
                        }

                        if (destinatariNotifica.Any()) _unitOfWork.Notifiche_Destinatari.AddRange(destinatariNotifica);

                        await _unitOfWork.CompleteAsync();
                        var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                        bodyMail += await _logicEm.GetBodyEM(em, firme, currentUser, TemplateTypeEnum.HTML);
                        bodyMail +=
                            $"<br/> <a href='{AppSettingsConfiguration.urlPEM_ViewEM}{em.UID_QRCode}'>Vedi online</a>";
                        results.Add(idGuid, $"{n_em} - OK");
                    }

                    if (!string.IsNullOrEmpty(bodyMail))
                        await _logicUtil.InvioMail(new MailModel
                        {
                            OGGETTO = "Invito a firmare i seguenti emendamenti",
                            DA = currentUser.email,
                            A = listaDestinatari.Select(p => p.email).Aggregate((m1, m2) => $"{m1};{m2}"),
                            MESSAGGIO = bodyMail
                        });

                    #endregion
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - InvitaAFirmare", e);
                throw e;
            }
        }

        #endregion

        #region CountRicevute

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

        #endregion

        #region CountInviate

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

        #endregion

        #region NotificaVista

        public async Task NotificaVista(long notificaId, Guid personaUId)
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

        #endregion

        #region GetListaDestinatari

        public async Task<Dictionary<string, string>> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo,
            PersonaDto persona)
        {
            try
            {
                var result = new Dictionary<string, string>();

                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM) tipo = TipoDestinatarioNotificaEnum.TUTTI;

                switch (tipo)
                {
                    case TipoDestinatarioNotificaEnum.TUTTI:
                        result = (await _logicPersone.GetProponenti())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        break;
                    case TipoDestinatarioNotificaEnum.CONSIGLIERI:
                        if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                            persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                        {
                            result = (await _logicPersone.GetAssessoriRiferimento())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                                 persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                        {
                            result = (await _logicPersone.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else
                        {
                            var consiglieri_In_Db = (await _logicPersone.GetConsiglieri())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName_GruppoCode);
                            foreach (var consigliere in consiglieri_In_Db)
                                result.Add(consigliere.Key, consigliere.Value);
                        }

                        break;
                    case TipoDestinatarioNotificaEnum.ASSESSORI:
                        if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                            persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                        {
                            result = (await _logicPersone.GetAssessoriRiferimento())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                                 persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                        {
                            result = (await _logicPersone.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else
                        {
                            var assessori_In_Db = (await _logicPersone.GetAssessoriRiferimento())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                            foreach (var assessori in assessori_In_Db) result.Add(assessori.Key, assessori.Value);
                        }

                        break;
                    case TipoDestinatarioNotificaEnum.GRUPPI:
                        result = (await _logicPersone.GetGruppiAttivi())
                            .ToDictionary(k => k.id.ToString(), z => z.descr);
                        break;
                    case TipoDestinatarioNotificaEnum.RELATORI:
                        result = (await _logicPersone.GetRelatori(atto))
                            .ToDictionary(k => k.UID_persona.ToString(), z => z.DisplayName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetListaDestinatari", e);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetListaDestinatari(TipoDestinatarioNotificaEnum tipo,
            PersonaDto persona)
        {
            try
            {
                var result = new Dictionary<string, string>();

                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM) tipo = TipoDestinatarioNotificaEnum.TUTTI;

                switch (tipo)
                {
                    case TipoDestinatarioNotificaEnum.TUTTI:
                        result = (await _logicPersone.GetProponenti())
                            .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        break;
                    case TipoDestinatarioNotificaEnum.CONSIGLIERI:
                        if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                            persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale)
                        {
                            result = (await _logicPersone.GetAssessoriRiferimento())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else if (persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                                 persona.CurrentRole == RuoliIntEnum.Segreteria_Politica)
                        {
                            result = (await _logicPersone.GetConsiglieriGruppo(persona.Gruppo.id_gruppo))
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName);
                        }
                        else
                        {
                            var consiglieri_In_Db = (await _logicPersone.GetConsiglieri())
                                .ToDictionary(p => p.UID_persona.ToString(), s => s.DisplayName_GruppoCode);
                            foreach (var consigliere in consiglieri_In_Db)
                                result.Add(consigliere.Key, consigliere.Value);
                        }

                        break;
                    case TipoDestinatarioNotificaEnum.GRUPPI:
                        result = (await _logicPersone.GetGruppiAttivi())
                            .ToDictionary(k => k.id.ToString(), z => z.descr);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetListaDestinatari", e);
                throw;
            }
        }

        #endregion
    }
}