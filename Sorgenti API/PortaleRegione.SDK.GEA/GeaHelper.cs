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
            // Analizza la risposta per estrarre il token
            // Ritorna il token ottenuto
            return responseData;
        }
 
        public async Task<object[]> RicercaAtti(string tipoAtto, string legislatura, string numeroAtto)
        {
            /*return await _geaApiService.RicercaAttiAsync(tipoAtto, legislatura, numeroAtto);*/
            return null;
        }
    }
}