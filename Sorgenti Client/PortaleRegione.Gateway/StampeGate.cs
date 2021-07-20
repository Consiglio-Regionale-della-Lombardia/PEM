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
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class StampeGate : BaseGateway
    {
        static  StampeGate _instance;  
   
        public static StampeGate Instance => _instance ?? (_instance = new StampeGate());

        private StampeGate()
        {
            
        }

        public static async Task InserisciStampa(BaseRequest<EmendamentiDto, StampaDto> model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("InserisciStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("InserisciStampa", ex);
                throw ex;
            }
        }

        public static async Task EliminaStampa(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe?id={id}";
                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaStampa", ex);
                throw ex;
            }
        }
        public static async Task ResetStampa(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/reset?id={id}";
                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ResetStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ResetStampa", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<StampaDto>> Get(int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/view";

                var model = new BaseRequest<StampaDto>
                {
                    page = page,
                    size = size
                };
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<StampaDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetStampe", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetStampe", ex);
                throw ex;
            }
        }
        
        public static async Task<StampaDto> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/id/{id}";
                var lst = JsonConvert.DeserializeObject<StampaDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetStampa", ex);
                throw ex;
            }
        }

        public static async Task AddInfo(Guid id, string message)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/id/{id}/add-info?message={message}";
                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Add info Stampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Add info Stampa", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<Stampa_InfoDto>> GetInfo(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/id/{id}/info";
                var lst = JsonConvert.DeserializeObject<IEnumerable<Stampa_InfoDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetInfoStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetInfoStampa", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<Stampa_InfoDto>> GetInfo()
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe/id/info";
                var lst = JsonConvert.DeserializeObject<IEnumerable<Stampa_InfoDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetInfoStampe", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetInfoStampe", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<StampaDto>> JobGetStampe(int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe/view";

                var model = new BaseRequest<StampaDto>
                {
                    page = page,
                    size = size
                };
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<StampaDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobGetStampe", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobGetStampe", ex);
                throw ex;
            }
        }

        public static async Task JobUnLockStampa(Guid stampaUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe/unlock";
                var body = JsonConvert.SerializeObject(new StampaRequest
                {
                    stampaUId = stampaUId
                });
                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobUnLockStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobUnLockStampa", ex);
                throw ex;
            }
        }

        public static async Task JobErrorStampa(Guid stampaUId, string errorMessage)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe/error";
                var body = JsonConvert.SerializeObject(new StampaRequest
                {
                    stampaUId = stampaUId,
                    messaggio = errorMessage
                });
                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobErrorStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobErrorStampa", ex);
                throw ex;
            }
        }

        public static async Task JobUpdateFileStampa(StampaDto stampa)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe";
                var body = JsonConvert.SerializeObject(stampa);
                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobUpdateFileStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobUpdateFileStampa", ex);
                throw ex;
            }
        }

        public static async Task JobSetInvioStampa(StampaDto stampa)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe/inviato";
                var body = JsonConvert.SerializeObject(stampa);
                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobSetInvioStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobSetInvioStampa", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<EmendamentiDto>> JobGetEmendamenti(string queryEM, int page,
            int size = 20)
        {
            try
            {
                var requestUrl = $"{apiUrl}/job/stampe/emendamenti";
                var body = JsonConvert.SerializeObject(new EmendamentiByQueryModel
                {
                    Query = queryEM,
                    page = page
                });

                var lst = JsonConvert.DeserializeObject<BaseResponse<EmendamentiDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("JobGetEmendamenti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("JobGetEmendamenti", ex);
                throw ex;
            }
        }

        public static async Task<FileResponse> DownloadStampa(Guid stampaUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/stampe?id={stampaUId}";

                var lst = await GetFile(requestUrl);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("DownloadStampa", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("DownloadStampa", ex);
                throw ex;
            }
        }
    }
}