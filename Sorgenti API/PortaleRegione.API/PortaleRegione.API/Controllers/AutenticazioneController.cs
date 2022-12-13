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
using PortaleRegione.Contracts;
using PortaleRegione.DTO;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Enum;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per l'autenticazione
    /// </summary>
    [AllowAnonymous]
    public class AutenticazioneController : BaseApiController
    {
        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="authLogic"></param>
        /// <param name="personeLogic"></param>
        /// <param name="legislatureLogic"></param>
        /// <param name="seduteLogic"></param>
        /// <param name="attiLogic"></param>
        /// <param name="dasiLogic"></param>
        /// <param name="firmeLogic"></param>
        /// <param name="attiFirmeLogic"></param>
        /// <param name="emendamentiLogic"></param>
        /// <param name="publicLogic"></param>
        /// <param name="notificheLogic"></param>
        /// <param name="esportaLogic"></param>
        /// <param name="stampeLogic"></param>
        /// <param name="utilsLogic"></param>
        /// <param name="adminLogic"></param>
        public AutenticazioneController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Autenticazione.Login)]
        public async Task<IHttpActionResult> Login(LoginRequest loginModel)
        {
            try
            {
                var result = await _authLogic.Login(loginModel);

                return Ok(result);
            }
            catch (Exception e)
            {
                //Log.Error("Login", e);
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
        [Route(ApiRoutes.Autenticazione.CambioRuolo)]
        public async Task<IHttpActionResult> CambioRuolo(RuoliIntEnum ruolo)
        {
            try
            {
                var result = await _authLogic.CambioRuolo(ruolo, Session);

                return Ok(result);
            }
            catch (Exception e)
            {
                //Log.Error("GetToken - Ruoli", e);
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
        [Route(ApiRoutes.Autenticazione.CambioGruppo)]
        public async Task<IHttpActionResult> CambioGruppo(int gruppo)
        {
            try
            {
                var result = await _authLogic.CambioGruppo(gruppo, Session);

                return Ok(result);
            }
            catch (Exception e)
            {
                //Log.Error("GetToken - CambioGruppo", e);
                return ErrorHandler(e);
            }
        }
    }
}