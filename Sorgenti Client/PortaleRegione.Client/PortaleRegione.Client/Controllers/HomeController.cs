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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    [Authorize]
    [RoutePrefix("home")]
    public class HomeController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                CheckCacheClientMode(ClientModeEnum.GRUPPI);

                var apiGateway = new ApiGateway(_Token);
                var model = new DashboardModel
                {
                    Sedute = await apiGateway.Sedute.GetAttive()
                };
                foreach (var seduta in model.Sedute.Results)
                {
                    var attiPEM = await apiGateway.Atti.Get(seduta.UIDSeduta, ClientModeEnum.TRATTAZIONE, 1, 99);
                    if (attiPEM.Results.Any())
                        model.PEM.Add(attiPEM);

                    var attiDASI = await apiGateway.DASI.GetBySeduta(seduta.UIDSeduta);
                    if (attiDASI == null)
                        attiDASI = new RiepilogoDASIModel();

                    model.DASI.Add(attiDASI);
                }

                return View("Index", model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}