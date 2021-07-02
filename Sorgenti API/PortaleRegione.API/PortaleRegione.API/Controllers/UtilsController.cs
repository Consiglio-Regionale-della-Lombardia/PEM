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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire operazioni di comodo
    /// </summary>
    [Authorize]
    [RoutePrefix("util")]
    public class UtilsController : BaseApiController
    {
        private readonly UtilsLogic _logic;

        public UtilsController(UtilsLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        ///     Endpoint di invio mail
        /// </summary>
        /// <param name="model">Modello di invio mail</param>
        /// <returns></returns>
        [HttpPost]
        [Route("mail")]
        public async Task<IHttpActionResult> SendMail(MailModel model)
        {
            try
            {
                await _logic.InvioMail(model);
                return Ok(true);
            }
            catch (Exception e)
            {
                Log.Error("Send Mail", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per caricamento documenti
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("upload")]
        public async Task<HttpResponseMessage> UploadDoc()
        {
            HttpResponseMessage result;
            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Files.Count == 1)
            {
                var ownerId = httpRequest.Form.Get("ownerUId");
                var docType = int.Parse(httpRequest.Form.Get("docType"));
                if (string.IsNullOrEmpty(ownerId) || docType < 1 || docType > 3)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                var postedFile = httpRequest.Files[0];
                var pathFile = _logic.ArchiviaDocumento(postedFile);
                await _logic.SalvaDocumento(ownerId, (TipoAllegatoEnum) docType, pathFile);

                result = Request.CreateResponse(HttpStatusCode.Created, pathFile);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return result;
        }
    }
}