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
using System.Web.Caching;
using System.Web.Mvc;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
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
        [HttpGet]
        [Route("view")]
        public async Task<ActionResult> Index(Guid id)
        {
            CheckCacheClientMode();
            var mode = (ClientModeEnum)HttpContext.Cache.Get(CacheHelper.CLIENT_MODE);
            var apiGateway = new ApiGateway(_Token);
            var seduta = await apiGateway.Sedute.Get(id);
            var model = new DashboardModel
            {
                Seduta = seduta
            };
            var attiPEM = await apiGateway.Atti.Get(id, mode, 1, 99);
            if (attiPEM.Results.Any())
                model.PEM.Add(attiPEM);

            var attiDASI = await apiGateway.DASI.GetBySeduta(id);
            if (attiDASI.Data.Results.Any())
                model.DASI.Add(attiDASI);

            return View("Index", model);
        }
        
        public async Task<ActionResult> Archivio(int page = 1, int size = 50)
        {
            CheckCacheClientMode();

            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.Sedute.Get(page, size);
            return View("Archivio", model);
        }

        private void CheckCacheClientMode()
        {
            var mode = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.CLIENT_MODE));
            if (mode != (int)ClientModeEnum.TRATTAZIONE)
            {
                HttpContext.Cache.Insert(
                    CacheHelper.CLIENT_MODE,
                    (int)ClientModeEnum.TRATTAZIONE,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    (key, value, reason) => { Console.WriteLine("Cache removed"); }
                );
            }
        }
    }
}