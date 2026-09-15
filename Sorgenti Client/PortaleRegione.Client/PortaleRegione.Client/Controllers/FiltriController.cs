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
using System.Web.Mvc;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller MVC unificato per i filtri preferiti utente.
    ///     Sostituisce DASIController.SalvaGruppoFiltri/GetGruppoFiltri/EliminaGruppoFiltri
    ///     ed e' usato anche dal modulo PEM (introdotto in v2026.5.1).
    /// </summary>
    [Authorize]
    [RoutePrefix("filtri")]
    public class FiltriController : BaseController
    {
        /// <summary>
        ///     Salva un filtro preferito. Il modulo (PEM=1, DASI=2) e' nel payload.
        /// </summary>
        [HttpPost]
        [Route("salva")]
        public async Task<ActionResult> Salva(FiltroPreferitoDto model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Filtri.Salva(model);
                return Json("OK");
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Restituisce i filtri preferiti dell'utente per il modulo richiesto.
        /// </summary>
        [HttpGet]
        [Route("{modulo:int}")]
        public async Task<ActionResult> Get(ModuloEnum modulo)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.Filtri.Get(modulo);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Elimina il filtro preferito identificato da nome+utente sul modulo richiesto.
        /// </summary>
        [HttpGet]
        [Route("elimina")]
        public async Task<ActionResult> Elimina(ModuloEnum modulo, string nome)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Filtri.Elimina(nome, modulo);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
