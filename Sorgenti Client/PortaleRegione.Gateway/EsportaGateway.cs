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
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
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
            var requestUrl =
                $"{apiUrl}/{ApiRoutes.Esporta.EsportaGrigliaExcel}";

            var body = JsonConvert.SerializeObject(model);
            var lst = await GetFile(requestUrl, body, _token);

            return lst;
        }

        public async Task<FileResponse> EsportaXLS_UOLA(EmendamentiViewModel model)
        {
            var requestUrl =
                $"{apiUrl}/{ApiRoutes.Esporta.EsportaGrigliaExcelUOLA}";

            var body = JsonConvert.SerializeObject(model);
            var lst = await GetFile(requestUrl, body, _token);

            return lst;
        }

        public async Task<FileResponse> EsportaXLSDASI(List<Guid> lista)
        {
            var requestUrl =
                $"{apiUrl}/{ApiRoutes.Esporta.EsportaGrigliaZip}";

            var body = JsonConvert.SerializeObject(lista);
            var lst = await GetFile(requestUrl, body, _token);

            return lst;
        }

        public async Task<FileResponse> EsportaWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Esporta.EsportaGrigliaWord.Replace("{id}", attoUId.ToString()).Replace("{ordine}", ordine.ToString()).Replace("{mode}", mode.ToString())}";

            var lst = await GetFile(requestUrl, _token);

            return lst;
        }
    }
}