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

using AutoMapper;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Routes;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le stampe
    /// </summary>
    [Authorize]
    public class StampeController : BaseApiController
    {
        /// <summary>
        ///     Endpoint per il riepilogo delle stampe
        /// </summary>
        /// <param name="model">Modello di richiesta generico con paginazione</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Stampe.GetAll)]
        public async Task<IHttpActionResult> GetStampe(BaseRequest<StampaDto> model)
        {
            try
            {
                var result = await _stampeLogic.GetStampe(model, CurrentUser, Request.RequestUri);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetStampe", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per una singola stampa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Stampe.Get)]
        public async Task<IHttpActionResult> GetStampa(Guid id)
        {
            try
            {
                var result = await _stampeLogic.GetStampa(id);
                return Ok(Mapper.Map<STAMPE, StampaDto>(result));
            }
            catch (Exception e)
            {
                Log.Error("GetStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per scaricare il file generato
        /// </summary>
        /// <param name="id">Guid stampa</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route(ApiRoutes.Stampe.Download)]
        public async Task<IHttpActionResult> DownloadStampa(Guid id)
        {
            try
            {
                var stampa = await _stampeLogic.GetStampa(id);
                if (stampa == null)
                {
                    return NotFound();
                }

                var response = ResponseMessage(await _stampeLogic.DownloadStampa(stampa));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("DownloadStampa", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per scaricare il file fascicolo generato
        /// </summary>
        /// <param name="nomeFile">Nome file</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route(ApiRoutes.Stampe.DownloadFolder)]
        public async Task<IHttpActionResult> DownloadFascicoloStampa(string nomeFile)
        {
            try
            {
                var response = ResponseMessage(await _stampeLogic.DownloadFascicoloStampa(nomeFile));
                return response;
            }
            catch (Exception e)
            {
                Log.Error("DownloadFascicoloStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare una stampa
        /// </summary>
        /// <param name="id">Guid stampa</param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Stampe.Delete)]
        public async Task<IHttpActionResult> EliminaStampa(Guid id)
        {
            try
            {
                var stampa = await _stampeLogic.GetStampa(id);
                if (stampa == null)
                {
                    return NotFound();
                }

                await _stampeLogic.EliminaStampa(stampa);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("EliminaStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per resettare la stampa in caso sia andata in errore
        /// </summary>
        /// <param name="id">Guid stampa</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Stampe.Reset)]
        public async Task<IHttpActionResult> ResetStampa(Guid id)
        {
            try
            {
                var stampa = await _stampeLogic.GetStampa(id);
                if (stampa == null)
                {
                    return NotFound();
                }

                await _stampeLogic.ResetStampa(stampa);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ResetStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere dei log info alla stampa
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Stampe.AddInfo)]
        public async Task<IHttpActionResult> AddInfoStampa(InfoModel model)
        {
            try
            {
                var stampa = await _stampeLogic.GetStampa(model.Id);
                if (stampa == null)
                {
                    return NotFound();
                }

                await _stampeLogic.AddInfo(stampa, model.Message);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Add info Stampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere la lista di informazioni loggate legate alla stampa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Stampe.GetInfo)]
        public async Task<IHttpActionResult> GetInfoStampa(Guid id)
        {
            try
            {
                var stampa = await _stampeLogic.GetStampa(id);
                if (stampa == null)
                {
                    return NotFound();
                }

                var result = await _stampeLogic.GetInfo(stampa);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Get info Stampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere la lista di informazioni loggate di tutte le stampe
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Stampe.GetAllInfo)]
        public async Task<IHttpActionResult> GetInfoStampe()
        {
            try
            {
                var result = await _stampeLogic.GetInfo();

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Get info Stampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eseguire la stampa
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Stampe.Print)]
        public async Task<IHttpActionResult> Stampa(string uid)
        {
            try
            {
                var response = ResponseMessage(await _stampeLogic.Print(uid));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Get info Stampa", e);
                return ErrorHandler(e);
            }
        }

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
        public StampeController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }
    }
}