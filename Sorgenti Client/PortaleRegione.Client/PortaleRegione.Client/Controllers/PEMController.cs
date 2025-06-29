﻿/*
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
using PortaleRegione.Client.Helpers;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
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
    ///     Controller sedute
    /// </summary>
    [Authorize]
    [RoutePrefix("pem")]
    public class PEMController : BaseController
    {
        public async Task<ActionResult> RiepilogoSedute(int page = 1, int size = 20)
        {
            var currentUser = CurrentUser;
            await CheckCacheGruppiAdmin(currentUser.CurrentRole);
            var apiGateway = new ApiGateway(Token);

            var model = new BaseRequest<SeduteDto>
            {
                page = page,
                size = size
            };

            var legislature = await apiGateway.Legislature.GetLegislature();
            model.filtro.Add(new FilterStatement<SeduteDto>
            {
                PropertyId = nameof(SeduteDto.id_legislatura),
                Operation = Operation.EqualTo,
                Value = legislature.First().id_legislatura,
                Connector = FilterStatementConnector.And
            });
            var results = await apiGateway.Sedute.Get(model);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoSedute_Admin", results);

            return View("RiepilogoSedute", results);
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("delete")]
        public async Task<ActionResult> EliminaSeduta(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            await apiGateway.Sedute.Elimina(id);
            return RedirectToAction("RiepilogoSedute", "PEM");
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("new")]
        public ActionResult NuovaSeduta()
        {
            return View("SedutaForm", new SeduteFormUpdateDto());
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("edit/{id:guid}")]
        public async Task<ActionResult> ModificaSeduta(Guid id)
        {
            var apiGateway = new ApiGateway(Token);
            var seduta = await apiGateway.Sedute.Get(id);
            return View("SedutaForm", seduta);
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("salva")]
        public async Task<ActionResult> SalvaSeduta(SeduteFormUpdateDto seduta)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                if (seduta.UIDSeduta == Guid.Empty)
                    await apiGateway.Sedute.Salva(seduta);
                else
                    await apiGateway.Sedute.Modifica(seduta);

                return Json(Url.Action("RiepilogoSedute", "PEM")
                    , JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        [Route("legislature")]
        public async Task<ActionResult> Filtri_GetLegislature()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.Legislature.GetLegislature(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("sedute-attive")]
        public async Task<ActionResult> SeduteAttive()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.Sedute.GetAttive(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("sedute-attive-mozu")]
        public async Task<ActionResult> SeduteAttiveMOZU()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.Sedute.GetAttiveMOZU(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("sedute-attive-dashboard")]
        public async Task<ActionResult> SeduteAttiveDashboard()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.Sedute.GetAttiveDashboard(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("seduta-by-data")]
        public async Task<ActionResult> SedutaByData(DateTime data)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var model = new BaseRequest<SeduteDto>
                {
                    filtro = new List<FilterStatement<SeduteDto>>
                    {
                        new FilterStatement<SeduteDto>
                        {
                            PropertyId = nameof(SeduteDto.Data_seduta),
                            Operation = Operation.EqualTo,
                            Value = data.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                };
                var seduta = await apiGateway.Sedute.Get(model);

                if (seduta.Paging.Entities > 1)
                {
                    throw new Exception($"Seduta [{data}] duplicata");
                }

                return Json(seduta.Results.First(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("filtra")]
        public async Task<ActionResult> Filtri_RiepilogoSedute()
        {
            int.TryParse(Request.Form["filtro_legislatura"], out var filtro_legislatura);
            int.TryParse(Request.Form["filtro_anno"], out var filtro_anno);
            var filtro_da = Request.Form["filtro_da"];
            var filtro_a = Request.Form["filtro_a"];
            int.TryParse(Request.Form["page"], out var filtro_page);
            int.TryParse(Request.Form["size"], out var filtro_size);

            if (filtro_page == 0)
                filtro_page = 1;

            var model = new BaseRequest<SeduteDto>
            {
                page = filtro_page,
                size = filtro_size
            };

            if (filtro_legislatura > 0)
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.id_legislatura),
                    Operation = Operation.EqualTo,
                    Value = filtro_legislatura,
                    Connector = FilterStatementConnector.And
                });

            if (filtro_anno > 0)
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.Between,
                    Value = new DateTime(filtro_anno, 1, 1).ToString("yyyy-MM-dd"),
                    Value2 = new DateTime(filtro_anno, 12, 31).ToString("yyyy-MM-dd"),
                    Connector = FilterStatementConnector.And
                });

            if (!string.IsNullOrEmpty(filtro_da))
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.GreaterThan,
                    Value = Convert.ToDateTime(filtro_da).ToString("yyyy-MM-dd"),
                    Connector = FilterStatementConnector.And
                });

            if (!string.IsNullOrEmpty(filtro_a))
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.LessThan,
                    Value = Convert.ToDateTime(filtro_a).ToString("yyyy-MM-dd"),
                    Connector = FilterStatementConnector.And
                });

            var apiGateway = new ApiGateway(Token);
            var results = await apiGateway.Sedute.Get(model);

            var mode = Convert.ToInt16(HttpContext.Cache.Get(GetCacheKey(CacheHelper.CLIENT_MODE)));
            if (mode == (int)ClientModeEnum.TRATTAZIONE)
            {
                return View("~/Views/AttiTrattazione/Archivio.cshtml", results);
            }

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoSedute_Admin", results);

            return View("RiepilogoSedute", results);
        }
    }
}