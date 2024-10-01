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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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
using PortaleRegione.DTO.Routes;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per gestire gli atti
    /// </summary>
    [Authorize]
    public class AttiController : BaseApiController
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
        public AttiController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic,
            StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic,
            notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint per scaricare il file testo pdf dell'atto
        /// </summary>
        /// <param name="path">Percorso file</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.DownloadDoc)]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(await _attiLogic.Download(path));

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
        [Route(ApiRoutes.PEM.Atti.GetAll)]
        public async Task<IHttpActionResult> GetAtti(BaseRequest<AttiDto> model)
        {
            try
            {
                if (model.id == Guid.Empty) throw new InvalidOperationException();

                var sedutaInDb = await _seduteLogic.GetSeduta(model.id);
                if (sedutaInDb == null) throw new InvalidOperationException("Seduta non valida");

                model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula

                var result = await _attiLogic.GetAtti(model
                    , Convert.ToInt16(CLIENT_MODE)
                    , CurrentUser
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
        [Route(ApiRoutes.PEM.Atti.Get)]
        public async Task<IHttpActionResult> GetAtto(Guid id)
        {
            try
            {
                var atto = await _attiLogic.GetAtto(id);
                var result = Mapper.Map<ATTI, AttiDto>(atto);
                result.Relatori = await _attiLogic.GetRelatori(atto.UIDAtto);
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
        [Route(ApiRoutes.PEM.Atti.Delete)]
        public async Task<IHttpActionResult> DeleteAtto(Guid id)
        {
            try
            {
                if (id == Guid.Empty) throw new InvalidOperationException();

                var attoInDb = await _attiLogic.GetAtto(id);

                if (attoInDb == null) return NotFound();

                await _attiLogic.DeleteAtto(attoInDb);

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
        [Route(ApiRoutes.PEM.Atti.Create)]
        public async Task<IHttpActionResult> NuovaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                if (string.IsNullOrEmpty(attoModel.NAtto) && attoModel.IDTipoAtto != (int)TipoAttoEnum.ALTRO)
                    throw new InvalidOperationException("Imposta il numero di atto");

                if (string.IsNullOrEmpty(attoModel.Oggetto)) throw new InvalidOperationException("Imposta l'oggetto");

                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    throw new InvalidOperationException(
                        "Impossibile settare una data di chiusura inferiore alla data di apertura");

                var nuovoAtto = await _attiLogic.NuovoAtto(attoModel, CurrentUser);
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
        [HttpPut]
        [Route(ApiRoutes.PEM.Atti.Edit)]
        public async Task<IHttpActionResult> ModificaAtto(AttiFormUpdateModel attoModel)
        {
            try
            {
                var attoInDb = await _attiLogic.GetAtto(attoModel.UIDAtto);

                if (attoInDb == null) return NotFound();

                if (attoModel.Data_chiusura <= attoModel.Data_apertura)
                    throw new InvalidOperationException(
                        "Impossibile settare una data di chiusura inferiore alla data di apertura");

                await _attiLogic.SalvaAtto(attoInDb, attoModel, CurrentUser);

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
        [Route(ApiRoutes.PEM.Atti.ModificaFascicoli)]
        public async Task<IHttpActionResult> ModificaFilesAtto(AttiDto atto)
        {
            try
            {
                var attoInDb = await _attiLogic.GetAtto(atto.UIDAtto);

                if (attoInDb == null) return NotFound();

                await _attiLogic.ModificaFascicoli(attoInDb, atto);

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
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Articoli.GetAll)]
        public async Task<IHttpActionResult> GetArticoli(Guid id)
        {
            try
            {
                var articoliDtos = await _attiLogic.GetArticoli(id);

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
        /// <param name="id">Guid atto</param>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.GrigliaTesti)]
        public async Task<IHttpActionResult> GetGrigliaTesto(Guid id, bool view = false)
        {
            try
            {
                var result = await _attiLogic.GetGrigliaTesto(id, view);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetGrigliaTesto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per ottenere la lista degli emendamenti di un atto per essere ordinati
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.GrigliaOrdinamento)]
        public async Task<IHttpActionResult> GetGrigliaOrdinamento(Guid id)
        {
            try
            {
                var result = await _attiLogic.GetGrigliaOrdinamento(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per creare articoli in un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="articoli">range articoli</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Articoli.Create)]
        public async Task<IHttpActionResult> CreaArticoli(Guid id, string articoli)
        {
            try
            {
                await _attiLogic.CreaArticoli(id, articoli);

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
        [HttpDelete]
        [Route(ApiRoutes.PEM.Atti.Articoli.Delete)]
        public async Task<IHttpActionResult> EliminaArticolo(Guid id)
        {
            try
            {
                var articolo = await _attiLogic.GetArticolo(id);
                if (articolo == null) return NotFound();

                await _attiLogic.DeleteArticolo(articolo);

                var listCommi = await _attiLogic.GetCommi(id);
                await _attiLogic.DeleteCommi(listCommi);

                foreach (var comma in listCommi)
                {
                    var listLettere = await _attiLogic.GetLettere(comma.UIDComma);
                    await _attiLogic.DeleteLettere(listLettere);
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
        /// <param name="expanded"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Commi.GetAll)]
        public async Task<IHttpActionResult> GetCommi(Guid id, bool expanded = false)
        {
            try
            {
                var commiDtos = (await _attiLogic.GetCommi(id, expanded)).Select(Mapper.Map<COMMI, CommiDto>);
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
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Commi.Create)]
        public async Task<IHttpActionResult> CreaCommi(Guid id, string commi)
        {
            try
            {
                var articolo = await _attiLogic.GetArticolo(id);
                if (articolo == null) return NotFound();

                await _attiLogic.CreaCommi(articolo, commi);
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
        [HttpDelete]
        [Route(ApiRoutes.PEM.Atti.Commi.Delete)]
        public async Task<IHttpActionResult> EliminaComma(Guid id)
        {
            try
            {
                var comma = await _attiLogic.GetComma(id);
                if (comma == null) return NotFound();

                await _attiLogic.DeleteComma(comma);

                var listLettere = await _attiLogic.GetLettere(comma.UIDComma);
                await _attiLogic.DeleteLettere(listLettere);

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
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Lettere.GetAll)]
        public async Task<IHttpActionResult> GetLettere(Guid id)
        {
            try
            {
                var lettereDtos = (await _attiLogic.GetLettere(id)).Select(Mapper.Map<LETTERE, LettereDto>);

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
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.Lettere.Create)]
        public async Task<IHttpActionResult> CreaLettere(Guid id, string lettere)
        {
            try
            {
                var comma = await _attiLogic.GetComma(id);
                if (comma == null) return NotFound();

                await _attiLogic.CreaLettere(comma, lettere);

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
        [HttpDelete]
        [Route(ApiRoutes.PEM.Atti.Lettere.Delete)]
        public async Task<IHttpActionResult> EliminaLettera(Guid id)
        {
            try
            {
                var lettera = await _attiLogic.GetLettera(id);
                if (lettera == null) return NotFound();

                await _attiLogic.DeleteLettere(lettera);

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
        [HttpPost]
        [Route(ApiRoutes.PEM.Atti.AggiornaRelatori)]
        public async Task<IHttpActionResult> SalvaRelatoriAtto(AttoRelatoriModel model)
        {
            try
            {
                var attoInDb = await _attiLogic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();

                await _attiLogic.SalvaRelatori(model.Id, model.Persone);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SalvaRelatoriAtto", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Salva testo dell'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route(ApiRoutes.PEM.Atti.AggiornaTesto)]
        public async Task<IHttpActionResult> SalvaTesto(TestoAttoModel model)
        {
            try
            {
                if (model == null) throw new Exception("Invalid object");

                await _attiLogic.SalvaTesto(model);
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
        [HttpPost]
        [Route(ApiRoutes.PEM.Atti.AbilitaFascicolo)]
        public async Task<IHttpActionResult> PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var user = CurrentUser;
                var attoInDb = await _attiLogic.GetAtto(model.Id);

                if (attoInDb == null) return NotFound();

                await _attiLogic.PubblicaFascicolo(attoInDb, model, user);

                if (model.Abilita)
                {
                    var request = new BaseRequest<EmendamentiDto>
                    {
                        id = model.Id,
                        page = 1,
                        size = 100000,
                        ordine = model.Ordinamento,
                        filtro = new List<FilterStatement<EmendamentiDto>>()
                        {
                            new FilterStatement<EmendamentiDto>()
                            {
                                Operation = Operation.EqualTo,
                                Value = model.Id,
                                PropertyId = nameof(EmendamentiDto.UIDAtto)
                            }
                        }
                    };

                    var list = new List<Guid>();
                    list = await _emendamentiLogic.GetEmendamentiSoloIds(request, CurrentUser, model.ClientMode);

                    await _stampeLogic.InserisciStampa(new NuovaStampaRequest()
                    {
                        Da = 0,
                        A = 0,
                        Lista = list,
                        Modulo = ModuloStampaEnum.PEM,
                        Ordinamento = model.Ordinamento,
                        UIDAtto = model.Id
                    }, CurrentUser);
                }

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
        [Route(ApiRoutes.PEM.Atti.SpostaUp)]
        public async Task<IHttpActionResult> SPOSTA_UP(Guid id)
        {
            try
            {
                await _attiLogic.SPOSTA_UP(id);

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
        [Route(ApiRoutes.PEM.Atti.SpostaDown)]
        public async Task<IHttpActionResult> SPOSTA_DOWN(Guid id)
        {
            try
            {
                await _attiLogic.SPOSTA_DOWN(id);

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
        [HttpPost]
        [Route(ApiRoutes.PEM.Atti.BloccoODG)]
        public async Task<IHttpActionResult> BloccoODG(BloccoODGModel model)
        {
            try
            {
                var attoInDb = await _attiLogic.GetAtto(model.Id);

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
        [HttpPost]
        [Route(ApiRoutes.PEM.Atti.JollyODG)]
        public async Task<IHttpActionResult> JollyODG(JollyODGModel model)
        {
            try
            {
                var attoInDb = await _attiLogic.GetAtto(model.Id);

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
        [Route(ApiRoutes.PEM.Atti.GetTipi)]
        public IHttpActionResult GetTipi(bool dasi = true)
        {
            try
            {
                return Ok(_attiLogic.GetTipi(dasi));
            }
            catch (Exception e)
            {
                Log.Error("GetTipi", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per spostare un atto in un altra seduta
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.PEM.Atti.SpostaInAltraSeduta)]
        public async Task<IHttpActionResult> SpostaInAltraSeduta(Guid uidAtto, Guid uidSeduta)
        {
            try
            {
                await _attiLogic.SpostaInAltraSeduta(uidAtto, uidSeduta);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("SpostaInAltraSeduta", e);
                return ErrorHandler(e);
            }
        }
    }
}