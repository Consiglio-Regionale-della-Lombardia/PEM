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
using System.Threading.Tasks;   
using System.Web.Mvc;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    /// Controller per la gestione degli Atti di Sindacato Ispettivo
    /// </summary>
    [Authorize]
    [RoutePrefix("dasi")]
    public class DASIController : BaseController
    {
        /// <summary>
        /// Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base al ruolo dell'utente loggato
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RiepilogoDASI(int page = 1, int size = 50, StatiAttoEnum stato = StatiAttoEnum.BOZZA, TipoAttoEnum tipo = TipoAttoEnum.TUTTI)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.Get(page, size, stato, tipo);

            if (CanAccess(new List<RuoliIntEnum> { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                return View("RiepilogoDASI_Admin", model);
            
            return View("RiepilogoDASI", model);
        }

        /// <summary>
        /// Endpoint per il salvataggio dell' atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> SalvaAtto(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                var result = await apiGateway.DASI.Salva(request);
                return Json(Url.Action("ViewAtto", "DASI", new
                {
                    id = result.UIDAtto
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Endpoint per consultare l'atto per esteso
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:guid}")]
        public async Task<ActionResult> ViewAtto(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                var atto = await apiGateway.DASI.Get(id);
                return View("AttoDASIView", atto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}