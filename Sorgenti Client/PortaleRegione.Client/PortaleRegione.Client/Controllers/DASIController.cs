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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
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
    public class 
        DASIController : BaseController
    {
        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base al ruolo dell'utente loggato
        /// </summary>
        /// <returns></returns>
        [Route("riepilogo")]
        public async Task<ActionResult> RiepilogoDASI(int page = 1, int size = 50, int view = (int)ViewModeEnum.GRID,
            int stato = (int)StatiAttoEnum.BOZZA, int tipo = (int)TipoAttoEnum.TUTTI)
        {
            var currentUser = CurrentUser;
            CheckCacheClientMode(ClientModeEnum.GRUPPI);
            await CheckCacheGruppiAdmin(currentUser.CurrentRole);
            var view_require_my_sign = Convert.ToBoolean(Request.QueryString["require_my_sign"]);

            var apiGateway = new ApiGateway(Token);
            var currentLegislatura = await apiGateway.Legislature.GetLegislaturaAttuale();
            var model = await apiGateway.DASI.Get(page, size, (StatiAttoEnum)stato, (TipoAttoEnum)tipo,
                currentUser.CurrentRole, currentLegislatura, view_require_my_sign);
            model.CurrentUser = currentUser;
            SetCache(page, size, tipo, stato, view);
            if (view == (int)ViewModeEnum.PREVIEW)
            {
                model.ViewMode = ViewModeEnum.PREVIEW;
                foreach (var atti in model.Data.Results)
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);
            }

            Session["RiepilogoDASI"] = model;

            if (CanAccess(new List<RuoliIntEnum>
                    { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                return View("RiepilogoDASI_Admin", model);

            return View("RiepilogoDASI", model);
        }

        [HttpPost]
        [Route("riepilogoUOLA")]
        public async Task<ActionResult> Riepilogo(FilterRequest model)
        {
            try
            {
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = 1,
                    size = 20,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
                };

                if (model == null)
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                if (!model.filters.Any())
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                request.page = model.page;
                request.size = model.size;

                var apiGateway = new ApiGateway(Token);

                request.filtro.AddRange(Utility.ParseFilterDasi(model.filters));

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
        [Route("salva-gruppo-filtri")]
        public async Task<ActionResult> SalvaGruppoFiltri(FiltroPreferitoDto model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.SalvaGruppoFiltri(model);
                return Json("OK");
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("elimina-gruppo-filtri")]
        public async Task<ActionResult> EliminaGruppoFiltri(string nomeFiltro)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.EliminaGruppoFiltri(nomeFiltro);
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("gruppo-filtri")]
        public async Task<ActionResult> GetGruppoFiltri()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.DASI.GetGruppoFiltri();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base al ruolo dell'utente loggato
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RiepilogoDASI()
        {
            var model = new RiepilogoDASIModel
            {
                CurrentUser = CurrentUser
            };

            if (CanAccess(new List<RuoliIntEnum>
                    { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                return View("RiepilogoDASI_Admin", model);

            return View("RiepilogoDASI", model);
        }

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
        public async Task<ActionResult> RiepilogoDASI_BySeduta(Guid id, int tipo = (int)TipoAttoEnum.TUTTI,
            int page = 1, int size = 50, int view = (int)ViewModeEnum.GRID,
            int stato = (int)StatiAttoEnum.PRESENTATO, string uidAtto = "")
        {
            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.DASI.GetBySeduta_Trattazione(id, (TipoAttoEnum)tipo, uidAtto, page, size);
            CheckCacheClientMode(ClientModeEnum.TRATTAZIONE);
            model.ClientMode = ClientModeEnum.TRATTAZIONE;
            model.CurrentUser = CurrentUser;
            SetCache(page, size, tipo, stato, view);
            if (view == (int)ViewModeEnum.PREVIEW)
            {
                model.ViewMode = ViewModeEnum.PREVIEW;
                foreach (var atti in model.Data.Results)
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);
            }

            Session["RiepilogoDASI"] = model;

            if (CanAccess(new List<RuoliIntEnum>
                    { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
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
                var apiGateway = new ApiGateway(Token);
                var result = await apiGateway.DASI.Salva(request);
                Session["RiepilogoDASI"] = null;
                return Json(Url.Action("ViewAtto", "DASI", new
                {
                    id = result.UIDAtto
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
                if(atto.Tipo == (int)TipoAttoEnum.RIS)
                {
                    var consiglieriPublic = await apiGateway.Persone.GetProponentiFirmatari(atto.Legislatura.ToString());
                    
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
        /// <param name="id">Guid</param>
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
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = 1,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };

                    if (model.Richiesta_Firma) // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/916
                        request.param.Add("RequireMySign", "true");

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
                        var listaErroriFirma = resultFirma.Select(itemFirma => $"{itemFirma.Value}").ToList();
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
                        var listaErroriDeposito =
                            resultDeposita.Select(itemDeposito => $"{itemDeposito.Value}").ToList();
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
                var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                model.CurrentStatus = modelInCache.Stato;
                model.CurrentType = modelInCache.Tipo;
                if (model.Tutti)
                {
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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

                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                if (model.Tutti)
                {
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

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
                
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = 1,
                    size = 20,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
                };

                if (model == null)
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                if (!model.filters.Any())
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                request.page = model.page;
                request.size = model.size;

                var apiGateway = new ApiGateway(Token);

                request.filtro.AddRange(Utility.ParseFilterDasi(model.filters));

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
                
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = 1,
                    size = 20,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
                };

                if (model == null)
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                if (!model.filters.Any())
                {
                    var resEmpty = new RiepilogoDASIModel
                    {
                        CurrentUser = CurrentUser
                    };
                    return Json(resEmpty);
                }

                request.page = model.page;
                request.size = model.size;

                var apiGateway = new ApiGateway(Token);

                request.filtro.AddRange(Utility.ParseFilterDasi(model.filters));

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
        ///     Controller per esportare gli atti
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("esportaXLS")]
        public async Task<ActionResult> EsportaXLS()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var model = Session["RiepilogoDASI"] as RiepilogoDASIModel;

                var request = new BaseRequest<AttoDASIDto>
                {
                    page = model.Data.Paging.Page,
                    size = model.Data.Paging.Total,
                    filtro = model.Data.Filters,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)model.ClientMode } }
                };
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
        [HttpGet]
        [Route("esportaZip")]
        public async Task<ActionResult> EsportaZip()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var lista = new List<Guid>();
                var model = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                lista.AddRange(model
                    .Data
                    .Results
                    .Select(i => i.UIDAtto));
                var file = await apiGateway.Esporta.EsportaZipDASI(lista);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        private void SetCache(int page, int size, int tipo, int stato, int viewModeEnum)
        {
            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.TIPO_DASI),
                tipo,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.STATO_DASI),
                stato,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.PAGE_DASI),
                page,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.SIZE_DASI),
                size,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.VIEW_MODE_DASI),
                viewModeEnum,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );
        }

        [HttpPost]
        [Route("filtra")]
        public async Task<ActionResult> Filtri_Riepilogo()
        {
            int.TryParse(Request.Form["reset"], out var reset_enabled);
            var apiGateway = new ApiGateway(Token);
            var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
            if (modelInCache == null)
            {
                modelInCache = new RiepilogoDASIModel
                {
                    ClientMode =
                        (ClientModeEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE))),
                    Stato = (StatiAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.STATO_DASI))),
                    Tipo = (TipoAttoEnum)Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.TIPO_DASI)))
                };
            }

            var view = Request.Form["view"];

            if (Convert.ToInt16(view) == (int)ViewModeEnum.PREVIEW)
            {
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = modelInCache.Data.Paging.Page,
                    size = modelInCache.Data.Paging.Limit,
                    filtro = modelInCache.Data.Filters,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                };
                var resultPreview = await apiGateway.DASI.Get(request);
                resultPreview.CurrentUser = CurrentUser;
                resultPreview.ClientMode = modelInCache.ClientMode;

                Session["RiepilogoDASI"] = resultPreview;

                SetCache(resultPreview.Data.Paging.Page, resultPreview.Data.Paging.Limit, (int)resultPreview.Tipo,
                    (int)resultPreview.Stato,
                    Convert.ToInt16(view));

                foreach (var atti in resultPreview.Data.Results)
                {
                    resultPreview.ViewMode = ViewModeEnum.PREVIEW;
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);

                    var firme_ante = await apiGateway.DASI.GetFirmatari(atti.UIDAtto, FirmeTipoEnum.PRIMA_DEPOSITO);
                    var firme_post = await apiGateway.DASI.GetFirmatari(atti.UIDAtto, FirmeTipoEnum.DOPO_DEPOSITO);
                    atti.FirmeAnte = firme_ante.ToList();
                    atti.FirmePost = firme_post.ToList();

                    atti.Firme = await Helpers.Utility.GetFirmatariDASI(
                        atti.FirmeAnte,
                        resultPreview.CurrentUser.UID_persona,
                        FirmeTipoEnum.PRIMA_DEPOSITO,
                        Token);
                    atti.Firme_dopo_deposito = await Helpers.Utility.GetFirmatariDASI(
                        atti.FirmePost,
                        resultPreview.CurrentUser.UID_persona,
                        FirmeTipoEnum.DOPO_DEPOSITO,
                        Token);
                }

                if (CanAccess(new List<RuoliIntEnum>
                        { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                    return View("RiepilogoDASI_Admin", resultPreview);

                return View("RiepilogoDASI", resultPreview);
            }

            if (modelInCache != null)
            {
                if (Convert.ToInt16(view) == (int)ViewModeEnum.GRID && modelInCache.ViewMode == ViewModeEnum.PREVIEW)
                {
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                    };
                    var resultGrid = await apiGateway.DASI.Get(request);
                    resultGrid.CurrentUser = CurrentUser;
                    resultGrid.ClientMode = modelInCache.ClientMode;
                    SetCache(resultGrid.Data.Paging.Page, resultGrid.Data.Paging.Limit, (int)resultGrid.Tipo,
                        (int)resultGrid.Stato,
                        Convert.ToInt16(view));

                    Session["RiepilogoDASI"] = resultGrid;

                    if (CanAccess(new List<RuoliIntEnum>
                            { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                        return View("RiepilogoDASI_Admin", resultGrid);

                    return View("RiepilogoDASI", resultGrid);
                }
            }

            var modeCache = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE)));
            var mode = modeCache != 0 ? (ClientModeEnum)modeCache : ClientModeEnum.GRUPPI;
            
            if (mode == ClientModeEnum.TRATTAZIONE)
            {
                if (reset_enabled == 1)
                {
                    modelInCache.Data.Paging.Page = 1;
                    var listaAppoggio = modelInCache.Data.Filters.ToList();
                    foreach (var filterStatement in listaAppoggio)
                    {
                        if (filterStatement.PropertyId.Equals(nameof(AttoDASIDto.UID_Atto_ODG))
                            || filterStatement.PropertyId.Equals(nameof(AttoDASIDto.UIDSeduta))
                            || filterStatement.PropertyId.Equals(nameof(AttoDASIDto.Tipo)))
                        {
                            continue;
                        }
                        else
                        {
                            modelInCache.Data.Filters.Remove(filterStatement);
                        }
                    }
                }
                else
                {
                    var modelTrattazione = await ElaboraFiltri();
                    foreach (var filterStatement in modelTrattazione.filtro)
                    {
                        if (modelInCache.Data.Filters.Any(f => f.PropertyId.Equals(filterStatement.PropertyId)))
                        {
                            continue;
                        }

                        modelInCache.Data.Filters.Add(filterStatement);
                    }
                }

                int.TryParse(Request.Form["page"], out var filtro_page);
                int.TryParse(Request.Form["size"], out var filtro_size);
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = filtro_page,
                    size = filtro_size,
                    filtro = modelInCache.Data.Filters,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } }
                };

                var resultGrid = await apiGateway.DASI.Get(request);
                resultGrid.CurrentUser = CurrentUser;
                resultGrid.ClientMode = modelInCache.ClientMode;
                SetCache(resultGrid.Data.Paging.Page, resultGrid.Data.Paging.Limit, (int)resultGrid.Tipo,
                    (int)resultGrid.Stato,
                    Convert.ToInt16(view));

                Session["RiepilogoDASI"] = resultGrid;

                if (CanAccess(new List<RuoliIntEnum>
                        { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                    return View("RiepilogoDASI_Admin", resultGrid);

                return View("RiepilogoDASI", resultGrid);
            }

            Session["RiepilogoDASI"] = null;
            
            if (reset_enabled == 1)
            {
                return RedirectToAction("RiepilogoDASI", "DASI");
            }

            var model = await ElaboraFiltri();
            var result = await apiGateway.DASI.Get(model);
            result.CurrentUser = CurrentUser;
            result.ClientMode = mode;
            SetCache(result.Data.Paging.Page, result.Data.Paging.Limit, (int)result.Tipo, (int)result.Stato,
                Convert.ToInt16(view));

            Session["RiepilogoDASI"] = result;

            if (CanAccess(new List<RuoliIntEnum>
                    { RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea }))
                return View("RiepilogoDASI_Admin", result);

            return View("RiepilogoDASI", result);
        }

        private async Task<BaseRequest<AttoDASIDto>> ElaboraFiltri()
        {
            var modeCache = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE)));
            var mode = modeCache != 0 ? (ClientModeEnum)modeCache : ClientModeEnum.GRUPPI;
            int.TryParse(Request.Form["page"], out var filtro_page);
            int.TryParse(Request.Form["size"], out var filtro_size);
            var view = Request.Form["view"];
            var filtro_oggetto = Request.Form["filtro_oggetto"];
            var filtro_stato = Request.Form["filtro_stato"];
            var filtro_tipo = Request.Form["filtro_tipo"];
            var filtro_mozione_urgente = Request.Form["filtro_mozione_urgente"];
            var filtro_tipo_risposta = Request.Form["filtro_tipo_risposta"];
            var filtro_natto = Request.Form["filtro_natto"];
            var filtro_natto2 = Request.Form["filtro_natto2"];
            var filtro_da = Request.Form["filtro_da"];
            var filtro_a = Request.Form["filtro_a"];
            var filtro_data_seduta = Request.Form["filtro_data_seduta"];
            var filtro_data_iscrizione_seduta = Request.Form["filtro_data_iscrizione_seduta"];
            var filtro_tipo_trattazione = Request.Form["Tipo"];
            var filtro_soggetto_dest = Request.Form["filtro_soggetto_dest"];
            var filtro_seduta = Request.Form["UIDSeduta"];
            var filtro_legislatura = Request.Form["filtro_legislatura"];
            var filtro_proponente = Request.Form["filtro_proponente"];
            var filtro_provvedimenti = Request.Form["filtro_provvedimenti"];

            var model = new BaseRequest<AttoDASIDto>
            {
                page = filtro_page != 0 ? filtro_page : 1,
                size = filtro_size,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)mode }, { "VIEW_MODE", view } }
            };

            var util = new UtilityFilter();

            util.AddFilter_ByNumeroAtto(ref model, filtro_natto, filtro_natto2);
            util.AddFilter_ByDataPresentazione(ref model, filtro_da, filtro_a);
            var sedutaUId = await GetSedutaByData(filtro_data_seduta);
            util.AddFilter_ByDataSeduta(ref model, sedutaUId);
            util.AddFilter_ByDataIscrizioneSeduta(ref model, filtro_data_iscrizione_seduta);
            util.AddFilter_ByOggetto_Testo(ref model, filtro_oggetto);
            util.AddFilter_ByStato(ref model, filtro_stato, CurrentUser);
            util.AddFilter_ByTipoRisposta(ref model, filtro_tipo_risposta);
            util.AddFilter_ByTipo(ref model, filtro_tipo, filtro_tipo_trattazione, mode);
            util.AddFilter_ByMozioneUrgente(ref model, filtro_mozione_urgente);
            util.AddFilter_BySoggetto(ref model, filtro_soggetto_dest);
            util.AddFilter_BySeduta(ref model, filtro_seduta);
            util.AddFilter_ByLegislatura(ref model, filtro_legislatura);
            util.AddFilter_Proponents(ref model, filtro_proponente);
            util.AddFilter_Provvedimenti(ref model, filtro_provvedimenti);

            return model;
        }

        private async Task<Guid> GetSedutaByData(string filtroDataSeduta)
        {
            var result = Guid.Empty;
            var success = DateTime.TryParse(filtroDataSeduta, out var data);

            if (success)
            {
                var modelSedute = new BaseRequest<SeduteDto>
                {
                    filtro = new List<FilterStatement<SeduteDto>>
                    {
                        new FilterStatement<SeduteDto>
                        {
                            PropertyId = nameof(SeduteDto.Data_seduta),
                            Operation = Operation.GreaterThanOrEqualTo,
                            Value = data.ToString("yyyy-MM-dd") + " 00:00:01"
                        },
                        new FilterStatement<SeduteDto>
                        {
                            PropertyId = nameof(SeduteDto.Data_seduta),
                            Operation = Operation.LessThanOrEqualTo,
                            Value = data.ToString("yyyy-MM-dd") + " 23:59:59"
                        }
                    }
                };
                var gate = new ApiGateway(Token);
                var resultSedute = await gate.Sedute.Get(modelSedute);
                if (resultSedute.Results.Any()) result = resultSedute.Results.First().UIDSeduta;
            }

            return result;
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

        /// <summary>
        ///     Controller per inviare l'atto al protocollo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("invia-al-protocollo")]
        [HttpGet]
        public async Task<ActionResult> InviaAlProtocollo(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.InviaAlProtocollo(id);
                return Json(Url.Action("RiepilogoDASI", "DASI"), JsonRequestBehavior.AllowGet);
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
        ///     Controller per scaricare il documento pdf dell'atto
        /// </summary>
        /// <param name="id"></param>
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
        public  async Task<ActionResult> GeneraZip(ReportDto report)
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
                await apiGateway.DASI.Salva_NuovaRisposta(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
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
                await apiGateway.DASI.Salva_Nota(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
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
            {;
                return Json(new ErrorResponse("Nessun file caricato."), JsonRequestBehavior.AllowGet);
            }

            // Ottieni il file dalla richiesta
            var file = Request.Files[0];

            // Valida il file (es. tipo MIME, dimensione massima)
            if (file.ContentLength <= 0)
            {
                return Json(new ErrorResponse( "Il file è vuoto."), JsonRequestBehavior.AllowGet);
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new ErrorResponse( "Formato file non valido. Solo PDF accettati."), JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Converte il file in un array di byte
                byte[] fileData;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                var request = new SalvaDocumentoRequest
                {
                    UIDAtto = Guid.Parse(Request.Form["UIDAtto"]),
                    Tipo = int.Parse(Request.Form["TipoDocumento"]),
                    Nome = file.FileName,
                    Contenuto = fileData
                };

                var apiGateway = new ApiGateway(Token);
                await apiGateway.DASI.Salva_DocumentoAtto(request);
                return Json("OK", JsonRequestBehavior.AllowGet);
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
    }
}