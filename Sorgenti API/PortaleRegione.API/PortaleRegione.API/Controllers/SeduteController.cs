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
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le sedute
    /// </summary>
    [Authorize]
    [RoutePrefix("sedute")]
    public class SeduteController : BaseApiController
    {
        private readonly SeduteLogic _logic;
        private readonly PersoneLogic _logicPersone;

        public SeduteController(SeduteLogic logic, PersoneLogic logicPersone)
        {
            _logic = logic;
            _logicPersone = logicPersone;
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo delle sedute
        /// </summary>
        /// <param name="model">Modello richiesta generico con paginazione</param>
        /// <returns></returns>
        [HttpPost]
        [Route("view")]
        public async Task<IHttpActionResult> GetSedute(BaseRequest<SeduteDto> model)
        {
            try
            {
                return Ok(await _logic.GetSedute(model, Request.RequestUri));
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
        [Route("")]
        public async Task<IHttpActionResult> GetSeduta(Guid id)
        {
            try
            {
                var result = await _logic.GetSeduta(id);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(Mapper.Map<SEDUTE, SeduteDto>(result));
            }
            catch (Exception e)
            {
                Log.Error("GetSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare virtualmente una seduta
        /// </summary>
        /// <param name="id">Guid seduta</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteSeduta(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest();
                }

                var sedutaInDb = await _logic.GetSeduta(id);

                if (sedutaInDb == null)
                {
                    return NotFound();
                }

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                await _logic.DeleteSeduta(Mapper.Map<SEDUTE, SeduteDto>(sedutaInDb), persona);

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
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> NuovaSeduta(SeduteDto sedutaDto)
        {
            try
            {
                if (!sedutaDto.Data_seduta.HasValue)
                {
                    return BadRequest("Manca la data seduta");
                }

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var seduta =
                    Mapper.Map<SEDUTE, SeduteDto>(await _logic.NuovaSeduta(Mapper.Map<SeduteDto, SEDUTE>(sedutaDto),
                        persona));
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
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> ModificaSeduta(SeduteFormUpdateDto sedutaDto)
        {
            try
            {
                if (!sedutaDto.Data_seduta.HasValue)
                {
                    return BadRequest("Manca la data seduta");
                }

                var sedutaInDb = await _logic.GetSeduta(sedutaDto.UIDSeduta);

                if (sedutaInDb == null)
                {
                    return NotFound();
                }

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                await _logic.ModificaSeduta(sedutaDto, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ModificaSeduta", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere la lista delle legislature disponibili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("legislature")]
        public async Task<IHttpActionResult> GetLegislature()
        {
            return Ok(await _logic.GetLegislature());
        }
    }
}