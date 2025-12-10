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
using System.Web.Mvc;
using PortaleRegione.Client.Models;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.Logger;
using Utility = PortaleRegione.Common.Utility;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller stampe
    /// </summary>
    [Authorize]
    [RoutePrefix("stampe")]
    public class StampeController : BaseController
    {
        // GET
        public async Task<ActionResult> Index(int page = 1, int size = 20)
        {
            var apiGateway = new ApiGateway(Token);
            var model = await apiGateway.Stampe.Get(page, size);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("Index_Admin", model);

            return View(model);
        }

        [HttpGet]
        [Route("print")]
        public async Task<ActionResult> Stampa(string uid)
        {
            var apiGateway = new ApiGateway(Token);
            var file = await apiGateway.Stampe.Stampa(uid);
            return File(file.Content, "application/pdf",
                file.FileName);
        }

        [HttpPost]
        [Route("nuova")]
        public async Task<ActionResult> NuovaStampa(StampaModel model)
        {
            var apiGateway = new ApiGateway(Token);
            var modelInCache = Session["RiepilogoEmendamenti"] as EmendamentiViewModel;

            if (model.Tutti)
            {
                var request = new BaseRequest<EmendamentiDto>
                {
                    id = modelInCache.Atto.UIDAtto,
                    page = 1,
                    size = modelInCache.Data.Paging.Total,
                    filtro = modelInCache.Data.Filters,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.Mode } },
                    ordine = modelInCache.Ordinamento
                };
                var list = await apiGateway.Emendamento.GetSoloIds(request);

                if (model.Lista != null)
                    foreach (var guid in model.Lista)
                        list.Remove(guid);

                model.Lista = list;
            }

            var res = model.Lista.ToList();
            if (model.da > 0 && model.a > 0)
                if (model.da >= 1 && model.a <= res.Count)
                {
                    var range = res.GetRange(model.da - 1, model.a - (model.da - 1));
                    res = range.ToList();
                }

            return Json(await apiGateway.Stampe.InserisciStampa(new NuovaStampaRequest
            {
                Lista = res,
                Modulo = ModuloStampaEnum.PEM,
                Ordinamento = modelInCache.Ordinamento,
                Da = model.da,
                A = model.a,
                UIDAtto = new Guid(model.uid_atto)
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("nuova-dasi")]
        public async Task<ActionResult> NuovaStampaDasi(StampaModel model)
        {
            var apiGateway = new ApiGateway(Token);
            var modelInCache = Session["RiepilogoDASI"] as RiepilogoDASIModel;
            
            try
            {
                if (model.Tutti)
                {
                    var request = new BaseRequest<AttoDASIDto>
                    {
                        page = 1,
                        size = 99999,
                        param = new Dictionary<string, object>
                        {
                            { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI }
                        }
                    };

                    if (modelInCache != null)
                    {
                        request.filtro.AddRange(modelInCache.Data.Filters);
                    }
                    else
                    {
                        // #1340
                        if (model.sort_settings_dasi.Any())
                        {
                            request.dettagliOrdinamento = model.sort_settings_dasi;
                        }

                        request.filtro.AddRange(Utility.ParseFilterDasi(model.filters_dasi));
                    }

                    var list = await apiGateway.DASI.GetSoloIds(request);

                    if (model.Lista != null)
                        foreach (var guid in model.Lista)
                            list.Remove(guid);

                    model.Lista = list;
                }

                var res = model.Lista.ToList();
                if (model.da > 0 && model.a > 0)
                    if (model.da >= 1 && model.a <= res.Count)
                    {
                        var range = res.GetRange(model.da - 1, model.a - (model.da - 1));
                        res = range.ToList();
                    }

                return Json(await apiGateway.Stampe.InserisciStampa(new NuovaStampaRequest
                {
                    Lista = res,
                    Modulo = ModuloStampaEnum.DASI,
                    Da = model.da,
                    A = model.a
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error("Aggiungi Stampa", ex);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per scaricare una stampa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> DownloadStampa(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.Stampe.DownloadStampa(id);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per scaricare una stampa
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("fascicolo/{nomeFile}")]
        public async Task<ActionResult> DownloadFascicoloStampa(string nomeFile)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var file = await apiGateway.Stampe.DownloadStampa(nomeFile);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("reset")]
        public async Task<ActionResult> ResetStampa(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            await apiGateway.Stampe.ResetStampa(id);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("elimina")]
        public async Task<ActionResult> EliminaStampa(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Stampe.EliminaStampa(id);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("info")]
        public async Task<ActionResult> InfoStampa(string uid)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var result = await apiGateway.Stampe.GetInfo(new Guid(uid));
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}