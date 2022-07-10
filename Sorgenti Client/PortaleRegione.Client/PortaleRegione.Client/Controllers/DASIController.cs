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
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.Logger;

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
        public async Task<ActionResult> RiepilogoDASI(int page = 1, int size = 50, int view = (int) ViewModeEnum.GRID,
            int stato = (int) StatiAttoEnum.BOZZA, int tipo = (int) TipoAttoEnum.TUTTI)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.Get(page, size, (StatiAttoEnum) stato, (TipoAttoEnum) tipo,
                _CurrentUser.CurrentRole);
            model.CurrentUser = _CurrentUser;
            SetCache(page, size, tipo, stato, view);
            if (view == (int) ViewModeEnum.PREVIEW)
            {
                model.ViewMode = ViewModeEnum.PREVIEW;
                foreach (var atti in model.Data.Results)
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);
            }

            Session["RiepilogoDASI"] = model;

            if (CanAccess(new List<RuoliIntEnum> {RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea}))
                return View("RiepilogoDASI_Admin", model);

            return View("RiepilogoDASI", model);
        }

        /// <summary>
        ///     Endpoint per visualizzare il riepilogo degli Atti di Sindacato ispettivo in base alla seduta
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("seduta")]
        public async Task<ActionResult> RiepilogoDASI_BySeduta(Guid id, int tipo = (int) TipoAttoEnum.TUTTI,
            int page = 1, int size = 50, int view = (int) ViewModeEnum.GRID,
            int stato = (int) StatiAttoEnum.PRESENTATO)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.GetBySeduta_Trattazione(id, (TipoAttoEnum) tipo, page, size);
            CheckCacheClientMode(ClientModeEnum.TRATTAZIONE);
            model.ClientMode = ClientModeEnum.TRATTAZIONE;

            SetCache(page, size, tipo, stato, view);
            if (view == (int) ViewModeEnum.PREVIEW)
            {
                model.ViewMode = ViewModeEnum.PREVIEW;
                foreach (var atti in model.Data.Results)
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);
            }

            Session["RiepilogoDASI"] = model;

            if (CanAccess(new List<RuoliIntEnum> {RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea}))
                return View("RiepilogoDASI_Admin", model);

            return View("RiepilogoDASI", model);
        }

        /// <summary>
        ///     Controller per aggiungere un atto di sindacato ispettivo. Restituisce il modello dell'atto pre-compilato.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("new")]
        public async Task<ActionResult> Nuovo(int tipo)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.GetNuovoModello((TipoAttoEnum) tipo);
            if (_CurrentUser.IsSegreteriaPolitica)
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
                var apiGateway = new ApiGateway(_Token);
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
                var apiGateway = new ApiGateway(_Token);
                var atto = await apiGateway.DASI.Get(id);
                if (!string.IsNullOrEmpty(atto.Oggetto_Modificato)
                    || !string.IsNullOrEmpty(atto.Premesse_Modificato)
                    || !string.IsNullOrEmpty(atto.Richiesta_Modificata)
                    || string.IsNullOrEmpty(atto.Atto_Certificato))
                    atto.BodyAtto = await apiGateway.DASI.GetBody(id, TemplateTypeEnum.HTML);
                else
                    atto.BodyAtto = atto.Atto_Certificato;

                atto.Firme = await Utility.GetFirmatari(
                    await apiGateway.DASI.GetFirmatari(id, FirmeTipoEnum.PRIMA_DEPOSITO),
                    _CurrentUser.UID_persona, _Token);

                if (atto.IDStato <= (int) StatiAttoEnum.PRESENTATO)
                    atto.Destinatari =
                        await Utility.GetDestinatariNotifica(await apiGateway.DASI.GetInvitati(id), _Token);

                return View("AttoDASIView", atto);
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
                var apiGateway = new ApiGateway(_Token);
                switch ((ActionEnum) azione)
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
                var apiGateway = new ApiGateway(_Token);
                if (model.Lista == null || !model.Lista.Any())
                {
                    var listaAtti = new RiepilogoDASIModel();
                    var limit = Convert.ToInt32(AppSettingsConfiguration.LimiteDocumentiDaProcessare);
                    var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = 1,
                        size = limit,
                        filtro = modelInCache.Data.Filters,
                        param = new Dictionary<string, object> {{"CLIENT_MODE", (int) modelInCache.ClientMode}}
                    };
                    listaAtti = await apiGateway.DASI.Get(request);
                    model.Lista = listaAtti.Data.Results.Select(a => a.UIDAtto).ToList();
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
                var apiGateway = new ApiGateway(_Token);
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
                var apiGateway = new ApiGateway(_Token);
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
        [Route("{id:guid}/edit")]
        public async Task<ActionResult> Modifica(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.GetModificaModello(id);
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
                var apiGateway = new ApiGateway(_Token);
                model.CurrentStatus = (StatiAttoEnum) Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI));
                model.CurrentType = (TipoAttoEnum) Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI));
                await apiGateway.DASI.CambioStato(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = (StatiAttoEnum) Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI)),
                    tipo = (TipoAttoEnum) Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI))
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
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.DASI.IscriviSeduta(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI)),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI))
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
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.DASI.RichiediIscrizione(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI)),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI))
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
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.DASI.RimuoviSeduta(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI)),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI))
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
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.DASI.RimuoviRichiestaIscrizione(model);
                var url = Url.Action("RiepilogoDASI", new
                {
                    stato = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.STATO_DASI)),
                    tipo = Convert.ToInt16(HttpContext.Cache.Get(CacheHelper.TIPO_DASI))
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
        ///     Controller per andare nella pagina preview degli atti
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="tipo"></param>
        /// <param name="stato"></param>
        /// <param name="viewModeEnum"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("preview")]
        //public async Task<ActionResult> Preview(int mode)
        //{
        //    var _mode = (ViewModeEnum) mode;
        //    if (_mode == ViewModeEnum.GRID)
        //    {
        //        //Ritorna la griglia
        //    }
        //}
        [HttpGet]
        [Route("{id:guid}/meta-data")]
        public async Task<ActionResult> GetMetaData(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
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
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.DASI.ModificaMetaDati(model);
                return Json(Url.Action("RiepilogoDASI", "DASI"), JsonRequestBehavior.AllowGet);
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
        [HttpGet]
        [Route("esportaXLS")]
        public async Task<ActionResult> EsportaXLS()
        {
            try
            {
                var model = Session["RiepilogoDASI"] as RiepilogoDASIModel;
                var apiGateway = new ApiGateway(_Token);
                var file = await apiGateway.Esporta.EsportaXLSDASI(model);
                return Json(Convert.ToBase64String(file.Content), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("EsportaXLSDASI", e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        private void SetCache(int page, int size, int tipo, int stato, int viewModeEnum)
        {
            HttpContext.Cache.Insert(
                CacheHelper.TIPO_DASI,
                tipo,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                CacheHelper.STATO_DASI,
                stato,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                CacheHelper.PAGE_DASI,
                page,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                CacheHelper.SIZE_DASI,
                size,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                CacheHelper.VIEW_MODE_DASI,
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
            Session["RiepilogoDASI"] = null;
            int.TryParse(Request.Form["reset"], out var reset_enabled);
            var mode = (ClientModeEnum) HttpContext.Cache.Get(CacheHelper.CLIENT_MODE);
            var view = Request.Form["view"];

            if (reset_enabled == 1)
            {
                if (mode == ClientModeEnum.GRUPPI)
                    return RedirectToAction("RiepilogoDASI", "DASI");

                var filtro_tipo_trattazione = Request.Form["Tipo"];
                var filtro_seduta = Request.Form["UIDSeduta"];
                return RedirectToAction("RiepilogoDASI_BySeduta", "DASI",
                    new {id = filtro_seduta, tipo = filtro_tipo_trattazione});
            }

            var apiGateway = new ApiGateway(_Token);
            var model = await ElaboraFiltri();
            var result = await apiGateway.DASI.Get(model);
            result.CurrentUser = _CurrentUser;
            result.ClientMode = mode;
            if (Convert.ToInt16(view) == (int) ViewModeEnum.PREVIEW)
                foreach (var atti in result.Data.Results)
                {
                    result.ViewMode = ViewModeEnum.PREVIEW;
                    atti.BodyAtto =
                        await apiGateway.DASI.GetBody(atti.UIDAtto, TemplateTypeEnum.HTML);
                }

            Session["RiepilogoDASI"] = result;

            if (CanAccess(new List<RuoliIntEnum> {RuoliIntEnum.Amministratore_PEM, RuoliIntEnum.Segreteria_Assemblea}))
                return View("RiepilogoDASI_Admin", result);

            return View("RiepilogoDASI", result);
        }

        private async Task<BaseRequest<AttoDASIDto>> ElaboraFiltri()
        {
            var mode = (ClientModeEnum) HttpContext.Cache.Get(CacheHelper.CLIENT_MODE);
            int.TryParse(Request.Form["page"], out var filtro_page);
            int.TryParse(Request.Form["size"], out var filtro_size);
            var view = Request.Form["view"];
            var filtro_oggetto = Request.Form["filtro_oggetto"];
            var filtro_stato = Request.Form["filtro_stato"];
            var filtro_tipo = Request.Form["filtro_tipo"];
            var filtro_tipo_risposta = Request.Form["filtro_tipo_risposta"];
            var filtro_natto = Request.Form["filtro_natto"];
            var filtro_natto2 = Request.Form["filtro_natto2"];
            var filtro_da = Request.Form["filtro_da"];
            var filtro_a = Request.Form["filtro_a"];
            var filtro_data_seduta = Request.Form["filtro_data_seduta"];
            var filtro_tipo_trattazione = Request.Form["Tipo"];
            var filtro_soggetto_dest = Request.Form["filtro_soggetto_dest"];
            var filtro_seduta = Request.Form["UIDSeduta"];
            var filtro_legislatura = Request.Form["filtro_legislatura"];

            var model = new BaseRequest<AttoDASIDto>
            {
                page = filtro_page != 0 ? filtro_page : 1,
                size = filtro_size,
                param = new Dictionary<string, object> {{"CLIENT_MODE", (int) mode}, {"VIEW_MODE", view}}
            };

            var util = new UtilityFilter();

            util.AddFilter_ByNumeroAtto(ref model, filtro_natto, filtro_natto2);
            util.AddFilter_ByDataPresentazione(ref model, filtro_da, filtro_a);
            var sedutaUId = await GetSedutaByData(filtro_data_seduta);
            util.AddFilter_ByDataSeduta(ref model, sedutaUId);
            util.AddFilter_ByOggetto_Testo(ref model, filtro_oggetto);
            util.AddFilter_ByStato(ref model, filtro_stato);
            util.AddFilter_ByTipoRisposta(ref model, filtro_tipo_risposta);
            util.AddFilter_ByTipo(ref model, filtro_tipo, filtro_tipo_trattazione, mode);
            util.AddFilter_BySoggetto(ref model, filtro_soggetto_dest);
            util.AddFilter_BySeduta(ref model, filtro_seduta);
            util.AddFilter_ByLegislatura(ref model, filtro_legislatura);

            return model;
        }

        private async Task<Guid> GetSedutaByData(string filtroDataSeduta)
        {
            var result = Guid.Empty;
            DateTime data;
            var success = DateTime.TryParse(filtroDataSeduta, out data);

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
                var gate = new ApiGateway(_Token);
                var resultSedute = await gate.Sedute.Get(modelSedute);
                if (resultSedute.Results.Any()) result = resultSedute.Results.First().UIDSeduta;
            }

            return result;
        }

        //FILTRI

        [HttpGet]
        [Route("stati")]
        public async Task<ActionResult> Filtri_GetStati()
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                return Json(await apiGateway.DASI.GetStati(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("tipi")]
        public async Task<ActionResult> Filtri_GetTipi()
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                return Json(await apiGateway.DASI.GetTipi(), JsonRequestBehavior.AllowGet);
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
                var apiGateway = new ApiGateway(_Token);
                return Json(await apiGateway.DASI.GetSoggettiInterrogabili(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NuovoCartaceo(int tipo)
        {
            throw new NotImplementedException();
        }
    }
}