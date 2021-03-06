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
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire le persone
    /// </summary>
    [Authorize]
    [RoutePrefix("persone")]
    public class PersoneController : BaseApiController
    {
        private readonly PersoneLogic _logicPersone;

        public PersoneController(PersoneLogic logicPersone)
        {
            _logicPersone = logicPersone;
        }

        /// <summary>
        ///     Endpoint per avere le informazioni di una persona
        /// </summary>
        /// <param name="id"></param>
        /// <param name="IsGiunta"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IHttpActionResult> GetPersona(Guid id, bool IsGiunta = false)
        {
            try
            {
                return Ok(await _logicPersone.GetPersona(id, IsGiunta));
            }
            catch (Exception e)
            {
                Log.Error("GetPersona", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere informazioni sul ruolo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ruolo/{id:int}")]
        public async Task<IHttpActionResult> GetRuolo(int id)
        {
            try
            {
                var ruolo = await _logicPersone.GetRuolo((RuoliIntEnum) id);
                return Ok(Mapper.Map<RUOLI, RuoliDto>(ruolo));
            }
            catch (Exception e)
            {
                Log.Error("GetRuolo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le informazioni sul capo gruppo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("gruppo/{id:int}/capo-gruppo")]
        public async Task<IHttpActionResult> GetCapoGruppo(int id)
        {
            try
            {
                var capoGruppo = await _logicPersone.GetCapoGruppo(id);
                return Ok(capoGruppo);
            }
            catch (Exception e)
            {
                Log.Error("GetCapoGruppo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le persone che compongono la segreteria del gruppo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notifica_firma"></param>
        /// <param name="notifica_deposito"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("gruppo/{id:int}/segreteria-politica")]
        public async Task<IHttpActionResult> GetSegreteriaPolitica(int id, bool notifica_firma, bool notifica_deposito)
        {
            try
            {
                var segreteriaPolitica =
                    await _logicPersone.GetSegreteriaPolitica(id, notifica_firma, notifica_deposito);
                return Ok(segreteriaPolitica);
            }
            catch (Exception e)
            {
                Log.Error("GetSegreteriaPolitica", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le persone che compongono la giunta regionale
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("segreteria-giunta-regionale")]
        public async Task<IHttpActionResult> GetSegreteriaGiuntaRegionale(bool notifica_firma, bool notifica_deposito)
        {
            try
            {
                var segreteriaGiuntaRegionale =
                    await _logicPersone.GetSegreteriaGiuntaRegionale(notifica_firma, notifica_deposito);
                return Ok(segreteriaGiuntaRegionale);
            }
            catch (Exception e)
            {
                Log.Error("GetSegreteriaGiuntaRegionale", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le persone che compongono la giunta regionale
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("giunta-regionale")]
        public async Task<IHttpActionResult> GetGiuntaRegionale()
        {
            try
            {
                var giuntaRegionale = await _logicPersone.GetGiuntaRegionale();
                return Ok(giuntaRegionale);
            }
            catch (Exception e)
            {
                Log.Error("GetGiuntaRegionale", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti gli assessori di riferimento per la legislatura attuale
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("assessori")]
        public async Task<IHttpActionResult> GetAssessoriRiferimento()
        {
            try
            {
                var personaDtos = await _logicPersone.GetAssessoriRiferimento();

                return Ok(personaDtos);
            }
            catch (Exception e)
            {
                Log.Error("GetAssessoriRiferimento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i relatori per la legislatura attuale
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("relatori")]
        public async Task<IHttpActionResult> GetRelatori(Guid? id)
        {
            try
            {
                return Ok(await _logicPersone.GetRelatori(id));
            }
            catch (Exception e)
            {
                Log.Error("GetRelatori", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i gruppi per la legislatura attuale
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("gruppi")]
        public async Task<IHttpActionResult> GetGruppi()
        {
            try
            {
                return Ok(await _logicPersone.GetGruppi());
            }
            catch (Exception e)
            {
                Log.Error("GetGruppi", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint richiesta cambio pin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("cambio-pin")]
        public async Task<IHttpActionResult> CambioPin(CambioPinModel model)
        {
            try
            {
                if (model.conferma_pin != model.nuovo_pin)
                    return BadRequest("Il nuovo PIN non combacia con quello di conferma!!!");

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var currentPin = await _logicPersone.GetPin(persona);
                if (currentPin == null)
                    return BadRequest("Pin non impostato");
                if (currentPin.RichiediModificaPIN)
                    return BadRequest("E' richiesto il reset del pin");
                if (currentPin.PIN_Decrypt != model.vecchio_pin)
                    return BadRequest("Il vecchio PIN non è corretto!!!");

                int valuePin;
                var checkTry = int.TryParse(model.nuovo_pin, out valuePin);
                if (!checkTry)
                    return BadRequest("Il pin deve contenere solo cifre numeriche");
                if (model.nuovo_pin.Length != 4)
                    return BadRequest("Il PIN dev'essere un numero di massimo 4 cifre!");

                model.PersonaUId = persona.UID_persona;

                await _logicPersone.CambioPin(model);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("CambioPin", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti le persone attive dalla vista utenti
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetPersone()
        {
            try
            {
                var persone = await _logicPersone.GetPersone();

                return Ok(persone);
            }
            catch (Exception e)
            {
                Log.Error("GetPersone", e);
                return ErrorHandler(e);
            }
        }


        [Authorize(Roles = RuoliEnum.Amministratore_PEM)]
        [HttpPost]
        [Route("reset-pin")]
        public async Task<IHttpActionResult> ResetPin(ResetPinModel model)
        {
            try
            {
                int valuePin;
                var checkTry = int.TryParse(model.nuovo_pin, out valuePin);
                if (!checkTry)
                    return BadRequest("Il pin deve contenere solo cifre numeriche");
                if (model.nuovo_pin.Length != 4)
                    return BadRequest("Il PIN dev'essere un numero di massimo 4 cifre!");

                await _logicPersone.ResetPin(model);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("ResetPin", e);
                return ErrorHandler(e);
            }
        }
    }
}