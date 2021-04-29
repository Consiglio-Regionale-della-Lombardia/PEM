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
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione dei file di esportazione emendamenti
    /// </summary>
    public class EsportaController : BaseApiController
    {
        private readonly EsportaLogic _logicEsporta;
        private readonly PersoneLogic _logicPersone;

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
        /// <returns></returns>
        [Route("emendamenti/esporta-griglia-xls")]
        [HttpGet]
        public async Task<IHttpActionResult> EsportaGrigliaExcel(Guid id, OrdinamentoEnum ordine, bool is_report = false)
        {
            try
            {
                var session = await GetSession();

                if (session._currentRole != RuoliIntEnum.Amministratore_PEM
                    && session._currentRole != RuoliIntEnum.Segreteria_Assemblea)
                {
                    if (is_report == true)
                        return BadRequest("Operazione non eseguibile per il ruolo assegnato");
                }

                var persona = await _logicPersone.GetPersona(session);

                if (is_report)
                    return ResponseMessage(await _logicEsporta.EsportaGrigliaReportExcel(id, persona));

                return ResponseMessage(await _logicEsporta.EsportaGrigliaExcel(id, ordine, persona));
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
        /// <returns></returns>
        [Route("emendamenti/esporta-griglia-doc")]
        [HttpGet]
        public async Task<IHttpActionResult> EsportaGrigliaWord(Guid id, OrdinamentoEnum ordine)
        {
            try
            {
                var session = await GetSession();
                var persona = await _logicPersone.GetPersona(session);

                var response =
                    ResponseMessage(await _logicEsporta.EsportaGrigliaWord(id, ordine, persona));

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