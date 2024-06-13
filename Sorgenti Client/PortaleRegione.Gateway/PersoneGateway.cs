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
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public class PersoneGateway : BaseGateway, IPersoneGateway
    {
        private readonly string _token;

        protected internal PersoneGateway()
        {

        }

        protected internal PersoneGateway(string token)
        {
            _token = token;
        }

        public async Task<LoginResponse> Login(LoginRequest model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Autenticazione.Login}";
            var body = JsonConvert.SerializeObject(model);

            return JsonConvert.DeserializeObject<LoginResponse>(await Post(requestUrl, body, _token));
        }

        public async Task<LoginResponse> CambioRuolo(RuoliIntEnum ruolo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Autenticazione.CambioRuolo.Replace("{ruolo}", ruolo.ToString())}";

            var lst = JsonConvert.DeserializeObject<LoginResponse>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<LoginResponse> CambioGruppo(int gruppo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Autenticazione.CambioGruppo.Replace("{gruppo}", gruppo.ToString())}";
            var lst = JsonConvert.DeserializeObject<LoginResponse>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> GetAssessoriRiferimento()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Gruppi.GetAssessori}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> GetRelatori(Guid? attoUId)
        {
            if (attoUId == null)
                attoUId = Guid.Empty;
            var requestUrl = $"{apiUrl}/{ApiRoutes.Gruppi.GetRelatori.Replace("{id}", attoUId.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<KeyValueDto>> GetGruppiAttivi()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Gruppi.GetAll}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<KeyValueDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<PersonaDto> Get(Guid id, bool isGiunta = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Persone.GetPersona.Replace("{id}", id.ToString()).Replace("{is_giunta}", isGiunta.ToString())}";
            var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> Get()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Persone.GetAll}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<RuoliDto> GetRuolo(RuoliIntEnum ruolo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Ruoli.GetRuolo.Replace("{id}", ((int)ruolo).ToString())}"; // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/888
            var lst = JsonConvert.DeserializeObject<RuoliDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task CheckPin(CambioPinModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Persone.CheckPin}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }
        public async Task SalvaPin(CambioPinModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Persone.CambioPin}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task<List<PersonaPublicDto>> GetProponentiFirmatari()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Persone.GetProponentiFirmatari}";
            var lst = JsonConvert.DeserializeObject<List<PersonaPublicDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> GetGiuntaRegionale()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Gruppi.GetGiunta}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> GetSegreteriaPolitica(int id, bool firma, bool deposito)
        {
            var requestUrl =
                $"{apiUrl}/{ApiRoutes.Gruppi.GetSegreteriaPoliticaGruppo.Replace("{id}", id.ToString()).Replace("{firma}", firma.ToString()).Replace("{deposito}", deposito.ToString())}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<PersonaDto> GetCapoGruppo(int id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Gruppi.GetCapoGruppo.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PersonaDto>> GetSegreteriaGiuntaRegionale(bool firma, bool deposito)
        {
            var requestUrl =
                $"{apiUrl}/{ApiRoutes.Gruppi.GetSegreteriaGiunta.Replace("{firma}", firma.ToString()).Replace("{deposito}", deposito.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl, _token));
            return lst;
        }
    }
}