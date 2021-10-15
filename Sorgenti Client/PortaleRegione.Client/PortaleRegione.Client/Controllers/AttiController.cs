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
            var _seduteGateway = new SeduteGateway(_Token);
            var sedutaInDb = await _seduteGateway.Get(id);

            var _attiGateway = new AttiGateway(_Token);
            var model = new AttiViewModel
            { Data = await _attiGateway.Get(id, mode, page, size), Seduta = sedutaInDb };

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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.Elimina(id);
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
            var _personeGateway = new PersoneGateway(_Token);
            var model = new AttiViewModel
            {
                Assessori = await _personeGateway.GetAssessoriRiferimento(),
                Relatori = await _personeGateway.GetRelatori(null),
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
            var _attiGateway = new AttiGateway(_Token);
            var _personeGateway = new PersoneGateway(_Token);
            var model = new AttiViewModel
            {
                Assessori = await _personeGateway.GetAssessoriRiferimento(),
                Relatori = await _personeGateway.GetRelatori(null),
                Atto = await _attiGateway.GetFormUpdate(id)
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
                var _attiGateway = new AttiGateway(_Token);
                AttiDto attoSalvato = null;
                if (atto.UIDAtto == Guid.Empty)
                    attoSalvato = await _attiGateway.Salva(atto);
                else
                    attoSalvato = await _attiGateway.Modifica(atto);

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
                var _attiGateway = new AttiGateway(_Token);
                var file = await _attiGateway.Download(path);
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
            var _attiGateway = new AttiGateway(_Token);
            var articoli = await _attiGateway.GetArticoli(id);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.CreaArticolo(id, articoli);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.EliminaArticolo(id);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.CreaComma(id, commi);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Controller per avere i commi
        /// </summary>
        /// <param name="id">Guid articolo</param>
        /// <returns></returns>
        [Route("commi")]
        public async Task<ActionResult> GetCommi(Guid id)
        {
            var _attiGateway = new AttiGateway(_Token);
            var commi = await _attiGateway.GetComma(id);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.EliminaComma(id);
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
            var _attiGateway = new AttiGateway(_Token);
            var lettere = await _attiGateway.GetLettere(id);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.CreaLettera(id, lettere);
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
            var _attiGateway = new AttiGateway(_Token);
            await _attiGateway.EliminaLettera(id);
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
                var _attiGateway = new AttiGateway(_Token);
                await _attiGateway.SalvaRelatori(model);

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
                var _attiGateway = new AttiGateway(_Token);
                await _attiGateway.PubblicaFascicolo(model);

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
            var _attiGateway = new AttiGateway(_Token);
            var atto = await _attiGateway.Get(id);
            await _attiGateway.SPOSTA_UP(id);
            return RedirectToAction("RiepilogoAtti", "Atti", new { id = atto.UIDSeduta });
        }

        [Authorize(Roles = RuoliExt.Amministratore_PEM + "," + RuoliExt.Segreteria_Assemblea)]
        [Route("sposta-down")]
        [HttpGet]
        public async Task<ActionResult> MoveDown(Guid id)
        {
            var _attiGateway = new AttiGateway(_Token);
            var atto = await _attiGateway.Get(id);
            await _attiGateway.SPOSTA_DOWN(id);
            return RedirectToAction("RiepilogoAtti", "Atti", new { id = atto.UIDSeduta });
        }
    }
}