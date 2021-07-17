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

using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using System;
using System.Data.Entity.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Helpers
{
    /// <summary>
    ///     Controller di base
    /// </summary>
    public class BaseApiController : ApiController
    {
        /// <summary>
        ///     Handler per catturare i messaggi di errore
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected IHttpActionResult ErrorHandler(Exception e)
        {
            var message = e.Message;
            if (e.GetType() == typeof(DbEntityValidationException))
            {
                var entityError = e as DbEntityValidationException;
                foreach (var entityValidationError in entityError.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationError.ValidationErrors)
                    {
                        message = validationError.ErrorMessage;
                        break;
                    }

                    break;
                }
            }

            if (e.InnerException != null)
            {
                if (e.InnerException.Source == "EntityFramework")
                {
                    message = e.InnerException.InnerException.Message;
                }
            }

            Console.WriteLine(message);
            return BadRequest(message);
        }

        /// <summary>
        ///     Metodo per avere l'utente loggato dal jwt
        /// </summary>
        /// <returns></returns>
        protected async Task<SessionManager> GetSession()
        {
            var identity = RequestContext.Principal.Identity as ClaimsIdentity;
            var session = new SessionManager();
            foreach (var identityClaim in identity.Claims)
            {
                switch (identityClaim.Type)
                {
                    case ClaimTypes.Role:
                        session._currentRole = (RuoliIntEnum) Convert.ToInt16(identityClaim.Value);
                        break;
                    case "gruppo":
                        session._currentGroup = Convert.ToInt32(identityClaim.Value);
                        break;
                    case ClaimTypes.Name:
                    {
                        session._currentUId = new Guid(identityClaim.Value);
                        break;
                    }
                }
            }

            return session;
        }
    }
}