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

using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Gateway;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
            CheckCacheClientMode(ClientModeEnum.TRATTAZIONE);
            var mode = (ClientModeEnum)HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE));
            var apiGateway = new ApiGateway(Token);
            var seduta = await apiGateway.Sedute.Get(id);
            var model = new DashboardModel
            {
                Seduta = seduta,
                CurrentUser = CurrentUser
            };
            var attiPEM = await apiGateway.Atti.Get(id, mode, 1, 99);
            if (attiPEM.Results.Any())
                model.PEM.Add(attiPEM);

            var attiDASI = await apiGateway.DASI.GetBySeduta(id);
            if (attiDASI == null)
                attiDASI = new RiepilogoDASIModel();
            model.DASI.Add(attiDASI);

            return View("Index", model);
        }

        public async Task<ActionResult> Archivio(int page = 1, int size = 50)
        {
            CheckCacheClientMode(ClientModeEnum.TRATTAZIONE);

            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.Sedute.Get(page, size);
            return View("Archivio", model);
        }
    }
}