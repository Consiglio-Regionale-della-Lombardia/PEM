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
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per endpoint di amministrazione
    /// </summary>
    [RoutePrefix("admin")]
    public class AdminController : BaseApiController
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
        public AdminController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
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
                await _adminLogic.ResetPin(request);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("ResetPin", e);
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
                await _adminLogic.ResetPassword(request);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("ResetPassword", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli utenti nel db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("users/view")]
        public async Task<IHttpActionResult> GetUtenti(BaseRequest<PersonaDto> model)
        {
            try
            {
                var currentUser = CurrentUser;
                var results = await _adminLogic.GetUtenti(model, currentUser, Request.RequestUri);
                return Ok(new RiepilogoUtentiModel
                {
                    Data = results,
                    Persona = currentUser
                });
            }
            catch (Exception e)
            {
                //Log.Error("GetUtenti", e);
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
                var result = await _adminLogic.GetUtente(id);

                return Ok(result);
            }
            catch (Exception e)
            {
                //Log.Error("GetUtente", e);
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
                var ruoli = await _adminLogic.GetRuoliAD(Session._currentRole == RuoliIntEnum.Amministratore_Giunta);

                return Ok(ruoli);
            }
            catch (Exception e)
            {
                //Log.Error("GetRuoli", e);
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
                var gruppiPoliticiAD =
                    await _adminLogic.GetGruppiPoliticiAD(
                        Session._currentRole == RuoliIntEnum.Amministratore_Giunta);

                return Ok(gruppiPoliticiAD);
            }
            catch (Exception e)
            {
                //Log.Error("GetGruppiPoliticiAD", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i gruppi per la legislatura attuale
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("gruppi-in-db")]
        public async Task<IHttpActionResult> GetGruppiInDb()
        {
            try
            {
                return Ok(await _personeLogic.GetGruppiInDb());
            }
            catch (Exception e)
            {
                //Log.Error("GetGruppiInDb", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un utente nel db (deleted = 1)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpDelete]
        [Route("user/{id:guid}")]
        public async Task<IHttpActionResult> DeleteUtente(Guid id)
        {
            try
            {
                var persona = await _adminLogic.GetPersona(id);
                if (persona == null) return NotFound();

                await _personeLogic.DeletePersona(persona.id_persona);

                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("DeleteUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare i dati dell'utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpPost]
        [Route("salva")]
        public async Task<IHttpActionResult> SalvaUtente(PersonaUpdateRequest request)
        {
            try
            {
                var uid_persona = await _adminLogic.SalvaUtente(request, Session._currentRole);
                return Ok(uid_persona);
            }
            catch (Exception e)
            {
                //Log.Error("SalvaUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un utente
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpDelete]
        [Route("elimina")]
        public async Task<IHttpActionResult> EliminaUtente(Guid id)
        {
            try
            {
                await _adminLogic.EliminaUtente(id);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("EliminaUtente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiornare i dati del gruppo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpPost]
        [Route("salva-gruppo")]
        public async Task<IHttpActionResult> SalvaGruppo(SalvaGruppoRequest request)
        {
            try
            {
                await _adminLogic.SalvaGruppo(request);
                return Ok();
            }
            catch (Exception e)
            {
                //Log.Error("SalvaGruppo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i gruppi nel db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("groups/view")]
        public async Task<IHttpActionResult> GetGruppi(BaseRequest<GruppiDto> model)
        {
            try
            {
                var user = CurrentUser;
                var results = await _adminLogic.GetGruppi(model);
                return Ok(new RiepilogoGruppiModel
                {
                    Results = results,
                    Persona = user
                });
            }
            catch (Exception e)
            {
                //Log.Error("GetGruppi", e);
                return ErrorHandler(e);
            }
        }
    }
}