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

using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller di base con funzionalità in comune
    /// </summary>
    public class BaseController : Controller
    {
        public PersonaDto CurrentUser
        {
            get
            {
                if (!HttpContext.User.Identity.IsAuthenticated) return null;
                var authCookie1 = Request.Cookies["PRCookies1"];
                var authCookie2 = Request.Cookies["PRCookies2"];
                var authCookie3 = Request.Cookies["PRCookies3"];
                if (authCookie1 == null && authCookie2 == null && authCookie3 == null) return null;
                if (string.IsNullOrEmpty(authCookie1.Value) && string.IsNullOrEmpty(authCookie2.Value) &&
                    string.IsNullOrEmpty(authCookie3.Value)) return null;
                var authenticationTicket1 = FormsAuthentication.Decrypt(authCookie1.Value);
                var authenticationTicket2 = FormsAuthentication.Decrypt(authCookie2.Value);
                var authenticationTicket3 = FormsAuthentication.Decrypt(authCookie3.Value);
                var data = JsonConvert.DeserializeObject<PersonaDto>(
                    $"{authenticationTicket1.UserData}{authenticationTicket2.UserData}{authenticationTicket3.UserData}");

                return data;
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            BaseGateway.apiUrl = ConfigurationManager.AppSettings["URL_API"];
        }

        protected void SliceBy3(string bodyMessage, ref string str1, ref string str2, ref string str3)
        {
            var charArray = bodyMessage.ToCharArray();
            var maxChar = charArray.Length / 3;
            foreach (var c in charArray)
                if (str1.Length != maxChar)
                    str1 += c;
                else if (str2.Length != maxChar)
                    str2 += c;
                else
                    str3 += c;
        }
    }
}