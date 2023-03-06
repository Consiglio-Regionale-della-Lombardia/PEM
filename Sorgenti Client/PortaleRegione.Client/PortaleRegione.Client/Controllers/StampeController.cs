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

using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        public async Task<ActionResult> Index(int page = 1, int size = 50)
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
            var request = new BaseRequest<EmendamentiDto, StampaDto>();

            var modelFiltro = string.Empty;
            if (model.filters != null)
            {
                modelFiltro = model.filters.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(modelFiltro))
            {
                if (Session["RiepilogoEmendamenti"] is EmendamentiViewModel modelInCache)
                    try
                    {
                        request.filtro = new List<FilterStatement<EmendamentiDto>>();
                        request.page = 1;
                        request.size = modelInCache.Data.Paging.Total;
                        request.filtro.AddRange(modelInCache.Data.Filters);
                        request.ordine = modelInCache.Ordinamento;
                        request.param = new Dictionary<string, object>
                        {
                            {
                                "CLIENT_MODE", (int) modelInCache.Mode
                            }
                        };
                    }
                    catch (Exception)
                    {
                    }
            }
            else
            {
                request.param = new Dictionary<string, object>
                {
                    { "UIDAtto", model.uid_atto },
                    { "Da", model.da },
                    { "A", model.a },
                    { "CLIENT_MODE", model.client_mode },
                    { "Ordine", model.ordine }
                };
                var split_filters = modelFiltro.Split(',').Select(i => new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.UIDEM),
                    Operation = Operation.EqualTo,
                    Value = i
                }).ToList();

                request.filtro = new List<FilterStatement<EmendamentiDto>>(split_filters);
            }

            request.entity = new StampaDto
            {
                UIDAtto = new Guid(model.uid_atto),
                Da = Convert.ToInt32(model.da),
                A = Convert.ToInt32(model.a),
                Ordine = Convert.ToInt32(model.ordine),
                CLIENT_MODE = Convert.ToInt32(model.client_mode)
            };
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Stampe.InserisciStampa(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("nuova-dasi")]
        public async Task<ActionResult> NuovaStampaDasi(StampaModel model)
        {
            var request = new BaseRequest<AttoDASIDto, StampaDto>();
            var modelFiltro = model.filters.FirstOrDefault();
            if (string.IsNullOrEmpty(modelFiltro))
            {
                request.filtro = new List<FilterStatement<AttoDASIDto>>();
                if (Session["RiepilogoDASI"] is RiepilogoDASIModel modelInCache)
                    try
                    {
                        request.page = 1;
                        request.size = modelInCache.Data.Paging.Total;
                        request.filtro.AddRange(modelInCache.Data.Filters);
                        request.param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } };
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
            }
            else
            {
                request.param = new Dictionary<string, object>
                {
                    { "Da", model.da },
                    { "A", model.a },
                    { "CLIENT_MODE", model.client_mode }
                };
                var split_filters = modelFiltro.Split(',').Select(i => new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDAtto),
                    Operation = Operation.EqualTo,
                    Value = i
                }).ToList();

                request.filtro = new List<FilterStatement<AttoDASIDto>>(split_filters);
            }

            request.entity = new StampaDto
            {
                Da = Convert.ToInt32(model.da),
                A = Convert.ToInt32(model.a),
                CLIENT_MODE = Convert.ToInt32(model.client_mode)
            };
            var apiGateway = new ApiGateway(Token);
            return Json(await apiGateway.Stampe.InserisciStampa(request), JsonRequestBehavior.AllowGet);
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

    public class StampaModel
    {
        public string da { get; set; }
        public string a { get; set; }
        public string client_mode { get; set; }
        public List<string> filters { get; set; }
        public string uid_atto { get; set; }
        public string ordine { get; set; }
    }
}
