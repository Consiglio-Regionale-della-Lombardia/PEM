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
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per l'autenticazione
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("autenticazione")]
    public class AutenticazioneController : BaseApiController
    {
        private readonly AuthLogic _logic;

        public AutenticazioneController(AuthLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        ///     Endpoint di login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Login(LoginRequest loginModel)
        {
            try
            {
                var result = await _logic.Login(loginModel);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Login", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per il cambio ruolo
        /// </summary>
        /// <param name="ruolo"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("cambio-ruolo")]
        public async Task<IHttpActionResult> GetToken(RuoliIntEnum ruolo)
        {
            try
            {
                var result = await _logic.CambioRuolo(ruolo, await GetSession());

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetToken - Ruoli", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per il cambio gruppo
        /// </summary>
        /// <param name="gruppo"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("cambio-gruppo")]
        public async Task<IHttpActionResult> GetToken(int gruppo)
        {
            try
            {
                var result = await _logic.CambioGruppo(gruppo, await GetSession());

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetToken - CambioGruppo", e);
                return ErrorHandler(e);
            }
        }
    }
}