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

using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ApiRoutes = PortaleRegione.DTO.Routes.ApiRoutes;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le notifiche
    /// </summary>
    [Authorize]
    public class NotificheController : BaseApiController
    {
        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="authLogic"></param>
        /// <param name="personeLogic"></param>
        /// <param name="legislatureLogic"></param>
        /// <param name="seduteLogic"></param>
        /// <param name="attiLogic"></param>
        /// <param name="dasiLogic"></param>
        /// <param name="firmeLogic"></param>
        /// <param name="attiFirmeLogic"></param>
        /// <param name="emendamentiLogic"></param>
        /// <param name="publicLogic"></param>
        /// <param name="notificheLogic"></param>
        /// <param name="esportaLogic"></param>
        /// <param name="stampeLogic"></param>
        /// <param name="utilsLogic"></param>
        /// <param name="adminLogic"></param>
        public NotificheController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint per avere tutte le notifiche inviate di un utente
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Notifiche.GetInviate)]
        public async Task<IHttpActionResult> GetNotificheInviate(BaseRequest<NotificaDto> model)
        {
            try
            {
                model.param.TryGetValue("Archivio", out var Archivio);
                var result = await _notificheLogic.GetNotificheInviate(model,
                    CurrentUser,
                    Convert.ToBoolean(Archivio),
                    Request.RequestUri);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNotificheInviate", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per settare notifica vista
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Notifiche.NotificaVista)]
        public async Task<IHttpActionResult> NotificaVista(string id)
        {
            try
            {
                await _notificheLogic.NotificaVista(id, Session._currentUId);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("NotificaVista", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutte le notifiche ricevute da un utente
        /// </summary>
        /// <param name="model">Modello di richiesta generico con paginazione</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Notifiche.GetRicevute)]
        public async Task<IHttpActionResult> GetNotificheRicevute(BaseRequest<NotificaDto> model)
        {
            try
            {
                model.param.TryGetValue("Archivio", out var Archivio);
                model.param.TryGetValue("Solo_Non_Viste", out var Solo_Non_Viste);
                var result = await _notificheLogic.GetNotificheRicevute(model,
                    CurrentUser,
                    Convert.ToBoolean(Archivio),
                    Convert.ToBoolean(Solo_Non_Viste),
                    Request.RequestUri);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNotificheRicevute", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere il conteggio delle notifiche ricevute da un utente
        /// </summary>
        /// <param name="model">Modello di richiesta generico con paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Notifiche.GetCounterRicevute)]
        public async Task<IHttpActionResult> GetCounterNotificheRicevute()
        {
            try
            {
                var result = await _notificheLogic.GetCounterNotificheRicevute(CurrentUser);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNotificheRicevute", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i destinatari di una notifica
        /// </summary>
        /// <param name="id">Id notifica</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Notifiche.GetDestinatari)]
        public async Task<IHttpActionResult> GetDestinatariNotifica(string id)
        {
            try
            {
                var result = await _notificheLogic.GetDestinatariNotifica(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetDestinatariNotifica", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per invitare a firmare un emendamento
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Notifiche.InvitoAFirmare)]
        public async Task<IHttpActionResult> InvitaAFirmare(ComandiAzioneModel model)
        {
            try
            {
                var user = CurrentUser;

                var invitoDaSegreteria =
                    user.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                    user.CurrentRole == RuoliIntEnum.Segreteria_Politica ||
                    user.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                    user.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale ||
                    user.CurrentRole == RuoliIntEnum.Amministratore_PEM;

                if (invitoDaSegreteria) return Ok(await _notificheLogic.InvitaAFirmare(model, user));

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null) throw new InvalidOperationException("Pin non impostato");

                if (pinInDb.RichiediModificaPIN) throw new InvalidOperationException("E' richiesto il reset del pin");

                if (model.Pin != pinInDb.PIN_Decrypt) throw new InvalidOperationException("Pin inserito non valido");

                return Ok(await _notificheLogic.InvitaAFirmare(model, user));
            }
            catch (Exception e)
            {
                Log.Error("InvitaAFirmare", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i destinatari da invitare alla firma
        /// </summary>
        /// <param name="atto"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.GetAllDestinatari)]
        public async Task<IHttpActionResult> GetListaDestinatari(Guid atto, int tipo)
        {
            try
            {
                return Ok(await _notificheLogic.GetListaDestinatari(atto, (TipoDestinatarioNotificaEnum)tipo, CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetListaDestinatari", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i destinatari da invitare alla firma
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetAllDestinatari)]
        public async Task<IHttpActionResult> GetListaDestinatari(int tipo)
        {
            try
            {
                return Ok(await _notificheLogic.GetListaDestinatari((TipoDestinatarioNotificaEnum)tipo, CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetListaDestinatari", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Firma viene accettata dal proponente
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Notifiche.AccettaPropostaFirma)]
        public async Task<IHttpActionResult> AccettaPropostaFirma(string id)
        {
            try
            {
                await _notificheLogic.AccettaPropostaFirma(id);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("AccettaPropostaFirma", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Ritiro firma accettato dal proponente
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Notifiche.AccettaRitiroFirma)]
        public async Task<IHttpActionResult> AccettaRitiroFirma(string id)
        {
            try
            {
                await _notificheLogic.AccettaRitiroFirma(id);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("AccettaRitiroFirma", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Archivia la notifica
        /// </summary>
        /// <param name="notifiche"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Notifiche.Archivia)]
        public async Task<IHttpActionResult> ArchiviaNotifiche(List<string> notifiche)
        {
            try
            {
                var user = CurrentUser;
                await _notificheLogic.ArchiviaNotifiche(notifiche, user);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("InvitaAFirmare", e);
                return ErrorHandler(e);
            }
        }
    }
}