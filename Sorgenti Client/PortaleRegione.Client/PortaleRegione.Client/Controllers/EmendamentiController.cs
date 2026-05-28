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
using ExpressionBuilder.Generics;
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
            int size = 20)
        {
            var mode = ClientModeEnum.GRUPPI;
            var contextMode = HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE));
            if (contextMode != null) mode = (ClientModeEnum)Convert.ToInt16(contextMode);

            SetCache(page, size, ordine, view);

            // v2026.5.1 - Fix doppia chiamata GetEmendamenti.
            // Il punto di ingresso restituisce ora SOLO l'involucro della pagina
            // (atto + paging vuoto) senza interrogare la pipeline emendamenti: la
            // griglia viene popolata dal pannello filtri via POST AJAX a
            // /emendamenti/riepilogo-emendamenti (inviaDatiChipsEM). Prima il vecchio
            // ComposeModel chiamava apiGateway.Emendamento.Get() generando una prima
            // query inutile, doppia rispetto a quella AJAX.
            var apiGateway = new ApiGateway(Token);
            var atto = await apiGateway.Atti.Get(id);
            var model = new EmendamentiViewModel
            {
                Atto = atto,
                Mode = mode,
                ViewMode = view,
                Ordinamento = ordine,
                CurrentUser = CurrentUser,
                Data = new BaseResponse<EmendamentiDto>(
                    page,
                    size,
                    new List<EmendamentiDto>(),
                    new List<FilterStatement<EmendamentiDto>>(),
                    0,
                    Request.Url)
            };

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoEM_Admin", model);
            return View("RiepilogoEM", model);
        }

        /// <summary>
        ///     Controller per visualizzare i dati degli emendamenti contenuti in un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
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

        // v2026.5.1 - ComposeModel rimosso: la pipeline di caricamento del riepilogo EM
        // e' stata semplificata. L'intestazione viene costruita inline in
        // RiepilogoEmendamenti (sola query Atti.Get), la griglia parte da lista vuota
        // e viene popolata dall'AJAX inviaDatiChipsEM in _FiltriRapidiEMPanel.cshtml.
        // L'arricchimento PREVIEW (BodyEM) e quello del consigliere (Firmatari /
        // Destinatari sugli emendamenti in stato <= Depositato) sono coperti dalla
        // pipeline server-side che alimenta l'AJAX, gli arricchimenti client-side
        // erano duplicati.

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
                await apiGateway.Emendamento.GetFirmatari(id, FirmeTipoEnum.TUTTE),
                CurrentUser.UID_persona, FirmeTipoEnum.TUTTE, Token);
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
                if (model.DocAllegatoGenerico != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                        model.DocAllegatoGenerico_Stream = memoryStream.ToArray();
                    }

                    // AGGIUNGERE VALIDAZIONE:
                    var validationAllegato = FileValidator.ValidateFile(
                        model.DocAllegatoGenerico.FileName,
                        model.DocAllegatoGenerico.ContentType,
                        model.DocAllegatoGenerico_Stream
                    );

                    if (!validationAllegato.IsValid)
                    {
                        return Json(new ErrorResponse(validationAllegato.ErrorMessage),
                            JsonRequestBehavior.AllowGet);
                    }

                    if (FileValidator.IsZipFile(model.DocAllegatoGenerico.FileName,
                            model.DocAllegatoGenerico_Stream))
                    {
                        return Json(new ErrorResponse(
                                "File ZIP non consentiti."),
                            JsonRequestBehavior.AllowGet);
                    }
                }

                if (model.DocEffettiFinanziari != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.DocEffettiFinanziari.InputStream.CopyToAsync(memoryStream);
                        model.DocEffettiFinanziari_Stream = memoryStream.ToArray();
                    }

                    // AGGIUNGERE VALIDAZIONE:
                    var validationEffetti = FileValidator.ValidateFile(
                        model.DocEffettiFinanziari.FileName,
                        model.DocEffettiFinanziari.ContentType,
                        model.DocEffettiFinanziari_Stream
                    );

                    if (!validationEffetti.IsValid)
                    {
                        return Json(new ErrorResponse(validationEffetti.ErrorMessage),
                            JsonRequestBehavior.AllowGet);
                    }

                    if (FileValidator.IsZipFile(model.DocEffettiFinanziari.FileName,
                            model.DocEffettiFinanziari_Stream))
                    {
                        return Json(new ErrorResponse(
                                "File ZIP non consentiti."),
                            JsonRequestBehavior.AllowGet);
                    }

                }

                var apiGateway = new ApiGateway(Token);
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
                        break;
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await apiGateway.Emendamento.Deposita(id, pin);
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
                    if (model.Filter == null)
                        return Json(new ErrorResponse(
                            "Filtri correnti non disponibili. Aggiornare la pagina e riprovare."),
                            JsonRequestBehavior.AllowGet);

                    var request = BuildBaseRequestEM(model.Filter);
                    request.size = -1;
                    var clientMode = (ClientModeEnum)model.Filter.clientMode;

                    var list = new List<Guid>();
                    if (model.Richiesta_Firma) // #879 Azione massiva: Visualizza solo gli EM per i quali e' richiesta la mia firma + Seleziona tutti + Firma massiva
                    {
                        var lista_propria_firma = await apiGateway.Emendamento.Get_RichiestaPropriaFirma(request.id,
                            clientMode, request.ordine, 1,
                            int.MaxValue);
                        list = lista_propria_firma.Data.Results.Select(i => i.UIDEM).ToList();
                    }
                    else
                    {
                        list = await apiGateway.Emendamento.GetSoloIds(request);
                    }

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
                        var resultDeposita = await apiGateway.Emendamento.Deposita(model);
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
                    if (model.Filter == null)
                        return Json(new ErrorResponse(
                            "Filtri correnti non disponibili. Aggiornare la pagina e riprovare."),
                            JsonRequestBehavior.AllowGet);

                    var request = BuildBaseRequestEM(model.Filter);
                    request.size = -1;
                    var list = await apiGateway.Emendamento.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                await apiGateway.Emendamento.CambioStato(model);
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
                    if (model.Filter == null)
                        return Json(new ErrorResponse(
                            "Filtri correnti non disponibili. Aggiornare la pagina e riprovare."),
                            JsonRequestBehavior.AllowGet);

                    var request = BuildBaseRequestEM(model.Filter);
                    request.size = -1;
                    var list = await apiGateway.Emendamento.GetSoloIds(request);

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
                    if (model.Filter == null)
                        return Json(new ErrorResponse(
                            "Filtri correnti non disponibili. Aggiornare la pagina e riprovare."),
                            JsonRequestBehavior.AllowGet);

                    var request = BuildBaseRequestEM(model.Filter);
                    request.size = -1;
                    var list = await apiGateway.Emendamento.GetSoloIds(request);

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

        /// <summary>
        ///     Endpoint asincrono del riepilogo Emendamenti: riceve la FilterRequestEM dal pannello
        ///     filtri JS, costruisce il BaseRequest e restituisce l'EmendamentiViewModel in JSON.
        /// </summary>
        [HttpPost]
        [Route("riepilogoEM")]
        public async Task<ActionResult> Riepilogo(FilterRequestEM model)
        {
            try
            {
                if (model == null || model.filters == null || !model.filters.Any())
                    return Json(new EmendamentiViewModel { CurrentUser = CurrentUser });

                var request = BuildBaseRequestEM(model);
                var apiGateway = new ApiGateway(Token);
                var res = await apiGateway.Emendamento.Get(request);

                // In modalita' PREVIEW il backend non popola BodyEM (campo pesante calcolato
                // dal template del singolo emendamento). Lo riempiamo qui chiamando GetBody per
                // ciascun risultato, come faceva il flusso legacy RiepilogoEmendamenti GET.
                if (res?.Data?.Results != null
                    && res.ViewMode == ViewModeEnum.PREVIEW)
                {
                    foreach (var dto in res.Data.Results)
                    {
                        if (dto == null) continue;
                        dto.BodyEM = await apiGateway.Emendamento.GetBody(dto.UIDEM, TemplateTypeEnum.HTML);
                    }
                }

                res.CurrentUser = CurrentUser;
                return Json(res);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Riepilogo "solo identificatori", usato dai comandi massivi quando il client
        ///     deve risolvere l'intero insieme che supera i filtri correnti.
        /// </summary>
        [HttpPost]
        [Route("riepilogoEMSoloIds")]
        public async Task<ActionResult> RiepilogoSoloIds(FilterRequestEM model)
        {
            try
            {
                if (model == null || model.filters == null || !model.filters.Any())
                    return Json(new List<Guid>());

                var request = BuildBaseRequestEM(model);
                var apiGateway = new ApiGateway(Token);
                var ids = await apiGateway.Emendamento.GetSoloIds(request);
                return Json(ids);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Endpoint del modale "Genera report": recupera l'insieme filtrato e lo passa
        ///     al gateway di esportazione nel formato richiesto (XLS/PDF/Word).
        /// </summary>
        [HttpPost]
        [Route("genera-report")]
        public async Task<ActionResult> GeneraReport(GeneraReportRequestEM model)
        {
            try
            {
                if (model?.Filter == null || model.Filter.filters == null || !model.Filter.filters.Any())
                    return Json(new ErrorResponse("Nessun filtro impostato"), JsonRequestBehavior.AllowGet);

                var request = BuildBaseRequestEM(model.Filter);
                request.size = -1;
                if (model.Columns != null && model.Columns.Count > 0)
                    request.columns = model.Columns;

                var apiGateway = new ApiGateway(Token);
                var viewModel = await apiGateway.Emendamento.Get(request);

                // Il select del modale espone ExportFormatEnum come stringa numerica
                // (1 = WORD, 2 = EXCEL); accettiamo anche le forme testuali per robustezza
                // verso eventuali integrazioni esterne.
                var formatRaw = (model.ExportFormat ?? string.Empty).Trim();
                ExportFormatEnum format;
                if (int.TryParse(formatRaw, out var formatInt)
                    && System.Enum.IsDefined(typeof(ExportFormatEnum), formatInt))
                {
                    format = (ExportFormatEnum)formatInt;
                }
                else if (!System.Enum.TryParse(formatRaw, true, out format))
                {
                    return Json(new ErrorResponse("Formato di esportazione non supportato"),
                        JsonRequestBehavior.AllowGet);
                }

                FileResponse file;
                switch (format)
                {
                    case ExportFormatEnum.EXCEL:
                        file = await apiGateway.Esporta.EsportaXLS(viewModel);
                        break;
                    case ExportFormatEnum.WORD:
                        file = await apiGateway.Esporta.EsportaWORD(viewModel);
                        break;
                    case ExportFormatEnum.EXCEL_UOLA:
                        // Export PEM dedicato alla segreteria UOLA (v2026.5.1):
                        // usa il gateway EsportaXLS_UOLA gia' esistente.
                        file = await apiGateway.Esporta.EsportaXLS_UOLA(viewModel);
                        break;
                    default:
                        return Json(new ErrorResponse("Formato di esportazione non supportato"),
                            JsonRequestBehavior.AllowGet);
                }

                return Json(file?.Url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Costruisce il <see cref="BaseRequest{EmendamentiDto}" /> a partire dalla
        ///     <see cref="FilterRequestEM" /> inviata dal client.
        /// </summary>
        private BaseRequest<EmendamentiDto> BuildBaseRequestEM(FilterRequestEM model)
        {
            var request = new BaseRequest<EmendamentiDto>
            {
                page = model.page > 0 ? model.page : 1,
                size = model.size != 0 ? model.size : 20,
                ordine = (OrdinamentoEnum)(model.ordine > 0 ? model.ordine : (int)OrdinamentoEnum.Presentazione),
                param = new Dictionary<string, object>
                {
                    { "CLIENT_MODE", model.clientMode },
                    { "VIEW_MODE", model.viewMode }
                }
            };

            if (model.sort_settings != null && model.sort_settings.Any())
                request.dettagliOrdinamento = model.sort_settings;

            if (model.columns_settings != null && model.columns_settings.Any())
                request.columns = model.columns_settings;

            request.filtro.AddRange(Common.Utility.ParseFilterEM(model.filters));

            var attoChip = model.filters
                .FirstOrDefault(f => f.property == nameof(EmendamentiDto.UIDAtto));
            if (attoChip != null && !string.IsNullOrEmpty(attoChip.value)
                && Guid.TryParse(attoChip.value, out var attoUid))
                request.id = attoUid;

            return request;
        }
    }
}