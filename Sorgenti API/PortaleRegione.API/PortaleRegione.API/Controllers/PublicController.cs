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
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using ApiRoutes = PortaleRegione.DTO.Routes.ApiRoutes;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli emendamenti pubblici
    /// </summary>
    [AllowAnonymous]
    public class PublicController : BaseApiController
    {
        /// <summary>
        ///     Endpoint per visualizzare il corpo dell'emendamento pubblico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Public.ViewEM)]
        public async Task<IHttpActionResult> ViewEM(Guid id)
        {
            try
            {
                try
                {
                    var em = await _emendamentiLogic.GetEM_ByQR(id);
                    if (em == null)
                    {
                        return NotFound();
                    }

                    var body = await _publicLogic.GetBody(em
                        , await _firmeLogic.GetFirme(em, FirmeTipoEnum.TUTTE));

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
        
        /// <summary>
        ///     Endpoint per visualizzare il corpo dell'atto pubblico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Public.ViewDASI)]
        public async Task<IHttpActionResult> ViewDASI(Guid id, bool approvato)
        {
            try
            {
                try
                {
                    var atto = await _dasiLogic.Get_ByQR(id);
                    if (atto == null)
                    {
                        return NotFound();
                    }

                    var currentUser = CurrentUser;
                    var firme = await _attiFirmeLogic.GetFirme(atto, FirmeTipoEnum.TUTTE);
                    var body = await _dasiLogic.GetBodyDASI(atto, firme, currentUser, TemplateTypeEnum.PDF, approvato, false);

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
        public PublicController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }
    }
}