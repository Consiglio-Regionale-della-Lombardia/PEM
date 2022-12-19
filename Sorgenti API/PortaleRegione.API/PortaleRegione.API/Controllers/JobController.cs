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
using PortaleRegione.Contracts;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione dei servizi esterni
    /// </summary>
    [Authorize(Roles = RuoliExt.SERVIZIO_JOB)]
    public class JobController : BaseApiController
    {
        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="authLogic"></param>
        /// <param name="personeLogic"></param>
        /// <param name="legislatureLogic"></param>
        /// <param name="seduteLogic"></param>
        /// <param name="attiLogic"></param>
        /// <param name="dasiLogic"></param>
        /// <param name="firmeLogic"></param>
        /// <param name="attiFirmeLogic"></param>
        /// <param name="emendamentiLogic"></param>
        /// <param name="publicLogic"></param>
        /// <param name="notificheLogic"></param>
        /// <param name="esportaLogic"></param>
        /// <param name="stampeLogic"></param>
        /// <param name="utilsLogic"></param>
        /// <param name="adminLogic"></param>
        public JobController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint per avere le stampe non ancora prese in carico
        /// </summary>
        /// <param name="model">Modello richiesta generico con paginazione</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route(ApiRoutes.Job.Stampe.GetAll)]
        public async Task<IHttpActionResult> GetStampe(BaseRequest<StampaDto> model)
        {
            try
            {
                var result = await _stampeLogic.GetStampe(model, Request.RequestUri);
                return Ok(result);
            }
            catch (Exception e)
            {
                //Log.Error("JOB - GetStampe", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per sbloccare una stampa
        /// </summary>
        /// <param name="model">Modello richiesta stampa</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Job.Stampe.Unlock)]
        public async Task<IHttpActionResult> UnLockStampa(StampaRequest model)
        {
            try
            {
                await _stampeLogic.UnLockStampa(model.stampaUId);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("JOB - UnLockStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per segnalare l'errore nella stampa
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Job.Stampe.ReportError)]
        public async Task<IHttpActionResult> ErroreStampa(StampaRequest model)
        {
            try
            {
                await _stampeLogic.ErroreStampa(model);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("JOB - UnLockStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare il file di stampa
        /// </summary>
        /// <param name="stampa"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ApiRoutes.Job.Stampe.UpdateFileStampa)]
        public async Task<IHttpActionResult> UpdateFileStampa(StampaDto stampa)
        {
            try
            {
                await _stampeLogic.UpdateFileStampa(stampa);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("UpdateFileStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare i dati di invio della stampa
        /// </summary>
        /// <param name="stampa"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ApiRoutes.Job.Stampe.SetInvioStampa)]
        public async Task<IHttpActionResult> SetInvioStampa(StampaDto stampa)
        {
            try
            {
                await _stampeLogic.SetInvioStampa(stampa);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("SetInvioStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo di emendamenti in base ad una query
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Job.Stampe.GetEmendamenti)]
        public async Task<IHttpActionResult> GetEmendamenti(ByQueryModel model)
        {
            try
            {
                var countEM = await _emendamentiLogic.CountEM(model.Query);
                return Ok(
                    new BaseResponse<EmendamentiDto>(
                        model.page,
                        100,
                        await _emendamentiLogic.GetEmendamenti(model),
                        null,
                        countEM)
                );
            }
            catch (Exception e)
            {
                //Log.Error("JOB - GetEmendamenti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo di atti sindacato ispettivo in base ad una query
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Job.Stampe.GetAtti)]
        public async Task<IHttpActionResult> GetDASI(ByQueryModel model)
        {
            try
            {
                var results = await _dasiLogic.GetByQuery(model);
                var count = await _dasiLogic.CountByQuery(model);
                return Ok(
                    new BaseResponse<AttoDASIDto>(
                        model.page,
                        100,
                        results,
                        null,
                        count)
                );
            }
            catch (Exception e)
            {
                //Log.Error("JOB - Get DASI", e);
                return ErrorHandler(e);
            }
        }
    }
}