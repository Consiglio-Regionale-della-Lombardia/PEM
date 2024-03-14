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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.Api.Public.Controllers
{
    public class MainController : ApiController
    {
        [Route(ApiRoutes.GetLegislature)]
        public async Task<IHttpActionResult> GetLegislasture()
        {
            return Ok(new List<object>());
        }
        
        [Route(ApiRoutes.GetTipi)]
        public async Task<IHttpActionResult> GetTipi()
        {
            return Ok(new List<object>());
        }
        
        [Route(ApiRoutes.GetTipiRisposte)]
        public async Task<IHttpActionResult> GetTipiRisposte()
        {
            return Ok(new List<object>());
        }
        
        [Route(ApiRoutes.GetStati)]
        public async Task<IHttpActionResult> GetStati()
        {
            return Ok(new List<object>());
        }
        
        [Route(ApiRoutes.GetGruppi)]
        public async Task<IHttpActionResult> GetGruppi(int legislatura)
        {
            return Ok(new List<object>());
        }
    }
}