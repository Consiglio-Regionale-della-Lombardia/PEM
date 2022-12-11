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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Collections.Generic;
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
                var apiGateway = new ApiGateway(Token);
                RiepilogoNotificheModel model;
                if (!is_inviate)
                    model = await apiGateway.Notifiche.GetNotificheRicevute(page, size, archivio);
                else
                    model = await apiGateway.Notifiche.GetNotificheInviate(page, size, archivio);

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
                var apiGateway = new ApiGateway(Token);
                var model = await apiGateway.Notifiche.GetNotificheRicevute(1, 1, false, true);

                return Json(model.Data.Paging.Total, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("{id:int}/destinatari")]
        public async Task<ActionResult> GetDestinatariNotifica(string id)
        {
            var apiGateway = new ApiGateway(Token);
            var destinatari = await apiGateway.Notifiche.GetDestinatariNotifica(id);
            var result = await Utility.GetDestinatariNotifica(destinatari, Token);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("destinatari")]
        public async Task<ActionResult> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo)
        {
            var apiGateway = new ApiGateway(Token);
            var destinatari = await apiGateway.Notifiche.GetListaDestinatari(atto, tipo);
            return Json(destinatari, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("destinatari-dasi")]
        public async Task<ActionResult> GetListaDestinatari(TipoDestinatarioNotificaEnum tipo)
        {
            var apiGateway = new ApiGateway(Token);
            var destinatari = await apiGateway.Notifiche.GetListaDestinatari(tipo);
            return Json(destinatari, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("accetta-proposta")]
        public async Task<ActionResult> AccettaPropostaFirma(string id)
        {
            var apiGateway = new ApiGateway(Token);
            await apiGateway.Notifiche.AccettaPropostaFirma(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("accetta-ritiro")]
        public async Task<ActionResult> AccettaRitiroFirma(string id)
        {
            var apiGateway = new ApiGateway(Token);
            await apiGateway.Notifiche.AccettaRitiroFirma(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("archivia")]
        public async Task<ActionResult> ArchiviaNotifica(List<string> notifiche)
        {
            var apiGateway = new ApiGateway(Token);
            await apiGateway.Notifiche.ArchiviaNotifiche(notifiche);
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}