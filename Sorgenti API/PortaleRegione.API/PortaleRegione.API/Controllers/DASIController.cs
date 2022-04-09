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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    /// Controller per gestire il modulo DASI
    /// </summary>
    [Authorize]
    [RoutePrefix("dasi")]
    public class DASIController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DASILogic _logic;
        private readonly PersoneLogic _logicPersone;
        private readonly AttiFirmeLogic _logicFirma;

        /// <summary>
        /// Controller per la gestione modulo DASI (Atti Sindacato Ispettivo)
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="logicPersone"></param>
        public DASIController(IUnitOfWork unitOfWork, DASILogic logic, PersoneLogic logicPersone, AttiFirmeLogic logicFirma)
        {
            _unitOfWork = unitOfWork;
            _logic = logic;
            _logicPersone = logicPersone;
            _logicFirma = logicFirma;
        }

        /// <summary>
        /// Endpoint per salvare un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Salva(AttoDASIDto request)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var nuovoAtto = await _logic.Salva(request, persona);

                return Created(new Uri(Request.RequestUri.ToString()), Mapper.Map<ATTI_DASI, AttoDASIDto>(nuovoAtto));
            }
            catch (Exception e)
            {
                Log.Error("Salva Atto DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        /// Endpoint per avere le inforazioni dell'atto archiviato
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var atto = await _logic.GetAttoDto(id, persona, personeInDbLight);

                return Ok(atto);
            }
            catch (Exception e)
            {
                Log.Error("Get Atto DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        /// Endpoint per avere il riepilogo filtrato di atti
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("riepilogo")]
        public async Task<IHttpActionResult> Riepilogo(BaseRequest<AttoDASIDto> request)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var response = await _logic.Get(request, persona, Request.RequestUri);

                return Ok(response);
            }
            catch (Exception e)
            {
                Log.Error("Riepilogo Atti DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        /// Endpoint per firmare un Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("firma")]
        public async Task<IHttpActionResult> Firma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var firmaUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                   persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logic.Firma(firmaModel, persona, null, true));
                }

                var pinInDb = await _logicPersone.GetPin(persona);
                if (pinInDb == null)
                {
                    return BadRequest("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    return BadRequest("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    return BadRequest("Pin inserito non valido");
                }

                return Ok(await _logic.Firma(firmaModel, persona, pinInDb));
            }
            catch (Exception e)
            {
                Log.Error("Firma - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ritirare la firma ad un Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ritiro-firma")]
        public async Task<IHttpActionResult> RitiroFirma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var firmaUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                   persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logic.RitiroFirma(firmaModel, persona));
                }

                var pinInDb = await _logicPersone.GetPin(persona);
                if (pinInDb == null)
                {
                    return BadRequest("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    return BadRequest("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    return BadRequest("Pin inserito non valido");
                }

                return Ok(await _logic.RitiroFirma(firmaModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("RitiroFirma - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare una firma da un Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("elimina-firma")]
        public async Task<IHttpActionResult> EliminaFirma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var firmaUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                   persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logic.EliminaFirma(firmaModel, persona));
                }

                var pinInDb = await _logicPersone.GetPin(persona);
                if (pinInDb == null)
                {
                    return BadRequest("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    return BadRequest("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    return BadRequest("Pin inserito non valido");
                }

                return Ok(await _logic.EliminaFirma(firmaModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("EliminaFirma - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per depositare gli Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="depositoModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposita")]
        public async Task<IHttpActionResult> Deposita(ComandiAzioneModel depositoModel)
        {
            try
            {
                if (ManagerLogic.BloccaDeposito)
                {
                    return BadRequest(
                        "E' in corso un'altra operazione di deposito. Riprova tra qualche secondo.");
                }

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var depositoUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                      persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (depositoUfficio)
                {
                    if (depositoModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logic.Deposita(depositoModel, persona));
                }

                var pinInDb = await _logicPersone.GetPin(persona);
                if (pinInDb == null)
                {
                    return BadRequest("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    return BadRequest("E' richiesto il reset del pin");
                }

                if (depositoModel.Pin != pinInDb.PIN_Decrypt)
                {
                    return BadRequest("Pin inserito non valido");
                }

                return Ok(await _logic.Deposita(depositoModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("Deposita - DASI", e);
                return ErrorHandler(e);
            }
            finally
            {
                ManagerLogic.BloccaDeposito = false;
            }
        }

        /// <summary>
        ///     Endpoint per avere i firmatari di un atto
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [Route("firmatari")]
        public async Task<IHttpActionResult> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            try
            {
                var atto = await _logic.Get(id);
                if (atto == null)
                {
                    return NotFound();
                }

                var result = await _logicFirma.GetFirme(atto, tipo);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatari", e);
                return ErrorHandler(e);
            }
        }
    }
}