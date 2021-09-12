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
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class AdminGate : BaseGateway
    {
        static  AdminGate _instance;  
   
        public static AdminGate Instance => _instance ?? (_instance = new AdminGate());

        private AdminGate()
        {
            
        }

        public static async Task<PersonaDto> GetPersona(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/view/{id}";
                var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetPersonaAdmin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPersonaAdmin", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<PersonaDto>> GetPersone(BaseRequest<PersonaDto> request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/users/view";
                var body = JsonConvert.SerializeObject(request);

                var lst = JsonConvert.DeserializeObject<BaseResponse<PersonaDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetPersoneAdmin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPersoneAdmin", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<KeyValueDto>> GetGruppiInDb()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/gruppi-in-db";

                var lst = JsonConvert.DeserializeObject<IEnumerable<KeyValueDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetGruppiInDb", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetGruppiInDb", ex);
                throw ex;
            }
        }


        public static async Task<Guid> SalvaPersona(PersonaUpdateRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/salva";
                var body = JsonConvert.SerializeObject(request);

                var result = JsonConvert.DeserializeObject<Guid>(await Post(requestUrl, body));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaPersona", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaPersona", ex);
                throw ex;
            }
        }

        public static async Task ResetPin(ResetRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/reset-pin";
                var body = JsonConvert.SerializeObject(request);

                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ResetPin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ResetPin", ex);
                throw ex;
            }
        }

        public static async Task ResetPassword(ResetRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/reset-password";
                var body = JsonConvert.SerializeObject(request);

                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ResetPassword", ex);
                throw ex;   
            }
            catch (Exception ex)
            {
                Log.Error("ResetPassword", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<RuoliDto>> GetRuoliAD()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/ad/ruoli";
                var lst = JsonConvert.DeserializeObject<IEnumerable<RuoliDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRuoliAD", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetRuoliAD", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<GruppoAD_Dto>> GetGruppiPoliticiAD()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/ad/gruppi-politici";
                var lst = JsonConvert.DeserializeObject<IEnumerable<GruppoAD_Dto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetGruppiPoliticiAD", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetGruppiPoliticiAD", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<GruppiDto>> GetGruppiAdmin(BaseRequest<GruppiDto> request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/groups/view";
                var body = JsonConvert.SerializeObject(request);
                var lst = JsonConvert.DeserializeObject<BaseResponse<GruppiDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetGruppiAdmin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetGruppiAdmin", ex);
                throw ex;
            }
        }
    }
}