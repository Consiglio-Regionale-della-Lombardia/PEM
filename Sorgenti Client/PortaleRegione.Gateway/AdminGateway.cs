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
using PortaleRegione.DTO;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
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
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetPersona.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl, _token));

            return lst;
        }

        public async Task<RiepilogoUtentiModel> GetPersone(BaseRequest<PersonaDto> request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetUtenti}";
            var body = JsonConvert.SerializeObject(request);

            var lst = JsonConvert.DeserializeObject<RiepilogoUtentiModel>(await Post(requestUrl, body, _token));

            return lst;
        }

        public async Task<IEnumerable<KeyValueDto>> GetGruppiInDb()
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetGruppiInDb}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<KeyValueDto>>(await Get(requestUrl, _token));

            return lst;
        }


        public async Task<Guid> SalvaPersona(PersonaUpdateRequest request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.SalvaUtente}";
            var body = JsonConvert.SerializeObject(request);

            var result = JsonConvert.DeserializeObject<Guid>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task EliminaPersona(Guid uid_persona)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.EliminaUtente.Replace("{id}", uid_persona.ToString())}";

            await Delete(requestUrl, _token);
        }

        public async Task ResetPin(ResetRequest request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.ResetPin}";
            var body = JsonConvert.SerializeObject(request);

            await Put(requestUrl, body, _token);
        }

        public async Task ResetPassword(ResetRequest request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.ResetPassword}";
            var body = JsonConvert.SerializeObject(request);

            await Put(requestUrl, body, _token);
        }

        public async Task<IEnumerable<RuoliDto>> GetRuoliAD()
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetRuoliAD}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<RuoliDto>>(await Get(requestUrl, _token));

            return lst;
        }

        public async Task<IEnumerable<GruppoAD_Dto>> GetGruppiPoliticiAD()
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetGruppiPoliticiAD}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<GruppoAD_Dto>>(await Get(requestUrl, _token));

            return lst;
        }

        public async Task<RiepilogoGruppiModel> GetGruppiAdmin(BaseRequest<GruppiDto> request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.GetGruppi}";
            var body = JsonConvert.SerializeObject(request);
            var lst = JsonConvert.DeserializeObject<RiepilogoGruppiModel>(await Post(requestUrl, body, _token));

            return lst;
        }

        public async Task SalvaGruppo(SalvaGruppoRequest request)
        {
            var requestUrl = $"{apiUrl}{ApiRoutes.Admin.SalvaGruppo}";
            var body = JsonConvert.SerializeObject(request);

            await Post(requestUrl, body, _token);
        }
    }
}