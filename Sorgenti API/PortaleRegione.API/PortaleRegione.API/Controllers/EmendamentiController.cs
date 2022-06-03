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
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli emendamenti
    /// </summary>
    [Authorize]
    [RoutePrefix("emendamenti")]
    public class EmendamentiController : BaseApiController
    {
        private readonly AttiLogic _logicAtti;
        private readonly EmendamentiLogic _logicEm;
        private readonly FirmeLogic _logicFirme;
        private readonly AdminLogic _logicAdmin;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PersoneLogic _logicPersone;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="logicPersone"></param>
        /// <param name="logicAtti"></param>
        /// <param name="logicEm"></param>
        /// <param name="logicFirme"></param>
        /// <param name="logicAdmin"></param>
        public EmendamentiController(
            IUnitOfWork unitOfWork,
            PersoneLogic logicPersone,
            AttiLogic logicAtti,
            EmendamentiLogic logicEm,
            FirmeLogic logicFirme,
            AdminLogic logicAdmin)
        {
            _unitOfWork = unitOfWork;
            _logicPersone = logicPersone;
            _logicAtti = logicAtti;
            _logicEm = logicEm;
            _logicFirme = logicFirme;
            _logicAdmin = logicAdmin;
        }

        /// <summary>
        ///     Endpoint per avere tutti gli emendamenti appartenenti ad un atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("view")]
        public async Task<IHttpActionResult> GetEmendamenti(BaseRequest<EmendamentiDto> model)
        {
            try
            {
                var atto = await _logicAtti.GetAtto(model.id);

                if (atto == null)
                {
                    return NotFound();
                }

                model.param.TryGetValue("CLIENT_MODE", out object CLIENT_MODE); // per trattazione aula
                model.param.TryGetValue("VIEW_MODE", out object viewMode); // per vista preview/griglia
                ViewModeEnum VIEW_MODE = ViewModeEnum.GRID;
                if (viewMode != null)
                    Enum.TryParse(viewMode.ToString(), out VIEW_MODE);
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var ricerca_presidente_regione = await _logicAdmin.GetUtenti(new BaseRequest<PersonaDto>
                {
                    page = 1,
                    size = 1,
                    filtro = new List<FilterStatement<PersonaDto>>
                    {
                        new FilterStatement<PersonaDto>
                        {
                            PropertyId = nameof(PersonaDto.Ruoli),
                            Operation = Operation.EqualTo,
                            Value = (int)RuoliIntEnum.Presidente_Regione,
                            Connector = FilterStatementConnector.And
                        }
                    }
                }, session
                    , Request.RequestUri);
                var presidente = ricerca_presidente_regione.Results.First();
                var results =
                    await _logicEm.GetEmendamenti(model, persona, Convert.ToInt16(CLIENT_MODE), (int)VIEW_MODE, presidente, Request.RequestUri);
                results.Atto = Mapper.Map<ATTI, AttiDto>(atto);
                return Ok(results);

            }
            catch (Exception e)
            {
                Log.Error("GetEmendamenti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere solo gli emendamenti appartenenti ad un atto dove è richiesta la firma dell'utente corrente
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("view-richiesta-propria-firma")]
        public async Task<IHttpActionResult> GetEmendamenti_RichiestaPropriaFirma(BaseRequest<EmendamentiDto> model)
        {
            try
            {
                var atto = await _logicAtti.GetAtto(model.id);

                if (atto == null)
                {
                    return NotFound();
                }

                object CLIENT_MODE;
                model.param.TryGetValue("CLIENT_MODE", out CLIENT_MODE); // per trattazione aula
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var results =
                    await _logicEm.GetEmendamenti_RichiestaPropriaFirma(model, persona, Convert.ToInt16(CLIENT_MODE));

                return Ok(new EmendamentiViewModel
                {
                    Data = new BaseResponse<EmendamentiDto>(
                        model.page,
                        model.size,
                        results,
                        model.filtro,
                        results.Count,
                        Request.RequestUri),
                    Atto = Mapper.Map<ATTI, AttiDto>(atto),
                    Mode = (ClientModeEnum)Convert.ToInt16(CLIENT_MODE),
                    CurrentUser = persona
                });

            }
            catch (Exception e)
            {
                Log.Error("GetEmendamenti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per scaricare il file allegato all'emendamento
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
                var response = ResponseMessage(await _logicEm.Download(path));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download Allegato EM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere un singolo emendamento preciso
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetEmendamento(Guid id)
        {
            //TODO: implementare i controlli anche sull'atto
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                var result = await _logicEm.GetEM_DTO(em, atto, persona, personeInDbLight);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint che restituisce il modello di emendamento da creare
        /// </summary>
        /// <param name="id"></param>
        /// <param name="em_riferimentoUId"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Responsabile_Segreteria_Politica + "," + RuoliExt.Segreteria_Politica + "," +
                           RuoliExt.Responsabile_Segreteria_Giunta + "," + RuoliExt.Segreteria_Giunta_Regionale +
                           "," + RuoliExt.Presidente_Regione)]
        [Route("new")]
        public async Task<IHttpActionResult> GetNuovoEmendamento(Guid id, Guid? em_riferimentoUId)
        {
            try
            {
                var atti = await _logicAtti.GetAtto(id);
                if (atti == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var result = await _logicEm.ModelloNuovoEM(atti, em_riferimentoUId, persona);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNuovoEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere l'oggetto emendamento da modificare
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Responsabile_Segreteria_Politica + "," + RuoliExt.Segreteria_Politica + "," +
                           RuoliExt.Responsabile_Segreteria_Giunta + "," + RuoliExt.Segreteria_Giunta_Regionale +
                           "," + RuoliExt.Presidente_Regione)]
        [Route("edit")]
        public async Task<IHttpActionResult> GetModificaEmendamento(Guid id)
        {
            try
            {
                var countFirme = await _logicFirme.CountFirme(id);
                if (countFirme > 1)
                {
                    return BadRequest(
                        $"Non è possibile modificare l'emendamento. Ci sono ancora {countFirme} firme attive.");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                return Ok(await _logicEm.ModelloModificaEM(await _logicEm.GetEM(id), persona));
            }
            catch (Exception e)
            {
                Log.Error("GetModificaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere un emendamento in database
        /// </summary>
        /// <param name="model">Modello emendamento da inserire</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Responsabile_Segreteria_Politica + "," + RuoliExt.Segreteria_Politica + "," +
                           RuoliExt.Responsabile_Segreteria_Giunta + "," + RuoliExt.Segreteria_Giunta_Regionale +
                           "," + RuoliExt.Presidente_Regione)]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> NuovoEmendamento(EmendamentiDto model)
        {
            try
            {
                if (!model.UIDPersonaProponente.HasValue)
                {
                    return BadRequest("L'emendamento deve avere un proponente");
                }

                if (model.IDParte == 0)
                {
                    return BadRequest("E' obbligatorio indicare l'elemento da emendare");
                }

                if (model.IDTipo_EM == 0)
                {
                    return BadRequest("E' obbligatorio indicare il modo");
                }

                if (string.IsNullOrEmpty(model.TestoEM_originale))
                {
                    return BadRequest("Il testo dell'emendamento non può essere vuoto");
                }

                if (model.IDParte == (int)PartiEMEnum.Articolo)
                {
                    if (!model.UIDArticolo.HasValue)
                    {
                        return BadRequest("Manca il valore dell'articolo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Capo)
                {
                    if (string.IsNullOrEmpty(model.NCapo))
                    {
                        return BadRequest("Manca il valore del capo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Titolo)
                {
                    if (string.IsNullOrEmpty(model.NTitolo))
                    {
                        return BadRequest("Manca il valore del titolo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Missione)
                {
                    if (!model.NTitoloB.HasValue || !model.NMissione.HasValue ||
                        !model.NProgramma.HasValue)
                    {
                        return BadRequest("I valori Missione - Programma - Titolo sono obbligatori");
                    }
                }

                var isGiunta = model.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID;
                var proponente = await _logicPersone.GetPersona(model.UIDPersonaProponente.Value, isGiunta);

                var em = await _logicEm.NuovoEmendamento(model, proponente, isGiunta);

                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                return Ok(await _logicEm.GetEM_DTO(em.UIDEM, atto, null, personeInDbLight));
            }
            catch (Exception e)
            {
                Log.Error("NuovoEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare un emendamento
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Responsabile_Segreteria_Politica + "," + RuoliExt.Segreteria_Politica + "," +
                           RuoliExt.Responsabile_Segreteria_Giunta + "," + RuoliExt.Segreteria_Giunta_Regionale +
                           "," + RuoliExt.Presidente_Regione)]
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> ModificaEmendamento(EmendamentiDto model)
        {
            try
            {
                var em = await _logicEm.GetEM(model.UIDEM);

                if (em == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                    && persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea)
                {
                    var countFirme = await _logicFirme.CountFirme(model.UIDEM);
                    if (countFirme > 1)
                    {
                        return BadRequest(
                            $"Non è possibile modificare l'emendamento. Ci sono ancora {countFirme} attive.");
                    }
                }

                await _logicEm.ModificaEmendamento(model, em, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ModificaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare virtualmente un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteEmendamento(Guid id)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var countFirme = await _logicFirme.CountFirme(id);
                if (countFirme > 0)
                {
                    return BadRequest("L'emendamento ha delle firme attive e non può essere eliminato");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                await _logicEm.DeleteEmendamento(em, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DeleteEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per proiettare un emendamento in aula
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="ordine">Ordine emendamento in votazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("proietta-view")]
        public async Task<IHttpActionResult> ViewerEmendamento(Guid id, int ordine)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                var result = await _logicEm.GetEM_ByProietta(id, ordine, persona);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("ProiettaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per conoscere l' emendamento proiettato in aula
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [HttpGet]
        [Route("proietta-view-live")]
        public async Task<IHttpActionResult> ViewerEmendamento(Guid id)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                var result = await _logicEm.GetEM_LiveProietta(id, persona);

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("ProiettaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per impostare l'emendamento da proiettare in aula
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("proietta")]
        public async Task<IHttpActionResult> ProiettaEmendamento(Guid id)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                await _logicEm.Proietta(em, session._currentUId);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ProiettaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i firmatari di un emendamento
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [Route("firmatari")]
        public async Task<IHttpActionResult> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var result = await _logicFirme.GetFirme(em, tipo);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatari", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli invitati di un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Route("invitati")]
        public async Task<IHttpActionResult> GetInvitati(Guid id)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var result = await _logicEm.GetInvitati(em);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetInvitati", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il corpo dell'emendamento da template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("template-body")]
        public async Task<IHttpActionResult> GetBody(GetBodyModel model)
        {
            try
            {
                var em = await _logicEm.GetEM(model.Id);
                if (em == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                var body = await _logicEm.GetBodyEM(em
                    , firme
                    , persona
                    , model.Template
                    , model.IsDeposito);

                return Ok(body);
            }
            catch (Exception e)
            {
                Log.Error("GetBody", e);
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
        public async Task<IHttpActionResult> GetBodyCopertina(CopertinaModel model)
        {
            try
            {
                var body = await _logicEm.GetCopertina(model);

                return Ok(body);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyCopertina", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per firmare gli emendamenti
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Presidente_Regione)]
        [HttpPost]
        [Route("firma")]
        public async Task<IHttpActionResult> FirmaEmendamento(ComandiAzioneModel firmaModel)
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

                    return Ok(await _logicEm.FirmaEmendamento(firmaModel, persona, null, true));
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

                return Ok(await _logicEm.FirmaEmendamento(firmaModel, persona, pinInDb));
            }
            catch (Exception e)
            {
                Log.Error("FirmaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ritirare la firma ad un emedamento
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ritiro-firma")]
        public async Task<IHttpActionResult> RitiroFirmaEmendamento(ComandiAzioneModel firmaModel)
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

                    return Ok(await _logicEm.RitiroFirmaEmendamento(firmaModel, persona));
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

                return Ok(await _logicEm.RitiroFirmaEmendamento(firmaModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("RitiroFirmaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare una firma da un emendamento
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("elimina-firma")]
        public async Task<IHttpActionResult> EliminaFirmaEmendamento(ComandiAzioneModel firmaModel)
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

                    return Ok(await _logicEm.EliminaFirmaEmendamento(firmaModel, persona));
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

                return Ok(await _logicEm.EliminaFirmaEmendamento(firmaModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("EliminaFirmaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per depositare gli emendamenti
        /// </summary>
        /// <param name="depositoModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposita")]
        public async Task<IHttpActionResult> DepositaEmendamento(ComandiAzioneModel depositoModel)
        {
            try
            {
                if (ManagerLogic.BloccaDeposito)
                {
                    return BadRequest(
                        "E' in corso un'altra operazione di deposito. Riprova tra qualche secondo.");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var depositoUfficio = persona.CurrentRole == RuoliIntEnum.Amministratore_PEM ||
                                      persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea;

                if (depositoUfficio)
                {
                    if (depositoModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        return BadRequest("Pin inserito non valido");
                    }

                    return Ok(await _logicEm.DepositaEmendamento(depositoModel, persona));
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

                return Ok(await _logicEm.DepositaEmendamento(depositoModel, persona));
            }
            catch (Exception e)
            {
                Log.Error("DepositaEmendamento", e);
                return ErrorHandler(e);
            }
            finally
            {
                ManagerLogic.BloccaDeposito = false;
            }
        }

        /// <summary>
        ///     Endpoint per ritirare un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ritira")]
        public async Task<IHttpActionResult> RitiraEmendamento(Guid id)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var atto = await _logicAtti.GetAtto(em.UIDAtto);
                if (DateTime.Now > atto.SEDUTE.Data_seduta)
                {
                    return BadRequest(
                        "Non è possibile ritirare l'emendamento durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                await _logicEm.RitiraEmendamento(em, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("RitiraEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("elimina")]
        public async Task<IHttpActionResult> EliminaEmendamento(Guid id)
        {
            try
            {
                var em = await _logicEm.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var firmatari = await _logicFirme.GetFirme(em, FirmeTipoEnum.ATTIVI);
                var firmatari_attivi = firmatari.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                if (firmatari_attivi.Any())
                {
                    return BadRequest("L'emendamento ha delle firme attive e non può essere eliminato");
                }

                var session = GetSession();
                await _logicEm.EliminaEmendamento(em, session._currentUId);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("EliminaEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere l'oggetto emendamento da modificare
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("edit-meta-dati")]
        public async Task<IHttpActionResult> GetModificaMetaDatiEmendamento(Guid id)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                return Ok(await _logicEm.ModelloModificaEM(await _logicEm.GetEM(id), persona));
            }
            catch (Exception e)
            {
                Log.Error("GetModificaMetaDatiEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare i metadati di un emendamento
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("meta-dati")]
        [HttpPut]
        public async Task<IHttpActionResult> ModificaMetaDatiEmendamento(EmendamentiDto model)
        {
            try
            {
                var em = await _logicEm.GetEM(model.UIDEM);

                if (em == null)
                {
                    return NotFound();
                }

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                persona.CurrentRole = session._currentRole;

                await _logicEm.ModificaMetaDatiEmendamento(model, em, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ModificaMetaDatiEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare lo stato di una lista di emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route("modifica-stato")]
        public async Task<IHttpActionResult> ModificaStato(ModificaStatoModel model)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                persona.CurrentRole = session._currentRole;
                return Ok(await _logicEm.ModificaStatoEmendamento(model, persona));
            }
            catch (Exception e)
            {
                Log.Error("ModificaStatoEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per assegnare un nuovo proponente ad una lista di emendamenti ritirati
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route("assegna-nuovo-proponente")]
        public async Task<IHttpActionResult> AssegnaNuovoPorponente(AssegnaProponenteModel model)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in model.Lista)
                {
                    var em = await _logicEm.GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    if (em.STATI_EM.IDStato != (int)StatiEnum.Ritirato)
                    {
                        results.Add(idGuid,
                            $"ERROR: l'emendamento è {em.STATI_EM.Stato}, è possibile assegnare un nuovo proponente solo se lo stato è RITIRATO.");
                        continue;
                    }

                    await _logicEm.AssegnaNuovoProponente(em, model);
                    results.Add(idGuid, "OK");
                }

                return Ok(results);
            }
            catch (Exception e)
            {
                Log.Error("AssegnaNuovoPorponente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per raggruppare emendamenti assegnando un colore esadecimale
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route("raggruppa")]
        public async Task<IHttpActionResult> RaggruppaEmendamenti(RaggruppaEmendamentiModel model)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in model.Lista)
                {
                    var em = await _logicEm.GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    await _logicEm.RaggruppaEmendamento(em, model.Colore);
                    results.Add(idGuid, "OK");
                }

                return Ok(results);
            }
            catch (Exception e)
            {
                Log.Error("RaggruppaEmendamenti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ordinare gli emendamenti di un atto in votazione
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordina")]
        public async Task<IHttpActionResult> ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _logicEm.ORDINA_EM_TRATTAZIONE(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ORDINA_EM_TRATTAZIONE", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ordinare gli emendamenti di un atto in votazione
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordinamento-concluso")]
        public async Task<IHttpActionResult> ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO(Guid id)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session._currentUId);
                persona.CurrentRole = session._currentRole;

                await _logicEm.ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO(id, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ORDINA_EM_TRATTAZIONE", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare un emendamento di un atto in votazione in posizione superiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordina-up")]
        public async Task<IHttpActionResult> UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _logicEm.UP_EM_TRATTAZIONE(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("UP_EM_TRATTAZIONE", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare un emendamento di un atto in votazione in posizione inferiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordina-down")]
        public async Task<IHttpActionResult> DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _logicEm.DOWN_EM_TRATTAZIONE(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DOWN_EM_TRATTAZIONE", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare un emendamento di un atto in votazione in una posizione precisa
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="pos">Int posizione dove spostare l'emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("sposta")]
        public async Task<IHttpActionResult> SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                await _logicEm.SPOSTA_EM_TRATTAZIONE(id, pos);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SPOSTA_EM_TRATTAZIONE", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le parti emendabili a DB
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("parti-em")]
        public async Task<IHttpActionResult> GetPartiEM()
        {
            try
            {
                return Ok(await _logicEm.GetPartiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetPartiEM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i tipi di emendmento
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tipi-em")]
        public async Task<IHttpActionResult> GetTipiEM()
        {
            try
            {
                return Ok(await _logicEm.GetTipiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetTipiEM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le missioni
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("missioni-em")]
        public async Task<IHttpActionResult> GetMissioniEM()
        {
            try
            {
                return Ok(await _logicEm.GetMissioni());
            }
            catch (Exception e)
            {
                Log.Error("GetMissioni", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i titoli / missione
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("titoli-missioni-em")]
        public async Task<IHttpActionResult> GetTitoliMissioni()
        {
            try
            {
                return Ok(await _logicEm.GetTitoliMissioni());
            }
            catch (Exception e)
            {
                Log.Error("GetTitoliMissioni", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli stati
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("stati-em")]
        public async Task<IHttpActionResult> GetStatiEM()
        {
            try
            {
                return Ok(await _logicEm.GetStatiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetStatiEM", e);
                return ErrorHandler(e);
            }
        }
    }
}