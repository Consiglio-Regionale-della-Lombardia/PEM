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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;

namespace PortaleRegione.Gateway
{
    public class TemplatesGateway : BaseGateway, ITemplatesGateway
    {
        private readonly string _token;

        protected internal TemplatesGateway(string token)
        {
            _token = token;
        }

        public async Task<BaseResponse<TemplatesItemDto>> GetAll()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Templates.GetAll}";

            var lst = JsonConvert.DeserializeObject<BaseResponse<TemplatesItemDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<TemplatesItemDto> Get(Guid uid)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Templates.Get.Replace("{id}", uid.ToString())}";

            var lst = JsonConvert.DeserializeObject<TemplatesItemDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task Delete(Guid uid)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Templates.Delete.Replace("{id}", uid.ToString())}";

            await Delete(requestUrl, _token);
        }

        public async Task<TemplatesItemDto> Save(TemplatesItemDto item)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Templates.Save}";
            var body = JsonConvert.SerializeObject(item);
            var result = JsonConvert.DeserializeObject<TemplatesItemDto>(await Post(requestUrl, body, _token));
            return result;
        }
    }
}