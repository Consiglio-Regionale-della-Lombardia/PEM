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
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
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
    ///     Controller per gestire il modulo DASI
    /// </summary>
    [Authorize]
    public class DASIController : BaseApiController
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
        public DASIController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
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
        ///     Endpoint che restituisce il modello di Atto di Sindacato Ispettivo da creare
        /// </summary>
        /// <param name="tipo">Tipo di atto</param>
        /// <returns></returns>
        [Route(ApiRoutes.DASI.GetNuovoModello)]
        public async Task<IHttpActionResult> GetNuovoModello(TipoAttoEnum tipo)
        {
            try
            {
                var result = await _dasiLogic.NuovoModello(tipo, CurrentUser);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("GetNuovoModello", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere l'oggetto atto da modificare
        /// </summary>
        /// <param name="id">Identificativo atto</param>
        /// <returns></returns>
        [Route(ApiRoutes.DASI.GetModificaModello)]
        public async Task<IHttpActionResult> GetModificaModello(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();
                var currentUser = CurrentUser;
                if (atto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA && !currentUser.IsSegreteriaAssemblea)
                    return NotFound();

                var countFirme = await _attiFirmeLogic.CountFirme(id);
                if (countFirme > 1 && (atto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA && !currentUser.IsSegreteriaAssemblea))
                    throw new InvalidOperationException(
                        $"Non è possibile modificare l'atto. Ci sono ancora {countFirme} firme attive.");

                return Ok(await _dasiLogic.ModificaModello(atto, currentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetModificaModello", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per salvare un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save)]
        public async Task<IHttpActionResult> Salva(AttoDASIDto request)
        {
            try
            {
                var nuovoAtto = await _dasiLogic.Salva(request, CurrentUser);

                return Created(new Uri(Request.RequestUri.ToString()), Mapper.Map<ATTI_DASI, AttoDASIDto>(nuovoAtto));
            }
            catch (Exception e)
            {
                Log.Error("Salva Atto DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per salvare le informazioni generali di un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_GeneralInfos)]
        public async Task<IHttpActionResult> Salva_InformazioniGenerali(AttoDASI_InformazioniGeneraliDto request)
        {
            try
            {
                await _dasiLogic.Salva_InformazioniGenerali(request, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva informazioni generali DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere una risposta ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_AddAnswer)]
        public async Task<IHttpActionResult> Salva_NuovaRisposta(AttiRisposteDto request)
        {
            try
            {
                await _dasiLogic.Salva_NuovaRisposta(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva nuovo risposta DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per aggiungere una risposta ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_RemoveAnswer)]
        public async Task<IHttpActionResult> Salva_RimuoviRisposta(AttiRisposteDto request)
        {
            try
            {
                await _dasiLogic.Salva_RimuoviRisposta(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva rimuovi risposta DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere un abbinamento ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_AddReference)]
        public async Task<IHttpActionResult> Salva_NuovoAbbinamento(AttiAbbinamentoDto request)
        {
            try
            {
                await _dasiLogic.Salva_NuovoAbbinamento(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva nuovo abbinamento DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere un abbinamento ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_RemoveReference)]
        public async Task<IHttpActionResult> Salva_RimuoviAbbinamento(AttiAbbinamentoDto request)
        {
            try
            {
                await _dasiLogic.Salva_RimuoviAbbinamento(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva rimuovi abbinamento DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per salvare i dettagli di una risposta ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_DetailAnswer)]
        public async Task<IHttpActionResult> Salva_DettagliRisposta(AttiRisposteDto request)
        {
            try
            {
                await _dasiLogic.Salva_DettagliRisposta(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva rimuovi abbinamento DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per aggiungere un monitoraggio ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_AddMonitoring)]
        public async Task<IHttpActionResult> Salva_NuovoMonitoraggio(AttiRisposteDto request)
        {
            try
            {
                await _dasiLogic.Salva_NuovoMonitoraggio(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva nuovo monitoraggio DASI", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per rimuovere un monitoraggio ad un atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Save_RemoveMonitoring)]
        public async Task<IHttpActionResult> Salva_RimuoviMonitoraggio(AttiRisposteDto request)
        {
            try
            {
                await _dasiLogic.Salva_RimuoviMonitoraggio(request);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva rimuovi monitoraggio DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per salvare la bozza di un atto cartaceo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.SaveCartaceo)]
        public async Task<IHttpActionResult> SalvaCartaceo(AttoDASIDto request)
        {
            try
            {
                await _dasiLogic.SalvaCartaceo(request, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per cambiare la priorità di una firma
        /// </summary>
        /// <param name="firma"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.CambiaPrioritaFirma)]
        public async Task<IHttpActionResult> CambiaPrioritaFirma(AttiFirmeDto firma)
        {
            try
            {
                await _dasiLogic.CambiaPrioritaFirma(firma);
                return Ok();
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le inforazioni dell'atto archiviato
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.Get)]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            try
            {
                var currentUser = CurrentUser;
                var atto = await _dasiLogic.GetAttoDto(id, currentUser);

                // #711 Controllo se l'utente che sta richiedendo (consigliere/assessore) ha una notifica pendente.
                // In quel caso aggiorno il campo "Visto" nei destinatari della notifica
                await _notificheLogic.ControlloNotifiche(currentUser, atto.UIDAtto);

                return Ok(atto);
            }
            catch (Exception e)
            {
                Log.Error("Get Atto DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo filtrato di atti
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.GetAll)]
        public async Task<IHttpActionResult> Riepilogo(BaseRequest<AttoDASIDto> request)
        {
            try
            {
                return Ok(await _dasiLogic.Get(request, CurrentUser, Request.RequestUri));
            }
            catch (Exception e)
            {
                Log.Error("Riepilogo Atti DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo filtrato di atti
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.GetAll_SoloIds)]
        public async Task<IHttpActionResult> RiepilogoSoloGuids(BaseRequest<AttoDASIDto> request)
        {
            try
            {
                return Ok(await _dasiLogic.GetSoloIds(request, CurrentUser, Request.RequestUri));
            }
            catch (Exception e)
            {
                Log.Error("Riepilogo Atti DASI", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il riepilogo degli atti cartacei
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetAllCartacei)]
        public async Task<IHttpActionResult> RiepilogoCartacei()
        {
            try
            {
                var response = await _dasiLogic.GetCartacei();
                return Ok(response);
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per firmare un Atto di Sindacato Ispettivo
        /// </summary>
        /// <param name="firmaModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.Firma)]
        public async Task<IHttpActionResult> Firma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;
                var firmaUfficio = user.IsSegreteriaAssemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                        throw new InvalidOperationException("Pin inserito non valido");

                    return Ok(await _dasiLogic.Firma(firmaModel, user, null, true));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null) throw new InvalidOperationException("Pin non impostato");

                if (pinInDb.RichiediModificaPIN) throw new InvalidOperationException("E' richiesto il reset del pin");

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                    throw new InvalidOperationException("Pin inserito non valido");

                return Ok(await _dasiLogic.Firma(firmaModel, user, pinInDb));
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
        [Route(ApiRoutes.DASI.RitiroFirma)]
        public async Task<IHttpActionResult> RitiroFirma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;
                var firmaUfficio = user.IsSegreteriaAssemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                        throw new InvalidOperationException("Pin inserito non valido");

                    return Ok(await _dasiLogic.RitiroFirma(firmaModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null) throw new InvalidOperationException("Pin non impostato");

                if (pinInDb.RichiediModificaPIN) throw new InvalidOperationException("E' richiesto il reset del pin");

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                    throw new InvalidOperationException("Pin inserito non valido");

                return Ok(await _dasiLogic.RitiroFirma(firmaModel, user));
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
        [Route(ApiRoutes.DASI.EliminaFirma)]
        public async Task<IHttpActionResult> EliminaFirma(ComandiAzioneModel firmaModel)
        {
            try
            {
                var user = CurrentUser;

                var firmaUfficio = user.IsSegreteriaAssemblea;

                if (firmaUfficio)
                {
                    if (firmaModel.Pin != AppSettingsConfiguration.MasterPIN)
                        throw new InvalidOperationException("Pin inserito non valido");

                    return Ok(await _dasiLogic.EliminaFirma(firmaModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null) throw new InvalidOperationException("Pin non impostato");

                if (pinInDb.RichiediModificaPIN) throw new InvalidOperationException("E' richiesto il reset del pin");

                if (firmaModel.Pin != pinInDb.PIN_Decrypt)
                    throw new InvalidOperationException("Pin inserito non valido");

                return Ok(await _dasiLogic.EliminaFirma(firmaModel, user));
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
        [Route(ApiRoutes.DASI.Presenta)]
        public async Task<IHttpActionResult> Presenta(ComandiAzioneModel presentazioneModel)
        {
            try
            {
                if (ManagerLogic.BloccaPresentazione)
                    throw new InvalidOperationException(
                        "E' in corso un'altra operazione di deposito. Riprova tra qualche secondo.");

                var user = CurrentUser;
                var presentazioneUfficio = user.IsSegreteriaAssemblea;

                if (presentazioneUfficio)
                {
                    if (presentazioneModel.Pin != AppSettingsConfiguration.MasterPIN)
                        throw new InvalidOperationException("Pin inserito non valido");

                    return Ok(await _dasiLogic.Presenta(presentazioneModel, user));
                }

                var pinInDb = await _personeLogic.GetPin(user);
                if (pinInDb == null) throw new InvalidOperationException("Pin non impostato");

                if (pinInDb.RichiediModificaPIN) throw new InvalidOperationException("E' richiesto il reset del pin");

                if (presentazioneModel.Pin != pinInDb.PIN_Decrypt)
                    throw new InvalidOperationException("Pin inserito non valido");

                return Ok(await _dasiLogic.Presenta(presentazioneModel, user));
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
        [Route(ApiRoutes.DASI.GetFirmatari)]
        public async Task<IHttpActionResult> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                var result = await _attiFirmeLogic.GetFirme(atto, tipo);
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
        [Route(ApiRoutes.DASI.GetBody)]
        public async Task<IHttpActionResult> GetBody(GetBodyModel model)
        {
            try
            {
                var atto = await _dasiLogic.Get(model.Id);
                if (atto == null) return NotFound();

                var body = await _dasiLogic.GetBodyDASI(atto
                    , await _attiFirmeLogic.GetFirme(atto, FirmeTipoEnum.TUTTE)
                    , CurrentUser
                    , model.Template,
                    model.privacy);

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
        [Route(ApiRoutes.DASI.GetBodyCopertina)]
        public async Task<IHttpActionResult> GetBodyCopertina(ByQueryModel model)
        {
            try
            {
                var body = await _dasiLogic.GetCopertina(model);

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
        [AllowAnonymous]
        [HttpGet]
        [Route(ApiRoutes.DASI.DownloadDoc)]
        public async Task<IHttpActionResult> Download(string path)
        {
            try
            {
                var response = ResponseMessage(_dasiLogic.Download(path));

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
        [Route(ApiRoutes.DASI.Elimina)]
        public async Task<IHttpActionResult> Elimina(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                await _dasiLogic.Elimina(atto, CurrentUser);

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
        [Route(ApiRoutes.DASI.Ritira)]
        public async Task<IHttpActionResult> Ritira(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                if (atto.UIDSeduta.HasValue)
                {
                    var seduta = await _seduteLogic.GetSeduta(atto.UIDSeduta.Value);
                    if (DateTime.Now > seduta.Data_seduta)
                        throw new InvalidOperationException(
                            "Non è possibile ritirare l'atto durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                }

                await _dasiLogic.Ritira(atto, CurrentUser);

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
        [Route(ApiRoutes.DASI.ModificaStato)]
        public async Task<IHttpActionResult> ModificaStato(ModificaStatoAttoModel model)
        {
            try
            {
                return Ok(await _dasiLogic.ModificaStato(model));
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
        [Route(ApiRoutes.DASI.IscriviSeduta)]
        public async Task<IHttpActionResult> IscrizioneSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                if (model.UidSeduta == Guid.Empty) throw new InvalidOperationException($"Guid [{model.UidSeduta}]");

                await _dasiLogic.IscrizioneSeduta(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("IscrizioneSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per richiedere l'iscrizione ad una seduta futura
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.RichiediIscrizione)]
        public async Task<IHttpActionResult> RichiediIscrizione(RichiestaIscrizioneDASIModel model)
        {
            try
            {
                await _dasiLogic.RichiediIscrizione(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("RichiediIscrizione", e);
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
        [Route(ApiRoutes.DASI.RimuoviSeduta)]
        public async Task<IHttpActionResult> RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                await _dasiLogic.RimuoviSeduta(model);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("RimuoviSeduta", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere la richiesta di iscrizione ad una data seduta
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.RimuoviRichiestaIscrizione)]
        public async Task<IHttpActionResult> RimuoviRichiestaIscrizione(RichiestaIscrizioneDASIModel model)
        {
            try
            {
                await _dasiLogic.RimuoviRichiesta(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("RimuoviRichiestaIscrizione", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Controller per proporre l'urgenza della mozione
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.ProponiUrgenzaMozione)]
        public async Task<IHttpActionResult> ProponiUrgenzaMozione(PromuoviMozioneModel model)
        {
            try
            {
                await _dasiLogic.ProponiMozioneUrgente(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ProponiMozioneUrgente", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Controller per proporre l'abbinata ad una mozione presentata
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.ProponiMozioneAbbinata)]
        public async Task<IHttpActionResult> ProponiMozioneAbbinata(PromuoviMozioneModel model)
        {
            try
            {
                await _dasiLogic.ProponiMozioneAbbinata(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ProponiMozioneAbbinata", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Controller per riservare il contatore per la gestione manuale/cartacea dell'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route(ApiRoutes.DASI.PresentazioneCartacea)]
        public async Task<IHttpActionResult> PresentazioneCartacea(PresentazioneCartaceaModel model)
        {
            try
            {
                await _dasiLogic.RichiestaPresentazioneCartacea(model, CurrentUser);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("PresentazioneCartacea", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli invitati di un atto
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        [Route(ApiRoutes.DASI.GetInvitati)]
        public async Task<IHttpActionResult> GetInvitati(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                var result = await _dasiLogic.GetInvitati(atto);
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
        [Route(ApiRoutes.DASI.GetStati)]
        public IHttpActionResult GetStati()
        {
            try
            {
                return Ok(_dasiLogic.GetStati(CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetStati", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i tipi di mozioni
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetTipiMOZ)]
        public IHttpActionResult GetTipiMOZ()
        {
            try
            {
                return Ok(_dasiLogic.GetTipiMOZ());
            }
            catch (Exception e)
            {
                Log.Error("GetTipiMOZ", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i soggetti interrogabili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetSoggettiInterrogabili)]
        public async Task<IHttpActionResult> GetSoggettiInterrogabili()
        {
            try
            {
                return Ok(await _dasiLogic.GetSoggettiInterrogabili());
            }
            catch (Exception e)
            {
                Log.Error("GetSoggettiInterrogabili", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere le mozioni iscritte a sedute attive
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetMOZAbbinabili)]
        public async Task<IHttpActionResult> GetMOZAbbinabili()
        {
            try
            {
                return Ok(await _dasiLogic.GetMOZAbbinabili(CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetMOZAbbinabili", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere gli atti delle sedute attive (pdl, pda, ris,...) che servono all'inserimento e all'iscrizione
        ///     degli odg
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetAttiSeduteAttive)]
        public async Task<IHttpActionResult> GetAttiSeduteAttive()
        {
            try
            {
                return Ok(await _dasiLogic.GetAttiSeduteAttive(CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error("GetAttiSeduteAttive", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per modificare i metadati di un atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPut]
        [Route(ApiRoutes.DASI.AggiornaMetaDati)]
        public async Task<IHttpActionResult> ModificaMetaDati(AttoDASIDto model)
        {
            try
            {
                var atto = await _dasiLogic.Get(model.UIDAtto);

                if (atto == null) return NotFound();

                await _dasiLogic.ModificaMetaDati(model, atto, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("ModificaMetaDati", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per scaricare il file generato
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.StampaImmediata)]
        public async Task<IHttpActionResult> Download(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                var response = ResponseMessage(await _dasiLogic.DownloadPDFIstantaneo(atto, CurrentUser));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per scaricare il documento pdf dell'atto con il testo e l’oggetto modificati
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.StampaImmediataPrivacy)]
        public async Task<IHttpActionResult> DownloadWithPrivacy(Guid id)
        {
            try
            {
                var atto = await _dasiLogic.Get(id);
                if (atto == null) return NotFound();

                var response = ResponseMessage(await _dasiLogic.DownloadPDFIstantaneo(atto, CurrentUser, true));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Download", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per inviare l'atto al protocollo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route(ApiRoutes.DASI.InviaAlProtocollo)]
        public async Task<IHttpActionResult> InviaAlProtocollo(Guid id)
        {
            try
            {
                await _dasiLogic.InviaAlProtocollo(id);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Invio al protocollo", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per declassare una lista di mozioni e farle tornare ORDINARIE
        /// </summary>
        /// <param name="data">Lista di mozioni urgenti da declassare</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.DeclassaMozione)]
        public async Task<IHttpActionResult> DeclassaMozione(List<string> data)
        {
            try
            {
                await _dasiLogic.DeclassaMozione(data);
                return Ok();
            }
            catch (Exception e)
            {
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per accodare una stampa DASI
        /// </summary>
        /// <param name="model">Modello specifico per richiesta stampa</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.InserisciStampaDifferita)]
        public async Task<IHttpActionResult> InserisciStampaDifferitaDASI(BaseRequest<AttoDASIDto, StampaDto> model)
        {
            try
            {
                var result = await _stampeLogic.InserisciStampa(model, CurrentUser);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Inserisci stampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per accodare una stampa DASI massiva
        /// </summary>
        /// <param name="model">Modello specifico per richiesta stampa</param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.InserisciStampaMassiva)]
        public async Task<IHttpActionResult> InserisciStampaMassivaDASI(NuovaStampaRequest request)
        {
            try
            {
                var result = await _stampeLogic.InserisciStampa(request, CurrentUser);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Inserisci stampa", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per salvare un gruppo di filtri
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.SalvaFiltriPreferiti)]
        public async Task<IHttpActionResult> SalvaGruppoFiltri(FiltroPreferitoDto request)
        {
            try
            {
                await _dasiLogic.SalvaGruppoFiltri(request, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva gruppo di filtri", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere il proprio gruppo di filtri preferito
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetFiltriPreferiti)]
        public async Task<IHttpActionResult> GetGruppoFiltri()
        {
            try
            {
                var res = await _dasiLogic.GetGruppoFiltri(CurrentUser);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get filtri preferiti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un gruppo di filtri preferiti
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.DASI.EliminaFiltriPreferiti)]
        public async Task<IHttpActionResult> EliminaGruppoFiltri(string nomeFiltro)
        {
            try
            {
                await _dasiLogic.EliminaGruppoFiltri(nomeFiltro, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Elimina filtri preferiti", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per generare i report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.GeneraReport)]
        public async Task<IHttpActionResult> GeneraReport(ReportDto request)
        {
            try
            {
                var file = await _dasiLogic.GeneraReport(request, CurrentUser);

                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("Genera report", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per generare l'estratto degli atti in zip
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.GeneraZIP)]
        public async Task<IHttpActionResult> GeneraZIP(ReportDto request)
        {
            try
            {
                var file = await _dasiLogic.GeneraZip(request, CurrentUser);

                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("Genera report", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per salvare un report
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.DASI.SalvaReport)]
        public async Task<IHttpActionResult> SalvaReport(ReportDto report)
        {
            try
            {
                await _dasiLogic.SalvaReport(report, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Salva report", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere i report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetReports)]
        public async Task<IHttpActionResult> GetReports()
        {
            try
            {
                var res = await _dasiLogic.GetReports(CurrentUser);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get reports", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per eliminare un report
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.DASI.EliminaReport)]
        public async Task<IHttpActionResult> EliminaReport(string nomeReport)
        {
            try
            {
                await _dasiLogic.EliminaReport(nomeReport, CurrentUser);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Elimina report", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti gli abbinamenti per una data legislatura
        /// </summary>
        /// <param name="legislaturaId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetAbbinamentiDisponibili)]
        public async Task<IHttpActionResult> GetAbbinamentiDisponibili(int legislaturaId, int page, int size)
        {
            try
            {
                var res = await _dasiLogic.GetAbbinamentiDisponibili(legislaturaId, page, size);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get abbinamenti disponibili", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per avere tutti i gruppi per una data legislatura
        /// </summary>
        /// <param name="legislaturaId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetGruppiDisponibili)]
        public async Task<IHttpActionResult> GetGruppiDisponibili(int legislaturaId, int page, int size)
        {
            try
            {
                var res = await _dasiLogic.GetGruppiDisponibili(legislaturaId, page, size);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get gruppi disponibili", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere tutti gli organi per una data legislatura
        /// </summary>
        /// <param name="legislaturaId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetOrganiDisponibili)]
        public async Task<IHttpActionResult> GetOrganiDisponibili(int legislaturaId)
        {
            try
            {
                var res = await _dasiLogic.GetOrganiDisponibili(legislaturaId);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get abbinamenti disponibili", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere le copertine disponibili da apporre ai report
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetReportsCovers)]
        public async Task<IHttpActionResult> GetReportsCovers()
        {
            try
            {
                var res = await _unitOfWork.Templates.GetAllByType(TemplateTypeEnum.REPORT_COVER);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get copertine disponibili", e);
                return ErrorHandler(e);
            }
        }
        
        /// <summary>
        ///     Endpoint per avere i template delle card disponibili
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.DASI.GetReportsCardTemplates)]
        public async Task<IHttpActionResult> GetReportsCardTemplates()
        {
            try
            {
                var itemCardsList = await _unitOfWork.Templates.GetAllByType(TemplateTypeEnum.REPORT_ITEM_CARD);
                var itemGridList = await _unitOfWork.Templates.GetAllByType(TemplateTypeEnum.REPORT_ITEM_GRID);
                var res = new List<TEMPLATES>();
                res.AddRange(itemCardsList);
                res.AddRange(itemGridList);

                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Get templates cards disponibili", e);
                return ErrorHandler(e);
            }
        }
    }
}