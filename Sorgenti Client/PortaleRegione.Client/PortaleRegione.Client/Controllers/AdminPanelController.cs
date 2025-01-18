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
using PortaleRegione.Client.Models;
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
    ///     Controller amministrazione
    /// </summary>
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
        public async Task<ActionResult> RiepilogoUtenti(int page = 1, int size = 20)
        {
            var request = new BaseRequest<PersonaDto> { page = page, size = size };
            var apiGateway = new ApiGateway(Token);
            return View("RiepilogoUtenti", await apiGateway.Admin.GetPersone(request));
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
            var filtro_no_cons = Request.Form["no_cons"];
            var filtro_legislatura = Request.Form["legislatura"];
            var filtro_ruoli = Request.Form["ruoli"];
            var filtro_gruppi = Request.Form["gruppi"];
            var request = new BaseRequest<PersonaDto> { page = filtro_page, size = filtro_size };
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
                    Connector = FilterStatementConnector.And
                });
            }

            if (filtro_no_cons == "on")
            {
                request.filtro.Add(new FilterStatement<PersonaDto>
                {
                    PropertyId = nameof(PersonaDto.No_Cons),
                    Operation = Operation.EqualTo,
                    Value = 1,
                    Connector = FilterStatementConnector.And
                });
            }

            if (!string.IsNullOrEmpty(filtro_legislatura))
            {
                var split_filter = filtro_legislatura.Split(',');
                foreach (var s in split_filter)
                {
                    request.filtro.Add(new FilterStatement<PersonaDto>
                    {
                        PropertyId = nameof(PersonaDto.legislature),
                        Operation = Operation.Contains,
                        Value = $"-{s}-",
                        Connector = FilterStatementConnector.And
                    });
                }
            }

            if (!string.IsNullOrEmpty(filtro_ruoli))
            {
                var split_filter = filtro_ruoli.Split(',');
                foreach (var s in split_filter)
                {
                    request.filtro.Add(new FilterStatement<PersonaDto>
                    {
                        PropertyId = nameof(PersonaDto.Ruoli),
                        Operation = Operation.EqualTo,
                        Value = s,
                        Connector = FilterStatementConnector.And
                    });
                }
            }

            if (!string.IsNullOrEmpty(filtro_gruppi))
            {
                var split_filter = filtro_gruppi.Split(',');
                foreach (var s in split_filter)
                {
                    request.filtro.Add(new FilterStatement<PersonaDto>
                    {
                        PropertyId = nameof(PersonaDto.id_gruppo_politico_rif),
                        Operation = Operation.EqualTo,
                        Value = s,
                        Connector = FilterStatementConnector.And
                    });
                }
            }

            var apiGateway = new ApiGateway(Token);
            var result = await apiGateway.Admin.GetPersone(request);
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
            var currentUser = CurrentUser;
            var apiGateway = new ApiGateway(Token);
            var persona = await apiGateway.Admin.GetPersona(id);
            var ruoli = await apiGateway.Admin.GetRuoliAD();
            if (currentUser.IsGiunta)
            {
                ruoli.RemoveAt(ruoli.FindIndex(i => i.IDruolo == (int)RuoliIntEnum.Presidente_Regione));
                ruoli.RemoveAt(ruoli.FindIndex(i => i.IDruolo == (int)RuoliIntEnum.Assessore_Sottosegretario_Giunta));
            }
            var gruppiAD = await apiGateway.Admin.GetGruppiPoliticiAD();
            var listaGruppiRuoliAD = ruoli.Select(ruolo => new AD_ObjectModel
            {
                GruppoAD = ruolo.ADGroup,
                Membro = persona.Gruppi.Contains(ruolo.ADGroup.Replace(@"CONSIGLIO\", "")),
                IsRuolo = true
            }).ToList();
            listaGruppiRuoliAD.AddRange(gruppiAD.Select(gruppo => new AD_ObjectModel
            {
                GruppoAD = gruppo.GruppoAD,
                Membro = persona.Gruppi.Contains(gruppo.GruppoAD.Replace(@"CONSIGLIO\", "")),
                IsRuolo = false
            }));
            if (currentUser.IsGiunta)
            {
                var gruppo_giunta = gruppiAD.First(i => i.id_gruppo >= 10000);
                persona.id_gruppo_politico_rif = gruppo_giunta.id_gruppo;

                var gruppo_ad = listaGruppiRuoliAD.First(i => !i.IsRuolo);
                gruppo_ad.Membro = true;

                return View("ViewUtente", new ViewUtenteModel
                {
                    Persona = persona,
                    CurrentUser = currentUser,
                    GruppiAD = listaGruppiRuoliAD,
                    GruppiInDB = new List<KeyValueDto>
                    {
                        new KeyValueDto { id = gruppo_giunta.id_gruppo, descr = "GIUNTA" }
                    }
                });
            }

            var gruppiInDb = await apiGateway.Persone.GetGruppiAttivi();

            return View("ViewUtente", new ViewUtenteModel
            {
                Persona = persona,
                GruppiAD = listaGruppiRuoliAD,
                GruppiInDB = gruppiInDb.ToList(),
                CurrentUser = currentUser
            });
        }

        /// <summary>
        ///     Controller per creare un nuovo utente
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpGet]
        [Route("new")]
        public async Task<ActionResult> NuovoUtente()
        {
            var currentUser = CurrentUser;
            var apiGateway = new ApiGateway(Token);
            var persona = new PersonaDto
            {
                userAD = @"CONSIGLIO\"
            };
            var ruoli = await apiGateway.Admin.GetRuoliAD();
            if (currentUser.IsAmministratoreGiunta)
            {
                ruoli.RemoveAt(ruoli.FindIndex(i => i.IDruolo == (int)RuoliIntEnum.Presidente_Regione));
                ruoli.RemoveAt(ruoli.FindIndex(i => i.IDruolo == (int)RuoliIntEnum.Assessore_Sottosegretario_Giunta));
            }
            var gruppiAD = await apiGateway.Admin.GetGruppiPoliticiAD();
            var listaGruppiRuoliAD = ruoli.Select(ruolo => new AD_ObjectModel
            {
                GruppoAD = ruolo.ADGroup,
                Membro = false,
                IsRuolo = true
            }).ToList();
            listaGruppiRuoliAD.AddRange(gruppiAD.Select(gruppo => new AD_ObjectModel
            {
                GruppoAD = gruppo.GruppoAD,
                Membro = false,
                IsRuolo = false
            }));
            if (currentUser.IsAmministratoreGiunta)
            {
                var gruppo_giunta = gruppiAD.First(i => i.id_gruppo >= 10000);
                persona.id_gruppo_politico_rif = gruppo_giunta.id_gruppo;

                var gruppo_ad = listaGruppiRuoliAD.First(i => !i.IsRuolo);
                gruppo_ad.Membro = true;

                return View("ViewUtente", new ViewUtenteModel
                {
                    Persona = persona,
                    CurrentUser = currentUser,
                    GruppiAD = listaGruppiRuoliAD,
                    GruppiInDB = new List<KeyValueDto>
                    {
                        new KeyValueDto { id = gruppo_giunta.id_gruppo, descr = "GIUNTA" }
                    }
                });
            }

            var gruppiInDb = await apiGateway.Persone.GetGruppiAttivi();

            return View("ViewUtente", new ViewUtenteModel
            {
                Persona = persona,
                CurrentUser = currentUser,
                GruppiAD = listaGruppiRuoliAD,
                GruppiInDB = gruppiInDb.ToList()
            });
        }

        /// <summary>
        ///     Controller per modificare l' utente
        /// </summary>
        /// <param name="persona"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpPost]
        [Route("salva")]
        public async Task<ActionResult> SalvaPersona(PersonaUpdateRequest request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                var currentUser = CurrentUser;
                if (currentUser.IsAmministratoreGiunta)
                {
                    var gruppiAD = await apiGateway.Admin.GetGruppiPoliticiAD();
                    if (currentUser.IsAmministratoreGiunta)
                    {
                        var gruppo_giunta = gruppiAD.First(i => i.id_gruppo >= 10000);
                        request.id_gruppo_politico_rif = gruppo_giunta.id_gruppo;
                        request.notifica_firma = true;
                        request.notifica_deposito = true;
                        var gruppo_ad = request.gruppiAd.First(i => !i.IsRuolo);
                        gruppo_ad.Membro = true;
                    }
                }

                var result = await apiGateway.Admin.SalvaPersona(request);

                return Json(Url.Action("ViewUtente", "AdminPanel", new { id = result })
                    , JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per eliminare l' utente
        /// </summary>
        /// <param name="persona"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpGet]
        [Route("elimina")]
        public async Task<ActionResult> EliminaPersona(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Admin.EliminaPersona(id);

                return Json(Url.Action("ViewUtente", "AdminPanel", new { id })
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
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Admin.ResetPin(request);
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
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
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Admin.ResetPassword(request);
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("gruppi-in-db")]
        public async Task<ActionResult> GetGruppiInDb()
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                return Json(await apiGateway.Admin.GetGruppiInDb(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per visualizzare i dati degli utenti
        /// </summary>
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [HttpGet]
        [Route("groups/view")]
        public async Task<ActionResult> RiepilogoGruppi()
        {
            var apiGateway = new ApiGateway(Token);
            var request = new BaseRequest<GruppiDto>();
            return View("RiepilogoGruppi", await apiGateway.Admin.GetGruppiAdmin(request));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Amministratore_Giunta)]
        [HttpPost]
        public async Task<ActionResult> SalvaGruppo(SalvaGruppoRequest request)
        {
            try
            {
                var apiGateway = new ApiGateway(Token);
                await apiGateway.Admin.SalvaGruppo(request);

                return Json(Url.Action("RiepilogoGruppi", "AdminPanel")
                    , JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        ///     Controller per visualizzare le sessioni
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("users/sessions")]
        public async Task<ActionResult> RiepilogoSessioni()
        {
            var apiGateway = new ApiGateway(Token);
            return View("RiepilogoSessioni", await apiGateway.Admin.GetSessioni());
        }
    }
}