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

using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione dei servizi esterni
    /// </summary>
    [Authorize(Roles = RuoliExt.SERVIZIO_JOB)]
    [RoutePrefix("job")]
    public class JobController : BaseApiController
    {
        private readonly StampeLogic _logic;
        private readonly EmendamentiLogic _logicEm;
        private readonly DASILogic _logicDasi;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="logicEm"></param>
        public JobController(StampeLogic logic, EmendamentiLogic logicEm, DASILogic logicDasi)
        {
            _logic = logic;
            _logicEm = logicEm;
            _logicDasi = logicDasi;
        }

        /// <summary>
        ///     Endpoint per avere le stampe non ancora prese in carico
        /// </summary>
        /// <param name="model">Modello richiesta generico con paginazione</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("stampe/view")]
        public async Task<IHttpActionResult> GetStampe(BaseRequest<StampaDto> model)
        {
            try
            {
                var result = await _logic.GetStampe(model, Request.RequestUri);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("JOB - GetStampe", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per sbloccare una stampa
        /// </summary>
        /// <param name="model">Modello richiesta stampa</param>
        /// <returns></returns>
        [HttpPost]
        [Route("stampe/unlock")]
        public async Task<IHttpActionResult> UnLockStampa(StampaRequest model)
        {
            try
            {
                await _logic.UnLockStampa(model.stampaUId);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("JOB - UnLockStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per segnalare l'errore nella stampa
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("stampe/error")]
        public async Task<IHttpActionResult> ErroreStampa(StampaRequest model)
        {
            try
            {
                await _logic.ErroreStampa(model);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("JOB - UnLockStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare il file di stampa
        /// </summary>
        /// <param name="stampa"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("stampe")]
        public async Task<IHttpActionResult> UpdateFileStampa(StampaDto stampa)
        {
            try
            {
                await _logic.UpdateFileStampa(stampa);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("UpdateFileStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare i dati di invio della stampa
        /// </summary>
        /// <param name="stampa"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("stampe/inviato")]
        public async Task<IHttpActionResult> SetInvioStampa(StampaDto stampa)
        {
            try
            {
                await _logic.SetInvioStampa(stampa);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SetInvioStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo di emendamenti in base ad una query
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("stampe/emendamenti")]
        public async Task<IHttpActionResult> GetEmendamenti(ByQueryModel model)
        {
            try
            {
                var countEM = await _logicEm.CountEM(model.Query);
                return Ok(
                    new BaseResponse<EmendamentiDto>(
                        model.page, 
                        100, 
                        await _logicEm.GetEmendamenti(model),
                    null, 
                        countEM)
                    );
            }
            catch (Exception e)
            {
                Log.Error("JOB - GetEmendamenti", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere il riepilogo di atti sindacato ispettivo in base ad una query
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("stampe/dasi")]
        public async Task<IHttpActionResult> GetDASI(ByQueryModel model)
        {
            try
            {
                var count = await _logicDasi.CountByQuery(model.Query);
                return Ok(
                    new BaseResponse<AttoDASIDto>(
                        model.page, 
                        100, 
                        await _logicDasi.GetByQuery(model),
                    null, 
                        count)
                    );
            }
            catch (Exception e)
            {
                Log.Error("JOB - Get DASI", e);
                return ErrorHandler(e);
            }
        }
    }
}