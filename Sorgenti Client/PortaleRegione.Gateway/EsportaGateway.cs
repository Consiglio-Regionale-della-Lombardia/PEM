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

using Newtonsoft.Json;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public class EsportaGateway : BaseGateway, IEsportaGateway
    {
        private readonly string _token;

        protected internal EsportaGateway(string token)
        {
            _token = token;
        }

        public async Task<FileResponse> EsportaXLS(EmendamentiViewModel model)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/emendamenti/esporta-griglia-xls";

                var body = JsonConvert.SerializeObject(model);
                var lst = await GetFile(requestUrl, body, _token);

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

        public async Task<FileResponse> EsportaXLS_UOLA(EmendamentiViewModel model)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/emendamenti/esporta-griglia-xls-segreteria";

                var body = JsonConvert.SerializeObject(model);
                var lst = await GetFile(requestUrl, body, _token);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EsportaXLS_UOLA", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EsportaXLS_UOLA", ex);
                throw ex;
            }
        }

        public async Task<FileResponse> EsportaXLSDASI(RiepilogoDASIModel model)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/dasi/esporta-griglia-xls";

                var body = JsonConvert.SerializeObject(model);
                var lst = await GetFile(requestUrl, body, _token);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EsportaXLSDASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EsportaXLSDASI", ex);
                throw ex;
            }
        }

        public async Task<FileResponse> EsportaWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/esporta-griglia-doc?id={attoUId}&ordine={ordine}&mode={mode}";

                var lst = await GetFile(requestUrl, _token);

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