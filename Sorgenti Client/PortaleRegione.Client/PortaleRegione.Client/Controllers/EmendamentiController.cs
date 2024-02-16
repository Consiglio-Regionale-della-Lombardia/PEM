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
using System.Web.Caching;
using System.Web.Mvc;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller emendamenti
    /// </summary>
    [Authorize]
    [RoutePrefix("emendamenti")]
    public class EmendamentiController : BaseController
    {
        /// <summary>
        ///     Controller per visualizzare i dati degli emendamenti contenuti in un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="ordine"></param>
        /// <param name="view"></param>
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> RiepilogoEmendamenti(Guid id,
            OrdinamentoEnum ordine = OrdinamentoEnum.Presentazione, ViewModeEnum view = ViewModeEnum.GRID, int page = 1,
            int size = 50)
        {
            var mode = ClientModeEnum.GRUPPI;
            var contextMode = HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE));
            if (contextMode != null) mode = (ClientModeEnum)Convert.ToInt16(contextMode);

            var view_require_my_sign = Convert.ToBoolean(Request.QueryString["require_my_sign"]);

            if (Session["RicaricaFiltri"] is bool)
                if (Convert.ToBoolean(Session["RicaricaFiltri"]))
                {
                    Session["RicaricaFiltri"] = false; //reset sessione
                    if (Session["RiepilogoEmendamenti"] is EmendamentiViewModel old_model)
                        try
                        {
                            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                                return View("RiepilogoEM_Admin", old_model);

                            return View("RiepilogoEM", old_model);
                        }
                        catch (Exception)
                        {
                            Session["RiepilogoEmendamenti"] = null;
                        }
                }

            SetCache(page, size, ordine, view);

            var composeModel = await ComposeModel(id, mode, ordine, view, page, size, view_require_my_sign);
            Session["RiepilogoEmendamenti"] = composeModel;

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoEM_Admin", composeModel);
            return View("RiepilogoEM", composeModel);
        }

        /// <summary>
        ///     Controller per visualizzare i dati degli emendamenti contenuti in un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("seduta/{id:guid}")]
        public ActionResult RiepilogoEmendamentiInSeduta(Guid id)
        {
            var mode = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE)));
            if (mode != (int)ClientModeEnum.TRATTAZIONE)
                HttpContext.Cache.Insert(
                    GetCacheKey(CacheHelper.CLIENT_MODE),
                    (int)ClientModeEnum.TRATTAZIONE,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    (key, value, reason) => { Console.WriteLine("Cache removed"); }
                );

            return RedirectToAction("RiepilogoEmendamenti", "Emendamenti", new { id });
        }

        private async Task<EmendamentiViewModel> ComposeModel(Guid id, ClientModeEnum mode,
            OrdinamentoEnum ordine, ViewModeEnum view, int page,
            int size, bool view_require_my_sign)
        {
            var apiGateway = new ApiGateway(Token);
            EmendamentiViewModel model;
            if (view_require_my_sign == false)
                model = await apiGateway.Emendamento.Get(id, mode, ordine, page, size);
            else
                model = await apiGateway.Emendamento.Get_RichiestaPropriaFirma(id, mode, ordine, page, size);
            model.ViewMode = view;
            if (view == ViewModeEnum.PREVIEW)
                foreach (var emendamentiDto in model.Data.Results)
                    emendamentiDto.BodyEM =
                        await apiGateway.Emendamento.GetBody(emendamentiDto.UIDEM, TemplateTypeEnum.HTML);

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return model;

            if (mode == ClientModeEnum.GRUPPI)
                foreach (var emendamentiDto in model.Data.Results)
                    if (emendamentiDto.IDStato <= (int)StatiEnum.Depositato)
                    {
                        if (emendamentiDto.ConteggioFirme > 0)
                            emendamentiDto.Firmatari = await Utility.GetFirmatari(
                                await apiGateway.Emendamento.GetFirmatari(emendamentiDto.UIDEM,
                                    FirmeTipoEnum.TUTTE),
                                CurrentUser.UID_persona, FirmeTipoEnum.TUTTE, Token, true);

                        emendamentiDto.Destinatari =
                            await Utility.GetDestinatariNotifica(
                                await apiGateway.Emendamento.GetInvitati(emendamentiDto.UIDEM), Token);
                    }

            return model;
        }

        private void SetCache(int page, int size, OrdinamentoEnum ordine, ViewModeEnum view)
        {
            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.ORDINAMENTO_PEM),
                (int)ordine,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.VIEW_MODE_PEM),
                view,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.PAGE_PEM),
                page,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                GetCacheKey(CacheHelper.SIZE_PEM),
                size,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );
        }

        [HttpGet]
        [Route("view/{id:guid}")]
        public async Task<ActionResult> ViewEmendamento(Guid id, string notificaId = "")
        {
            var apiGateway = new ApiGateway(Token);
            if (!string.IsNullOrEmpty(notificaId)) await apiGateway.Notifiche.NotificaVista(notificaId);
            var em = await apiGateway.Emendamento.Get(id);

            if (string.IsNullOrEmpty(em.EM_Certificato))
                em.BodyEM = await apiGateway.Emendamento.GetBody(id, TemplateTypeEnum.HTML);
            else
                em.BodyEM = em.EM_Certificato;

            em.Firme = await Utility.GetFirmatari(
                await apiGateway.Emendamento.GetFirmatari(id, FirmeTipoEnum.PRIMA_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.PRIMA_DEPOSITO, Token);
            em.Firme_dopo_deposito = await Utility.GetFirmatari(
                await apiGateway.Emendamento.GetFirmatari(id, FirmeTipoEnum.DOPO_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.DOPO_DEPOSITO, Token);
            if (em.IDStato <= (int)StatiEnum.Depositato)
                em.Destinatari =
                    await Utility.GetDestinatariNotifica(await apiGateway.Emendamento.GetInvitati(id), Token);
            em.ATTI = await apiGateway.Atti.Get(em.UIDAtto);

            Session["RicaricaFiltri"] = true;

            return View(em);
        }

        [HttpGet]
        [Route("{id:guid}/meta-data")]
        public async Task<ActionResult> GetMetaData(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var em = await apiGateway.Emendamento.Get(id);

                if (em.ATTI == null)
                    em.ATTI = await apiGateway.Atti.Get(em.UIDAtto);

                return Json(em, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per aggiungere un emendamento. Restituisce il modello dell'emendamento pre-compilato.
        /// </summary>
        /// <param name="id">Guid atto di riferimento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/new")]
        public async Task<ActionResult> NuovoEmendamento(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var emModel = await apiGateway.Emendamento.GetNuovoModel(id, Guid.Empty);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("EmendamentoFormAdmin", emModel);
            if (HttpContext.User.IsInRole(RuoliExt.Segreteria_Giunta_Regionale) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Politica) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Giunta) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Politica))
                return View("EmendamentoFormSegreteria", emModel);
            return View("EmendamentoForm", emModel);
        }

        /// <summary>
        ///     Controller per aggiungere un sub-emendamento. Restituisce il modello dell'emendamento pre-compilato.
        /// </summary>
        /// <param name="id">Guid atto di riferimento</param>
        /// <param name="ref_em">Guid emendamento di riferimento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/new/{ref_em:guid}")]
        public async Task<ActionResult> NuovoSUBEmendamento(Guid id, Guid ref_em)
        {
            var apiGateway = new ApiGateway(Token);
            var emModel = await apiGateway.Emendamento.GetNuovoModel(id, ref_em);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("EmendamentoFormAdmin", emModel);
            if (HttpContext.User.IsInRole(RuoliExt.Segreteria_Giunta_Regionale) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Politica) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Giunta) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Politica))
                return View("EmendamentoFormSegreteria", emModel);
            return View("EmendamentoForm", emModel);
        }

        /// <summary>
        ///     Controller per modificare un emendamento. Restituisce il modello dell'emendamento pre-compilato.
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/edit")]
        public async Task<ActionResult> ModificaEmendamento(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var emModel = await apiGateway.Emendamento.GetModificaModel(id);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("EmendamentoFormAdmin", emModel);
            if (HttpContext.User.IsInRole(RuoliExt.Segreteria_Giunta_Regionale) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Politica) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Giunta) ||
                HttpContext.User.IsInRole(RuoliExt.Responsabile_Segreteria_Politica))
                return View("EmendamentoFormSegreteria", emModel);
            return View("EmendamentoForm", emModel);
        }

        /// <summary>
        ///     Controller per aggiungere o modificare un emendamento
        /// </summary>
        /// <param name="model">Modello emendamento</param>
        /// <returns></returns>
        [Route("salva")]
        [HttpPost]
        public async Task<ActionResult> SalvaEmendamento(EmendamentiDto model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                Session["RiepilogoEmendamenti"] = null;
                var uidEm = model.UIDEM;
                if (model.UIDEM == Guid.Empty)
                {
                    var newEm = await apiGateway.Emendamento.Salva(model);
                    uidEm = newEm.UIDEM;
                    //return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                    //{
                    //    id = model.UIDAtto
                    //}), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    await apiGateway.Emendamento.Modifica(model);
                }


                return Json(Url.Action("ViewEmendamento", "Emendamenti", new
                {
                    id = uidEm
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per modificare i metadati di un emendamento. Restituisce il modello dell'emendamento pre-compilato.
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/edit-meta-dati")]
        public async Task<ActionResult> ModificaMetaDatiEmendamento(Guid id)
        {
            Session["RicaricaFiltri"] = true;
            var apiGateway = new ApiGateway(Token);
            var emModel = await apiGateway.Emendamento.GetModificaMetaDatiModel(id);
            return View("MetaDatiForm", emModel);
        }

        /// <summary>
        ///     Controller per aggiungere o modificare i metadati di un emendamento
        /// </summary>
        /// <param name="model">Modello emendamento</param>
        /// <returns></returns>
        [Route("meta-dati")]
        [HttpPost]
        public async Task<ActionResult> SalvaMetaDatiEmendamento(EmendamentiFormModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.ModificaMetaDati(model.Emendamento);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                {
                    id = model.Emendamento.UIDAtto
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per modificare i metadati di un emendamento
        /// </summary>
        /// <param name="model">Modello emendamento</param>
        /// <returns></returns>
        [Route("meta-dati-em")]
        [HttpPost]
        public async Task<ActionResult> SalvaMetaDatiEM(EmendamentiDto model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.ModificaMetaDati(model);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                {
                    id = model.UIDAtto
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Esegui azione su emendamento selezionato
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="azione"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("azioni")]
        public async Task<ActionResult> EseguiAzione(Guid id, int azione, string pin = "")
        {
            try
            {
                Session["RiepilogoEmendamenti"] = null;
                var apiGateway = new ApiGateway(Token);
                switch ((ActionEnum)azione)
                {
                    case ActionEnum.ELIMINA:
                        var em = await apiGateway.Emendamento.Get(id);
                        await apiGateway.Emendamento.Elimina(id);
                        return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                        {
                            id = em.UIDAtto
                        }), JsonRequestBehavior.AllowGet);
                    case ActionEnum.RITIRA:
                        await apiGateway.Emendamento.Ritira(id);
                        break;
                    case ActionEnum.FIRMA:
                        var resultFirma = await apiGateway.Emendamento.Firma(id, pin);
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
                        var resultDeposita = await apiGateway.Emendamento.Deposita(id, pin);
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
        ///     Esegui azioni massive
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
                    var listaView = new EmendamentiViewModel();
                    var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;
                    var request = new BaseRequest<EmendamentiDto>
                    {
                        id = modelInCache.Atto.UIDAtto,
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Total,
                        filtro = modelInCache.Data.Filters,
                        ordine = modelInCache.Ordinamento,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.Mode } }
                    };
                    if (model.Richiesta_Firma) // #879 (fix) Azione massiva: Visualizza solo gli EM/SUBEM per i quali è richiesta la mia firma + Seleziona tutti + Firma massiva
                        listaView = await apiGateway.Emendamento.Get_RichiestaPropriaFirma(request.id,
                            modelInCache.Mode, modelInCache.Ordinamento, modelInCache.Data.Paging.Page, modelInCache.Data.Paging.Limit);
                    else
                        listaView = await apiGateway.Emendamento.Get(request);
                    var list = listaView.Data.Results.Select(a => a.UIDEM).ToList();

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                switch (model.Azione)
                {
                    case ActionEnum.FIRMA:
                        var resultFirma = await apiGateway.Emendamento.Firma(model);
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

                        return Json(
                            new
                            {
                                message =
                                    "Nessuna firma effettuata"
                            }, JsonRequestBehavior.AllowGet);
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await apiGateway.Emendamento.Deposita(model);
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


                        return Json(
                            new
                            {
                                message =
                                    "Nessuna deposito effettuato"
                            }, JsonRequestBehavior.AllowGet);
                    case ActionEnum.INVITA:
                        var resultInvita = await apiGateway.Notifiche.NotificaEM(model);
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
        ///     Ritira firma
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="pin">pin</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ritiro-firma")]
        public async Task<ActionResult> RitiroFirma(Guid id, string pin = "")
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var resultRitiro = await apiGateway.Emendamento.RitiraFirma(id, pin);
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
        ///     Elimina firma
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="pin">pin</param>
        /// <returns></returns>
        [Route("elimina-firma")]
        public async Task<ActionResult> EliminaFirma(Guid id, string pin = "")
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var resultEliminaFirma = await apiGateway.Emendamento.EliminaFirma(id, pin);
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
        ///     Restituisce i dati dei firmatari per un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="tipo"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("firmatari")]
        public async Task<ActionResult> GetFirmatariEmendamento(Guid id, FirmeTipoEnum tipo, bool tag = false)
        {
            var apiGateway = new ApiGateway(Token);

            var firme = await apiGateway.Emendamento.GetFirmatari(id, tipo);
            var result = await Utility.GetFirmatari(firme, CurrentUser.UID_persona, tipo, Token, tag);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Restituisce i dati degli inviti per un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("preview")]
        public async Task<ActionResult> GetBody_Anteprima(Guid id, TemplateTypeEnum type)
        {
            var apiGateway = new ApiGateway(Token);
            var result = await apiGateway.Emendamento.GetBody(id, type);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per esportare gli emendamenti di un atto
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("esporta-xls")]
        public async Task<ActionResult> EsportaXLS()
        {
            try
            {
                var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;

                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.Esporta.EsportaXLS(modelInCache);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per esportare gli emendamenti di un atto
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("esporta-xls-segreteria")]
        public async Task<ActionResult> EsportaXLS_UOLA()
        {
            try
            {
                var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;

                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.Esporta.EsportaXLS_UOLA(modelInCache);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per esportare gli emendamenti di un atto
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("esportaDOC")]
        public async Task<ActionResult> EsportaDOC(Guid id, OrdinamentoEnum ordine)
        {
            try
            {
                var mode = (ClientModeEnum)HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE));
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.Esporta.EsportaWORD(id, ordine, mode);
                return Json(file.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per modificare lo stato di una lista di emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("modifica-stato")]
        public async Task<ActionResult> ModificaStatoEmendamento(ModificaStatoModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                if (model.Tutti)
                {
                    var listaView = new EmendamentiViewModel();
                    var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;
                    var request = new BaseRequest<EmendamentiDto>
                    {
                        id = modelInCache.Atto.UIDAtto,
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Total,
                        filtro = modelInCache.Data.Filters,
                        ordine = modelInCache.Ordinamento,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.Mode } }
                    };
                    listaView = await apiGateway.Emendamento.Get(request);
                    var list = listaView.Data.Results.Select(a => a.UIDEM).ToList();

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                await apiGateway.Emendamento.CambioStato(model);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per raggruppare emendamenti assegnando un colore esadecimale
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("raggruppa")]
        public async Task<ActionResult> RaggruppaEmendamenti(RaggruppaEmendamentiModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                if (model.Tutti)
                {
                    var listaView = new EmendamentiViewModel();
                    var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;
                    var request = new BaseRequest<EmendamentiDto>
                    {
                        id = modelInCache.Atto.UIDAtto,
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Total,
                        filtro = modelInCache.Data.Filters,
                        ordine = modelInCache.Ordinamento,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.Mode } }
                    };
                    listaView = await apiGateway.Emendamento.Get(request);
                    var list = listaView.Data.Results.Select(a => a.UIDEM).ToList();

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                var resultRaggruppamento = await apiGateway.Emendamento.Raggruppa(model);
                var listaErroriRaggruppamento = new List<string>();
                foreach (var item in resultRaggruppamento)
                    listaErroriRaggruppamento.Add($"{item.Value}");

                if (listaErroriRaggruppamento.Count > 0)
                    return Json(
                        new
                        {
                            message =
                                "Raggruppamento eseguito con successo!"
                        }, JsonRequestBehavior.AllowGet);


                return Json(
                    new
                    {
                        message =
                            "Nessuna raggruppamento effettuato"
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per assegnare un nuovo proponente ad una lista di emendamenti ritirati
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("assegna-nuovo-proponente")]
        public async Task<ActionResult> AssegnaNuovoPorponente(AssegnaProponenteModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                if (model.Tutti)
                {
                    var listaView = new EmendamentiViewModel();
                    var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;
                    var request = new BaseRequest<EmendamentiDto>
                    {
                        id = modelInCache.Atto.UIDAtto,
                        page = modelInCache.Data.Paging.Page,
                        size = modelInCache.Data.Paging.Total,
                        filtro = modelInCache.Data.Filters,
                        ordine = modelInCache.Ordinamento,
                        param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.Mode } }
                    };
                    listaView = await apiGateway.Emendamento.Get(request);
                    var list = listaView.Data.Results.Select(a => a.UIDEM).ToList();

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                var resultNuovoProponente = await apiGateway.Emendamento.AssegnaNuovoPorponente(model);
                var listaErroriNuovoProponente = new List<string>();
                foreach (var item in resultNuovoProponente)
                    listaErroriNuovoProponente.Add(
                        $"{listaErroriNuovoProponente.Count + 1} - {item.Value}");
                if (listaErroriNuovoProponente.Count > 0)
                    throw new Exception(
                        $"Riepilogo procedura di assegnazione nuovo proponente: {listaErroriNuovoProponente.Aggregate((i, j) => i + ", " + j)}");
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare gli emendamenti di un atto in votazione
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordina")]
        public async Task<ActionResult> ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);

                await apiGateway.Emendamento.ORDINA_EM_TRATTAZIONE(id);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per comunicare l'effettiva conclusione dell'operazione di ordinamento emendamenti nell'atto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("ordinamento-concluso")]
        public async Task<ActionResult> OrdinamentoConcluso(ComandiAzioneModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.OrdinamentoConcluso(model);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione superiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("ordina-up")]
        public async Task<ActionResult> UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.UP_EM_TRATTAZIONE(id);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione inferiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("ordina-down")]
        public async Task<ActionResult> DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.DOWN_EM_TRATTAZIONE(id);
                Session["RiepilogoEmendamenti"] = null;
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione precisa
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="pos">Int posizione</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("sposta")]
        public async Task<ActionResult> SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Emendamento.SPOSTA_EM_TRATTAZIONE(id, pos);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("tags")]
        public async Task<ActionResult> GetTags()
        {
            var apiGateway = new ApiGateway(Token);
            var result = await apiGateway.Emendamento.GetTags();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per scaricare il documento pdf dell'emendamento
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
                var file = await apiGateway.Emendamento.Download(id);
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
        [Route("stati-em")]
        public async Task<ActionResult> Filtri_GetStatiEM()
        {
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Emendamento.GetStati(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("tipi-em")]
        public async Task<ActionResult> Filtri_GetTipiEM()
        {
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Emendamento.GetTipi(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("parti-em")]
        public async Task<ActionResult> Filtri_GetPartiEM()
        {
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Emendamento.GetParti(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("missioni-em")]
        public async Task<ActionResult> Filtri_GetMissioniEM()
        {
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Emendamento.GetMissioni(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("titoli-missioni-em")]
        public async Task<ActionResult> Filtri_GetTitoliMissioniEM()
        {
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Emendamento.GetTitoliMissioni(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("filtra")]
        public async Task<ActionResult> Filtri_RiepilogoEM()
        {
            Session["RiepilogoEmendamenti"] = null;
            var mode = 1;
            var model = ElaboraFiltriEM(ref mode);

            int.TryParse(Request.Form["reset"], out var reset_enabled);
            if (reset_enabled == 1)
                return RedirectToAction("RiepilogoEmendamenti", "Emendamenti", new
                {
                    model.id,
                    mode,
                    ordine = (int)model.ordine
                });

            var apiGateway = new ApiGateway(Token);
            var modelResult = await apiGateway.Emendamento.Get(model);

            if (modelResult.ViewMode == ViewModeEnum.PREVIEW)
                foreach (var emendamentiDto in modelResult.Data.Results)
                    emendamentiDto.BodyEM =
                        await apiGateway.Emendamento.GetBody(emendamentiDto.UIDEM, TemplateTypeEnum.HTML);

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
            {
                Session["RiepilogoEmendamenti"] = modelResult;
                return View("RiepilogoEM_Admin", modelResult);
            }

            if (Convert.ToInt16(mode) == (int)ClientModeEnum.GRUPPI)
                foreach (var emendamentiDto in modelResult.Data.Results)
                    if (emendamentiDto.STATI_EM.IDStato <= (int)StatiEnum.Depositato)
                    {
                        if (emendamentiDto.ConteggioFirme > 0)
                            emendamentiDto.Firmatari = await Utility.GetFirmatari(
                                await apiGateway.Emendamento.GetFirmatari(emendamentiDto.UIDEM, FirmeTipoEnum.TUTTE),
                                CurrentUser.UID_persona, FirmeTipoEnum.TUTTE, Token, true);

                        emendamentiDto.Destinatari =
                            await Utility.GetDestinatariNotifica(
                                await apiGateway.Emendamento.GetInvitati(emendamentiDto.UIDEM), Token);
                    }

            Session["RiepilogoEmendamenti"] = modelResult;
            return View("RiepilogoEM", modelResult);
        }

        private BaseRequest<EmendamentiDto> ElaboraFiltriEM(ref int mode)
        {
            int.TryParse(Request.Form["page"], out var filtro_page);
            int.TryParse(Request.Form["size"], out var filtro_size);
            int.TryParse(Request.Form["mode"], out var mode_result);
            if (mode_result == 0)
                mode_result = 1;
            int.TryParse(Request.Form["ordine"], out var ordine);
            if (ordine == 0)
                ordine = 1;
            var view = Request.Form["view"];
            var atto = Request.Form["atto"];
            var filtro_text1 = Request.Form["filtro_text1"];
            var filtro_text2 = Request.Form["filtro_text2"];
            int.TryParse(Request.Form["filtro_text_connector"], out var filtro_text_connector);
            var filtro_n_em = Request.Form["filtro_n_em"];
            var filtro_stato = Request.Form["filtro_stato"];
            var filtro_tipo = Request.Form["filtro_tipo"];
            var filtro_parte = Request.Form["filtro_parte"];
            var filtro_parte_articolo = Request.Form["filtro_parte_articolo"];
            var filtro_parte_comma = Request.Form["filtro_parte_comma"];
            var filtro_parte_lettera = Request.Form["filtro_parte_lettera"];
            var filtro_parte_letteraOLD = Request.Form["filtro_parte_letteraOLD"];
            var filtro_parte_titolo = Request.Form["filtro_parte_titolo"];
            var filtro_parte_capo = Request.Form["filtro_parte_capo"];
            var filtro_parte_missione = Request.Form["filtro_parte_missione"];
            var filtro_parte_programma = Request.Form["filtro_parte_programma"];
            var filtro_my = Request.Form["filtro_my"];
            var filtro_effetti_finanziari = Request.Form["filtro_effetti_finanziari"];
            var filtro_gruppo = Request.Form["filtro_gruppo"];
            var filtro_proponente = Request.Form["filtro_proponente"];
            var filtro_firmatari = Request.Form["filtro_firmatari"];
            var filtro_tags = Request.Form["tags"];

            mode = mode_result;
            if (ordine == 0)
                ordine = 1;
            var model = new BaseRequest<EmendamentiDto>
            {
                page = filtro_page,
                size = filtro_size,
                param = new Dictionary<string, object> { { "CLIENT_MODE", mode_result }, { "VIEW_MODE", view } },
                ordine = (OrdinamentoEnum)ordine,
                id = new Guid(atto)
            };

            Common.Utility.AddFilter_ByAtto(ref model, atto);
            Common.Utility.AddFilter_ByText(ref model, filtro_text1, filtro_text2, filtro_text_connector);
            Common.Utility.AddFilter_ByNUM(ref model, filtro_n_em);
            Common.Utility.AddFilter_ByState(ref model, filtro_stato);
            Common.Utility.AddFilter_ByPart(ref model,
                filtro_parte, filtro_parte_titolo, filtro_parte_capo,
                filtro_parte_articolo, filtro_parte_comma, filtro_parte_lettera, filtro_parte_letteraOLD,
                filtro_parte_missione, filtro_parte_programma);
            Common.Utility.AddFilter_ByType(ref model, filtro_tipo);
            Common.Utility.AddFilter_My(ref model, CurrentUser.UID_persona, filtro_my);
            Common.Utility.AddFilter_Financials(ref model, filtro_effetti_finanziari);
            Common.Utility.AddFilter_Groups(ref model, filtro_gruppo);
            Common.Utility.AddFilter_Proponents(ref model, filtro_proponente);
            Common.Utility.AddFilter_Signers(ref model, filtro_firmatari);
            Common.Utility.AddFilter_Tags(ref model, filtro_tags);

            return model;
        }
    }
}