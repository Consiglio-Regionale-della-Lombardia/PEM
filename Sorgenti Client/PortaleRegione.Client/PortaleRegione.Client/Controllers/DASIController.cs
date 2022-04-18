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
using Newtonsoft.Json;
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

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
        public async Task<ActionResult> RiepilogoDASI(int page = 1, int size = 50,
            StatiAttoEnum stato = StatiAttoEnum.BOZZA, TipoAttoEnum tipo = TipoAttoEnum.TUTTI)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.Get(page, size, stato, tipo, _CurrentUser);
            SetCache(page, size, tipo, stato);
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
        public async Task<ActionResult> Nuovo(TipoAttoEnum tipo)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.DASI.GetNuovoModello(tipo);
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
                if (string.IsNullOrEmpty(atto.Atto_Certificato))
                    atto.BodyAtto = await apiGateway.DASI.GetBody(id, TemplateTypeEnum.HTML);
                else
                    atto.BodyAtto = atto.Atto_Certificato;

                atto.Firme = await Utility.GetFirmatari(
                    await apiGateway.DASI.GetFirmatari(id, FirmeTipoEnum.PRIMA_DEPOSITO),
                    _CurrentUser.UID_persona, FirmeTipoEnum.PRIMA_DEPOSITO, _Token);

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
                //TODO: ELIMINA E RITIRA MASSIVO
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
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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
                    throw new NotImplementedException("Azione non funzionante");

                //TODO: INVITO MASSIVO
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
                        var listaErroriDeposito = resultDeposita.Select(itemDeposito => $"{itemDeposito.Value}").ToList();
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
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model.Azione), model.Azione, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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
                return Json(JsonConvert.DeserializeObject<ErrorResponse>(e.Message), JsonRequestBehavior.AllowGet);
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

        private void SetCache(int page, int size, TipoAttoEnum tipo, StatiAttoEnum stato)
        {
            HttpContext.Cache.Insert(
                "TipoDASI",
                (int) tipo,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                "StatoDASI",
                (int) stato,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                "Page",
                page,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );

            HttpContext.Cache.Insert(
                "Size",
                size,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => { Console.WriteLine("Cache removed"); }
            );
        }
    }
}