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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.Client.Models;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller amministrazione
    /// </summary>
    [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Amministratore_Giunta)]
    [RoutePrefix("adminpanel")]
    public class AdminPanelController : BaseController
    {
        /// <summary>
        ///     Controller per visualizzare i dati degli utenti
        /// </summary>
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("users/view")]
        public async Task<ActionResult> RiepilogoUtenti(int page = 1, int size = 50)
        {
            return View("RiepilogoUtenti", await AdminGate.GetPersone(new BaseRequest<PersonaDto>
            {
                page = page,
                size = size
            }));
        }

        /// <summary>
        ///     Controller per ricercare gli utenti
        /// </summary>
        /// <param name="id">Guid utente</param>
        /// <returns></returns>
        [HttpPost]
        [Route("users/view")]
        public async Task<ActionResult> SearchUsers()
        {
            int.TryParse(Request.Form["page"], out var filtro_page);
            int.TryParse(Request.Form["size"], out var filtro_size);
            var filtro_q = Request.Form["q"];
            var request = new BaseRequest<PersonaDto> {page = filtro_page, size = filtro_size};
            if (!string.IsNullOrEmpty(filtro_q))
            {
                request.filtro.Add(new FilterStatement<PersonaDto>
                {
                    PropertyId = nameof(PersonaDto.nome),
                    Operation = Operation.Contains,
                    Value = filtro_q,
                    Connector = FilterStatementConnector.Or
                });
                request.filtro.Add(new FilterStatement<PersonaDto>
                {
                    PropertyId = nameof(PersonaDto.cognome),
                    Operation = Operation.Contains,
                    Value = filtro_q,
                    Connector = FilterStatementConnector.Or
                });
            }

            var result = await AdminGate.GetPersone(request);
            return View("RiepilogoUtenti", result);
        }

        /// <summary>
        ///     Controller per visualizzare i dati dell' utente
        /// </summary>
        /// <param name="id">Guid utente</param>
        /// <returns></returns>
        [HttpGet]
        [Route("view/{id:guid}")]
        public async Task<ActionResult> ViewUtente(Guid id)
        {
            var persona = await AdminGate.GetPersona(id);
            var ruoli = await AdminGate.GetRuoliAD();
            var gruppiAD = await AdminGate.GetGruppiPoliticiAD();
            var listaGruppiRuoliAD = ruoli.Select(ruolo => new AD_ObjectModel
            {
                GruppoAD = ruolo.ADGroup, Membro = persona.Gruppi.Contains(ruolo.ADGroup.Replace(@"CONSIGLIO\", "")),
                IsRuolo = true
            }).ToList();
            listaGruppiRuoliAD.AddRange(gruppiAD.Select(gruppo => new AD_ObjectModel
            {
                GruppoAD = gruppo.GruppoAD,
                Membro = persona.Gruppi.Contains(gruppo.GruppoAD.Replace(@"CONSIGLIO\", "")), IsRuolo = false
            }));

            return View("ViewUtente", new ViewUtenteModel
            {
                Persona = persona,
                GruppiAD = listaGruppiRuoliAD
            });
        }

        /// <summary>
        ///     Controller per modificare l' utente
        /// </summary>
        /// <param name="persona"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("salva")]
        public async Task<ActionResult> SalvaPersona(PersonaUpdateRequest request)
        {
            try
            {
                await AdminGate.ModificaPersona(request);

                return Json(Url.Action("ViewUtente", "AdminPanel", new {id = request.UID_persona})
                    , JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per resettare il pin dell' utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("reset-pin")]
        [HttpPost]
        public async Task<ActionResult> ResetPin(ResetRequest request)
        {
            await AdminGate.ResetPin(request);
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per resettare la password dell' utente
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("reset-password")]
        [HttpPost]
        public async Task<ActionResult> ResetPassword(ResetRequest request)
        {
            try
            {
                await AdminGate.ResetPassword(request);
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}