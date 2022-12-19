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
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Linq;
using System.Threading.Tasks;
using SeduteDto = PortaleRegione.DTO.Domain.SeduteDto;

namespace PortaleRegione.Gateway
{
    public class SeduteGateway : BaseGateway, ISeduteGateway
    {
        private readonly string _token;

        protected internal SeduteGateway(string token)
        {
            _token = token;
        }

        public async Task<BaseResponse<SeduteDto>> Get(int page, int size)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.GetAll}";

            var model = new BaseRequest<SeduteDto>
            {
                page = page,
                size = size
            };
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<SeduteFormUpdateDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.Get.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<SeduteFormUpdateDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<BaseResponse<SeduteDto>> Get(BaseRequest<SeduteDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.GetAll}";
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task Salva(SeduteFormUpdateDto seduta)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.Create}";
            var body = JsonConvert.SerializeObject(seduta);
            await Post(requestUrl, body, _token);
        }

        public async Task<BaseResponse<SeduteDto>> GetAttive()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.GetAttive}";
            var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Get(requestUrl, _token));
            lst!.Results = lst.Results.OrderBy(item => item.Data_seduta);
            return lst;
        }

        public async Task<BaseResponse<SeduteDto>> GetAttiveMOZU()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.GetAttiveMOZU}";
            var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Get(requestUrl, _token));
            lst!.Results = lst.Results.OrderBy(item => item.Data_seduta);
            return lst;
        }

        public async Task<BaseResponse<SeduteDto>> GetAttiveDashboard()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.GetAttiveDashboard}";
            var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Get(requestUrl, _token));
            lst!.Results = lst.Results.OrderBy(item => item.Data_seduta);
            return lst;
        }

        public async Task Modifica(SeduteFormUpdateDto seduta)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.Edit}";
            var body = JsonConvert.SerializeObject(seduta);
            await Put(requestUrl, body, _token);
        }

        public async Task Elimina(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Sedute.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }
    }
}