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
    [System.Web.Http.Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
    [System.Web.Http.RoutePrefix("admin")]
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
        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("reset-pin")]
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
        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("reset-password")]
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
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("users/view")]
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
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("view/{id:guid}")]
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
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("ad/ruoli")]
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
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("ad/gruppi-politici")]
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
        ///     Endpoint per avere tutti i gruppi per la legislatura attuale
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("gruppi-in-db")]
        public async Task<IHttpActionResult> GetGruppiInDb()
        {
            try
            {
                return Ok(await _logicPersone.GetGruppiInDb());
            }
            catch (Exception e)
            {
                Log.Error("GetGruppiInDb", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un utente nel db (deleted = 1)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpDelete]
        [System.Web.Http.Route("user/{id:guid}")]
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
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("salva")]
        public async Task<IHttpActionResult> SalvaUtente(PersonaUpdateRequest request)
        {
            try
            {
                var session = await GetSession();
                var uid_persona = await _logic.SalvaUtente(request, session._currentRole);
                return Ok(uid_persona);
            }
            catch (Exception e)
            {
                Log.Error("SalvaUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// Endpoint per eliminare un utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [System.Web.Http.HttpDelete]
        [System.Web.Http.Route("elimina")]
        public async Task<IHttpActionResult> EliminaUtente(Guid id)
        {
            try
            {
                var session = await GetSession();
                await _logic.EliminaUtente(id);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("EliminaUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// Endpoint per aggiornare i dati del gruppo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("salva-gruppo")]
        public async Task<IHttpActionResult> SalvaGruppo(SalvaGruppoRequest request)
        {
            try
            {
                await _logic.SalvaGruppo(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SalvaGruppo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i gruppi nel db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("groups/view")]
        public async Task<IHttpActionResult> GetGruppi(BaseRequest<GruppiDto> model)
        {
            try
            {
                var results = await _logic.GetGruppi(model, Request.RequestUri);
                return Ok(results);
            }
            catch (Exception e)
            {
                Log.Error("GetGruppi", e);
                return ErrorHandler(e);
            }
        }
    }
}