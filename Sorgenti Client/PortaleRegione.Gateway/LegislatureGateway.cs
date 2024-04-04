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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Routes;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PortaleRegione.Gateway
{
    public class LegislatureGateway : BaseGateway, ILegislatureGateway
    {
        private readonly string _token;

        protected internal LegislatureGateway(string token)
        {
            _token = token;
        }

        public async Task<LegislaturaDto> GetLegislatura(int idLegislatura)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Legislature.Get.Replace("{id}", idLegislatura.ToString())}";

            var lst = JsonConvert.DeserializeObject<LegislaturaDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<int> GetLegislaturaAttuale()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Legislature.GetAttuale}";

            var lst = JsonConvert.DeserializeObject<int>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<LegislaturaDto>> GetLegislature()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Legislature.GetAll}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<LegislaturaDto>>(await Get(requestUrl, _token));
            return lst;
        }
    }
}