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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using Utility = PortaleRegione.Common.Utility;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller per la gestione degli Atti di Sindacato Ispettivo
    /// </summary>
    [Authorize]
    [RoutePrefix("dasi")]
    public class DASIController : BaseController
    {
        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base al ruolo dell'utente loggato
        /// </summary>
        /// <returns></returns>
        [Route("riepilogo")]
        public async Task<ActionResult> RiepilogoDASI()
        {
            var currentUser = CurrentUser;
            if (CanAccess(new List<RuoliIntEnum>
                {
                    RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea,
                    RuoliIntEnum.Segreteria_Assemblea_Read
                }))
            {
                return View("RiepilogoDASI_Admin", new RiepilogoDASIModel
                {
                    CurrentUser = currentUser
                });
            }

            return View("RiepilogoDASI", new RiepilogoDASIModel
            {
                CurrentUser = currentUser
            });
        }

        // #1636 - Contatore degli atti per i quali e' richiesta la firma dell'utente: alimenta il
        // numero "(n)" mostrato dentro la spunta "Visualizza solo gli atti per i quali e' richiesta
        // la mia firma" in area consiglieri. Chiamata una sola volta all'apertura del riepilogo
        // (poi il client aggiorna il numero dopo le firme o dalla paginazione col flag attivo).
        [HttpGet]
        [Route("contatore-firme")]
        public async Task<ActionResult> ContatoreFirme()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.ContatoreFirme();

                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("riepilogoUOLA")]
        public async Task<ActionResult> Riepilogo(FilterRequest model)
        {
            try
            {
                if (!model.filters.Any())
                    return Json(new RiepilogoDASIModel { CurrentUser = CurrentUser });

                var request = BuildDasiRequest(model);

                var apiGateway = new ApiGateway(Token);

                var res = await apiGateway.DASI.Get(request);
                res.CurrentUser = CurrentUser;

                return Json(res);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("riepilogoSoloIds")]
        public async Task<ActionResult> RiepilogoSoloIds(FilterRequest model)
        {
            try
            {
                if (!model.filters.Any())
                    return Json(new RiepilogoDASIModel { CurrentUser = CurrentUser });

                var request = BuildDasiRequest(model);

                var apiGateway = new ApiGateway(Token);

                var res = await apiGateway.DASI.GetSoloIds(request);
                return Json(res);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        // Le tre action salva-gruppo-filtri/elimina-gruppo-filtri/gruppo-filtri sono
        // state spostate nel nuovo FiltriController unificato (v2026.5.1).
        // Il client DASI ora invoca /filtri/salva, /filtri/{modulo}, /filtri/elimina.


        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo cartacei
        /// </summary>
        /// <returns></returns>
        [Route("cartacei")]
        public async Task<ActionResult> RiepilogoCartacei()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.DASI.GetCartacei(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base alla seduta
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("seduta")]
        public ActionResult RiepilogoDASI_BySeduta(Guid id, int tipo = (int)TipoAttoEnum.TUTTI,
            int page = 1, int size = 20, int view = (int)ViewModeEnum.GRID,
            int stato = (int)StatiAttoEnum.PRESENTATO, string uidAtto = "", int legislatura = 0)
        {
            CheckCacheClientMode(ClientModeEnum.TRATTAZIONE);
            var model = new RiepilogoDASIModel
            {
                ClientMode = ClientModeEnum.TRATTAZIONE,
                Tipo = (TipoAttoEnum)tipo,
                CurrentUser = CurrentUser
            };

            if (CanAccess(new List<RuoliIntEnum>
                    { RuoliIntEnum.Amministratore_PEM
                        , RuoliIntEnum.Segreteria_Assemblea
                        , RuoliIntEnum.Segreteria_Assemblea_Read }))
                return View("RiepilogoDASI_Admin", model);

            return View("RiepilogoDASI", model);
        }

        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base alla seduta in formato json
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("seduta-json")]
        public async Task<ActionResult> RiepilogoDASI_BySedutaJSON(Guid id, int tipo = (int)TipoAttoEnum.TUTTI)
        {
            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.DASI.GetBySeduta_Trattazione(id, (TipoAttoEnum)tipo, "", 1, 100);
            var items = model.Data.Results.Select(i => new KeyValueDto { sigla = i.Display, descr = i.OggettoView() })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per aggiungere un atto di sindacato ispettivo. Restituisce il modello dell'atto pre-compilato.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("new")]
        public async Task<ActionResult> Nuovo(int tipo)
        {
            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.DASI.GetNuovoModello((TipoAttoEnum)tipo);
            model.CurrentUser = CurrentUser;
            if (CurrentUser.IsSegreteriaPolitica)
                return View("DASIForm_Segreteria", model);

            return View("DASIForm", model);
        }

        /// <summary>
        ///     Endpoint per il salvataggio dell' atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> SalvaAtto(AttoDASIDto request)
        {
            try
            {
                if (request.DocAllegatoGenerico != null)
                {
                    // Leggi il contenuto
                    using (var memoryStream = new MemoryStream())
                    {
                        await request.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                        request.DocAllegatoGenerico_Stream = memoryStream.ToArray();
                    }

                    // AGGIUNGERE VALIDAZIONE:
                    var validationResult = FileValidator.ValidateFile(
                        request.DocAllegatoGenerico.FileName,
                        request.DocAllegatoGenerico.ContentType,
                        request.DocAllegatoGenerico_Stream
                    );

                    if (!validationResult.IsValid)
                    {
                        return Json(new ErrorResponse(validationResult.ErrorMessage), 
                            JsonRequestBehavior.AllowGet);
                    }

                    if (FileValidator.IsZipFile(request.DocAllegatoGenerico.FileName, 
                            request.DocAllegatoGenerico_Stream))
                    {
                        return Json(new ErrorResponse(
                                "File ZIP non consentiti per motivi di sicurezza."), 
                            JsonRequestBehavior.AllowGet);
                    }
                }
                
                var currentUser = CurrentUser;
                var apiGateway = new ApiGateway(Token);
                var result = await apiGateway.DASI.Salva(request);
                return Json(Url.Action("ViewAtto", "DASI", new
                {
                    id = result.UIDAtto,
                    name = currentUser.DisplayName,
                    codice_gruppo = currentUser.Gruppo.codice_gruppo
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per consultare l'atto per esteso
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:guid}")]
        public async Task<ActionResult> ViewAtto(Guid id)
        {
            try
            {
                var currentUser = CurrentUser;
                var apiGateway = new ApiGateway(Token);
                var atto = await apiGateway.DASI.Get(id);
                atto.BodyAtto = await apiGateway.DASI.GetBody(id, TemplateTypeEnum.HTML, true);

                var firme_ante = await apiGateway.DASI.GetFirmatari(id, FirmeTipoEnum.PRIMA_DEPOSITO);
                var firme_post = await apiGateway.DASI.GetFirmatari(id, FirmeTipoEnum.DOPO_DEPOSITO);
                atto.FirmeAnte = firme_ante.ToList();
                atto.FirmePost = firme_post.ToList();

                atto.Firme = await Helpers.Utility.GetFirmatariDASI(
                    atto.FirmeAnte,
                    currentUser.UID_persona,
                    FirmeTipoEnum.PRIMA_DEPOSITO,
                    Token);
                atto.Firme_dopo_deposito = await Helpers.Utility.GetFirmatariDASI(
                    atto.FirmePost,
                    currentUser.UID_persona,
                    FirmeTipoEnum.DOPO_DEPOSITO,
                    Token);

                if (!atto.IsChiuso)
                    atto.Destinatari =
                        await Helpers.Utility.GetDestinatariNotifica(await apiGateway.DASI.GetInvitati(id), Token);


                var result = new DASIFormModel
                {
                    CurrentUser = currentUser,
                    Atto = atto
                };
                if (atto.Tipo == (int)TipoAttoEnum.RIS)
                {
                    var consiglieriPublic =
                        await apiGateway.Persone.GetProponentiFirmatari(atto.Legislatura.ToString());

                    if (consiglieriPublic.Any())
                    {
                        result.ListaConsiglieriPublic = consiglieriPublic;
                    }
                }

                if (currentUser.IsSegreteriaAssemblea)
                {
                    return View("AttoDASIView_Admin", result);
                }

                return View("AttoDASIView", result);
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Propaga a Global.asax per redirect al login #1592
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Esegui azione su Atti di Sindacato Ispettivo selezionato
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="azione"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("azioni")]
        public async Task<ActionResult> EseguiAzione(Guid id, int azione, string pin = "")
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                switch ((ActionEnum)azione)
                {
                    case ActionEnum.ELIMINA:
                        await apiGateway.DASI.Elimina(id);
                        return Json(Url.Action("RiepilogoDASI", "DASI"), JsonRequestBehavior.AllowGet);
                    case ActionEnum.RITIRA:
                        await apiGateway.DASI.Ritira(id);
                        break;
                    case ActionEnum.FIRMA:
                        var resultFirma = await apiGateway.DASI.Firma(id, pin);
                        var listaErroriFirma = new List<string>();
                        foreach (var itemFirma in resultFirma)
                            listaErroriFirma.Add($"{itemFirma.Value}");
                        if (listaErroriFirma.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"{listaErroriFirma.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await apiGateway.DASI.Presenta(id, pin);
                        var listaErroriDeposito = new List<string>();
                        foreach (var itemDeposito in resultDeposita)
                            listaErroriDeposito.Add(
                                $"{itemDeposito.Value}");
                        if (listaErroriDeposito.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"{listaErroriDeposito.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(azione), azione, null);
                }

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Esegui azione su Atti di Sindacato Ispettivo selezionato
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("azioni-massive")]
        public async Task<ActionResult> EseguiAzioneMassive(ComandiAzioneModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                if (model.Tutti)
                {
                    // "Seleziona tutto": gli id vengono risolti dai filtri correnti inviati dal
                    // client (model.Filter), non piu' dalla Session. La lista model.Lista contiene
                    // gli atti deselezionati, che vengono sottratti dal totale.
                    if (model.Filter == null)
                        return Json(new ErrorResponse(
                            "Filtri correnti non disponibili. Aggiornare la pagina e riprovare."),
                            JsonRequestBehavior.AllowGet);

                    var request = BuildDasiRequest(model.Filter);
                    request.size = 99999;

                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                switch (model.Azione)
                {
                    case ActionEnum.FIRMA:
                        var resultFirma = await apiGateway.DASI.Firma(model);
                        var listaErroriFirma = new List<string>();
                        foreach (var itemFirma in resultFirma)
                        {
                            listaErroriFirma.Add($"{itemFirma.Value}");
                        }
                        if (listaErroriFirma.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"{listaErroriFirma.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);

                        return Json(
                            new
                            {
                                message =
                                    "Nessuna firma effettuata"
                            }, JsonRequestBehavior.AllowGet);
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await apiGateway.DASI.Presenta(model);
                        var listaErroriDeposito = new List<string>();
                        foreach (var itemDeposito in resultDeposita)
                        {
                            listaErroriDeposito.Add(
                                $"{itemDeposito.Value}");
                        }
                        if (listaErroriDeposito.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"{listaErroriDeposito.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);


                        return Json(
                            new
                            {
                                message =
                                    "Nessuna presentazione effettuata"
                            }, JsonRequestBehavior.AllowGet);

                    case ActionEnum.INVITA:
                        var resultInvita = await apiGateway.Notifiche.NotificaDASI(model);
                        var listaErroriInvita = new List<string>();
                        foreach (var itemInvito in resultInvita)
                            listaErroriInvita.Add(
                                $"{itemInvito.Value}");
                        if (listaErroriInvita.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"{listaErroriInvita.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);


                        return Json(
                            new
                            {
                                message =
                                    "Nessuna invito effettuato"
                            }, JsonRequestBehavior.AllowGet);
                    case ActionEnum.RITIRA:
                        throw new InvalidOperationException("Azione non abilitata");
                    case ActionEnum.ELIMINA:
                        throw new InvalidOperationException("Azione non abilitata");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model.Azione), model.Azione, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Ritira firma di un Atti di Sindacato Ispettivo
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="pin">pin</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ritiro-firma")]
        public async Task<ActionResult> RitiroFirma(Guid id, string pin = "")
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var resultRitiro = await apiGateway.DASI.RitiraFirma(id, pin);
                var listaErroriRitiroFirma = new List<string>();
                foreach (var itemRitiroFirma in resultRitiro)
                    listaErroriRitiroFirma.Add(
                        $"{listaErroriRitiroFirma.Count + 1} - {itemRitiroFirma.Value}");
                if (listaErroriRitiroFirma.Count > 0)
                    return Json(
                        new
                        {
                            message =
                                $"Riepilogo procedura di ritiro firma: {listaErroriRitiroFirma.Aggregate((i, j) => i + ", " + j)}"
                        }, JsonRequestBehavior.AllowGet);

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Elimina firma di un Atti di Sindacato Ispettivo
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="pin">pin</param>
        /// <returns></returns>
        [Route("elimina-firma")]
        public async Task<ActionResult> EliminaFirma(Guid id, string pin = "")
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var resultEliminaFirma = await apiGateway.DASI.EliminaFirma(id, pin);
                var listaErroriEliminaFirma = new List<string>();
                foreach (var itemEliminaFirma in resultEliminaFirma)
                    listaErroriEliminaFirma.Add(
                        $"{listaErroriEliminaFirma.Count + 1} - {itemEliminaFirma}");
                if (listaErroriEliminaFirma.Count > 0)
                    return Json(
                        new
                        {
                            message =
                                $"Riepilogo procedura di elimina firma: {listaErroriEliminaFirma.Aggregate((i, j) => i + ", " + j)}"
                        }, JsonRequestBehavior.AllowGet);

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per modificare un atto. Restituisce il modello dell'atto pre-compilato.
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/edit")]
        public async Task<ActionResult> Modifica(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.DASI.GetModificaModello(id);
            model.CurrentUser = CurrentUser;
            if (model.CurrentUser.IsSegreteriaPolitica
                || model.CurrentUser.IsSegreteriaAssemblea)
                return View("DASIForm_Segreteria", model);

            return View("DASIForm", model);
        }

        /// <summary>
        ///     Controller per modificare lo stato di una lista di atti
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("modifica-stato")]
        public async Task<ActionResult> ModificaStato(ModificaStatoAttoModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                // CurrentStatus/CurrentType dalla cache del riepilogo (stessa fonte della url di ritorno),
                // non piu' dalla Session. Azione su singolo atto: il vecchio ramo "Tutti" era morto.
                model.CurrentStatus = (StatiAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI)));
                model.CurrentType = (TipoAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)));

                await apiGateway.DASI.CambioStato(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = (StatiAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = (TipoAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per iscrivere una o più sedute ad un atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("iscrivi-seduta")]
        public async Task<ActionResult> IscriviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.IscriviSeduta(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per richiedere l'iscrizione ad una seduta futura
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("richiedi-iscrizione")]
        public async Task<ActionResult> RichiediIscrizione(RichiestaIscrizioneDASIModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.RichiediIscrizione(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per rimuovere una atto da una seduta
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-seduta")]
        public async Task<ActionResult> RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.RimuoviSeduta(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per rimuovere la richiesta di iscrizione ad una data seduta
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-richiesta")]
        public async Task<ActionResult> RimuoviRichiestaIscrizione(RichiestaIscrizioneDASIModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.RimuoviRichiestaIscrizione(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per proporre l'urgenza della mozione
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("proponi-urgenza")]
        public async Task<ActionResult> ProponiUrgenzaMozione(PromuoviMozioneModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.ProponiMozioneUrgente(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per proporre l'abbinata ad una mozione presentata
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("proponi-abbinata")]
        public async Task<ActionResult> ProponiAbbinataMozione(PromuoviMozioneModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.ProponiMozioneAbbinata(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                });
                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per riservare il contatore per la gestione manuale/cartacea dell'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("presentazione-cartacea")]
        public async Task<ActionResult> PresentazioneCartacea(PresentazioneCartaceaModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.PresentazioneCartacea(model);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("{id:guid}/meta-data")]
        public async Task<ActionResult> GetMetaData(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var atto = await apiGateway.DASI.Get(id);

                return Json(atto, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per modificare i metadati di un atto
        /// </summary>
        /// <param name="model">Modello atto</param>
        /// <returns></returns>
        [Route("meta-dati")]
        [HttpPost]
        public async Task<ActionResult> SalvaMetaDati(AttoDASIDto model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.ModificaMetaDati(model);
                return Json(Url.Action("RiepilogoDASI", "DASI"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("moz-abbinabili")]
        public async Task<ActionResult> GetMOZAbbinabili()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.DASI.GetMOZAbbinabili(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("odg/atti-sedute-attive")]
        public async Task<ActionResult> GetAttiSeduteAttive()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.DASI.GetAttiSeduteAttive(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per esportare gli atti
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("excel-rapido")]
        public async Task<ActionResult> EsportaXLSRapido(FilterRequest model)
        {
            try
            {
                // #994
                if (model == null || !model.filters.Any())
                    return Json(new RiepilogoDASIModel { CurrentUser = CurrentUser });

                var request = BuildDasiRequest(model);

                var apiGateway = new ApiGateway(Token);

                var soloIds = await apiGateway.DASI.GetSoloIds(request);
                var file = await apiGateway.Esporta.EsportaXLSDASI(soloIds);

                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per esportare gli atti
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("zip-rapido")]
        public async Task<ActionResult> EsportaZipRapido(FilterRequest model)
        {
            try
            {
                // #994
                if (model == null || !model.filters.Any())
                    return Json(new RiepilogoDASIModel { CurrentUser = CurrentUser });

                var request = BuildDasiRequest(model);

                var apiGateway = new ApiGateway(Token);

                var soloIds = await apiGateway.DASI.GetSoloIds(request);
                var file = await apiGateway.Esporta.EsportaZipDASI(soloIds);

                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     #1611: estrazione rapida in excel (formato consiglieri) conforme al nuovo sistema a chips.
        ///     L'action GET EsportaXLSConsiglieri leggeva i filtri dalla Session["RiepilogoDASI"], che nel
        ///     nuovo flusso client-side (inviaDatiChips -> CreaTabella) non viene popolata: da qui la
        ///     NullReferenceException. Qui i filtri arrivano via POST, come per EsportaXLSRapido/Riepilogo.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("excel-consiglieri-rapido")]
        public async Task<ActionResult> EsportaXLSConsiglieriRapido(FilterRequest model)
        {
            try
            {
                if (model == null || !model.filters.Any())
                    return Json(new RiepilogoDASIModel { CurrentUser = CurrentUser });

                var request = BuildDasiRequest(model);

                var apiGateway = new ApiGateway(Token);

                var soloIds = await apiGateway.DASI.GetSoloIds(request);
                var file = await apiGateway.Esporta.EsportaXLSConsiglieriDASI(soloIds);

                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Punto unico di costruzione della richiesta DASI a partire dai filtri inviati dal client
        ///     (FilterRequest / chips). Sostituisce la vecchia ricostruzione basata su Session["RiepilogoDASI"]:
        ///     ricerca, estrazioni rapide e azioni massive "Seleziona tutto" passano tutti da qui.
        /// </summary>
        private BaseRequest<AttoDASIDto> BuildDasiRequest(FilterRequest model)
        {
            // #1633 - la spunta "Visualizza solo gli atti per i quali e' richiesta la mia firma"
            // invia uno pseudo-filtro "AttiDaFirmare". Qui lo traduco nel parametro RequireMySign
            // (#539, gia' gestito lato API da AddRequireMySignData) e lo escludo dal filtro SQL
            // generico, che non conosce questa proprieta'. Cosi' ricerca, "Seleziona tutto", azioni
            // massive ed estrazioni rapide - che passano tutte da qui - restano coerenti.
            var richiestaPropriaFirma = model.filters
                .Any(f => f.property == "AttiDaFirmare"
                          && !string.IsNullOrEmpty(f.value)
                          && f.value.Equals("true", StringComparison.OrdinalIgnoreCase));

            var filtriEffettivi = model.filters
                .Where(f => f.property != "AttiDaFirmare")
                .ToList();

            var request = new BaseRequest<AttoDASIDto>
            {
                page = model.page > 0 ? model.page : 1,
                size = model.size != 0 ? model.size : 20,
                param = new Dictionary<string, object>
                {
                    { "CLIENT_MODE", model.clientMode },
                    { nameof(FilterRequest.viewMode), model.viewMode },
                    { "RequireMySign", richiestaPropriaFirma }
                }
            };

            if (model.sort_settings != null && model.sort_settings.Any())
                request.dettagliOrdinamento = model.sort_settings;

            if (model.columns_settings != null && model.columns_settings.Any())
                request.columns = model.columns_settings;

            request.filtro.AddRange(Utility.ParseFilterDasi(filtriEffettivi));

            return request;
        }



        /// <summary>
        ///     Controller per scaricare il documento pdf dell'atto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("file")]
        public async Task<ActionResult> Download(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.DASI.Download(id);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per scaricare il documento pdf dell'atto con il testo e l’oggetto modificati
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("file-privacy")]
        public async Task<ActionResult> DownloadWithPrivacy(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.DASI.DownloadWithPrivacy(id);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        //FILTRI

        [HttpGet]
        [Route("stati")]
        public async Task<ActionResult> Filtri_GetStati()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var resFromDb = await apiGateway.DASI.GetStati();
                var resList = resFromDb.ToList();
                var clientMode = (ClientModeEnum)HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE));
                if (clientMode == ClientModeEnum.TRATTAZIONE)
                {
                    var removeStatusList = new List<int>
                    {
                        (int)StatiAttoEnum.TUTTI,
                        (int)StatiAttoEnum.BOZZA,
                        (int)StatiAttoEnum.BOZZA_RISERVATA
                    };
                    resList.RemoveAll(res => removeStatusList.Contains(res.IDStato));
                }

                return Json(resList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("tipi-moz")]
        public async Task<ActionResult> Filtri_GetTipiMOZ()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.DASI.GetTipiMOZ(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("soggetti-interrogabili")]
        public async Task<ActionResult> Filtri_GetSoggettiInterrogabili()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.DASI.GetSoggettiInterrogabili(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("declassa-mozione")]
        public async Task<ActionResult> DeclassaMozione(List<string> data)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.DeclassaMozione(data);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("salva-atto-cartaceo")]
        public async Task<ActionResult> SalvaAttoCartaceo(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.SalvaCartaceo(request);
                return Json(Url.Action("ViewAtto", "DASI", new
                {
                    id = request.UIDAtto
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("cambia-priorita-firma")]
        public async Task<ActionResult> CambiaPrioritaFirma(AttiFirmeDto firma)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.CambiaPrioritaFirma(firma);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("cambia-ordine-visualizzazione-firme")]
        public async Task<ActionResult> UpdateOrdineVisualizzazione(List<AttiFirmeDto> updatedList)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.CambiaOrdineVisualizzazioneFirme(updatedList);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per generare la dcr dell'atto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("genera-report-dcr")]
        public async Task<ActionResult> GeneraReportDcr(Guid id, int tipo)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                var filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.UIDAtto),
                        value = id.ToString()
                    }
                };
                var request = new ReportDto
                {
                    filters = JsonConvert.SerializeObject(filters),
                    exportformat = (int)ExportFormatEnum.WORD,
                    dataviewtype = (int)DataViewTypeEnum.TEMPLATE,
                    wordsize = (int)WordSizeEnum.A4
                };

                if (tipo.Equals((int)TipoAttoEnum.MOZ))
                {
                    request.dataviewtype_template = AppSettingsConfiguration.MOZ_UIDTemplateReportDCR;
                }
                else if (tipo.Equals((int)TipoAttoEnum.ODG))
                {
                    request.dataviewtype_template = AppSettingsConfiguration.ODG_UIDTemplateReportDCR;
                }
                else if (tipo.Equals((int)TipoAttoEnum.RIS))
                {
                    request.dataviewtype_template = AppSettingsConfiguration.RIS_UIDTemplateReportDCR;
                }

                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per generare la copertina per il presidente
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tipo"></param>
        /// <param name="tipo_risposta"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("genera-report-copertina-presidente")]
        public async Task<ActionResult> GeneraReportCopertinaPresidente(Guid id, int tipo, int tipo_risposta)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                var filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.UIDAtto),
                        value = id.ToString()
                    }
                };
                var request = new ReportDto
                {
                    filters = JsonConvert.SerializeObject(filters),
                    exportformat = (int)ExportFormatEnum.WORD,
                    dataviewtype = (int)DataViewTypeEnum.TEMPLATE,
                    wordsize = (int)WordSizeEnum.A3
                };

                switch ((TipoAttoEnum)tipo)
                {
                    case TipoAttoEnum.ITL:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_SCRITTA_UIDTemplateReportCopertinaPresidente;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.ORALE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_ORALE_UIDTemplateReportCopertinaPresidente;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_COMMISSIONE_UIDTemplateReportCopertinaPresidente;
                        }

                        break;
                    }
                    case TipoAttoEnum.ITR:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_SCRITTA_UIDTemplateReportCopertinaPresidente;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_COMMISSIONE_UIDTemplateReportCopertinaPresidente;
                        }

                        break;
                    }
                    case TipoAttoEnum.IQT:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.IQT_UIDTemplateReportCopertinaPresidente;
                        break;
                    case TipoAttoEnum.MOZ:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.MOZ_UIDTemplateReportCopertinaPresidente;
                        break;
                    case TipoAttoEnum.ODG:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.ODG_UIDTemplateReportCopertinaPresidente;
                        break;
                    case TipoAttoEnum.RIS:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.RIS_UIDTemplateReportCopertinaPresidente;
                        break;
                }

                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per generare la copertina per l'ufficio
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("genera-report-copertina-ufficio")]
        public async Task<ActionResult> GeneraReportCopertinaUfficio(Guid id, int tipo, int tipo_risposta)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                var filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.UIDAtto),
                        value = id.ToString()
                    }
                };
                var request = new ReportDto
                {
                    filters = JsonConvert.SerializeObject(filters),
                    exportformat = (int)ExportFormatEnum.WORD,
                    dataviewtype = (int)DataViewTypeEnum.TEMPLATE,
                    wordsize = (int)WordSizeEnum.A3
                };

                switch ((TipoAttoEnum)tipo)
                {
                    case TipoAttoEnum.ITL:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_SCRITTA_UIDTemplateReportCopertinaUfficio;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.ORALE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_ORALE_UIDTemplateReportCopertinaUfficio;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_COMMISSIONE_UIDTemplateReportCopertinaUfficio;
                        }

                        break;
                    }
                    case TipoAttoEnum.ITR:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_SCRITTA_UIDTemplateReportCopertinaUfficio;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_COMMISSIONE_UIDTemplateReportCopertinaUfficio;
                        }

                        break;
                    }
                    case TipoAttoEnum.IQT:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.IQT_UIDTemplateReportCopertinaUfficio;
                        break;
                    case TipoAttoEnum.MOZ:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.MOZ_UIDTemplateReportCopertinaUfficio;
                        break;
                    case TipoAttoEnum.ODG:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.ODG_UIDTemplateReportCopertinaUfficio;
                        break;
                    case TipoAttoEnum.RIS:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.RIS_UIDTemplateReportCopertinaUfficio;
                        break;
                }

                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per generare la copertina per l'ufficio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tipo"></param>
        /// <param name="tipo_risposta"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("genera-report-lettera")]
        public async Task<ActionResult> GeneraReportLettera(Guid id, int tipo, int tipo_risposta)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                var filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.UIDAtto),
                        value = id.ToString()
                    }
                };
                var request = new ReportDto
                {
                    filters = JsonConvert.SerializeObject(filters),
                    exportformat = (int)ExportFormatEnum.WORD,
                    dataviewtype = (int)DataViewTypeEnum.TEMPLATE,
                    wordsize = (int)WordSizeEnum.A4
                };

                switch ((TipoAttoEnum)tipo)
                {
                    case TipoAttoEnum.ITL:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_SCRITTA_UIDTemplateReportLettera;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.ORALE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_ORALE_UIDTemplateReportLettera;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITL_COMMISSIONE_UIDTemplateReportLettera;
                        }

                        break;
                    }
                    case TipoAttoEnum.ITR:
                    {
                        if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.SCRITTA)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_SCRITTA_UIDTemplateReportLettera;
                        }
                        else if ((TipoRispostaEnum)tipo_risposta == TipoRispostaEnum.COMMISSIONE)
                        {
                            request.dataviewtype_template =
                                AppSettingsConfiguration.ITR_COMMISSIONE_UIDTemplateReportLettera;
                        }

                        break;
                    }
                    case TipoAttoEnum.MOZ:
                    {
                        request.dataviewtype_template =
                            AppSettingsConfiguration.MOZ_UIDTemplateReportLettera;

                        break;
                    }
                    case TipoAttoEnum.ODG:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.ODG_UIDTemplateReportLettera;
                        break;
                    case TipoAttoEnum.RIS:
                        request.dataviewtype_template =
                            AppSettingsConfiguration.RIS_UIDTemplateReportLettera;
                        break;
                }

                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        ///     Controller per generare la copertina per l'ufficio
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("genera-report-lettera-assemblea-moz")]
        public async Task<ActionResult> GeneraReportLetteraAssembleaMOZ(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                var filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.UIDAtto),
                        value = id.ToString()
                    }
                };
                var request = new ReportDto
                {
                    filters = JsonConvert.SerializeObject(filters),
                    exportformat = (int)ExportFormatEnum.WORD,
                    dataviewtype = (int)DataViewTypeEnum.TEMPLATE,
                    wordsize = (int)WordSizeEnum.A4
                };

                request.dataviewtype_template =
                    AppSettingsConfiguration.MOZ_COMMISSIONE_UIDTemplateReportLettera;

                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per scaricare il documento pdf dell'atto
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("genera-report")]
        public async Task<ActionResult> GeneraReport(ReportDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.DASI.GeneraReport(request);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("salva-report")]
        public async Task<ActionResult> SalvaReport(ReportDto report)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.SalvaReport(report);
                return Json("OK");
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("elimina-report")]
        public async Task<ActionResult> EliminaReport(string nomeReport)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.EliminaReport(nomeReport);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("get-reports")]
        public async Task<ActionResult> GetReports()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetReports();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("view-abbinamenti-disponibili")]
        public async Task<ActionResult> GetAbbinamentiDisponibili(int legislaturaId, int page, int size)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetAbbinamentiDisponibili(legislaturaId, page, size);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Route("commissioni-attive")]
        public async Task<ActionResult> GetCommissioniAttive()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetCommissioniAttive();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("view-gruppi-disponibili")]
        public async Task<ActionResult> GetGruppiDisponibili(int legislaturaId, int page, int size)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetGruppiDisponibili(legislaturaId, page, size);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("view-organi-disponibili")]
        public async Task<ActionResult> GetOrganiDisponibili(int legislaturaId)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetOrganiDisponibili(legislaturaId);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("view-reports-covers")]
        public async Task<ActionResult> GetReportsCovers()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetReportsCovers();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("view-reports-card-templates")]
        public async Task<ActionResult> GetReportsCardTemplates()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetReportsCardTemplates();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("genera-zip")]
        public async Task<ActionResult> GeneraZip(ReportDto report)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.DASI.GeneraZIP(report);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per il salvataggio dell' atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-info-generali")]
        public async Task<ActionResult> Salva_InformazioniGeneraliAtto(AttoDASI_InformazioniGeneraliDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_InformazioniGenerali(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per il aggiungere un nuovo abbinamento all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-abbinamento")]
        public async Task<ActionResult> Salva_NuovoAbbinamento(AttiAbbinamentoDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_NuovoAbbinamento(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere un abbinamento all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-abbinamento")]
        public async Task<ActionResult> Salva_RimuoviAbbinamento(AttiAbbinamentoDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_Abbinamento(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere una risposta all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-nuova-risposta")]
        public async Task<ActionResult> Salva_NuovaRisposta(AttiRisposteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var risposta = await apiGateway.DASI.Salva_NuovaRisposta(request);
                return Json(risposta, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere una risposta all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-risposta")]
        public async Task<ActionResult> Salva_RimuoviRisposta(AttiRisposteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_Risposta(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare i dettagli di una risposta all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-dettagli-risposta")]
        public async Task<ActionResult> Salva_DettagliRisposta(AttiRisposteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_DettagliRisposta(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare le informazioni di una risposta all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-informazioni-risposta")]
        public async Task<ActionResult> Salva_InformazioniRisposta(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_InformazioniRisposta(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per aggiungere un organo monitorato all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-monitoraggio")]
        public async Task<ActionResult> Salva_NuovoMonitoraggio(AttiRisposteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_NuovoMonitoraggio(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere il monitoraggio all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-monitoraggio")]
        public async Task<ActionResult> Salva_RimuoviMonitoraggio(AttiRisposteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_Monitoraggio(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere il monitoraggio all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-info-monitoraggio")]
        public async Task<ActionResult> Salva_InformazioniMonitoraggio(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_InfoMonitoraggio(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare le informazioni di chiusura iter
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-info-chiusura-iter")]
        public async Task<ActionResult> Salva_InformazioniChiusuraIter(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_InfoChiusuraIter(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare una nota all'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-nota")]
        public async Task<ActionResult> Salva_Nota(NoteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var nota = await apiGateway.DASI.Salva_Nota(request);
                return Json(nota, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere una nota dall'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-nota")]
        public async Task<ActionResult> Salva_RimuoviNota(NoteDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_Nota(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare le informazioni riguardanti la privacy dall'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-privacy")]
        public async Task<ActionResult> Salva_PrivacyAtto(AttoDASIDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_PrivacyAtto(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare un documento caricato dall'utente per un atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-documento")]
        public async Task<ActionResult> Salva_Documento()
        {
            // Verifica che la richiesta contenga dei file
            if (Request.Files == null || Request.Files.Count == 0)
            {
                return Json(new ErrorResponse("Nessun file caricato."), JsonRequestBehavior.AllowGet);
            }

            // Ottieni il file dalla richiesta
            var file = Request.Files[0];

            // Valida il file (es. tipo MIME, dimensione massima)
            if (file.ContentLength <= 0)
            {
                return Json(new ErrorResponse("Il file è vuoto."), JsonRequestBehavior.AllowGet);
            }

            // Leggi il contenuto del file
            byte[] fileData;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            // NUOVA VALIDAZIONE COMPLETA
            var validationResult = FileValidator.ValidateFile(
                file.FileName, 
                file.ContentType, 
                fileData
            );

            if (!validationResult.IsValid)
            {
                return Json(new ErrorResponse(validationResult.ErrorMessage), 
                    JsonRequestBehavior.AllowGet);
            }

            // Verifica aggiuntiva: blocca file ZIP anche se mascherati
            if (FileValidator.IsZipFile(file.FileName, fileData))
            {
                return Json(new ErrorResponse(
                        "File ZIP e archivi compressi non sono consentiti per motivi di sicurezza."),
                    JsonRequestBehavior.AllowGet);
            }
            
            try
            {
                var request = new SalvaDocumentoRequest
                {
                    UIDAtto = Guid.Parse(Request.Form["UIDAtto"]),
                    Tipo = int.Parse(Request.Form["TipoDocumento"]),
                    Nome = file.FileName,
                    Contenuto = fileData
                };

                if (!string.IsNullOrEmpty(Request.Form["Uid"]))
                {
                    request.Uid = Guid.Parse(Request.Form["Uid"]);
                }

                var apiGateway = new ApiGateway(Token);
                var documento = await apiGateway.DASI.Salva_DocumentoAtto(request);
                return Json(documento, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per rimuovere un documento dall'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-documento")]
        public async Task<ActionResult> Salva_RimuoviDocumento(AttiDocumentiDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_Documento(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per pubblicare un documento dall'atto
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("pubblica-documento")]
        public async Task<ActionResult> Salva_PubblicaDocumento(AttiDocumentiDto request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Pubblica_Documento(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per salvare massivamente i dati di una lista di atti
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("salva-comando-massivo")]
        public async Task<ActionResult> Salva_ComandoMassivo(SalvaComandoMassivoRequest request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_ComandoMassivo(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        ///     Endpoint per rimuovere massivamente: iscrizione in seduta, urgenza e abbinamento
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rimuovi-comando-massivo")]
        public async Task<ActionResult> Rimuovi_ComandoMassivo(RimuoviComandoMassivoRequest request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Rimuovi_ComandoMassivo(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        ///     Avvia la protocollazione EDMA dell'atto: e' la action che la
        ///     view invoca dal click "Protocolla" della segreteria, dopo il
        ///     modale di conferma. La risposta porta segnatura, id pratica,
        ///     id documento e l'eventuale messaggio di errore: la view la usa
        ///     per popolare la casella Protocollo e mostrare un toast.
        /// </summary>
        [Route("protocolla/{id:guid}")]
        [HttpPost]
        public async Task<ActionResult> Protocolla(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var esito = await apiGateway.DASI.Protocolla(id);
                return Json(esito, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Salva manualmente il campo Protocollo dell'atto. Endpoint
        ///     riservato alla segreteria;
        ///     l'autorizzazione finale viene comunque verificata dal server
        ///     (ruolo + feature flag EDMA_AbilitaEditManualeProtocollo).
        /// </summary>
        [Route("protocollo-manuale/{id:guid}")]
        [HttpPost]
        public async Task<ActionResult> ProtocolloManuale(Guid id, string protocollo)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.SalvaProtocolloManuale(id, protocollo);
                return Json(new { Protocollo = protocollo ?? string.Empty }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
