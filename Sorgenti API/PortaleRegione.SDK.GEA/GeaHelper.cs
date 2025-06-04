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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using PortaleRegione.DTO.Request;
using PortaleRegione.SDK.GEA.Contracts;

namespace PortaleRegione.SDK.GEA
{
    public class GeaHelper
    {
        public async Task<string> EffettuaLogin(string username, string password)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var endpoint = $"http://10.177.4.12:8082/alfresco/s/api/login?u={username}&pw={password}";
            var requestContent = new StringContent($"{{\"username\": \"{username}\", \"password\": \"{password}\"}}", Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            // Carico l’XML in un XDocument
            var doc = XDocument.Parse(responseData);
            var ticketValue = doc.Root?.Value;
            return ticketValue ?? string.Empty;
        }
 
        public async Task<object[]> RicercaAtti(string token, CercaAttiGeaRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(10);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", token);
            
                var endpoint = $"http://10.177.4.12:8082/alfresco/s/crl/atto/ricerca/avanzata";
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                // Leggo la risposta come stringa JSON
                var responseData = await response.Content.ReadAsStringAsync();
            
                var result = JsonConvert.DeserializeObject<object[]>(responseData);
                return result ?? Array.Empty<object>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}