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
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per endpoint di amministrazione
    /// </summary>
    [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Amministratore_Giunta)]
    [RoutePrefix("admin")]
    public class AdminController : BaseApiController
    {
        private readonly AdminLogic _logic;
        private readonly PersoneLogic _logicPersone;

        public AdminController(AdminLogic logic, PersoneLogic logicPersone)
        {
            _logic = logic;
            _logicPersone = logicPersone;
        }

        /// <summary>
        ///     Endpoint per resettare il pin dell'utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("reset-pin")]
        public async Task<IHttpActionResult> ResetPin(ResetRequest request)
        {
            try
            {
                await _logic.ResetPin(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ResetPin", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per resettare la password dell'utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("reset-password")]
        public async Task<IHttpActionResult> ResetPassword(ResetRequest request)
        {
            try
            {
                await _logic.ResetPassword(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ResetPassword", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli utenti nel db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> GetUtenti(BaseRequest<PersonaDto> model)
        {
            try
            {
                var session = await GetSession();
                var results = await _logic.GetUtenti(model, session, Request.RequestUri);
                return Ok(results);
            }
            catch (Exception e)
            {
                Log.Error("GetUtenti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere un utente nel db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("view/{id:guid}")]
        public async Task<IHttpActionResult> GetUtente(Guid id)
        {
            try
            {
                var result = await _logic.GetUtente(id);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i ruoli AD disponibili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ad/ruoli")]
        public async Task<IHttpActionResult> GetRuoliAD()
        {
            try
            {
                var session = await GetSession();
                var currentUser = await _logic.GetPersona(session);
                var ruoli = await _logic.GetRuoliAD(currentUser.CurrentRole == RuoliIntEnum.Amministratore_Giunta);

                return Ok(ruoli);
            }
            catch (Exception e)
            {
                Log.Error("GetRuoli", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i gruppi politici AD disponibili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ad/gruppi-politici")]
        public async Task<IHttpActionResult> GetGruppiPoliticiAD()
        {
            try
            {
                var session = await GetSession();
                var gruppiPoliticiAD =
                    await _logic.GetGruppiPoliticiAD(
                        session._currentRole == RuoliIntEnum.Amministratore_Giunta);

                return Ok(gruppiPoliticiAD);
            }
            catch (Exception e)
            {
                Log.Error("GetGruppiPoliticiAD", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un utente nel db (deleted = 1)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("user/{id:guid}")]
        public async Task<IHttpActionResult> DeleteUtente(Guid id)
        {
            try
            {
                var persona = await _logic.GetPersona(id);
                if (persona == null)
                {
                    return NotFound();
                }

                await _logicPersone.DeletePersona(persona.id_persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DeleteUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// Endpoint per aggiornare i dati dell'utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("salva")]
        public async Task<IHttpActionResult> UpdateUtente(PersonaUpdateRequest request)
        {
            try
            {
                await _logic.UpdateUtente(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("UpdateUtente", e);
                return ErrorHandler(e);
            }
        }
    }
}