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
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

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
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="logicPersone"></param>
        /// <param name="logicSedute"></param>
        /// <param name="logic"></param>
        /// <param name="logicStampe"></param>
        public AttiController(IUnitOfWork unitOfWork,
            PersoneLogic logicPersone,
            SeduteLogic logicSedute,
            AttiLogic logic,
            StampeLogic logicStampe)
        {
            _unitOfWork = unitOfWork;
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
                if (model.id == Guid.Empty) throw new InvalidOperationException();

                var sedutaInDb = await _logicSedute.GetSeduta(model.id);
                if (sedutaInDb == null) throw new InvalidOperationException("Seduta non valida");

                object CLIENT_MODE;
                model.param.TryGetValue("CLIENT_MODE", out CLIENT_MODE); // per trattazione aula
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var result = await _logic.GetAtti(model
                    , Convert.ToInt16(CLIENT_MODE)
                    , persona, personeInDbLight
                    , Request.RequestUri);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetAtti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere un singolo atto
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
                result.Relatori = await _logic.GetRelatori(atto.UIDAtto);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare virtualmente un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteAtto(Guid id)
        {
            try
            {
                if (id == Guid.Empty) throw new InvalidOperationException();

                var attoInDb = await _logic.GetAtto(id);

                if (attoInDb == null) return NotFound();

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
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> NuovaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                if (string.IsNullOrEmpty(attoModel.NAtto) && attoModel.IDTipoAtto != (int)TipoAttoEnum.ALTRO) throw new InvalidOperationException("Imposta il numero di atto");

                if (string.IsNullOrEmpty(attoModel.Oggetto)) throw new InvalidOperationException("Imposta l'oggetto");

                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    throw new InvalidOperationException("Impossibile settare una data di chiusura inferiore alla data di apertura");

                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);
                var nuovoAtto = await _logic.NuovoAtto(attoModel, persona);
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
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("modifica")]
        [HttpPut]
        public async Task<IHttpActionResult> ModificaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(attoModel.UIDAtto);

                if (attoInDb == null) return NotFound();

                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    throw new InvalidOperationException("Impossibile settare una data di chiusura inferiore alla data di apertura");

                var session = GetSession();
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

                if (attoInDb == null) return NotFound();

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
        ///     Endpoint per avere tutti gli articoli/commi/lettere
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("griglia-testi")]
        public async Task<IHttpActionResult> GetGrigliaTesto(Guid id, bool viewEm = false)
        {
            try
            {
                var result = await _logic.GetGrigliaTesto(id, viewEm);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetGrigliaTesto", e);
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
                if (articolo == null) return NotFound();

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
        public async Task<IHttpActionResult> GetCommi(Guid id, bool expanded = false)
        {
            try
            {
                var commiDtos = (await _logic.GetCommi(id, expanded)).Select(Mapper.Map<COMMI, CommiDto>);
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
                if (articolo == null) return NotFound();

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
                if (comma == null) return NotFound();

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
                if (comma == null) return NotFound();

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
                if (lettera == null) return NotFound();

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
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("relatori")]
        [HttpPost]
        public async Task<IHttpActionResult> SalvaRelatoriAtto(AttoRelatoriModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();

                await _logic.SalvaRelatori(model.Id, model.Persone);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SalvaRelatoriAtto", e);
                return ErrorHandler(e);
            }
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("salva-testo")]
        [HttpPost]
        public async Task<IHttpActionResult> SalvaTesto(TestoAttoModel model)
        {
            try
            {
                if (model == null)
                {
                    throw new Exception("Invalid object");
                }

                await _logic.SalvaTesto(model);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SalvaTesto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per la pubblicazione del fascicolo Presentazione/Votazione
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("abilita-fascicolazione")]
        [HttpPost]
        public async Task<IHttpActionResult> PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();

                var session = GetSession();
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
                            Ordine = (int)model.Ordinamento
                        },
                        ordine = model.Ordinamento
                    }, persona);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("PubblicaFascicolo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare avanti un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("sposta-up")]
        public async Task<IHttpActionResult> SPOSTA_UP(Guid id)
        {
            try
            {
                await _logic.SPOSTA_UP(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SPOSTA_UP", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare indietro un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("sposta-down")]
        public async Task<IHttpActionResult> SPOSTA_DOWN(Guid id)
        {
            try
            {
                await _logic.SPOSTA_DOWN(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SPOSTA_DOWN", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Controller per bloccare la presentazione degli ordini del giorno per l'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("bloccoODG")]
        [HttpPost]
        public async Task<IHttpActionResult> BloccoODG(BloccoODGModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();
                attoInDb.BloccoODG = Convert.ToBoolean(model.Blocco);
                await _unitOfWork.CompleteAsync();

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("BloccoODG", e);
                return ErrorHandler(e);
            }

        }

        /// <summary>
        ///     Controller per bloccare la presentazione degli ordini del giorno per l'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("jollyODG")]
        [HttpPost]
        public async Task<IHttpActionResult> JollyODG(JollyODGModel model)
        {
            try
            {
                var attoInDb = await _logic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();
                attoInDb.Jolly = Convert.ToBoolean(model.Jolly);
                await _unitOfWork.CompleteAsync();

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("JollyODG", e);
                return ErrorHandler(e);
            }

        }

        /// <summary>
        ///     Endpoint per avere i tipi
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tipi")]
        public async Task<IHttpActionResult> GetTipi(bool dasi = true)
        {
            try
            {
                return Ok(_logic.GetTipi(dasi));
            }
            catch (Exception e)
            {
                Log.Error("GetTipi", e);
                return ErrorHandler(e);
            }
        }
    }
}