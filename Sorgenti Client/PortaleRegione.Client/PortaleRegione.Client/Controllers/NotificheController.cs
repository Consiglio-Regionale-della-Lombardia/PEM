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

using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller notifiche
    /// </summary>
    [Authorize]
    [RoutePrefix("notifiche")]
    public class NotificheController : BaseController
    {
        [HttpGet]
        [Route("view")]
        public async Task<ActionResult> RiepilogoNotifiche(bool is_inviate, bool archivio, int page = 1, int size = 50)
        {
            try
            {
                var _notificheGateway = new NotificheGateway(_Token);
                BaseResponse<NotificaDto> model;
                if (!is_inviate)
                    model = await _notificheGateway.GetNotificheRicevute(page, size, archivio);
                else
                    model = await _notificheGateway.GetNotificheInviate(page, size, archivio);

                return View("RiepilogoNotifiche", model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("counter-notifiche-ricevute")]
        public async Task<ActionResult> CounterRiepilogoNotifiche()
        {
            try
            {
                var _notificheGateway = new NotificheGateway(_Token);
                BaseResponse<NotificaDto> model;
                model = await _notificheGateway.GetNotificheRicevute(1, 1, false, true);

                return Json(model.Paging.Total, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("{id:int}/destinatari")]
        public async Task<ActionResult> GetDestinatariNotifica(int id)
        {
            var _notificheGateway = new NotificheGateway(_Token);
            var destinatari = await _notificheGateway.GetDestinatariNotifica(id);
            var result = await Utility.GetDestinatariNotifica(destinatari, _Token);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("destinatari")]
        public async Task<ActionResult> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo)
        {
            var _notificheGateway = new NotificheGateway(_Token);
            var destinatari = await _notificheGateway.GetListaDestinatari(atto, tipo);
            return Json(destinatari, JsonRequestBehavior.AllowGet);
        }
    }
}