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
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.Api.Public.Business_Layer;
using PortaleRegione.DTO.Request.Public;
using PortaleRegione.Logger;

namespace PortaleRegione.Api.Public.Controllers
{
    /// <summary>
    ///     Controller responsabile della gestione delle richieste relative agli atti di indirizzo e sindacato ispettivo.
    ///     Offre operazioni di lettura su legislature, tipi di atto, stati, gruppi, cariche, commissioni, firmatari e atti.
    /// </summary>
    public class MainController : ApiController
    {
        private readonly MainLogic _logic;

        /// <summary>
        ///     Inizializza una nuova istanza del MainController.
        /// </summary>
        /// <param name="logic">L'istanza della logica di business da utilizzare per elaborare le richieste.</param>
        public MainController(MainLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        ///     Recupera un elenco di legislature disponibili.
        /// </summary>
        /// <returns>Un'azione risultato che rappresenta l'elenco delle legislature.</returns>
        [Route(ApiRoutes.GetLegislature)]
        public async Task<IHttpActionResult> GetLegislasture()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Ok(await _logic.GetLegislature());
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Fornisce un elenco di tipi di atto disponibili.
        /// </summary>
        /// <returns>Un'azione risultato con i tipi di atto.</returns>
        [Route(ApiRoutes.GetTipi)]
        public Task<IHttpActionResult> GetTipi()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Task.FromResult<IHttpActionResult>(Ok(_logic.GetTipi()));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return Task.FromResult(ErrorHandler(e));
            }
        }

        /// <summary>
        ///     Ottiene un elenco di tipi di risposta possibili.
        /// </summary>
        /// <returns>Un'azione risultato con i tipi di risposta.</returns>
        [Route(ApiRoutes.GetTipiRisposte)]
        public Task<IHttpActionResult> GetTipiRisposte()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Task.FromResult<IHttpActionResult>(Ok(_logic.GetTipiRisposta()));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return Task.FromResult(ErrorHandler(e));
            }
        }

        /// <summary>
        ///     Restituisce un elenco degli stati disponibili per gli atti.
        /// </summary>
        /// <returns>Un'azione risultato con gli stati degli atti.</returns>
        [Route(ApiRoutes.GetStati)]
        public Task<IHttpActionResult> GetStati()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Task.FromResult<IHttpActionResult>(Ok(_logic.GetStati()));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return Task.FromResult(ErrorHandler(e));
            }
        }

        /// <summary>
        ///     Recupera l'elenco dei gruppi disponibili per una data legislatura.
        /// </summary>
        /// <param name="request">L'oggetto richiesta contenente l'ID della legislatura di interesse.</param>
        /// <returns>Un'azione risultato con i gruppi per la legislatura specificata.</returns>
        [Route(ApiRoutes.GetGruppi)]
        [HttpPost]
        public async Task<IHttpActionResult> GetGruppi(GruppiRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                return Ok(await _logic.GetGruppiByLegislatura(request.id_legislatura));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Fornisce un elenco delle cariche disponibili per una specifica legislatura.
        /// </summary>
        /// <param name="request">La richiesta contenente l'ID della legislatura.</param>
        /// <returns>Un'azione risultato con le cariche per la legislatura specificata.</returns>
        [Route(ApiRoutes.GetCaricheGiunta)]
        [HttpPost]
        public async Task<IHttpActionResult> GetCaricheGiunta(CaricheRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                return Ok(await _logic.GetCaricheByLegislatura(request.id_legislatura));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Ottiene un elenco delle commissioni disponibili per una determinata legislatura.
        /// </summary>
        /// <param name="request">La richiesta contenente l'ID della legislatura.</param>
        /// <returns>Un'azione risultato con le commissioni per la legislatura specificata.</returns>
        [Route(ApiRoutes.GetCommissioni)]
        [HttpPost]
        public async Task<IHttpActionResult> GetCommissioni(CommissioniRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                return Ok(await _logic.GetCommissioniByLegislatura(request.id_legislatura));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Recupera un elenco di firmatari disponibili per una specifica legislatura.
        /// </summary>
        /// <param name="request">La richiesta contenente l'ID della legislatura.</param>
        /// <returns>Un'azione risultato con i firmatari per la legislatura specificata.</returns>
        [Route(ApiRoutes.GetFirmatari)]
        [HttpPost]
        public async Task<IHttpActionResult> GetFirmatari(FirmatariRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                return Ok(await _logic.GetFirmatariByLegislatura(request.id_legislatura));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Cerca gli atti pubblici basati sui criteri specificati nella richiesta.
        /// </summary>
        /// <param name="request">L'oggetto richiesta contenente i criteri di ricerca.</param>
        /// <returns>Un'azione risultato con gli atti che corrispondono ai criteri di ricerca.</returns>
        [Route(ApiRoutes.GetSearch)]
        [HttpPost]
        public async Task<IHttpActionResult> GetSearch(CercaRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                if (request.page <= 0)
                    throw new InvalidOperationException("Impostare una pagina valida >= 1");
                if (request.size <= 0)
                    throw new InvalidOperationException("Inserire una paginazione valida >=1");
                if (request.size > 500)
                    throw new InvalidOperationException("Massimo risultati per pagina = 500");

                return Ok(await _logic.Cerca(request));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Scarica i documenti di un atto
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.ScaricaDocumento)]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(_logic.ScaricaDocumento(path));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download Allegato Atto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Restituisce i dettagli di un atto specifico, identificato dall'UID fornito nella richiesta.
        /// </summary>
        /// <param name="request">L'oggetto richiesta contenente l'UID dell'atto.</param>
        /// <returns>Un'azione risultato con i dettagli dell'atto specificato.</returns>
        [Route(ApiRoutes.GetAtto)]
        [HttpPost]
        public async Task<IHttpActionResult> GetAtto(AttoRequest request)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;

            try
            {
                var parseguid = Guid.TryParse(request.uidAtto, out var guid);
                if (!parseguid)
                    throw new InvalidOperationException("L'identificativo dell'atto non è valido.");

                return Ok(await _logic.GetAtto(guid, Request.RequestUri.OriginalString.Replace(Request.RequestUri.PathAndQuery, "")));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Gestisce e formatta le eccezioni catturate nei metodi del controller.
        /// </summary>
        /// <param name="e">L'eccezione catturata.</param>
        /// <returns>Un'azione risultato che rappresenta l'errore.</returns>
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
                if (e.InnerException.Source == "EntityFramework")
                    message = e.InnerException.InnerException.Message;

            Console.WriteLine(message);
            return BadRequest(message);
        }
    }
}