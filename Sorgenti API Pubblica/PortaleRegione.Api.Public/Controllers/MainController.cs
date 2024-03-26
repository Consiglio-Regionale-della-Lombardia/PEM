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
    ///     Controller pubblico per la consultazione degli atti di indirizzo e sindacato ispettivo
    /// </summary>
    public class MainController : ApiController
    {
        private readonly MainLogic _logic;

        /// <summary>
        /// Costruttore
        /// </summary>
        public MainController(MainLogic logic)
        {
            _logic = logic;
        }
        /// <summary>
        ///     Endpoint per avere la lista delle legislature disponibili
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per avere i tipi di atto disponibili
        /// </summary>
        /// <returns></returns>
        [Route(ApiRoutes.GetTipi)]
        public async Task<IHttpActionResult> GetTipi()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Ok(_logic.GetTipi());
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i tipi di risposte disponibili
        /// </summary>
        /// <returns></returns>
        [Route(ApiRoutes.GetTipiRisposte)]
        public async Task<IHttpActionResult> GetTipiRisposte()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Ok(_logic.GetTipiRisposta());
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli stati disponibili
        /// </summary>
        /// <returns></returns>
        [Route(ApiRoutes.GetStati)]
        public async Task<IHttpActionResult> GetStati()
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                return Ok(_logic.GetStati());
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i gruppi disponibili per legislatura
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per avere le cariche disponibili per legislatura
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per avere le commissioni disponibili per legislatura
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per avere i firmatari disponibili per legislatura
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per cercare gli atti pubblici
        /// </summary>
        /// <returns></returns>
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
        ///     Endpoint per cercare gli atti pubblici
        /// </summary>
        /// <returns></returns>
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

                return Ok(await _logic.GetAtto(guid));
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Handler per catturare i messaggi di errore
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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