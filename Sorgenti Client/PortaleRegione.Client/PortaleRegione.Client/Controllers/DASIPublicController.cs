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
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("dasi")]
    public class DASIPublicController : Controller
    {
        [HttpGet]
        [Route("public/originale/{id:guid}")]
        public async Task<ActionResult> IndexOriginale(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var atto = await apiGateway.DASI_Pubblico.GetBody(id);
                return View("Index", (object)atto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("public/{id:guid}")]
        public async Task<ActionResult> IndexApprovato(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway();
                var atto = await apiGateway.DASI_Pubblico.GetBody(id, true);
                return View("Index", (object)atto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        internal static string apiUrl = AppSettingsConfiguration.URL_API_PUBLIC;


        [HttpGet]
        [Route("public/web/{id:guid}")]
        public async Task<ActionResult> GetAtto(Guid id)
        {
            var url = $"{apiUrl}/atto";
            var body = new AttoRequest { uidAtto = id.ToString() };

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsync(url, content);
                }
                catch (TaskCanceledException ex)
                {
                    // Se il task viene cancellato (tipicamente per timeout)
                    return Json(new ErrorResponse("Timeout: l'endpoint non ha risposto in tempo."), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    // Gestisce altri errori generici di connessione
                    return Json(new ErrorResponse("Si è verificato un errore durante la chiamata all'endpoint."), JsonRequestBehavior.AllowGet);
                }

                // Se il codice di stato non indica successo, gestiamo i casi specifici
                if (!response.IsSuccessStatusCode)
                {
                    await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            return Json(new ErrorResponse($"Errore 404 - Endpoint non trovato."), JsonRequestBehavior.AllowGet);
                        case HttpStatusCode.BadRequest:
                            return Json(new ErrorResponse($"Errore 400 - Richiesta non valida."), JsonRequestBehavior.AllowGet);
                        case HttpStatusCode.Unauthorized:
                            return Json(new ErrorResponse($"Errore 401 - Non autorizzato."), JsonRequestBehavior.AllowGet);
                        case HttpStatusCode.Forbidden:
                            return Json(new ErrorResponse($"Errore 403 - Accesso proibito."), JsonRequestBehavior.AllowGet);
                        case HttpStatusCode.InternalServerError:
                            return Json(new ErrorResponse(
                                $"Errore 500 - Errore interno del server."), JsonRequestBehavior.AllowGet);
                        default:
                            return Json(new ErrorResponse(
                                $"Errore {response.StatusCode} - Risposta non valida dall'endpoint."), JsonRequestBehavior.AllowGet);
                    }
                }

                // Se tutto va bene, leggo la risposta e la restituisco in formato JSON
                var stringResponse = await response.Content.ReadAsStringAsync();
                return Json(stringResponse, JsonRequestBehavior.AllowGet);
            }
        }

        public class AttoRequest
        {
            public string uidAtto { get; set; }
        }
    }
}