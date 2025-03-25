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
using Newtonsoft.Json;
using PortaleRegione.DTO.Routes;

namespace PortaleRegione.Gateway
{
    public class DASIGateway_Pubblico: BaseGateway, IDASIGateway_Pubblico
    {
        public async Task<string> GetBody(Guid id, bool approvato = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Public.ViewDASI.Replace("{id}", id.ToString()).Replace("{approvato}", approvato.ToString())}";
            var result = await Get(requestUrl, string.Empty);
            var lst = JsonConvert.DeserializeObject<string>(result);

            return lst;
        }
    }
}