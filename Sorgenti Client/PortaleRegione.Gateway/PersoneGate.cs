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
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class PersoneGate : BaseGateway
    {
        static  PersoneGate _instance;  
   
        public static PersoneGate Instance => _instance ?? (_instance = new PersoneGate());

        private PersoneGate()
        {
            
        }

        public static async Task<LoginResponse> Login(string username, string password)
        {
            try
            {
                var requestUrl = $"{apiUrl}/autenticazione";
                var body = JsonConvert.SerializeObject(new LoginRequest
                {
                    Username = username,
                    Password = password
                });

                return JsonConvert.DeserializeObject<LoginResponse>(await Post(requestUrl, body, false));
            }
            catch (Exception ex)
            {
                Log.Error("Login", ex);
                throw ex;
            }
        }

        public static async Task<LoginResponse> CambioRuolo(RuoliIntEnum ruolo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/autenticazione/cambio-ruolo?ruolo={ruolo}";

                var lst = JsonConvert.DeserializeObject<LoginResponse>(await Get(requestUrl));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CambioRuolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CambioRuolo", ex);
                throw ex;
            }
        }

        public static async Task<LoginResponse> CambioGruppo(int gruppo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/autenticazione/cambio-gruppo?gruppo={gruppo}";

                var lst = JsonConvert.DeserializeObject<LoginResponse>(await Get(requestUrl));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CambioGruppo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CambioGruppo", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> GetAssessoriRiferimento()
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/assessori";

                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAssessoriRiferimento", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAssessoriRiferimento", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> GetRelatori(Guid? attoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/relatori";
                if (attoUId != Guid.Empty)
                    requestUrl += $"?id={attoUId}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRelatori", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetRelatori", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<KeyValueDto>> GetGruppiAttivi()
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/gruppi";

                var lst = JsonConvert.DeserializeObject<IEnumerable<KeyValueDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetGruppi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetGruppi", ex);
                throw ex;
            }
        }

        public static async Task<PersonaDto> Get(Guid id, bool isGiunta = false)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/{id}";
                if (isGiunta)
                    requestUrl += $"?IsGiunta={isGiunta}";

                var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetPersona", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPersona", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> Get()
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/all";
                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetPersone", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPersone", ex);
                throw ex;
            }
        }

        public static async Task<RuoliDto> GetRuolo(RuoliIntEnum ruolo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/{(int) ruolo}";

                var lst = JsonConvert.DeserializeObject<RuoliDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRuolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPersona", ex);
                throw ex;
            }
        }

        public static async Task CheckPin(CambioPinModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/check-pin";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CheckPin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CheckPin", ex);
                throw ex;
            }
        }
        public static async Task SalvaPin(CambioPinModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/cambio-pin";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaPin", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaPin", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> GetGiuntaRegionale()
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/giunta-regionale";

                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetGiuntaRegionale", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetGiuntaRegionale", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> GetSegreteriaPolitica(int id, bool notifica_firma,
            bool notifica_deposito)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/persone/gruppo/{id}/segreteria-politica?notifica_firma={notifica_firma}&notifica_deposito={notifica_deposito}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetSegreteriaPolitica", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetSegreteriaPolitica", ex);
                throw ex;
            }
        }

        public static async Task<PersonaDto> GetCapoGruppo(int id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/persone/gruppo/{id}/capo-gruppo";

                var lst = JsonConvert.DeserializeObject<PersonaDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetCapoGruppo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetCapoGruppo", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<PersonaDto>> GetSegreteriaGiuntaRegionale(bool notifica_firma,
            bool notifica_deposito)
        {
            try
            {
                var requestUrl =
                    $"{apiUrl}/persone/segreteria-giunta-regionale?notifica_firma={notifica_firma}&notifica_deposito={notifica_deposito}";
                var lst = JsonConvert.DeserializeObject<IEnumerable<PersonaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetSegreteriaGiuntaRegionale", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetSegreteriaGiuntaRegionale", ex);
                throw ex;
            }
        }
    }
}