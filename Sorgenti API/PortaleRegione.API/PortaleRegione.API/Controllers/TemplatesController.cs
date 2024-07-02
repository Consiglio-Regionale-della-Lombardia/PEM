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
using AutoMapper;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Routes;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize(Roles = RuoliExt.Amministratore_PEM)]
    public class TemplatesController : BaseApiController
    {
        /// <summary>
        /// 
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
        public TemplatesController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic, LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic, FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic, EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic, UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic, seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic, esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Templates.GetAll)]
        public async Task<IHttpActionResult> GetAll()
        {
            try
            {
                var result = await _adminLogic.GetTemplates(Request.RequestUri);

                return Ok(result);
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [Route(ApiRoutes.Templates.Get)]
        public async Task<IHttpActionResult> GetTemplateFromDb(Guid id)
        {
            try
            {
                var template = await _adminLogic.GetTemplateFromDb(id);
                if (template == null) return NotFound();

                return Ok(template);
            }
            catch (Exception e)
            {
                Log.Error("GetTemplate", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Templates.Delete)]
        public async Task<IHttpActionResult> DeleteTemplate(Guid id)
        {
            try
            {
                var template = await _unitOfWork.Templates.Get(id);
                if (template == null) return NotFound();

                await _adminLogic.DeleteTemplate(template);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Template", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Templates.Save)]
        public async Task<IHttpActionResult> SaveTemplates(TemplatesItemDto request)
        {
            try
            {
                var res = await _adminLogic.SaveTemplate(request);

                return Created(new Uri(Request.RequestUri.ToString()), res);
            }
            catch (Exception e)
            {
                Log.Error("Salva Template", e);
                return ErrorHandler(e);
            }
        }
    }
}