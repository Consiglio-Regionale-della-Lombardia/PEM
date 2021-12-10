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

using PortaleRegione.Gateway;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("public")]
    public class EMPublicController : BaseController
    {
        [HttpGet]
        [Route("em")]
        public async Task<ActionResult> Index(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var em = await apiGateway.Emendamento_Pubblico.GetBody(id);
                return View("Index", (object)em);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> GetEMPublic2(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var em = await apiGateway.Emendamento_Pubblico.GetBody(id);
                return View("Index", (object)em);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}