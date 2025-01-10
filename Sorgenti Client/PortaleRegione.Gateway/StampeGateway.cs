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
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Gateway
{
    public class StampeGateway : BaseGateway, IStampeGateway
    {
        private readonly string _token;

        protected internal StampeGateway(string token)
        {
            _token = token;
        }

        public async Task<FileResponse> Stampa(string uid)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.Print}?uid={uid}";
            return await GetFile(requestUrl, _token);
        }
        
        public async Task<StampaDto> InserisciStampa(NuovaStampaRequest request)
        {
            var requestUrl = $"{apiUrl}/";
            if (request.Modulo == ModuloStampaEnum.PEM)
                requestUrl += $"{ApiRoutes.PEM.InserisciStampaMassiva}";
            else if (request.Modulo == ModuloStampaEnum.DASI)
                requestUrl += $"{ApiRoutes.DASI.InserisciStampaMassiva}";

            var body = JsonConvert.SerializeObject(request);
            return JsonConvert.DeserializeObject<StampaDto>(await Post(requestUrl, body, _token));
        }
        
        public async Task EliminaStampa(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }
        public async Task ResetStampa(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.Reset.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task<BaseResponse<StampaDto>> Get(int page, int size)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.GetAll}";
            var model = new BaseRequest<StampaDto>
            {
                page = page,
                size = size
            };
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<BaseResponse<StampaDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<StampaDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.Get.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<StampaDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task AddInfo(Guid id, string message)
        {
            var body = new InfoModel
            {
                Id = id,
                Message = message
            };
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.AddInfo}";
            await Post(requestUrl, JsonConvert.SerializeObject(body), _token);
        }

        public async Task<IEnumerable<Stampa_InfoDto>> GetInfo(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.GetInfo.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<Stampa_InfoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<Stampa_InfoDto>> GetInfo()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.GetAllInfo}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<Stampa_InfoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<BaseResponse<AttoDASIDto>> JobGetDASI(string query, int page, int size = 20)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.GetAtti}";
            var body = JsonConvert.SerializeObject(new ByQueryModel
            {
                Query = query,
                page = page
            });
            var lst = JsonConvert.DeserializeObject<BaseResponse<AttoDASIDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<BaseResponse<StampaDto>> JobGetStampe(int page, int size)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.GetAll}";
            var model = new BaseRequest<StampaDto>
            {
                page = page,
                size = size
            };
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<BaseResponse<StampaDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task JobUnLockStampa(Guid stampaUId)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.Unlock}";
            var body = JsonConvert.SerializeObject(new StampaRequest
            {
                stampaUId = stampaUId
            });
            await Post(requestUrl, body, _token);
        }

        public async Task JobErrorStampa(Guid stampaUId, string errorMessage)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.ReportError}";
            var body = JsonConvert.SerializeObject(new StampaRequest
            {
                stampaUId = stampaUId,
                messaggio = errorMessage
            });
            await Post(requestUrl, body, _token);
        }

        public async Task JobUpdateFileStampa(StampaDto stampa)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.UpdateFileStampa}";
            var body = JsonConvert.SerializeObject(stampa);
            await Put(requestUrl, body, _token);
        }

        public async Task JobSetInvioStampa(StampaDto stampa)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.SetInvioStampa}";
            var body = JsonConvert.SerializeObject(stampa);
            await Put(requestUrl, body, _token);
        }

        public async Task<BaseResponse<EmendamentiDto>> JobGetEmendamenti(string queryEm, int page, int size = 20)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Job.Stampe.GetEmendamenti}";
            var body = JsonConvert.SerializeObject(new ByQueryModel
            {
                Query = queryEm,
                page = page,
                size = size
            });
            var lst = JsonConvert.DeserializeObject<BaseResponse<EmendamentiDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<FileResponse> DownloadStampa(Guid stampaUId)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.Download.Replace("{id}", stampaUId.ToString())}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }

        
        public async Task<FileResponse> DownloadStampa(string nomeFile)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Stampe.DownloadFolder.Replace("{nomeFile}", nomeFile)}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }
    }
}