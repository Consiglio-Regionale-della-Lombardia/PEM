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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
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
    public class EmendamentiController : BaseApiController
    {
        /// <summary>
        ///     Endpoint per avere tutti gli emendamenti appartenenti ad un atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.PEM.Emendamenti.GetAll)]
        public async Task<IHttpActionResult> GetEmendamenti(BaseRequest<EmendamentiDto> model)
        {
            try
            {
                var atto = await _attiLogic.GetAtto(model.id);

                if (atto == null)
                {
                    return NotFound();
                }

                model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
                model.param.TryGetValue("VIEW_MODE", out var viewMode); // per vista preview/griglia
                var VIEW_MODE = ViewModeEnum.GRID;
                if (viewMode != null)
                    Enum.TryParse(viewMode.ToString(), out VIEW_MODE);
                var user = CurrentUser;
                var ricerca_presidente_regione = await _adminLogic.GetUtenti(new BaseRequest<PersonaDto>
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
                }, user
                    , Request.RequestUri);
                var presidente = ricerca_presidente_regione.Results.First();
                var results =
                    await _emendamentiLogic.GetEmendamenti(model, user, Convert.ToInt16(CLIENT_MODE), (int)VIEW_MODE,
                        presidente, Request.RequestUri);
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
        [Route(ApiRoutes.PEM.Emendamenti.GetAllRichiestaPropriaFirma)]
        public async Task<IHttpActionResult> GetEmendamenti_RichiestaPropriaFirma(BaseRequest<EmendamentiDto> model)
        {
            try
            {
                var atto = await _attiLogic.GetAtto(model.id);
                if (atto == null)
                {
                    return NotFound();
                }

                model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
                var user = CurrentUser;
                var results =
                    await _emendamentiLogic.GetEmendamenti_RichiestaPropriaFirma(model, user, Convert.ToInt16(CLIENT_MODE));

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
                    CurrentUser = user
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
        [AllowAnonymous]
        [HttpGet]
        [Route(ApiRoutes.PEM.Emendamenti.DownloadDoc)]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(await _emendamentiLogic.Download(path));

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
        [Route(ApiRoutes.PEM.Emendamenti.Get)]
        public async Task<IHttpActionResult> GetEmendamento(Guid id)
        {
            //TODO: implementare i controlli anche sull'atto
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);

                var result = await _emendamentiLogic.GetEM_DTO(em, atto, CurrentUser);
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
        /// <param name="sub_id"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea + "," +
                           RuoliExt.Consigliere_Regionale + "," + RuoliExt.Assessore_Sottosegretario_Giunta + "," +
                           RuoliExt.Responsabile_Segreteria_Politica + "," + RuoliExt.Segreteria_Politica + "," +
                           RuoliExt.Responsabile_Segreteria_Giunta + "," + RuoliExt.Segreteria_Giunta_Regionale +
                           "," + RuoliExt.Presidente_Regione)]
        [Route(ApiRoutes.PEM.Emendamenti.GetNuovoModello)]
        public async Task<IHttpActionResult> GetNuovoEmendamento(Guid id, Guid? sub_id)
        {
            try
            {
                var atti = await _attiLogic.GetAtto(id);
                if (atti == null)
                {
                    return NotFound();
                }

                var result = await _emendamentiLogic.ModelloNuovoEM(atti, sub_id, CurrentUser);
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
        [Route(ApiRoutes.PEM.Emendamenti.GetModificaModello)]
        public async Task<IHttpActionResult> GetModificaEmendamento(Guid id)
        {
            try
            {
                var countFirme = await _firmeLogic.CountFirme(id);
                if (countFirme > 1)
                {
                    return BadRequest(
                        $"Non è possibile modificare l'emendamento. Ci sono ancora {countFirme} firme attive.");
                }

                return Ok(await _emendamentiLogic.ModelloModificaEM(await _emendamentiLogic.GetEM(id), CurrentUser));
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
        [Route(ApiRoutes.PEM.Emendamenti.Create)]
        public async Task<IHttpActionResult> NuovoEmendamento(EmendamentiDto model)
        {
            try
            {
                if (model.UIDPersonaProponente == Guid.Empty)
                {
                    throw new InvalidOperationException("L'emendamento deve avere un proponente");
                }

                if (model.IDParte == 0)
                {
                    throw new InvalidOperationException("E' obbligatorio indicare l'elemento da emendare");
                }

                if (model.IDTipo_EM == 0)
                {
                    throw new InvalidOperationException("E' obbligatorio indicare il modo");
                }

                if (string.IsNullOrEmpty(model.TestoEM_originale))
                {
                    throw new InvalidOperationException("Il testo dell'emendamento non può essere vuoto");
                }

                if (model.IDParte == (int)PartiEMEnum.Articolo)
                {
                    if (!model.UIDArticolo.HasValue)
                    {
                        throw new InvalidOperationException("Manca il valore dell'articolo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Capo)
                {
                    if (string.IsNullOrEmpty(model.NCapo))
                    {
                        throw new InvalidOperationException("Manca il valore del capo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Titolo)
                {
                    if (string.IsNullOrEmpty(model.NTitolo))
                    {
                        throw new InvalidOperationException("Manca il valore del titolo");
                    }
                }

                if (model.IDParte == (int)PartiEMEnum.Missione)
                {
                    if (!model.NTitoloB.HasValue || !model.NMissione.HasValue ||
                        !model.NProgramma.HasValue)
                    {
                        throw new InvalidOperationException("I valori Missione - Programma - Titolo sono obbligatori");
                    }
                }

                var isGiunta = model.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID;
                var proponente = await _personeLogic.GetPersona(model.UIDPersonaProponente, isGiunta);

                var em = await _emendamentiLogic.NuovoEmendamento(model, proponente);

                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                return Ok(await _emendamentiLogic.GetEM_DTO(em.UIDEM, atto, null));
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
        [Route(ApiRoutes.PEM.Emendamenti.Edit)]
        public async Task<IHttpActionResult> ModificaEmendamento(EmendamentiDto model)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(model.UIDEM);
                if (em == null)
                {
                    return NotFound();
                }

                var user = CurrentUser;
                if (!user.IsSegreteriaAssemblea)
                {
                    var countFirme = await _firmeLogic.CountFirme(model.UIDEM);
                    if (countFirme > 1)
                    {
                        return BadRequest(
                            $"Non è possibile modificare l'emendamento. Ci sono ancora {countFirme} attive.");
                    }
                }

                await _emendamentiLogic.ModificaEmendamento(model, em, user);

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
        [Route(ApiRoutes.PEM.Emendamenti.Delete)]
        public async Task<IHttpActionResult> DeleteEmendamento(Guid id)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var countFirme = await _firmeLogic.CountFirme(id);
                if (countFirme > 0)
                {
                    throw new InvalidOperationException("L'emendamento ha delle firme attive e non può essere eliminato");
                }

                await _emendamentiLogic.DeleteEmendamento(em, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DeleteEmendamento", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i firmatari di un emendamento
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [Route(ApiRoutes.PEM.Emendamenti.GetFirmatari)]
        public async Task<IHttpActionResult> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var result = await _firmeLogic.GetFirme(em, tipo);
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
        [Route(ApiRoutes.PEM.Emendamenti.GetInvitati)]
        public async Task<IHttpActionResult> GetInvitati(Guid id)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var result = await _emendamentiLogic.GetInvitati(em);
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
        [Route(ApiRoutes.PEM.Emendamenti.GetBody)]
        public async Task<IHttpActionResult> GetBody(GetBodyModel model)
        {
            try
            {
                var emDto = await _emendamentiLogic.GetEM_DTO(model.Id);
                var firme = await _firmeLogic.GetFirme(emDto, FirmeTipoEnum.TUTTE);
                var body = await _emendamentiLogic.GetBodyEM(emDto
                    , firme.ToList()
                    , CurrentUser
                    , model.Template);

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
        [Route(ApiRoutes.PEM.Emendamenti.GetBodyCopertina)]
        public async Task<IHttpActionResult> GetBodyCopertina(CopertinaModel model)
        {
            try
            {
                var body = await _emendamentiLogic.GetCopertina(model);
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
        [HttpPost]
        [Route(ApiRoutes.PEM.Emendamenti.Firma)]
        public async Task<IHttpActionResult> FirmaEmendamento(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;
                var firmaUfficio = user.IsSegreteriaAssemblea;
                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        throw new InvalidOperationException("Pin inserito non valido");
                    }

                    var resultFirmaUfficio = await _emendamentiLogic.Firma(firmaModel, user, null, true);
                    return Ok(resultFirmaUfficio);
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null)
                {
                    throw new InvalidOperationException("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    throw new InvalidOperationException("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    throw new InvalidOperationException("Pin inserito non valido");
                }

                var result = await _emendamentiLogic.Firma(firmaModel, user, pinInDb);
                return Ok(result);
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
        [Route(ApiRoutes.PEM.Emendamenti.RitiroFirma)]
        public async Task<IHttpActionResult> RitiroFirmaEmendamento(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;
                var firmaUfficio = user.IsSegreteriaAssemblea;
                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        throw new InvalidOperationException("Pin inserito non valido");
                    }

                    return Ok(await _emendamentiLogic.RitiroFirmaEmendamento(firmaModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null)
                {
                    throw new InvalidOperationException("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    throw new InvalidOperationException("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    throw new InvalidOperationException("Pin inserito non valido");
                }

                return Ok(await _emendamentiLogic.RitiroFirmaEmendamento(firmaModel, user));
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
        [Route(ApiRoutes.PEM.Emendamenti.EliminaFirma)]
        public async Task<IHttpActionResult> EliminaFirmaEmendamento(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;
                var firmaUfficio = user.IsSegreteriaAssemblea;
                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        throw new InvalidOperationException("Pin inserito non valido");
                    }

                    return Ok(await _emendamentiLogic.EliminaFirmaEmendamento(firmaModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null)
                {
                    throw new InvalidOperationException("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    throw new InvalidOperationException("E' richiesto il reset del pin");
                }

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                {
                    throw new InvalidOperationException("Pin inserito non valido");
                }

                return Ok(await _emendamentiLogic.EliminaFirmaEmendamento(firmaModel, user));
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
        [Route(ApiRoutes.PEM.Emendamenti.Deposita)]
        public async Task<IHttpActionResult> DepositaEmendamento(ComandiAzioneModel depositoModel)
        {
            try
            {
                if (ManagerLogic.BloccaDeposito)
                {
                    return BadRequest(
                        "E' in corso un'altra operazione di deposito. Riprova tra qualche secondo.");
                }

                var user = CurrentUser;

                var depositoUfficio = user.IsSegreteriaAssemblea;

                if (depositoUfficio)
                {
                    if (depositoModel.Pin != AppSettingsConfiguration.MasterPIN)
                    {
                        throw new InvalidOperationException("Pin inserito non valido");
                    }

                    return Ok(await _emendamentiLogic.DepositaEmendamento(depositoModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null)
                {
                    throw new InvalidOperationException("Pin non impostato");
                }

                if (pinInDb.RichiediModificaPIN)
                {
                    throw new InvalidOperationException("E' richiesto il reset del pin");
                }

                if (depositoModel.Pin != pinInDb.PIN_Decrypt)
                {
                    throw new InvalidOperationException("Pin inserito non valido");
                }

                return Ok(await _emendamentiLogic.DepositaEmendamento(depositoModel, user));
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
        [Route(ApiRoutes.PEM.Emendamenti.Ritira)]
        public async Task<IHttpActionResult> RitiraEmendamento(Guid id)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var atto = await _attiLogic.GetAtto(em.UIDAtto);
                if (DateTime.Now > atto.SEDUTE.Data_seduta)
                {
                    return BadRequest(
                        "Non è possibile ritirare l'emendamento durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                }

                await _emendamentiLogic.RitiraEmendamento(em, CurrentUser);

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
        [Route(ApiRoutes.PEM.Emendamenti.Elimina)]
        public async Task<IHttpActionResult> EliminaEmendamento(Guid id)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var firmatari = await _firmeLogic.GetFirme(em, FirmeTipoEnum.ATTIVI);
                var firmatari_attivi = firmatari.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                if (firmatari_attivi.Any())
                {
                    throw new InvalidOperationException("L'emendamento ha delle firme attive e non può essere eliminato");
                }

                await _emendamentiLogic.EliminaEmendamento(em, Session._currentUId);

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
        [Route(ApiRoutes.PEM.Emendamenti.GetModificaModelloMetaDati)]
        public async Task<IHttpActionResult> GetModificaMetaDatiEmendamento(Guid id)
        {
            try
            {
                return Ok(await _emendamentiLogic.ModelloModificaEM(await _emendamentiLogic.GetEM(id), CurrentUser));
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
        [Route(ApiRoutes.PEM.Emendamenti.AggiornaMetaDati)]
        [HttpPut]
        public async Task<IHttpActionResult> ModificaMetaDatiEmendamento(EmendamentiDto model)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(model.UIDEM);

                if (em == null)
                {
                    return NotFound();
                }

                await _emendamentiLogic.ModificaMetaDatiEmendamento(model, em, CurrentUser);

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
        [Route(ApiRoutes.PEM.Emendamenti.ModificaStato)]
        public async Task<IHttpActionResult> ModificaStato(ModificaStatoModel model)
        {
            try
            {
                return Ok(await _emendamentiLogic.ModificaStatoEmendamento(model, CurrentUser));
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
        [Route(ApiRoutes.PEM.Emendamenti.AssegnaNuovoProponente)]
        public async Task<IHttpActionResult> AssegnaNuovoProponente(AssegnaProponenteModel model)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in model.Lista)
                {
                    var em = await _emendamentiLogic.GetEM(idGuid);
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

                    await _emendamentiLogic.AssegnaNuovoProponente(em, model);
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
        [Route(ApiRoutes.PEM.Emendamenti.Raggruppa)]
        public async Task<IHttpActionResult> RaggruppaEmendamenti(RaggruppaEmendamentiModel model)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in model.Lista)
                {
                    var em = await _emendamentiLogic.GetEM(idGuid);
                    if (em == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    await _emendamentiLogic.RaggruppaEmendamento(em, model.Colore);
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
        [Route(ApiRoutes.PEM.Emendamenti.Ordina)]
        public async Task<IHttpActionResult> ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _emendamentiLogic.ORDINA_EM_TRATTAZIONE(id);
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
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route(ApiRoutes.PEM.Emendamenti.OrdinamentoConcluso)]
        public async Task<IHttpActionResult> OrdinamentoConcluso(ComandiAzioneModel model)
        {
            try
            {
                if (model.Azione != ActionEnum.ORDINA)
                {
                    throw new Exception("Azione non valida");
                }

                await _emendamentiLogic.OrdinamentoConcluso(model, CurrentUser);
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
        [Route(ApiRoutes.PEM.Emendamenti.OrdinaUp)]
        public async Task<IHttpActionResult> UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _emendamentiLogic.UP_EM_TRATTAZIONE(id);
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
        [Route(ApiRoutes.PEM.Emendamenti.OrdinaDown)]
        public async Task<IHttpActionResult> DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await _emendamentiLogic.DOWN_EM_TRATTAZIONE(id);
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
        [Route(ApiRoutes.PEM.Emendamenti.Sposta)]
        public async Task<IHttpActionResult> SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                await _emendamentiLogic.SPOSTA_EM_TRATTAZIONE(id, pos);
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
        [Route(ApiRoutes.PEM.Emendamenti.GetParti)]
        public async Task<IHttpActionResult> GetPartiEM()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetPartiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetPartiEM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i tipi di emendamento
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Emendamenti.GetTipi)]
        public async Task<IHttpActionResult> GetTipiEM()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetTipiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetTipiEM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere la lista dei tags
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Emendamenti.GetTags)]
        public async Task<IHttpActionResult> GetTags()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetTags());
            }
            catch (Exception e)
            {
                Log.Error("GetTags", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per scaricare il file generato
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Emendamenti.StampaImmediata)]
        public async Task<IHttpActionResult> Download(Guid id)
        {
            try
            {
                var em = await _emendamentiLogic.GetEM(id);
                if (em == null)
                {
                    return NotFound();
                }

                var response = ResponseMessage(await _emendamentiLogic.DownloadPDFIstantaneo(em, CurrentUser));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download", e);
                return ErrorHandler(e);
            }
        }


        /// <summary>
        ///     Endpoint per avere le missioni
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Emendamenti.GetMissioni)]
        public async Task<IHttpActionResult> GetMissioniEM()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetMissioni());
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
        [Route(ApiRoutes.PEM.Emendamenti.GetTitoliMissioni)]
        public async Task<IHttpActionResult> GetTitoliMissioni()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetTitoliMissioni());
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
        [Route(ApiRoutes.PEM.Emendamenti.GetStati)]
        public async Task<IHttpActionResult> GetStatiEM()
        {
            try
            {
                return Ok(await _emendamentiLogic.GetStatiEM());
            }
            catch (Exception e)
            {
                Log.Error("GetStatiEM", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per accodare una stampa
        /// </summary>
        /// <param name="model">Modello specifico per richiesta stampa</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.PEM.InserisciStampaDifferita)]
        public async Task<IHttpActionResult> InserisciStampaDifferita(BaseRequest<EmendamentiDto, StampaDto> model)
        {
            try
            {
                var result = await _stampeLogic.InserisciStampa(model, CurrentUser);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("InserisciStampaDifferita", e);
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
        public EmendamentiController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
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