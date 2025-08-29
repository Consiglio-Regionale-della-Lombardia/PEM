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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller di base con funzionalità in comune
    /// </summary>
    public class BaseController : Controller
    {
        internal string GetVersion()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);
            return fileVersionInfo.FileVersion;
        }

        internal bool CanAccess(List<RuoliIntEnum> ruoli)
        {
            for (var i = 0; i < ruoli.Count; i++)
            {
                var check = User.IsInRole(((int)ruoli[i]).ToString());
                if (check)
                    return true;
            }

            return false;
        }

        protected PersonaDto CurrentUser
        {
            get
            {
                if (!HttpContext.User.Identity.IsAuthenticated) return null;
                var persona = TryReadChunkedCookies.GetJson<PersonaDto>(Request, "PRCookies");
                
                if(persona == null) return null;
                
                return persona;
            }
        }

        protected string Token
        {
            get
            {
                if (!HttpContext.User.Identity.IsAuthenticated) return default;
                try
                {
                    var jwt = TryReadChunkedCookies.GetString(Request, "SCookies");
                
                    if(jwt == null) return null;
                
                    return jwt;
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
            var mode = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE)));
            if (mode != (int)_mode)
            {
                HttpContext.Cache.Insert(
                    GetCacheKey(CacheHelper.CLIENT_MODE),
                    (int)_mode,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    (key, value, reason) => { Console.WriteLine("Cache removed"); }
                );
            }
        }

        internal string GetCacheKey(string key)
        {
            if (CurrentUser == null)
                return "";
            return $"{key}_{CurrentUser.UID_persona}";
        }

        internal async Task CheckCacheGruppiAdmin(RuoliIntEnum ruolo)
        {
            if (ruolo == RuoliIntEnum.Amministratore_PEM)
            {
                var currentGroups = HttpContext.Cache.Get(GetCacheKey(CacheHelper.GRUPPI_ATTIVI));
                if (currentGroups == null)
                {
                    var apiGateway = new ApiGateway(Token);
                    var gruppi = await apiGateway.Persone.GetGruppiAttivi();
                    HttpContext.Cache.Insert(
                        GetCacheKey(CacheHelper.GRUPPI_ATTIVI),
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

        public string GetLegislaturaDaUtente()
        {
            var split = CurrentUser.legislature.Split('-');
            var legislatura_corrente = split[split.Length - 2];
            return legislatura_corrente;
        }
    }
}