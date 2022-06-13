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
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione dei file di esportazione emendamenti
    /// </summary>
    public class EsportaController : BaseApiController
    {
        private readonly EsportaLogic _logicEsporta;
        private readonly PersoneLogic _logicPersone;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logicEsporta"></param>
        /// <param name="logicPersone"></param>
        public EsportaController(EsportaLogic logicEsporta, PersoneLogic logicPersone)
        {
            _logicEsporta = logicEsporta;
            _logicPersone = logicPersone;
        }

        /// <summary>
        ///     Endpoint per esportare la griglia emendamenti in formato excel
        /// </summary>
        /// <param name="id">Guid atto</param>
        /// <param name="ordine">ordinamento emendamenti atto</param>
        /// <param name="mode"></param>
        /// <param name="is_report"></param>
        /// <returns></returns>
        [Route("dasi/esporta-griglia-xls")]
        [HttpPost]
        public async Task<IHttpActionResult> EsportaGrigliaExcelDASI(RiepilogoDASIModel model)
        {
            try
            {
                var session = GetSession();

                if (session._currentRole != RuoliIntEnum.Amministratore_PEM
                    && session._currentRole != RuoliIntEnum.Segreteria_Assemblea)
                {
                    return BadRequest("Operazione non eseguibile per il ruolo assegnato");
                }

                var persona = await _logicPersone.GetPersona(session);

                var file = await _logicEsporta.EsportaGrigliaExcelDASI(model, persona);
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
        /// <param name="id">Guid atto</param>
        /// <param name="ordine">ordinamento emendamenti atto</param>
        /// <param name="mode"></param>
        /// <param name="is_report"></param>
        /// <returns></returns>
        [Route("emendamenti/esporta-griglia-xls")]
        [HttpGet]
        public async Task<IHttpActionResult> EsportaGrigliaExcel(Guid id, OrdinamentoEnum ordine, ClientModeEnum mode,
            bool is_report = false)
        {
            try
            {
                var session = GetSession();

                if (session._currentRole != RuoliIntEnum.Amministratore_PEM
                    && session._currentRole != RuoliIntEnum.Segreteria_Assemblea)
                {
                    if (is_report)
                    {
                        return BadRequest("Operazione non eseguibile per il ruolo assegnato");
                    }
                }

                var persona = await _logicPersone.GetPersona(session);

                if (is_report)
                {
                    return ResponseMessage(await _logicEsporta.EsportaGrigliaReportExcel(id, ordine, mode, persona));
                }

                var file = await _logicEsporta.EsportaGrigliaExcel(id, ordine, mode, persona);
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
        [Route("emendamenti/esporta-griglia-doc")]
        [HttpGet]
        public async Task<IHttpActionResult> EsportaGrigliaWord(Guid id, OrdinamentoEnum ordine, ClientModeEnum mode)
        {
            try
            {
                var session = GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var response =
                    ResponseMessage(await _logicEsporta.HTMLtoWORD(id, ordine, mode, persona));

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