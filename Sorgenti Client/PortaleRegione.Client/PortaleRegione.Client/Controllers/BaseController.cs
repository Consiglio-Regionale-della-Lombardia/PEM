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

using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Gateway;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller di base con funzionalità in comune
    /// </summary>
    public class BaseController : Controller
    {
        internal bool CanAccess(List<RuoliIntEnum> ruoli)
        {
            for (int i = 0; i < ruoli.Count; i++)
            {
                var check = User.IsInRole(((int)ruoli[i]).ToString());
                if (check)
                    return true;
            }

            return false;
        }

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

        public string Token
        {
            get
            {
                if (!HttpContext.User.Identity.IsAuthenticated) return default;
                try
                {
                    var jwtCookie1 = Request.Cookies["SCookies1"];
                    if (jwtCookie1 is null)
                        throw new Exception("Sessione scaduta");

                    var jwtCookie2 = Request.Cookies["SCookies2"];
                    var jwtCookie3 = Request.Cookies["SCookies3"];
                    var jwtTicket1 = FormsAuthentication.Decrypt(jwtCookie1.Value);
                    var jwtTicket2 = FormsAuthentication.Decrypt(jwtCookie2.Value);
                    var jwtTicket3 = FormsAuthentication.Decrypt(jwtCookie3.Value);
                    var current_jwt = string.Join("", jwtTicket1.UserData, jwtTicket2.UserData, jwtTicket3.UserData);

                    return current_jwt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    FormsAuthentication.SignOut();
                    throw new UnauthorizedAccessException("Sessione scaduta");
                }
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            BaseGateway.apiUrl = AppSettingsConfiguration.URL_API;
        }

        internal void CheckCacheClientMode(ClientModeEnum _mode)
        {
            var mode = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.CLIENT_MODE));
            if (mode != (int)_mode)
            {
                HttpContext.Cache.Insert(
                    CacheHelper.CLIENT_MODE,
                    (int)_mode,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    (key, value, reason) => { Console.WriteLine("Cache removed"); }
                );
            }
        }

        internal async Task CheckCacheGruppiAdmin(RuoliIntEnum ruolo)
        {
            if (ruolo == RuoliIntEnum.Amministratore_PEM)
            {
                var currentGroups = HttpContext.Cache.Get(CacheHelper.GRUPPI_ATTIVI);
                if (currentGroups == null)
                {
                    var apiGateway = new ApiGateway(Token);
                    var gruppi = await apiGateway.Persone.GetGruppiAttivi();
                    HttpContext.Cache.Insert(
                        CacheHelper.GRUPPI_ATTIVI,
                        JsonConvert.SerializeObject(gruppi),
                        null,
                        Cache.NoAbsoluteExpiration,
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.NotRemovable,
                        (key, value, reason) => { Console.WriteLine("Cache removed"); }
                    );
                }
            }
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