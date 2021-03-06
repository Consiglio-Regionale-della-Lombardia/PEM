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
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli atti
    /// </summary>
    [Authorize]
    [RoutePrefix("atti")]
    public class AttiController : BaseApiController
    {
        private readonly AttiLogic _logic;
        private readonly PersoneLogic _logicPersone;
        private readonly SeduteLogic _logicSedute;
        private readonly StampeLogic _logicStampe;

        public AttiController(PersoneLogic logicPersone, SeduteLogic logicSedute, AttiLogic logic,
            StampeLogic logicStampe)
        {
            _logicPersone = logicPersone;
            _logicSedute = logicSedute;
            _logic = logic;
            _logicStampe = logicStampe;
        }

        /// <summary>
        ///     Endpoint per scaricare il file testo pdf dell'atto
        /// </summary>
        /// <param name="path">Percorso file</param>
        /// <returns></returns>
        [HttpGet]
        [Route("file")]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(await _logic.Download(path));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("DownloadStampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti gli atti appartenenti ad una seduta
        /// </summary>
        /// <param name="model">Modello richiesta generica con paginazione</param>
        /// <returns></returns>
        [HttpPost]
        [Route("view")]
        public async Task<IHttpActionResult> GetAtti(BaseRequest<AttiDto> model)
        {
            try
            {
                if (model.id == Guid.Empty) return BadRequest();

                var sedutaInDb = await _logicSedute.GetSeduta(model.id);
                if (sedutaInDb == null)
                    return BadRequest("Seduta non valida");

                object CLIENT_MODE;
                model.param.TryGetValue("CLIENT_MODE", out CLIENT_MODE); // per trattazione aula
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var result = await _logic.GetAtti(model, Convert.ToInt16(CLIENT_MODE), persona,
                    Request.RequestUri);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetAtti", e);
                return BadRequest(e.Message);
            }
        }
        
        /// <summary>
        /// Endpoint per avere un singolo atto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAtto(Guid id)
        {
            try
            {
                var atto = await _logic.GetAtto(id);
                var result = Mapper.Map<ATTI, AttiDto>(atto);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetAtto", e);
                return BadRequest(e.Message);
            }
        }
        
        /// <summary>
        ///     Endpoint per eliminare virtualmente un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteAtto(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest();

                var attoInDb = await _logic.GetAtto(id);

                if (attoInDb == null)
                    return NotFound();

                await _logic.DeleteAtto(attoInDb);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("DeleteAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere un atto in database
        /// </summary>
        /// <param name="attoModel">Modello atto da inserire</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> NuovaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                if (string.IsNullOrEmpty(attoModel.NAtto))
                    return BadRequest("Imposta il numero di atto");
                if (string.IsNullOrEmpty(attoModel.Oggetto))
                    return BadRequest("Imposta l'oggetto");
                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    return BadRequest("Impossibile settare una data di chiusura inferiore alla data di apertura");

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var atto = Mapper.Map<AttiFormUpdateModel, ATTI>(attoModel);
                var nuovoAtto = await _logic.NuovoAtto(atto, persona);
                return Created(new Uri(Request.RequestUri.ToString()), Mapper.Map<ATTI, AttiDto>(nuovoAtto));
            }
            catch (Exception e)
            {
                Log.Error("NuovaAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare un atto
        /// </summary>
        /// <param name="attoModel">Modello atto da modificare</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("modifica")]
        [HttpPut]
        public async Task<IHttpActionResult> ModificaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(attoModel.UIDAtto);

                if (attoInDb == null)
                    return NotFound();

                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    return BadRequest("Impossibile settare una data di chiusura inferiore alla data di apertura");

                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                await _logic.SalvaAtto(attoInDb, attoModel, persona);

                return Ok(Mapper.Map<ATTI, AttiDto>(attoInDb));
            }
            catch (Exception e)
            {
                Log.Error("ModificaAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare i fascicoli di un atto
        /// </summary>
        /// <param name="atto">Modello atto da modificare</param>
        /// <returns></returns>
        [HttpPut]
        [Route("fascicoli")]
        public async Task<IHttpActionResult> ModificaFilesAtto(AttiDto atto)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(atto.UIDAtto);

                if (attoInDb == null)
                    return NotFound();

                await _logic.ModificaFascicoli(attoInDb, atto);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Modifica Fascicoli Atto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti gli articoli di un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Route("articoli")]
        public async Task<IHttpActionResult> GetArticoli(Guid id)
        {
            try
            {
                var articoliDtos = await _logic.GetArticoli(id);

                return Ok(articoliDtos);
            }
            catch (Exception e)
            {
                Log.Error("GetArticoli", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per creare articoli in un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="articoli">range articoli</param>
        /// <returns></returns>
        [Route("crea-articoli")]
        [HttpGet]
        public async Task<IHttpActionResult> CreaArticoli(Guid id, string articoli)
        {
            try
            {
                await _logic.CreaArticoli(id, articoli);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("CreaArticoli", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un articolo
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("elimina-articolo")]
        [HttpDelete]
        public async Task<IHttpActionResult> EliminaArticolo(Guid id)
        {
            try
            {
                var articolo = await _logic.GetArticolo(id);
                if (articolo == null)
                    return NotFound();

                await _logic.DeleteArticolo(articolo);

                var listCommi = await _logic.GetCommi(id);
                await _logic.DeleteCommi(listCommi);

                foreach (var comma in listCommi)
                {
                    var listLettere = await _logic.GetLettere(comma.UIDComma);
                    await _logic.DeleteLettere(listLettere);
                }

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("EliminaArticolo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i commi di un articolo
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("commi")]
        public async Task<IHttpActionResult> GetCommi(Guid id)
        {
            try
            {
                var commiDtos = (await _logic.GetCommi(id)).Select(Mapper.Map<COMMI, CommiDto>);

                return Ok(commiDtos);
            }
            catch (Exception e)
            {
                Log.Error("GetCommi", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per creare commi nell'articolo
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <param name="commi">range commi</param>
        /// <returns></returns>
        [Route("crea-commi")]
        [HttpGet]
        public async Task<IHttpActionResult> CreaCommi(Guid id, string commi)
        {
            try
            {
                var articolo = await _logic.GetArticolo(id);
                if (articolo == null)
                    return NotFound();

                await _logic.CreaCommi(articolo, commi);
                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("CreaCommi", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un comma
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <returns></returns>
        [Route("elimina-comma")]
        [HttpDelete]
        public async Task<IHttpActionResult> EliminaComma(Guid id)
        {
            try
            {
                var comma = await _logic.GetComma(id);
                if (comma == null)
                    return NotFound();

                await _logic.DeleteComma(comma);

                var listLettere = await _logic.GetLettere(comma.UIDComma);
                await _logic.DeleteLettere(listLettere);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("EliminaComma", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutte le lettere di un comma
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <returns></returns>
        [Route("lettere")]
        public async Task<IHttpActionResult> GetLettere(Guid id)
        {
            try
            {
                var lettereDtos = (await _logic.GetLettere(id)).Select(Mapper.Map<LETTERE, LettereDto>);

                return Ok(lettereDtos);
            }
            catch (Exception e)
            {
                Log.Error("GetLettere", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per creare lettere in un comma
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <param name="lettere">range lettere</param>
        /// <returns></returns>
        [Route("crea-lettere")]
        [HttpGet]
        public async Task<IHttpActionResult> CreaLettere(Guid id, string lettere)
        {
            try
            {
                var comma = await _logic.GetComma(id);
                if (comma == null)
                    return NotFound();

                await _logic.CreaLettere(comma, lettere);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("CreaLettere", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare una lettera
        /// </summary>
        /// <param name="id">Guid lettera</param>
        /// <returns></returns>
        [Route("elimina-lettera")]
        [HttpDelete]
        public async Task<IHttpActionResult> EliminaLettera(Guid id)
        {
            try
            {
                var lettera = await _logic.GetLettera(id);
                if (lettera == null)
                    return NotFound();

                await _logic.DeleteLettere(lettera);

                return Ok("OK");
            }
            catch (Exception e)
            {
                Log.Error("EliminaLettera", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per salvare i relatori di un atto
        /// </summary>
        /// <param name="model">Modello richiesta salvataggio relatori</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("relatori")]
        [HttpPost]
        public async Task<IHttpActionResult> SalvaRelatoriAtto(AttoRelatoriModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null)
                    return NotFound();

                await _logic.SalvaRelatori(model.Id, model.Persone);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SalvaRelatoriAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per la pubblicazione del fascicolo Presentazione/Votazione
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("abilita-fascicolazione")]
        [HttpPost]
        public async Task<IHttpActionResult> PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null)
                    return NotFound();
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);
                await _logic.PubblicaFascicolo(attoInDb, model, persona);

                if (model.Abilita)
                    await _logicStampe.InserisciStampa(new BaseRequest<EmendamentiDto, StampaDto>
                    {
                        entity = new StampaDto
                        {
                            UIDAtto = model.Id,
                            Da = 0,
                            A = 0,
                            Ordine = (int) model.Ordinamento
                        }
                    }, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("PubblicaFascicolo", e);
                return ErrorHandler(e);
            }
        }
    }
}