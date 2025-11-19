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
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using PortaleRegione.Client.Helpers;

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
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }
        
        /// <summary>
        ///     Richiesta di autenticazione del client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            
            if (authCookie == null) return;
            
            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            if (authTicket == null) return;
            
            var persona = TryReadChunkedCookies.GetJson<PersonaDto>(Request, "PRCookies");
            if (persona == null) return;
            
            var formsIdentity = new FormsIdentity(authTicket);
            var claimsIdentity = new ClaimsIdentity(formsIdentity);
            
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, ((int)persona.CurrentRole).ToString()));

            HttpContext.Current.User = new ClaimsPrincipal(claimsIdentity);
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
            
            // ACT32: Gestione errore ValidateRequest
            if (exc is HttpRequestValidationException)
            {
                Response.StatusCode = 400; // Bad Request invece di 500
                Response.ContentType = "application/json";
                Response.Write(JsonConvert.SerializeObject(new 
                { 
                    message = "Il contenuto inserito contiene caratteri non consentiti per motivi di sicurezza. Rimuovere tag HTML pericolosi come <script>, <iframe>, ecc."
                }));
                Response.End();
                return;
            }
            
            if (exc.GetType() == typeof(UnauthorizedAccessException))
            {
                var controller = new AutenticazioneController();
                controller.Logout();
                Response.Redirect($"~/Login?ReturnUrl={Request.Url.LocalPath}");
            }
        }
    }
}