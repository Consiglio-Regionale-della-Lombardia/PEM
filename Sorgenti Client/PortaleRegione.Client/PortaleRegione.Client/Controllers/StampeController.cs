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
using ExpressionBuilder.Generics;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

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
            var apiGateway = new ApiGateway(_Token);
            var model = await apiGateway.Stampe.Get(page, size);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("Index_Admin", model);

            return View(model);
        }

        [HttpPost]
        [Route("nuova")]
        public async Task<ActionResult> NuovaStampa(BaseRequest<EmendamentiDto, StampaDto> model)
        {
            object UIDAtto;
            model.param.TryGetValue("UIDAtto", out UIDAtto);
            object DA;
            model.param.TryGetValue("Da", out DA);
            object A;
            model.param.TryGetValue("A", out A);
            object client_mode;
            model.param.TryGetValue("CLIENT_MODE", out client_mode);
            object ordine;
            model.param.TryGetValue("Ordine", out ordine);
            model.entity = new StampaDto
            {
                UIDAtto = new Guid(UIDAtto.ToString()),
                Da = Convert.ToInt16(DA),
                A = Convert.ToInt16(A),
                Ordine = Convert.ToInt32(ordine),
                CLIENT_MODE = Convert.ToInt32(client_mode)
            };

            
                if (Session["RiepilogoEmendamenti"] is EmendamentiViewModel modelInCache)
                    try
                    {
                        if(model.filtro == null)
                            model.filtro = new List<FilterStatement<EmendamentiDto>>();
                        model.page = 1;
                        model.size = modelInCache.Data.Paging.Total;
                        model.filtro.AddRange(modelInCache.Data.Filters);
                        model.ordine = modelInCache.Ordinamento;
                        model.param = new Dictionary<string, object>
                        {
                            {
                                "CLIENT_MODE", (int) modelInCache.Mode
                            }
                        };
                    }
                    catch (Exception)
                    {
                    }


            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Stampe.InserisciStampa(model);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("nuova-dasi")]
        public async Task<ActionResult> NuovaStampaDasi(BaseRequest<AttoDASIDto, StampaDto> model)
        {
            object DA;
            model.param.TryGetValue("Da", out DA);
            object A;
            model.param.TryGetValue("A", out A);

            model.entity = new StampaDto
            {
                Da = Convert.ToInt16(DA),
                A = Convert.ToInt16(A)
            };
            
            if (model.filtro == null)
            {
                model.filtro = new List<FilterStatement<AttoDASIDto>>();
                if (Session["RiepilogoDASI"] is RiepilogoDASIModel modelInCache)
                    try
                    {
                        model.page = 1;
                        model.size = modelInCache.Data.Paging.Total;
                        model.filtro.AddRange(modelInCache.Data.Filters);
                        model.param = new Dictionary<string, object> { { "CLIENT_MODE", (int)modelInCache.ClientMode } };
                    }
                    catch (Exception)
                    {
                    }
            }

            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Stampe.InserisciStampa(model);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
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
                var apiGateway = new ApiGateway(_Token);
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
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Stampe.ResetStampa(id);
            return Json(Url.Action("Index", "Stampe"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("elimina")]
        public async Task<ActionResult> EliminaStampa(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.Stampe.EliminaStampa(id);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
