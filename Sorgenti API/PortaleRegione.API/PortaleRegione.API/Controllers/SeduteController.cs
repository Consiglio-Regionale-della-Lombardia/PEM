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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using ApiRoutes = PortaleRegione.DTO.Routes.ApiRoutes;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le sedute
    /// </summary>
    [Authorize]
    public class SeduteController : BaseApiController
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
        public SeduteController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo delle sedute
        /// </summary>
        /// <param name="model">Modello richiesta generico con paginazione</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.PEM.Sedute.GetAll)]
        public async Task<IHttpActionResult> GetSedute(BaseRequest<SeduteDto> model)
        {
            try
            {
                return Ok(await _seduteLogic.GetSedute(model, Request.RequestUri));
            }
            catch (Exception e)
            {
                Log.Error("GetSedute", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere una seduta precisa
        /// </summary>
        /// <param name="id">Guid seduta</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Sedute.Get)]
        public async Task<IHttpActionResult> GetSeduta(Guid id)
        {
            try
            {
                var result = await _seduteLogic.GetSeduta(id);

                if (result == null) return NotFound();

                return Ok(Mapper.Map<SEDUTE, SeduteDto>(result));
            }
            catch (Exception e)
            {
                Log.Error("GetSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le sedute attive
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Sedute.GetAttive)]
        public async Task<IHttpActionResult> GetSeduteAttive()
        {
            try
            {
                var result = await _seduteLogic.GetSeduteAttive(CurrentUser);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetSeduteAttive", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le sedute attive per mozioni urgenti
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Sedute.GetAttiveMOZU)]
        public async Task<IHttpActionResult> GetSeduteAttiveMOZU()
        {
            try
            {
                var result = await _seduteLogic.GetSeduteAttiveMOZU();
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetSeduteAttiveMOZU", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le sedute attive per la dashboard
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Sedute.GetAttiveDashboard)]
        public async Task<IHttpActionResult> GetSeduteAttiveDashboard()
        {
            try
            {
                var result = await _seduteLogic.GetSeduteAttiveDashboard();
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetSeduteAttiveDashboard", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare virtualmente una seduta
        /// </summary>
        /// <param name="id">Guid seduta</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpDelete]
        [Route(ApiRoutes.PEM.Sedute.Delete)]
        public async Task<IHttpActionResult> DeleteSeduta(Guid id)
        {
            try
            {
                if (id == Guid.Empty) return BadRequest();

                var sedutaInDb = await _seduteLogic.GetSeduta(id);

                if (sedutaInDb == null) return NotFound();

                await _seduteLogic.DeleteSeduta(Mapper.Map<SEDUTE, SeduteDto>(sedutaInDb), CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DeleteSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere una seduta a database
        /// </summary>
        /// <param name="sedutaDto">Modello seduta da inserire</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route(ApiRoutes.PEM.Sedute.Create)]
        public async Task<IHttpActionResult> NuovaSeduta(SeduteDto sedutaDto)
        {
            try
            {
                if (sedutaDto.Data_seduta <= DateTime.Now) throw new InvalidOperationException("Data seduta non valida");

                var seduta =
                    Mapper.Map<SEDUTE, SeduteDto>(await _seduteLogic.NuovaSeduta(Mapper.Map<SeduteDto, SEDUTE>(sedutaDto),
                        CurrentUser));
                return Created(new Uri(Request.RequestUri + "/" + seduta.UIDSeduta), seduta);
            }
            catch (Exception e)
            {
                Log.Error("NuovaSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare una seduta
        /// </summary>
        /// <param name="sedutaDto">Modello seduta da modificare</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route(ApiRoutes.PEM.Sedute.Edit)]
        public async Task<IHttpActionResult> ModificaSeduta(SeduteFormUpdateDto sedutaDto)
        {
            try
            {
                var sedutaInDb = await _seduteLogic.GetSeduta(sedutaDto.UIDSeduta);

                if (sedutaInDb == null) return NotFound();

                await _seduteLogic.ModificaSeduta(sedutaDto, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ModificaSeduta", e);
                return ErrorHandler(e);
            }
        }
    }
}