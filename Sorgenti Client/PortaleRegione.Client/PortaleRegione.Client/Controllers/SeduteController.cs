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
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller sedute
    /// </summary>
    [Authorize]
    [RoutePrefix("sedute")]
    public class SeduteController : BaseController
    {
        public async Task<ActionResult> RiepilogoSedute(int page = 1, int size = 50)
        {
            var _seduteGateway = new SeduteGateway(_Token);
            var model = await _seduteGateway.Get(page, size);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoSedute_Admin", model);

            return View("RiepilogoSedute", model);
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("delete")]
        public async Task<ActionResult> EliminaSeduta(Guid id)
        {
            var _seduteGateway = new SeduteGateway(_Token);
            await _seduteGateway.Elimina(id);
            return RedirectToAction("RiepilogoSedute", "Sedute");
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
            var _seduteGateway = new SeduteGateway(_Token);
            var seduta = await _seduteGateway.Get(id);
            return View("SedutaForm", seduta);
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [HttpPost]
        [Route("salva")]
        public async Task<ActionResult> SalvaSeduta(SeduteFormUpdateDto seduta)
        {
            try
            {
                var _seduteGateway = new SeduteGateway(_Token);
                if (seduta.UIDSeduta == Guid.Empty)
                    await _seduteGateway.Salva(seduta);
                else
                    await _seduteGateway.Modifica(seduta);

                return Json(Url.Action("RiepilogoSedute", "Sedute")
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
                var _seduteGateway = new SeduteGateway(_Token);
                return Json(await _seduteGateway.GetLegislature(), JsonRequestBehavior.AllowGet);
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
                    Value = filtro_legislatura
                });

            if (filtro_anno > 0)
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.Between,
                    Value = new DateTime(filtro_anno, 1, 1).ToString("yyyy-MM-dd"),
                    Value2 = new DateTime(filtro_anno, 12, 31).ToString("yyyy-MM-dd")
                });

            if (!string.IsNullOrEmpty(filtro_da))
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.GreaterThan,
                    Value = Convert.ToDateTime(filtro_da).ToString("yyyy-MM-dd")
                });

            if (!string.IsNullOrEmpty(filtro_a))
                model.filtro.Add(new FilterStatement<SeduteDto>
                {
                    PropertyId = nameof(SeduteDto.Data_seduta),
                    Operation = Operation.LessThan,
                    Value = Convert.ToDateTime(filtro_a).ToString("yyyy-MM-dd")
                });

            if (!model.filtro.Any())
                return RedirectToAction("RiepilogoSedute");
            var _seduteGateway = new SeduteGateway(_Token);
            var results = await _seduteGateway.Get(model);
            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoSedute_Admin", results);

            return View("RiepilogoSedute", results);
        }
    }
}