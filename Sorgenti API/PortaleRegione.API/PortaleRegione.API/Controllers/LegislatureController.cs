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
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli emendamenti
    /// </summary>
    [Authorize]
    [RoutePrefix("legislature")]
    public class LegislatureController : BaseApiController
    {
        private readonly LegislatureLogic _logic;

        public LegislatureController(LegislatureLogic logic)
        {
            _logic = logic;
        }
        
        /// <summary>
        ///     Endpoint per avere la lista delle legislature disponibili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetLegislature()
        {
            return Ok(await _logic.GetLegislature());
        }
    }
}