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
using System.Web.Mvc;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    [Authorize(Roles = RuoliExt.Amministratore_PEM)]
    [RoutePrefix("templates")]
    public class TemplateController : BaseController
    {
        [HttpGet]
        [Route("view")]
        public async Task<ActionResult> RiepilogoTemplates()
        {
            var apiGateway = new ApiGateway(Token);
            var res = await apiGateway.Templates.GetAll();

            return View("View", res);
        }

        [HttpGet]
        [Route("{id}/edit")]
        public async Task<ActionResult> EditTemplate(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var res = await apiGateway.Templates.Get(id);

            return View("TemplateForm", res);
        }
        
        [HttpGet]
        [Route("new")]
        public Task<ActionResult> NewTemplate()
        {
            var res = new TemplatesItemDto();

            return Task.FromResult<ActionResult>(View("TemplateForm", res));
        }

        [HttpPost]
        [Route("delete")]
        public async Task<ActionResult> RemoveTemplate(List<Guid> uid)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Templates.Delete(uid.First());

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("save")]
        public async Task<ActionResult> SaveTemplate(TemplatesItemDto item)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var result = await apiGateway.Templates.Save(item);
                return Json(Url.Action("EditTemplate", "Template", new
                {
                    id = result.Uid
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}