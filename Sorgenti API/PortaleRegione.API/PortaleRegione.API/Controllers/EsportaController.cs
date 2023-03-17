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

using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ApiRoutes = PortaleRegione.DTO.Routes.ApiRoutes;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione dei file di esportazione emendamenti
    /// </summary>
    public class EsportaController : BaseApiController
    {
        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="authLogic"></param>
        /// <param name="personeLogic"></param>
        /// <param name="legislatureLogic"></param>
        /// <param name="seduteLogic"></param>
        /// <param name="attiLogic"></param>
        /// <param name="dasiLogic"></param>
        /// <param name="firmeLogic"></param>
        /// <param name="attiFirmeLogic"></param>
        /// <param name="emendamentiLogic"></param>
        /// <param name="publicLogic"></param>
        /// <param name="notificheLogic"></param>
        /// <param name="esportaLogic"></param>
        /// <param name="stampeLogic"></param>
        /// <param name="utilsLogic"></param>
        /// <param name="adminLogic"></param>
        public EsportaController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic, StampeLogic stampeLogic,
            UtilsLogic utilsLogic, AdminLogic adminLogic) : base(unitOfWork, authLogic, personeLogic, legislatureLogic,
            seduteLogic, attiLogic, dasiLogic, firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic,
            esportaLogic, stampeLogic, utilsLogic, adminLogic)
        {
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato zip
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Esporta.EsportaGrigliaZip)]
        public async Task<IHttpActionResult> EsportaGrigliaZipDasi(List<Guid> data)
        {
            try
            {
                var file = await _esportaLogic.EsportaGrigliaZipDASI(data);
                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("EsportaGrigliaXLS", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Esporta.EsportaGrigliaExcelDasi)]
        public async Task<IHttpActionResult> EsportaGrigliaExcelDasi(List<Guid> data)
        {
            try
            {
                var file = await _esportaLogic.EsportaGrigliaExcelDASI(data);
                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("EsportaGrigliaXLS", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Esporta.EsportaGrigliaExcel)]
        public async Task<IHttpActionResult> EsportaGrigliaExcel(EmendamentiViewModel model)
        {
            try
            {
                var file = await _esportaLogic.EsportaGrigliaExcel(model, CurrentUser);
                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("EsportaGrigliaXLS", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Esporta.EsportaGrigliaExcelUOLA)]
        public async Task<IHttpActionResult> EsportaGrigliaExcel_UOLA(EmendamentiViewModel model)
        {
            try
            {
                if (Session._currentRole != RuoliIntEnum.Amministratore_PEM
                    && Session._currentRole != RuoliIntEnum.Segreteria_Assemblea)
                    throw new InvalidOperationException("Operazione non eseguibile per il ruolo assegnato");

                var file = await _esportaLogic.EsportaGrigliaReportExcel(model, CurrentUser);
                return ResponseMessage(file);
            }
            catch (Exception e)
            {
                Log.Error("EsportaGrigliaXLS", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato word
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="ordine">ordinamento emendamenti atto</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Esporta.EsportaGrigliaWord)]
        public async Task<IHttpActionResult> EsportaGrigliaWord(Guid id, OrdinamentoEnum ordine, ClientModeEnum mode)
        {
            try
            {
                var response =
                    ResponseMessage(await _esportaLogic.HTMLtoWORD(id, ordine, mode, CurrentUser));

                return response;
            }
            catch (Exception e)
            {
                Log.Error("EsportaGrigliaDOC", e);
                return ErrorHandler(e);
            }
        }
    }
}