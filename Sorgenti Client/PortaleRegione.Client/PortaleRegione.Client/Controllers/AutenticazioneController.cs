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
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
            return View(new LoginRequest());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LoginRequest model, string returnUrl)
        {
            if (!ModelState.IsValid) return View("FormAutenticazione", model);
            LoginResponse response;
            try
            {
                var apiGateway = new ApiGateway(_Token);
                response = await apiGateway.Persone.Login(model.Username, model.Password);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                model.MessaggioErrore = e.Message;
                return View("FormAutenticazione", model);
            }

            await SalvaDatiInCookies(response.persona, response.jwt, model.Username);

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
                var apiGateway = new ApiGateway(_Token);
                response = await apiGateway.Persone.CambioRuolo(ruolo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return HttpNotFound(e.Message);
            }

            await SalvaDatiInCookies(response.persona, response.jwt, response.persona.userAD.Replace(@"CONSIGLIO\", ""));

            //if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("RiepilogoSedute", "PEM");
        }

        [Authorize]
        [HttpGet]
        [Route("cambio-gruppo")]
        public async Task<ActionResult> CambioGruppo(int gruppo, string returnUrl)
        {
            LoginResponse response;
            try
            {
                var apiGateway = new ApiGateway(_Token);
                response = await apiGateway.Persone.CambioGruppo(gruppo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return HttpNotFound(e.Message);
            }

            await SalvaDatiInCookies(response.persona, response.jwt, response.persona.userAD.Replace(@"CONSIGLIO\", ""));

            return RedirectToAction("RiepilogoSedute", "PEM");
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
                1, $"{username}1", DateTime.Now, DateTime.Now.AddHours(2), false, p1
            );
            var authTicket2 = new FormsAuthenticationTicket
            (
                1, $"{username}2", DateTime.Now, DateTime.Now.AddHours(2), false, p2
            );
            var authTicket3 = new FormsAuthenticationTicket
            (
                1, $"{username}3", DateTime.Now, DateTime.Now.AddHours(2), false, p3
            );

            var enTicket1 = FormsAuthentication.Encrypt(authTicket1);
            var faCookie1 = new HttpCookie("PRCookies1", enTicket1) { HttpOnly = true };
            Response.Cookies.Add(faCookie1);
            var enTicket2 = FormsAuthentication.Encrypt(authTicket2);
            var faCookie2 = new HttpCookie("PRCookies2", enTicket2) { HttpOnly = true };
            Response.Cookies.Add(faCookie2);
            var enTicket3 = FormsAuthentication.Encrypt(authTicket3);
            var faCookie3 = new HttpCookie("PRCookies3", enTicket3) { HttpOnly = true };
            Response.Cookies.Add(faCookie3);

            #endregion

            #region JWT DATA

            var securetyTicket1 = new FormsAuthenticationTicket
            (
                1, "token_jwt1", DateTime.Now, DateTime.Now.AddHours(2), false, jwt1
            );
            var securetyTicket2 = new FormsAuthenticationTicket
            (
                1, "token_jwt2", DateTime.Now, DateTime.Now.AddHours(2), false, jwt2
            );
            var securetyTicket3 = new FormsAuthenticationTicket
            (
                1, "token_jwt3", DateTime.Now, DateTime.Now.AddHours(2), false, jwt3
            );

            var sTicket1 = FormsAuthentication.Encrypt(securetyTicket1);
            var sCookie1 = new HttpCookie("SCookies1", sTicket1) { HttpOnly = true };
            Response.Cookies.Add(sCookie1);
            var sTicket2 = FormsAuthentication.Encrypt(securetyTicket2);
            var sCookie2 = new HttpCookie("SCookies2", sTicket2) { HttpOnly = true };
            Response.Cookies.Add(sCookie2);
            var sTicket3 = FormsAuthentication.Encrypt(securetyTicket3);
            var sCookie3 = new HttpCookie("SCookies3", sTicket3) { HttpOnly = true };
            Response.Cookies.Add(sCookie3);

            #endregion

            #region GRUPPI DATA

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
            {
                var apiGateway = new ApiGateway(jwt);
                var gruppi = await apiGateway.Persone.GetGruppiAttivi();
                string g1 = string.Empty, g2 = string.Empty, g3 = string.Empty;

                SliceBy3(JsonConvert.SerializeObject(gruppi), ref g1, ref g2, ref g3);

                var groupsTicket1 = new FormsAuthenticationTicket
                (
                    1, "gruppi1", DateTime.Now, DateTime.Now.AddHours(2), false, g1
                );
                var groupsTicket2 = new FormsAuthenticationTicket
                (
                    1, "gruppi2", DateTime.Now, DateTime.Now.AddHours(2), false, g2
                );
                var groupsTicket3 = new FormsAuthenticationTicket
                (
                    1, "gruppi3", DateTime.Now, DateTime.Now.AddHours(2), false, g3
                );
                var gTicket1 = FormsAuthentication.Encrypt(groupsTicket1);
                var gCookie1 = new HttpCookie("GCookies1", gTicket1) { HttpOnly = true };
                Response.Cookies.Add(gCookie1);
                var gTicket2 = FormsAuthentication.Encrypt(groupsTicket2);
                var gCookie2 = new HttpCookie("GCookies2", gTicket2) { HttpOnly = true };
                Response.Cookies.Add(gCookie2);
                var gTicket3 = FormsAuthentication.Encrypt(groupsTicket3);
                var gCookie3 = new HttpCookie("GCookies3", gTicket3) { HttpOnly = true };
                Response.Cookies.Add(gCookie3);
            }

            #endregion

            FormsAuthentication.SetAuthCookie(username, false);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            LogoutFlow();
            return RedirectToAction("FormAutenticazione", "Autenticazione");
        }

        private void LogoutFlow()
        {
            if (Response != null)
            {
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
        }

#if DEBUG == true
        [Authorize]
        [HttpGet]
        [Route("cambio-utente")]
        public async Task<ActionResult> CambiaUtente(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var persona = await apiGateway.Persone.Get(id);

            LogoutFlow();

            return RedirectToAction("FormAutenticazioneDEBUG", new { username = $"***{persona.userAD.Replace(@"CONSIGLIO\", "")}", password = "xx"});
        }

        [AllowAnonymous]
        [Route("login-debug")]
        public ActionResult FormAutenticazioneDEBUG(string username, string password)
        {
            return View("FormAutenticazione", new LoginRequest
            {
                Username = username,
                Password = password
            });
        }
#endif


    }
}