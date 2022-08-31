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

using PortaleRegione.Client.Models;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller atti
    /// </summary>
    [Authorize]
    [RoutePrefix("atti")]
    public class AttiController : BaseController
    {
        /// <summary>
        ///     Controller per visualizzare i dati degli atti contenuti in una seduta
        /// </summary>
        /// <param name="id">Guid seduta</param>
        /// <param name="page">Pagina corrente</param>
        /// <param name="size">Paginazione</param>
        /// <returns></returns>
        [Route("{id:guid}")]
        public async Task<ActionResult> RiepilogoAtti(Guid id, ClientModeEnum mode = ClientModeEnum.GRUPPI,
            int page = 1, int size = 50)
        {
            Session["RiepilogoEmendamenti"] = null;
            var apiGateway = new ApiGateway(_Token);
            var sedutaInDb = await apiGateway.Sedute.Get(id);
            var model = new AttiViewModel
            { Data = await apiGateway.Atti.Get(id, mode, page, size), Seduta = sedutaInDb };

            if (HttpContext.User.IsInRole(RuoliExt.Amministratore_PEM) ||
                HttpContext.User.IsInRole(RuoliExt.Segreteria_Assemblea))
                return View("RiepilogoAtti_Admin", model);

            return View("RiepilogoAtti", model);
        }

        /// <summary>
        ///     Controller per eliminare l'atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("{sedutaUId:guid}/delete")]
        public async Task<ActionResult> EliminaAtto(Guid sedutaUId, Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.Elimina(id);
            return RedirectToAction("RiepilogoAtti", new
            {
                id = sedutaUId
            });
        }

        /// <summary>
        ///     Controller per aggiungere un atto
        /// </summary>
        /// <param name="id">Guid seduta di riferimento</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("{id:guid}/new")]
        public async Task<ActionResult> NuovoAtto(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = new AttiViewModel
            {
                Assessori = await apiGateway.Persone.GetAssessoriRiferimento(),
                Relatori = await apiGateway.Persone.GetRelatori(null),
                Atto = new AttiFormUpdateModel
                {
                    UIDSeduta = id,
                    IDTipoAtto = (int)TipoAttoEnum.PDL,
                    Notifica_deposito_differita = true
                }
            };

            return View("AttoForm", model);
        }

        /// <summary>
        ///     Controller per modificare un atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("edit/{id:guid}")]
        public async Task<ActionResult> ModificaAtto(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var model = new AttiViewModel
            {
                Assessori = await apiGateway.Persone.GetAssessoriRiferimento(),
                Relatori = await apiGateway.Persone.GetRelatori(null),
                Atto = await apiGateway.Atti.GetFormUpdate(id)
            };

            return View("AttoForm", model);
        }

        /// <summary>
        ///     Controller per salvare o modificare l'atto
        /// </summary>
        /// <param name="atto">Modello atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("salva")]
        [HttpPost]
        public async Task<ActionResult> SalvaAtto(AttiFormUpdateModel atto)
        {
            try
            {
                if (atto.DocAtto != null)
                    if (atto.DocAtto.ContentType != "application/pdf")
                        throw new InvalidOperationException("I file devono essere in formato PDF");
                var apiGateway = new ApiGateway(_Token);
                AttiDto attoSalvato = null;
                if (atto.UIDAtto == Guid.Empty)
                    attoSalvato = await apiGateway.Atti.Salva(atto);
                else
                    attoSalvato = await apiGateway.Atti.Modifica(atto);

                return Json(new ClientJsonResponse<AttiDto>
                {
                    entity = attoSalvato,
                    url = Url.Action("RiepilogoAtti", "Atti", new
                    {
                        id = atto.UIDSeduta
                    })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per scaricare il documento pdf dell'atto
        /// </summary>
        /// <param name="path">Percorso del documento atto</param>
        /// <returns></returns>
        [HttpGet]
        [Route("file")]
        public async Task<ActionResult> Download(string path)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                var file = await apiGateway.Atti.Download(path);
                return File(file.Content, "application/pdf",
                    file.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per avere gli articoli
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <returns></returns>
        [Route("articoli")]
        public async Task<ActionResult> GetArticoli(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var articoli = await apiGateway.Atti.GetArticoli(id);
            return Json(articoli, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per creare articoli nell'atto
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="articoli">articoli</param>
        /// <returns></returns>
        [Route("crea-articoli")]
        public async Task<ActionResult> CreaArticoli(Guid id, string articoli)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.CreaArticolo(id, articoli);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per elminare un articolo
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("elimina-articolo")]
        public async Task<ActionResult> EliminaArticolo(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.EliminaArticolo(id);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per creare commi agli articoli
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="commi">commi</param>
        /// <returns></returns>
        [Route("crea-commi")]
        public async Task<ActionResult> CreaCommi(Guid id, string commi)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.CreaComma(id, commi);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per avere i commi
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("commi")]
        public async Task<ActionResult> GetCommi(Guid id, bool expanded = false)
        {
            var apiGateway = new ApiGateway(_Token);
            var commi = await apiGateway.Atti.GetComma(id, expanded);
            return Json(commi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per elminare un comma
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <returns></returns>
        [Route("elimina-comma")]
        public async Task<ActionResult> EliminaComma(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.EliminaComma(id);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per avere le lettere
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <returns></returns>
        [Route("lettere")]
        public async Task<ActionResult> GetLettere(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var lettere = await apiGateway.Atti.GetLettere(id);
            return Json(lettere, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per creare lettere ai commi
        /// </summary>
        /// <param name="id">Guid comma</param>
        /// <param name="lettere">lettere</param>
        /// <returns></returns>
        [Route("crea-lettere")]
        public async Task<ActionResult> CreaLettere(Guid id, string lettere)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.CreaLettera(id, lettere);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per elminare una lettera
        /// </summary>
        /// <param name="id">Guid lettera</param>
        /// <returns></returns>
        [Route("elimina-lettera")]
        public async Task<ActionResult> EliminaLettere(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            await apiGateway.Atti.EliminaLettera(id);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per salvare o modificare l'atto
        /// </summary>
        /// <param name="atto">Modello atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("relatori")]
        [HttpPost]
        public async Task<ActionResult> SalvaRelatoriAtto(AttoRelatoriModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.Atti.SalvaRelatori(model);

                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///     Controller per salvare o modificare l'atto
        /// </summary>
        /// <param name="atto">Modello atto</param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("abilita-fascicolazione")]
        [HttpPost]
        public async Task<ActionResult> PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.Atti.PubblicaFascicolo(model);

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("sposta-up")]
        [HttpGet]
        public async Task<ActionResult> MoveUp(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var atto = await apiGateway.Atti.Get(id);
            await apiGateway.Atti.SPOSTA_UP(id);
            return RedirectToAction("RiepilogoAtti", "Atti", new { id = atto.UIDSeduta });
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("sposta-down")]
        [HttpGet]
        public async Task<ActionResult> MoveDown(Guid id)
        {
            var apiGateway = new ApiGateway(_Token);
            var atto = await apiGateway.Atti.Get(id);
            await apiGateway.Atti.SPOSTA_DOWN(id);
            return RedirectToAction("RiepilogoAtti", "Atti", new { id = atto.UIDSeduta });
        }

       /// <summary>
       /// Controller per bloccare la presentazione degli ordini del giorno per un atto
       /// </summary>
       /// <param name="model"></param>
       /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("bloccoODG")]
        [HttpPost]
        public async Task<ActionResult> BloccoODG(BloccoODGModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.Atti.BloccoODG(model);

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// Controller per attivare o disattivare il jolly che toglie il limite alla presentazione degli ordini del giorno
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("jollyODG")]
        [HttpPost]
        public async Task<ActionResult> JollyODG(JollyODGModel model)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                await apiGateway.Atti.JollyODG(model);

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("tipi")]
        public async Task<ActionResult> GetTipi(bool dasi = true)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                return Json(await apiGateway.Atti.GetTipi(dasi), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Route("griglia-testi")]
        public async Task<ActionResult> GetGrigliaTesto(Guid id)
        {
            try
            {
                var apiGateway = new ApiGateway(_Token);
                return Json(await apiGateway.Atti.GetGrigliaTesto(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }


    }
}