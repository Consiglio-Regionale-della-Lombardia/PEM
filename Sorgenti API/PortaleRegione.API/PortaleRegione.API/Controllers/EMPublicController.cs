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
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli emendamenti pubblici
    /// </summary>
    [RoutePrefix("public")]
    public class EMPublicController : BaseApiController
    {
        private readonly EmendamentiLogic _logicEm;
        private readonly EMPublicLogic _logicPublic;
        private readonly FirmeLogic _logicFirme;

        public EMPublicController(EmendamentiLogic logicEm, EMPublicLogic logicPublic, FirmeLogic logicFirme)
        {
            _logicEm = logicEm;
            _logicPublic = logicPublic;
            _logicFirme = logicFirme;
        }

        /// <summary>
        /// Endpoint per visualizzare il corpo dell'emendamento
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("em")]
        public async Task<IHttpActionResult> Index(Guid id)
        {
            try
            {
                try
                {
                    var em = await _logicEm.GetEM_ByQR(id);
                    if (em == null)
                    {
                        return NotFound();
                    }

                    var body = await _logicPublic.GetBody(em
                        , await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE));

                    return Ok(body);
                }
                catch (Exception e)
                {
                    Log.Error("GetBody", e);
                    return ErrorHandler(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}