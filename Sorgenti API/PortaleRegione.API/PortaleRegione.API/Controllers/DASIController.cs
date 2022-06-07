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
        private readonly SeduteLogic _logicPem;

        /// <summary>
        /// Controller per la gestione modulo DASI (Atti Sindacato Ispettivo)
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="logicPersone"></param>
        public DASIController(IUnitOfWork unitOfWork, DASILogic logic, PersoneLogic logicPersone, AttiFirmeLogic logicFirma, SeduteLogic logicPEM)
        {
            _unitOfWork = unitOfWork;
            _logic = logic;
            _logicPersone = logicPersone;
            _logicFirma = logicFirma;
            _logicPem = logicPEM;
        }

        /// <summary>
        ///     Endpoint che restituisce il modello di Atto di Sindacato Ispettivo da creare
        /// </summary>
        /// <param name="tipo">Tipo di atto</param>
        /// <returns></returns>
        [Route("new")]
        public async Task<IHttpActionResult> GetNuovoModello(TipoAttoEnum tipo)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var result = await _logic.NuovoModello(tipo, persona);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNuovoModello", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///      Endpoint per avere l'oggetto atto da modificare
        /// </summary>
        /// <param name="id">Identificativo atto</param>
        /// <returns></returns>
        [Route("edit")]
        public async Task<IHttpActionResult> GetModificaModello(Guid id)
        {
            try
            {
                var atto = await _logic.Get(id);
                if (atto == null)
                {
                    return NotFound();
                }

                var countFirme = await _logicFirma.CountFirme(id);
                if (countFirme > 1)
                {
                    return BadRequest(
                        $"Non è possibile modificare l'atto. Ci sono ancora {countFirme} firme attive.");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                return Ok(await _logic.ModificaModello(atto, persona));
            }
            catch (Exception e)
            {
                Log.Error("GetModificaModello", e);
                return ErrorHandler(e);
            }
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
                var session = GetSession();
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
                var session = GetSession();
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
                var session = GetSession();
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
                var session = GetSession();
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
                var session = GetSession();
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
                var session = GetSession();
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
        ///     Endpoint per presentare gli Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="presentazioneModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("presenta")]
        public async Task<IHttpActionResult> Presenta(ComandiAzioneModel presentazioneModel)
        {
            try
            {
                if (ManagerLogic.BloccaPresentazione)
                {
                    return BadRequest(
                        "E' in corso un'altra operazione di presentazione. Riprova tra qualche secondo.");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var presentazioneUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                      persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (presentazioneUfficio)
                {
                    if (presentazioneModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logic.Presenta(presentazioneModel, persona));
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

                if (presentazioneModel.Pin != pinInDb.PIN_Decrypt)
                {
                    return BadRequest("Pin inserito non valido");
                }

                return Ok(await _logic.Presenta(presentazioneModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("Presentazione - DASI", e);
                return ErrorHandler(e);
            }
            finally
            {
                ManagerLogic.BloccaPresentazione = false;
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

        /// <summary>
        ///     Endpoint per avere il corpo dell'atto da template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("template-body")]
        public async Task<IHttpActionResult> GetBody(GetBodyModel model)
        {
            try
            {
                var atto = await _logic.Get(model.Id);
                if (atto == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var body = await _logic.GetBodyDASI(atto
                    , await _logicFirma.GetFirme(atto, FirmeTipoEnum.TUTTE)
                    , persona
                    , model.Template
                    , model.IsDeposito);

                return Ok(body);
            }
            catch (Exception e)
            {
                Log.Error("GetBody - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere la copertina del fascicolo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("template/copertina")]
        public async Task<IHttpActionResult> GetBodyCopertina(ByQueryModel model)
        {
            try
            {
                var body = await _logic.GetCopertina(model);

                return Ok(body);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyCopertina", e);
                return ErrorHandler(e);
            }
        }


        /// <summary>
        ///     Endpoint per scaricare il file allegato all'atto
        /// </summary>
        /// <param name="path">Percorso file</param>
        /// <returns></returns>
        [HttpGet]
        [Route("file")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(await _logic.Download(path));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download Allegato Atto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un atto
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        [HttpGet]
        [Route("elimina")]
        public async Task<IHttpActionResult> Elimina(Guid id)
        {
            try
            {
                var atto = await _logic.Get(id);
                if (atto == null)
                {
                    return NotFound();
                }

                var firmatari = await _logicFirma.GetFirme(atto, FirmeTipoEnum.ATTIVI);
                var firmatari_attivi = firmatari.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                if (firmatari_attivi.Any())
                {
                    return BadRequest("L'atto ha delle firme attive e non può essere eliminato");
                }

                var session = GetSession();
                await _logic.Elimina(atto, session._currentUId);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Elimina Atto - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ritirare un atto
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ritira")]
        public async Task<IHttpActionResult> Ritira(Guid id)
        {
            try
            {
                var atto = await _logic.Get(id);
                if (atto == null)
                {
                    return NotFound();
                }

                if (atto.UIDSeduta.HasValue)
                {
                    var seduta = await _logicPem.GetSeduta(atto.UIDSeduta.Value);
                    if (DateTime.Now > seduta.Data_seduta)
                    {
                        return BadRequest(
                            "Non è possibile ritirare l'atto durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                    }
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                await _logic.Ritira(atto, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Ritira Atto - DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare lo stato di una lista di atti
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route("modifica-stato")]
        public async Task<IHttpActionResult> ModificaStato(ModificaStatoAttoModel model)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                persona.CurrentRole = session._currentRole;
                return Ok(await _logic.ModificaStato(model, persona));
            }
            catch (Exception e)
            {
                Log.Error("ModificaStatoEmendamento", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per aggiungere una seduta all'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("iscrizione-seduta")]
        public async Task<IHttpActionResult> IscrizioneSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                persona.CurrentRole = session._currentRole;
                await _logic.IscrizioneSeduta(model, persona);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("IscrizioneSeduta", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per rimuovere un atto dalla seduta
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("rimuovi-seduta")]
        public async Task<IHttpActionResult> RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                await _logic.RimuoviSeduta(model);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("RimuoviSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli invitati di un atto
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        [Route("invitati")]
        public async Task<IHttpActionResult> GetInvitati(Guid id)
        {
            try
            {
                var atto = await _logic.Get(id);
                if (atto == null)
                {
                    return NotFound();
                }

                var result = await _logic.GetInvitati(atto);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetInvitati", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere gli stati
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("stati")]
        public async Task<IHttpActionResult> GetStati()
        {
            try
            {
                return Ok(_logic.GetStati());
            }
            catch (Exception e)
            {
                Log.Error("GetStati", e);
                return ErrorHandler(e);
            }
        }
    }
}