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
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le notifiche
    /// </summary>
    [Authorize]
    [RoutePrefix("notifiche")]
    public class NotificheController : BaseApiController
    {
        private readonly PersoneLogic _logicPersone;
        private readonly NotificheLogic _logic;

        public NotificheController(PersoneLogic logicPersone, NotificheLogic logic)
        {
            _logicPersone = logicPersone;
            _logic = logic;
        }

        /// <summary>
        ///     Endpoint per avere tutte le notifiche inviate di un utente
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("view-inviate")]
        public async Task<IHttpActionResult> GetNotificheInviate(BaseRequest<NotificaDto> model)
        {
            try
            {
                object Archivio;
                model.param.TryGetValue("Archivio", out Archivio);
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var result = await _logic.GetNotificheInviate(model, persona, Convert.ToBoolean(Archivio));

                return Ok(new BaseResponse<NotificaDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await _logic.CountInviate(model, persona, Convert.ToBoolean(Archivio)),
                    Request.RequestUri));
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("vista/{notificaId:long}")]
        public async Task<IHttpActionResult> NotificaVista(long notificaId)
        {
            try
            {
                var session = await GetSession();
                await _logic.NotificaVista(notificaId, session._currentUId);

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
        [Route("view-ricevute")]
        public async Task<IHttpActionResult> GetNotificheRicevute(BaseRequest<NotificaDto> model)
        {
            try
            {
                object Archivio;
                model.param.TryGetValue("Archivio", out Archivio);
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var result = await _logic.GetNotificheRicevute(model, persona, Convert.ToBoolean(Archivio));

                return Ok(new BaseResponse<NotificaDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await _logic.CountRicevute(model, persona, Convert.ToBoolean(Archivio)),
                    Request.RequestUri));
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
        [Route("{id:int}/destinatari")]
        public async Task<IHttpActionResult> GetDestinatariNotifica(int id)
        {
            try
            {
                var result = await _logic.GetDestinatariNotifica(id);
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
        [Route("invita")]
        public async Task<IHttpActionResult> InvitaAFirmareEmendamento(ComandiAzioneModel model)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var invitoDaSegreteria =
                    persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica ||
                    persona.CurrentRole == RuoliIntEnum.Segreteria_Politica ||
                    persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta ||
                    persona.CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale ||
                    persona.CurrentRole == RuoliIntEnum.Amministratore_PEM;

                if (invitoDaSegreteria)
                    return Ok(await _logic.InvitaAFirmareEmendamento(model, persona));

                var pinInDb = await _logicPersone.GetPin(persona);
                if (pinInDb == null)
                    return BadRequest("Pin non impostato");
                if (pinInDb.RichiediModificaPIN)
                    return BadRequest("E' richiesto il reset del pin");
                if (model.Pin != pinInDb.PIN_Decrypt)
                    return BadRequest("Pin inserito non valido");

                return Ok(await _logic.InvitaAFirmareEmendamento(model, persona));
            }
            catch (Exception e)
            {
                Log.Error("InvitaAFirmareEmendamento", e);
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
        [Route("destinatari")]
        public async Task<IHttpActionResult> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                return Ok(await _logic.GetListaDestinatari(atto, tipo, persona));
            }
            catch (Exception e)
            {
                Log.Error("GetListaDestinatari", e);
                return ErrorHandler(e);
            }
        }
    }
}