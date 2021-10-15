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

using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller persone
    /// </summary>
    [Authorize]
    [RoutePrefix("persone")]
    public class PersoneController : BaseController
    {
        [Route("{id:guid}")]
        public async Task<ActionResult> GetPersona(Guid id)
        {
            var _personeGateway = new PersoneGateway(_Token);
            return Json(await _personeGateway.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult> GetPersone()
        {
            var _personeGateway = new PersoneGateway(_Token);
            return Json(await _personeGateway.Get(), JsonRequestBehavior.AllowGet);
        }

        [Route("cambio-pin")]
        public ActionResult CambioPin()
        {
            return View("CambioPinForm", new CambioPinModel());
        }

        [Route("check-cambio-pin")]
        [HttpPost]
        public async Task<ActionResult> CheckPin(CambioPinModel model)
        {
            try
            {
                var _personeGateway = new PersoneGateway(_Token);
                await _personeGateway.CheckPin(model);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }

        }

        [Route("conferma-cambio-pin")]
        [HttpPost]
        public async Task<ActionResult> SalvaPin(CambioPinModel model)
        {
            try
            {
                var _personeGateway = new PersoneGateway(_Token);
                await _personeGateway.SalvaPin(model);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("gruppi-politici")]
        public async Task<ActionResult> GetGruppi()
        {
            var _personeGateway = new PersoneGateway(_Token);
            return Json(await _personeGateway.GetGruppiAttivi(), JsonRequestBehavior.AllowGet);
        }
    }
}