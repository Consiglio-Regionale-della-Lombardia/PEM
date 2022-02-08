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

using System.Threading.Tasks;
using System.Web.Mvc;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller atti dasi trattazione
    /// </summary>
    [Authorize]
    [RoutePrefix("attitrattazione")]
    public class AttiTrattazioneController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View("Index");
        }
        
        public async Task<ActionResult> Archivio(int page = 1, int size = 50)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.Sedute.Get(page, size);
            return View("Archivio", model);
        }
    }
}