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
using Newtonsoft.Json;
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
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> RiepilogoEmendamenti(Guid id, ClientModeEnum mode,
            OrdinamentoEnum ordine = OrdinamentoEnum.Presentazione, int page = 1, int size = 50)
        {
            var view_grid = Request.QueryString["view"];
            var view_require_my_sign = Convert.ToBoolean(Request.QueryString["require_my_sign"]);

            HttpContext.Cache.Insert(
                "OrdinamentoEM",
                (int) ordine,
                null,
                DateTime.Now.AddMinutes(2),
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            EmendamentiViewModel model;
            if (view_require_my_sign == false)
                model = await EMGate.Get(id, mode, ordine, page, size);
            else
                model = await EMGate.Get_RichiestaPropriaFirma(id, mode, ordine, page, size);

            if (!string.IsNullOrEmpty(view_grid))
                foreach (var emendamentiDto in model.Data.Results)
                    emendamentiDto.BodyEM = await EMGate.GetBody(emendamentiDto.UIDEM, TemplateTypeEnum.PDF);

            if (HttpContext.User.IsInRole(RuoliEnum.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Assemblea))
                return View("RiepilogoEM_Admin", model);

            if (mode == ClientModeEnum.GRUPPI)
                foreach (var emendamentiDto in model.Data.Results)
                    if (emendamentiDto.STATI_EM.IDStato <= (int) StatiEnum.Depositato)
                    {
                        if (emendamentiDto.ConteggioFirme > 0)
                            emendamentiDto.Firmatari = await Utility.GetFirmatariEM(
                                await EMGate.GetFirmatari(emendamentiDto.UIDEM, FirmeTipoEnum.TUTTE),
                                CurrentUser.UID_persona, FirmeTipoEnum.TUTTE, true);

                        emendamentiDto.Destinatari =
                            await Utility.GetDestinatariNotifica(await EMGate.GetInvitati(emendamentiDto.UIDEM));
                    }

            return View("RiepilogoEM", model);
        }

        [HttpGet]
        [Route("view/{id:guid}")]
        public async Task<ActionResult> ViewEmendamento(Guid id, long notificaId = 0)
        {
            if (notificaId > 0) await NotificheGate.NotificaVista(notificaId);

            var em = await EMGate.Get(id);

            if (string.IsNullOrEmpty(em.EM_Certificato))
                em.BodyEM = await EMGate.GetBody(id, TemplateTypeEnum.HTML);
            else
                em.BodyEM = em.EM_Certificato;

            em.Firme = await Utility.GetFirmatariEM(
                await EMGate.GetFirmatari(id, FirmeTipoEnum.PRIMA_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.PRIMA_DEPOSITO);
            em.Firme_dopo_deposito = await Utility.GetFirmatariEM(
                await EMGate.GetFirmatari(id, FirmeTipoEnum.DOPO_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.DOPO_DEPOSITO);
            if (em.STATI_EM.IDStato <= (int) StatiEnum.Depositato)
                em.Destinatari =
                    await Utility.GetDestinatariNotifica(await EMGate.GetInvitati(id));

            return View(em);
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
            var emModel = await EMGate.GetNuovoModel(id, Guid.Empty);
            if (HttpContext.User.IsInRole(RuoliEnum.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Assemblea))
                return View("EmendamentoFormAdmin", emModel);
            if (HttpContext.User.IsInRole(RuoliEnum.Segreteria_Giunta_Regionale) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Politica) ||
                HttpContext.User.IsInRole(RuoliEnum.Responsabile_Segreteria_Giunta) ||
                HttpContext.User.IsInRole(RuoliEnum.Responsabile_Segreteria_Politica))
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
            var emModel = await EMGate.GetNuovoModel(id, ref_em);
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
            var emModel = await EMGate.GetModificaModel(id);
            if (HttpContext.User.IsInRole(RuoliEnum.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Assemblea))
                return View("EmendamentoFormAdmin", emModel);
            if (HttpContext.User.IsInRole(RuoliEnum.Segreteria_Giunta_Regionale) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Politica) ||
                HttpContext.User.IsInRole(RuoliEnum.Responsabile_Segreteria_Giunta) ||
                HttpContext.User.IsInRole(RuoliEnum.Responsabile_Segreteria_Politica))
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
                if (model.UIDEM == Guid.Empty)
                {
                    await EMGate.Salva(model);
                    return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                    {
                        id = model.UIDAtto
                    }), JsonRequestBehavior.AllowGet);
                }

                await EMGate.Modifica(model);
                return Json(Url.Action("ViewEmendamento", "Emendamenti", new
                {
                    id = model.UIDEM
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
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
            var emModel = await EMGate.GetModificaMetaDatiModel(id);
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
                await EMGate.ModificaMetaDati(model.Emendamento);
                return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                {
                    id = model.Emendamento.UIDAtto
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Esegui azione su emendamento selezionato
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("azioni")]
        public async Task<ActionResult> EseguiAzione(Guid id, int azione, string pin = "")
        {
            try
            {
                switch ((ActionEnum) azione)
                {
                    case ActionEnum.ELIMINA:
                        var em = await EMGate.Get(id);
                        await EMGate.Elimina(id);
                        return Json(Url.Action("RiepilogoEmendamenti", "Emendamenti", new
                        {
                            id = em.UIDAtto
                        }), JsonRequestBehavior.AllowGet);
                    case ActionEnum.RITIRA:
                        await EMGate.Ritira(id);
                        break;
                    case ActionEnum.FIRMA:
                        var resultFirma = await EMGate.Firma(id, pin);
                        var listaErroriFirma = new List<string>();
                        foreach (var itemFirma in resultFirma.Where(itemFirma => itemFirma.Value.Contains("ERROR")))
                            listaErroriFirma.Add($"{listaErroriFirma.Count + 1} - {itemFirma.Value.Substring(7)}");
                        if (listaErroriFirma.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"Errori nella procedura di firma: {listaErroriFirma.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await EMGate.Deposita(id, pin);
                        var listaErroriDeposito = new List<string>();
                        foreach (var itemDeposito in resultDeposita.Where(itemDeposito =>
                            itemDeposito.Value.Contains("ERROR")))
                            listaErroriDeposito.Add(
                                $"{listaErroriDeposito.Count + 1} - {itemDeposito.Value.Substring(7)}");
                        if (listaErroriDeposito.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"Errori nella procedura di deposito: {listaErroriDeposito.Aggregate((i, j) => i + ", " + j)}"
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
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Esegui azione su emendamento selezionato
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpPost]
        [Route("azioni-massive")]
        public async Task<ActionResult> EseguiAzioneMassive(ComandiAzioneModel model)
        {
            try
            {
                if (model.ListaEmendamenti == null || !model.ListaEmendamenti.Any())
                {
                    var listaEM = await EMGate.Get(model.AttoUId, (ClientModeEnum) model.ClientMode,
                        OrdinamentoEnum.Default, 1, 50);
                    model.ListaEmendamenti = listaEM.Data.Results.Select(em => em.UIDEM).ToList();
                }

                switch (model.Azione)
                {
                    case ActionEnum.FIRMA:
                        var resultFirma = await EMGate.Firma(model);
                        var listaErroriFirma = new List<string>();
                        foreach (var itemFirma in resultFirma.Where(itemFirma => itemFirma.Value.Contains("ERROR")))
                            listaErroriFirma.Add($"{listaErroriFirma.Count + 1} - {itemFirma.Value.Substring(7)}");
                        if (listaErroriFirma.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"Errori nella procedura di firma: {listaErroriFirma.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    case ActionEnum.DEPOSITA:
                        var resultDeposita = await EMGate.Deposita(model);
                        var listaErroriDeposito = new List<string>();
                        foreach (var itemDeposito in resultDeposita.Where(itemDeposito =>
                            itemDeposito.Value.Contains("ERROR")))
                            listaErroriDeposito.Add(
                                $"{listaErroriDeposito.Count + 1} - {itemDeposito.Value.Substring(7)}");
                        if (listaErroriDeposito.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"Errori nella procedura di deposito: {listaErroriDeposito.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    case ActionEnum.INVITA:
                        var resultInvita = await NotificheGate.NotificaEM(model);
                        var listaErroriInvita = new List<string>();
                        foreach (var itemInvito in resultInvita.Where(itemInvita =>
                            itemInvita.Value.Contains("ERROR")))
                            listaErroriInvita.Add(
                                $"{listaErroriInvita.Count + 1} - {itemInvito.Value.Substring(7)}");
                        if (listaErroriInvita.Count > 0)
                            return Json(
                                new
                                {
                                    message =
                                        $"Errori nella procedura di invito: {listaErroriInvita.Aggregate((i, j) => i + ", " + j)}"
                                }, JsonRequestBehavior.AllowGet);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model.Azione), model.Azione, null);
                }

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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
                var resultRitiro = await EMGate.RitiraFirma(id, pin);
                var listaErroriRitiroFirma = new List<string>();
                foreach (var itemRitiroFirma in resultRitiro.Where(itemRitiroFirma =>
                    itemRitiroFirma.Value.Contains("ERROR")))
                    listaErroriRitiroFirma.Add(
                        $"{listaErroriRitiroFirma.Count + 1} - {itemRitiroFirma.Value.Substring(7)}");
                if (listaErroriRitiroFirma.Count > 0)
                    throw new Exception(
                        $"Errori nella procedura di ritiro firma: {listaErroriRitiroFirma.Aggregate((i, j) => i + ", " + j)}");

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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
                var resultEliminaFirma = await EMGate.EliminaFirma(id, pin);
                var listaErroriEliminaFirma = new List<string>();
                foreach (var itemEliminaFirma in resultEliminaFirma.Where(itemRitiroFirma =>
                    itemRitiroFirma.Value.Contains("ERROR")))
                    listaErroriEliminaFirma.Add(
                        $"{listaErroriEliminaFirma.Count + 1} - {itemEliminaFirma.Value.Substring(7)}");
                if (listaErroriEliminaFirma.Count > 0)
                    throw new Exception(
                        $"Errori nella procedura di elimina firma: {listaErroriEliminaFirma.Aggregate((i, j) => i + ", " + j)}");

                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Restituisce i dati dei firmatari per un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("firmatari")]
        public async Task<ActionResult> GetFirmatariEmendamento(Guid id, FirmeTipoEnum tipo, bool tag = false)
        {
            var firme = await EMGate.GetFirmatari(id, tipo);
            var result = await Utility.GetFirmatariEM(firme, CurrentUser.UID_persona, tipo, tag);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Restituisce i dati degli inviti per un emendamento
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [HttpGet]
        [Route("preview")]
        public async Task<ActionResult> GetBody_Anteprima(Guid id, TemplateTypeEnum type)
        {
            var result = await EMGate.GetBody(id, type);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per esportare gli emendamenti di un atto
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("esportaXLS")]
        public async Task<ActionResult> EsportaXLS(Guid id, OrdinamentoEnum ordine)
        {
            try
            {
                var file = await StampeGate.EsportaXLS(id, ordine);
                return File(file.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
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
                var file = await StampeGate.EsportaWORD(id, ordine);
                return File(file.Content, "application/doc", file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
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
                await EMGate.CambioStato(model);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
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
                var resultRaggruppamento = await EMGate.Raggruppa(model);
                var listaErroriRaggruppamento = new List<string>();
                foreach (var item in resultRaggruppamento.Where(item =>
                    item.Value.Contains("ERROR")))
                    listaErroriRaggruppamento.Add(
                        $"{listaErroriRaggruppamento.Count + 1} - {item.Value.Substring(7)}");
                if (listaErroriRaggruppamento.Count > 0)
                    throw new Exception(
                        $"Errori nella procedura di raggruppamento: {listaErroriRaggruppamento.Aggregate((i, j) => i + ", " + j)}");
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
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
                var resultNuovoProponente = await EMGate.AssegnaNuovoPorponente(model);
                var listaErroriNuovoProponente = new List<string>();
                foreach (var item in resultNuovoProponente.Where(item =>
                    item.Value.Contains("ERROR")))
                    listaErroriNuovoProponente.Add(
                        $"{listaErroriNuovoProponente.Count + 1} - {item.Value.Substring(7)}");
                if (listaErroriNuovoProponente.Count > 0)
                    throw new Exception(
                        $"Errori nella procedura di assegnazione nuovo proponente: {listaErroriNuovoProponente.Aggregate((i, j) => i + ", " + j)}");
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per proiettare l'emendamento in aula
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpGet]
        [Route("proietta")]
        public async Task<ActionResult> ProiettaEmendamento(Guid id)
        {
            try
            {
                await EMGate.Proietta(id);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per visualizzare la pagina viewer che proietta gli emendamenti in ordine di votazione
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("viewer")]
        public async Task<ActionResult> ViewerProietta(Guid id, int ordine = 1)
        {
            var proietta = await EMGate.Proietta_View(id, ordine);
            var em = proietta.EM;
            em.BodyEM = em.EM_Certificato;

            em.Firme = await Utility.GetFirmatariEM(
                await EMGate.GetFirmatari(em.UIDEM, FirmeTipoEnum.PRIMA_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.PRIMA_DEPOSITO);
            em.Firme_dopo_deposito = await Utility.GetFirmatariEM(
                await EMGate.GetFirmatari(em.UIDEM, FirmeTipoEnum.DOPO_DEPOSITO),
                CurrentUser.UID_persona, FirmeTipoEnum.DOPO_DEPOSITO);

            proietta.EM = em;
            return View(proietta);
        }

        /// <summary>
        ///     Controller per ordinare gli emendamenti di un atto in votazione
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [HttpGet]
        [Route("ordina")]
        public async Task<ActionResult> ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await EMGate.ORDINA_EM_TRATTAZIONE(id);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione superiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("ordina-up")]
        public async Task<ActionResult> UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await EMGate.UP_EM_TRATTAZIONE(id);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione inferiore
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("ordina-down")]
        public async Task<ActionResult> DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                await EMGate.DOWN_EM_TRATTAZIONE(id);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per ordinare un emendamento di un atto in votazione in posizione precisa
        /// </summary>
        /// <param name="id">Guid emendamento</param>
        /// <param name="pos">Int posizione</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
        [Route("sposta")]
        public async Task<ActionResult> SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                await EMGate.SPOSTA_EM_TRATTAZIONE(id, pos);
                return Json(Request.UrlReferrer.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse {message = e.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        //FILTRI

        [HttpGet]
        [Route("stati-em")]
        public async Task<ActionResult> Filtri_GetStatiEM()
        {
            return Json(await EMGate.GetStati(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("tipi-em")]
        public async Task<ActionResult> Filtri_GetTipiEM()
        {
            return Json(await EMGate.GetTipi(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("parti-em")]
        public async Task<ActionResult> Filtri_GetPartiEM()
        {
            return Json(await EMGate.GetParti(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("missioni-em")]
        public async Task<ActionResult> Filtri_GetMissioniEM()
        {
            return Json(await EMGate.GetMissioni(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("filtra")]
        public async Task<ActionResult> Filtri_RiepilogoEM()
        {
            var mode = 1;
            var model = ElaboraFiltriEM(ref mode);

            if (!model.filtro.Any())
                return RedirectToAction("RiepilogoEmendamenti", "Emendamenti", new
                {
                    model.id,
                    mode,
                    ordine = (int) model.ordine
                });

            var modelResult = await EMGate.Get(model);

            if (HttpContext.User.IsInRole(RuoliEnum.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliEnum.Segreteria_Assemblea))
                return View("RiepilogoEM_Admin", modelResult);

            if (Convert.ToInt16(mode) == (int) ClientModeEnum.GRUPPI)
                foreach (var emendamentiDto in modelResult.Data.Results)
                    if (emendamentiDto.STATI_EM.IDStato <= (int) StatiEnum.Depositato)
                    {
                        if (emendamentiDto.ConteggioFirme > 0)
                            emendamentiDto.Firmatari = await Utility.GetFirmatariEM(
                                await EMGate.GetFirmatari(emendamentiDto.UIDEM, FirmeTipoEnum.TUTTE),
                                CurrentUser.UID_persona, FirmeTipoEnum.TUTTE, true);

                        emendamentiDto.Destinatari =
                            await Utility.GetDestinatariNotifica(await EMGate.GetInvitati(emendamentiDto.UIDEM));
                    }

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

            mode = mode_result;
            if (ordine == 0)
                ordine = 1;
            var model = new BaseRequest<EmendamentiDto>
            {
                page = filtro_page,
                size = filtro_size,
                param = new Dictionary<string, object> {{"CLIENT_MODE", mode_result}},
                ordine = (OrdinamentoEnum) ordine,
                id = new Guid(atto)
            };

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

            return model;
        }
    }
}