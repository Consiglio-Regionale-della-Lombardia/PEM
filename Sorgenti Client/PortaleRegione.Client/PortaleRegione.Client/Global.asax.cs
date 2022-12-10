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
using PortaleRegione.Client.Controllers;
using PortaleRegione.DTO.Domain;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace PortaleRegione.Client
{
    /// <summary>
    ///     Classe di ingresso dell'applicazione
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        ///     Metodo di ingresso dell'applicazione
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }

        /// <summary>
        ///     Richiesta di autenticazione del client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var prCookie1 = Request.Cookies["PRCookies1"];
                var prCookie2 = Request.Cookies["PRCookies2"];
                var prCookie3 = Request.Cookies["PRCookies3"];
                if (prCookie1 != null && prCookie2 != null && prCookie3 != null)
                {
                    var prTicket1 = FormsAuthentication.Decrypt(prCookie1.Value);
                    var prTicket2 = FormsAuthentication.Decrypt(prCookie2.Value);
                    var prTicket3 = FormsAuthentication.Decrypt(prCookie3.Value);
                    var data = JsonConvert.DeserializeObject<PersonaDto>(
                        $"{prTicket1.UserData}{prTicket2.UserData}{prTicket3.UserData}");

                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var formsIdentity = new FormsIdentity(authTicket);
                    var claimsIdentity = new ClaimsIdentity(formsIdentity);
                    claimsIdentity.AddClaim(
                        new Claim(ClaimTypes.Role, Convert.ToString((int)data.CurrentRole)));

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    HttpContext.Current.User = claimsPrincipal;
                }
            }
        }

        /// <summary>
        ///     Errore nel client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();
            Response.Clear();
            if (exc.GetType() == typeof(UnauthorizedAccessException))
            {
                var controller = new AutenticazioneController();
                controller.Logout();
                Response.Redirect($"~/Login?ReturnUrl={Request.Url.LocalPath}");
            }
        }
    }
}