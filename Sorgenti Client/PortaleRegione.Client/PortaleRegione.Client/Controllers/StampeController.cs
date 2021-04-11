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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller stampe
    /// </summary>
    [Authorize]
    [RoutePrefix("stampe")]
    public class StampeController : BaseController
    {
        // GET
        public async Task<ActionResult> Index(int page = 1, int size = 50)
        {
            var model = await StampeGate.GetStampe(page, size);
            if (HttpContext.User.IsInRole(RuoliEnum.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Assemblea))
                return View("Index_Admin", model);

            return View(model);
        }

        [HttpPost]
        [Route("nuova")]
        public async Task<ActionResult> NuovaStampa(BaseRequest<EmendamentiDto, StampaDto> model)
        {
            object UIDAtto;
            model.param.TryGetValue("UIDAtto", out UIDAtto);
            object DA;
            model.param.TryGetValue("Da", out DA);
            object A;
            model.param.TryGetValue("A", out A);
            model.entity = new StampaDto
            {
                UIDAtto = new Guid(UIDAtto.ToString()),
                Da = Convert.ToInt16(DA),
                A = Convert.ToInt16(A)
            };

            await StampeGate.InserisciStampa(model);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per scaricare una stampa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> DownloadStampa(Guid id)
        {
            try
            {
                var file = await StampeGate.DownloadStampa(id);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("reset")]
        public async Task<ActionResult> ResetStampa(Guid id)
        {
            await StampeGate.ResetStampa(id);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
        }
    }
}