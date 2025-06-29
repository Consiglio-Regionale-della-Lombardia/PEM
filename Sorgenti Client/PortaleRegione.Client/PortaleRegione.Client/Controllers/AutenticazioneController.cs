﻿/*
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
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using static PortaleRegione.DTO.Routes.ApiRoutes;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller di autenticazione
    /// </summary>
    [Authorize]
    public class AutenticazioneController : BaseController
    {
        [AllowAnonymous]
        [Route("login")]
        public ActionResult FormAutenticazione()
        {
            return View(new AutenticazioneModel(GetVersion()));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(AutenticazioneModel model, string returnUrl)
        {
            model.versione = GetVersion();
            if (!ModelState.IsValid)
                return View("FormAutenticazione", model);
            LoginResponse response;
            try
            {
                var apiGateway = new ApiGateway(Token);
                response = await apiGateway.Persone.Login(model.LoginRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                try
                {
                    var erroreJson = JsonConvert.DeserializeObject<ErrorResponse>(e.Message);
                    model.LoginRequest.MessaggioErrore = erroreJson != null ? erroreJson.message : e.Message;
                }
                catch (Exception)
                {
                    model.LoginRequest.MessaggioErrore = e.Message;
                }

                return View("FormAutenticazione", model);
            }

            await SalvaDatiInCookies(response.persona, response.jwt, model.LoginRequest.Username);

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        [Route("cambio-ruolo")]
        public async Task<ActionResult> CambioRuolo(RuoliIntEnum ruolo, string returnUrl)
        {
            LoginResponse response;
            try
            {
                var apiGateway = new ApiGateway(Token);
                response = await apiGateway.Persone.CambioRuolo(ruolo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return HttpNotFound(e.Message);
            }

            await SalvaDatiInCookies(response.persona, response.jwt,
                response.persona.userAD.Replace(@"CONSIGLIO\", ""));

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        [Route("cambio-gruppo")]
        public async Task<ActionResult> CambioGruppo(int gruppo, string returnUrl)
        {
            LoginResponse response;
            try
            {
                var apiGateway = new ApiGateway(Token);
                response = await apiGateway.Persone.CambioGruppo(gruppo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return HttpNotFound(e.Message);
            }

            await SalvaDatiInCookies(response.persona, response.jwt,
                response.persona.userAD.Replace(@"CONSIGLIO\", ""));

            return RedirectToAction("Index", "Home");
        }

        private async Task SalvaDatiInCookies(PersonaDto persona, string jwt, string username)
        {
            string jwt1 = string.Empty, jwt2 = string.Empty, jwt3 = string.Empty;
            SliceBy3(jwt, ref jwt1, ref jwt2, ref jwt3);

            var persona_json = JsonConvert.SerializeObject(persona);
            string p1 = string.Empty, p2 = string.Empty, p3 = string.Empty;

            SliceBy3(persona_json, ref p1, ref p2, ref p3);

            #region USER DATA

            var authTicket1 = new FormsAuthenticationTicket
            (
                1, $"{username}1", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                p1
            );
            var authTicket2 = new FormsAuthenticationTicket
            (
                1, $"{username}2", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                p2
            );
            var authTicket3 = new FormsAuthenticationTicket
            (
                1, $"{username}3", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                p3
            );

            var enTicket1 = FormsAuthentication.Encrypt(authTicket1);
            var faCookie1 = new HttpCookie("PRCookies1", enTicket1)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(faCookie1);
            var enTicket2 = FormsAuthentication.Encrypt(authTicket2);
            var faCookie2 = new HttpCookie("PRCookies2", enTicket2)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(faCookie2);
            var enTicket3 = FormsAuthentication.Encrypt(authTicket3);
            var faCookie3 = new HttpCookie("PRCookies3", enTicket3)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(faCookie3);

            #endregion

            #region JWT DATA

            var securetyTicket1 = new FormsAuthenticationTicket
            (
                1, "token_jwt1", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                jwt1
            );
            var securetyTicket2 = new FormsAuthenticationTicket
            (
                1, "token_jwt2", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                jwt2
            );
            var securetyTicket3 = new FormsAuthenticationTicket
            (
                1, "token_jwt3", DateTime.Now, DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN), true,
                jwt3
            );

            var sTicket1 = FormsAuthentication.Encrypt(securetyTicket1);
            var sCookie1 = new HttpCookie("SCookies1", sTicket1)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(sCookie1);
            var sTicket2 = FormsAuthentication.Encrypt(securetyTicket2);
            var sCookie2 = new HttpCookie("SCookies2", sTicket2)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(sCookie2);
            var sTicket3 = FormsAuthentication.Encrypt(securetyTicket3);
            var sCookie3 = new HttpCookie("SCookies3", sTicket3)
                { Expires = DateTime.Now.AddHours(AppSettingsConfiguration.COOKIE_EXPIRE_IN) };
            Response.Cookies.Add(sCookie3);

            #endregion

            #region GRUPPI DATA

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
            {
                var apiGateway = new ApiGateway(jwt);
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

            #endregion

            FormsAuthentication.SetAuthCookie(username, true);
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            await LogoutFlow();
            return RedirectToAction("FormAutenticazione", "Autenticazione");
        }

        private async Task LogoutFlow()
        {
            if (Response != null)
            {
                var currentUser = CurrentUser;
                
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Persone.Logout(currentUser.UID_persona);

                if (Response.Cookies.AllKeys.Contains("PRCookies1"))
                    Response.Cookies.Remove("PRCookies1");
                if (Response.Cookies.AllKeys.Contains("PRCookies2"))
                    Response.Cookies.Remove("PRCookies2");
                if (Response.Cookies.AllKeys.Contains("PRCookies3"))
                    Response.Cookies.Remove("PRCookies3");
                if (Response.Cookies.AllKeys.Contains("SCookies1"))
                    Response.Cookies.Remove("SCookies1");
                if (Response.Cookies.AllKeys.Contains("SCookies2"))
                    Response.Cookies.Remove("SCookies2");
                if (Response.Cookies.AllKeys.Contains("SCookies3"))
                    Response.Cookies.Remove("SCookies3");
                if (Response.Cookies.AllKeys.Contains("GCookies1"))
                    Response.Cookies.Remove("GCookies1");
                if (Response.Cookies.AllKeys.Contains("GCookies2"))
                    Response.Cookies.Remove("GCookies2");
                if (Response.Cookies.AllKeys.Contains("GCookies3"))
                    Response.Cookies.Remove("GCookies3");
            }

            FormsAuthentication.SignOut();

            try
            {
                if (HttpContext?.Cache == null) return;
                foreach (DictionaryEntry entry in HttpContext.Cache)
                    HttpContext.Cache.Remove((string)entry.Key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

#if DEBUG == true
        [Authorize]
        [HttpGet]
        [Route("cambio-utente")]
        public async Task<ActionResult> CambiaUtente(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var persona = await apiGateway.Persone.Get(id);

            LogoutFlow();

            return RedirectToAction("FormAutenticazioneDEBUG",
                new { username = $"***{persona.userAD.Replace(@"CONSIGLIO\", "")}", password = "xx" });
        }

        [AllowAnonymous]
        [Route("login-debug")]
        public ActionResult FormAutenticazioneDEBUG(string username, string password)
        {
            return View("FormAutenticazione", new AutenticazioneModel(GetVersion())
            {
                LoginRequest = new LoginRequest
                {
                    Username = username,
                    Password = password
                }
            });
        }
#endif
    }
}