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

using Microsoft.IdentityModel.Tokens;
using PortaleRegione.BAL;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PortaleRegione.API
{
    /// <summary>
    ///     Middleware autenticazione JWT
    /// </summary>
    public class TokenValidationHandler : DelegatingHandler
    {
        /// <summary>
        ///     Metodo che cerca di recuperare il JWT token dall'header della richiesta
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                return false;
            }

            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : null;
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        ///     Metodo che intercetta le request in ingresso
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;
            string token;

            //chek if a token exists in the request header
            if (!TryRetrieveToken(request, out token))
            {
                statusCode = HttpStatusCode.Unauthorized;
                //allow requests with no token - whether a action method needs an authentication can be set with the claimsauthorization attribute
                return await base.SendAsync(request, cancellationToken);
            }

            try
            {
                var securityKey =
                    new SymmetricSecurityKey(Encoding.Default.GetBytes(AppSettingsConfiguration.JWT_MASTER));


                var handler = new JwtSecurityTokenHandler();

                //Replace the issuer and audience with your URL (ex. http:localhost:12345)
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = AppSettingsConfiguration.URL_API,
                    ValidIssuer = AppSettingsConfiguration.URL_API,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    LifetimeValidator = LifetimeValidator,
                    IssuerSigningKey = securityKey
                };

                //extract and assign the user of the jwt
                Thread.CurrentPrincipal = handler.ValidateToken(token, validationParameters, out var _);
                HttpContext.Current.User = handler.ValidateToken(token, validationParameters, out var _);

                return await base.SendAsync(request, cancellationToken);
            }
            catch (SecurityTokenValidationException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception)
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            return await Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(statusCode),
                cancellationToken);
        }


        /// <summary>
        ///     Metodo per capire se il JWT è valido
        /// </summary>
        /// <param name="notBefore"></param>
        /// <param name="expires"></param>
        /// <param name="securityToken"></param>
        /// <param name="validationParameters"></param>
        /// <returns></returns>
        public bool LifetimeValidator(DateTime? notBefore,
            DateTime? expires,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (expires == null)
            {
                return false;
            }

            return DateTime.UtcNow < expires;
        }
    }
}