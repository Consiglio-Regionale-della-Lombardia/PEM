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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public class EsportaGate : BaseGateway
    {
        private static EsportaGate _instance;

        private EsportaGate()
        {
        }

        public static EsportaGate Instance => _instance ?? (_instance = new EsportaGate());

        public static async Task<FileResponse> EsportaXLS(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode,
            bool is_report = false)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/emendamenti/esporta-griglia-xls?id={attoUId}&ordine={ordine}&mode={mode}&is_report={is_report}";

                var lst = await GetFile(requestUrl);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EsportaXLS", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EsportaXLS", ex);
                throw ex;
            }
        }

        public static async Task<FileResponse> EsportaWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/esporta-griglia-doc?id={attoUId}&ordine={ordine}&mode={mode}";

                var lst = await GetFile(requestUrl);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EsportaWORD", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EsportaWORD", ex);
                throw ex;
            }
        }
    }
}