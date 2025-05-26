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
using System.Text;
using System.Threading.Tasks;
using PortaleRegione.SDK.GEA.Contracts;

namespace PortaleRegione.SDK.GEA.Persistance
{
    internal class GeaApiService : IGeaApiService
    {
        private readonly HttpClient _httpClient;
 
        public GeaApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
 
        public async Task<string> LoginAsync(string username, string password)
        {
            var endpoint = "../api/login";
            var requestContent = new StringContent($"{{\"username\": \"{username}\", \"password\": \"{password}\"}}", Encoding.UTF8, "application/json");
 
            var response = await _httpClient.PostAsync(endpoint, requestContent);
            response.EnsureSuccessStatusCode();
 
            var responseData = await response.Content.ReadAsStringAsync();
            // Analizza la risposta per estrarre il token
            // Ritorna il token ottenuto
            return responseData;
        }
 
        public async Task<object[]> RicercaAttiAsync(string tipoAtto, string legislatura, string numeroAtto)
        {
            var endpoint = "../crl/atto/ricerca/semplice";
            var requestContent = new StringContent($"{{\"tipoAtto\": \"{tipoAtto}\", \"legislatura\": \"{legislatura}\", \"numeroAtto\": \"{numeroAtto}\"}}", Encoding.UTF8, "application/json");
 
            var response = await _httpClient.PostAsync(endpoint, requestContent);
            response.EnsureSuccessStatusCode();
 
            var responseData = await response.Content.ReadAsStringAsync();
            // Analizza la risposta per estrarre la lista degli atti
            // Ritorna la lista degli atti trovati
            return null;
        }
    }
}