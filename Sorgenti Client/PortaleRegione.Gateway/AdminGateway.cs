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
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public class AdminGateway : BaseGateway, IAdminGateway
    {
        private readonly string _token;

        protected internal AdminGateway(string token)
        {
            _token = token;
        }

        public async Task<PersonaDto> GetPersona(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/view/{id}";
                var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl, _token));

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

        public async Task<RiepilogoUtentiModel> GetPersone(BaseRequest<PersonaDto> request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/users/view";
                var body = JsonConvert.SerializeObject(request);

                var lst = JsonConvert.DeserializeObject<RiepilogoUtentiModel>(await Post(requestUrl, body, _token));

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

        public async Task<IEnumerable<KeyValueDto>> GetGruppiInDb()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/gruppi-in-db";

                var lst = JsonConvert.DeserializeObject<IEnumerable<KeyValueDto>>(await Get(requestUrl, _token));

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


        public async Task<Guid> SalvaPersona(PersonaUpdateRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/salva";
                var body = JsonConvert.SerializeObject(request);

                var result = JsonConvert.DeserializeObject<Guid>(await Post(requestUrl, body, _token));
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

        public async Task EliminaPersona(Guid uid_persona)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/elimina?id={uid_persona}";

                await Delete(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaPersona", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaPersona", ex);
                throw ex;
            }

        }

        public async Task ResetPin(ResetRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/reset-pin";
                var body = JsonConvert.SerializeObject(request);

                await Put(requestUrl, body, _token);
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

        public async Task ResetPassword(ResetRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/reset-password";
                var body = JsonConvert.SerializeObject(request);

                await Put(requestUrl, body, _token);
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

        public async Task<IEnumerable<RuoliDto>> GetRuoliAD()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/ad/ruoli";
                var lst = JsonConvert.DeserializeObject<IEnumerable<RuoliDto>>(await Get(requestUrl, _token));

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

        public async Task<IEnumerable<GruppoAD_Dto>> GetGruppiPoliticiAD()
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/ad/gruppi-politici";
                var lst = JsonConvert.DeserializeObject<IEnumerable<GruppoAD_Dto>>(await Get(requestUrl, _token));

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

        public async Task<RiepilogoGruppiModel> GetGruppiAdmin(BaseRequest<GruppiDto> request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/groups/view";
                var body = JsonConvert.SerializeObject(request);
                var lst = JsonConvert.DeserializeObject<RiepilogoGruppiModel>(await Post(requestUrl, body, _token));

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

        public async Task SalvaGruppo(SalvaGruppoRequest request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/admin/salva-gruppo";
                var body = JsonConvert.SerializeObject(request);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaGruppo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaGruppo", ex);
                throw ex;
            }

        }
    }
}