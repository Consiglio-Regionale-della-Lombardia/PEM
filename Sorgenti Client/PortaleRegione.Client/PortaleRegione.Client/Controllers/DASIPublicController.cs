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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("dasi")]
    public class DASIPublicController : Controller
    {
        [HttpGet]
        [Route("public/originale/{id:guid}")]
        public async Task<ActionResult> IndexOriginale(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var atto = await apiGateway.DASI_Pubblico.GetBody(id);
                return View("Index", (object)atto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("public/{id:guid}")]
        public async Task<ActionResult> IndexApprovato(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var atto = await apiGateway.DASI_Pubblico.GetBody(id, true);
                return View("Index", (object)atto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        internal static string apiUrl = AppSettingsConfiguration.URL_API_PUBLIC;

        [HttpGet]
        [Route("public/web/{id:guid}")]
        public async Task<ActionResult> GetAtto(Guid id)
        {
            var url = $"{apiUrl}/atto";
            var body = new AttoRequest { uidAtto = id.ToString() };

            using (var httpClient = new HttpClient())
            {
                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var stringResponse = await response.Content.ReadAsStringAsync();

                return Json(stringResponse, JsonRequestBehavior.AllowGet);
            }
        }

        public class AttoRequest
        {
            public string uidAtto { get; set; }
        }
    }
}