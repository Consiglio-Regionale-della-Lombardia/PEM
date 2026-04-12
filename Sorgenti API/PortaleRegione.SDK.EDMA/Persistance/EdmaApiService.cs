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
using PortaleRegione.SDK.EDMA.Contracts;
using PortaleRegione.SDK.EDMA.Helpers;
using PortaleRegione.SDK.EDMA.Models;

namespace PortaleRegione.SDK.EDMA.Persistance
{
    public class EdmaApiService : IEdmaApiService
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;

        public EdmaApiService(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
        }

        /// <summary>
        ///     Crea un documento in EDMA
        /// </summary>
        public async Task<EdmaResponse> CreaDocumentoAsync(DocumentoBase documento, byte[] fileBytes, string estensioneFile)
        {
            try
            {
                var xmlBody = EdmaXmlHelper.GeneraCreaDocumentoXml(documento, fileBytes, estensioneFile);
                var endpoint = $"{_url}/EdmaWeb/service/DocumentoFile/creaDocumento?no-session=true";
                var responseData = await PostXmlAsync(endpoint, xmlBody);

                return new EdmaResponse
                {
                    Success = true,
                    RawResponse = responseData
                };
            }
            catch (Exception e)
            {
                return new EdmaResponse
                {
                    Success = false,
                    Message = e.Message,
                    RawResponse = e.ToString()
                };
            }
        }

        /// <summary>
        ///     Invia un documento al protocollo EDMA
        /// </summary>
        public async Task<EdmaResponse> ProtocollaDocumentoAsync(string idDocumento, SchedaProtocollo scheda)
        {
            try
            {
                var xmlBody = EdmaXmlHelper.GeneraProtocollaXml(scheda);
                var endpoint = $"{_url}/EdmaWeb/service/DocumentoFile/{idDocumento}/protocolla?no-session=true";
                var responseData = await PostXmlAsync(endpoint, xmlBody);

                return new EdmaResponse
                {
                    Success = true,
                    RawResponse = responseData
                };
            }
            catch (Exception e)
            {
                return new EdmaResponse
                {
                    Success = false,
                    Message = e.Message,
                    RawResponse = e.ToString()
                };
            }
        }

        /// <summary>
        ///     Carica un metadocumento in EDMA
        /// </summary>
        public async Task<EdmaResponse> CaricaMetaDocumentoAsync(MetaDocumento metaDocumento)
        {
            try
            {
                var xmlBody = new XElement("metadocumento",
                    new XAttribute("class", "it.lispa.edma.erato.po.Metadocumento"),
                    new XAttribute("id", "5"),
                    new XAttribute("resolves-to", "it.lispa.edma.erato.po.Metadocumento"),
                    new XElement("codice", metaDocumento.Codice ?? string.Empty),
                    new XElement("descrizione", metaDocumento.Descrizione ?? string.Empty),
                    new XElement("daProtocollare", metaDocumento.DaProtocollare.ToString().ToLower()),
                    new XElement("daFirmare", metaDocumento.DaFirmare.ToString().ToLower()),
                    new XElement("fascicolo", metaDocumento.Fascicolo.ToString().ToLower()),
                    new XElement("modelloobbligatorio", metaDocumento.ModelloObbligatorio.ToString().ToLower()),
                    new XElement("privato", metaDocumento.Privato.ToString().ToLower()),
                    new XElement("esportabile", metaDocumento.Esportabile.ToString().ToLower()),
                    new XElement("id", metaDocumento.Id)
                ).ToString();

                var endpoint = $"{_url}/EdmaWeb/service/Metadocumento/caricaMetadocumento?no-session=true";
                var responseData = await PostXmlAsync(endpoint, xmlBody);

                return new EdmaResponse
                {
                    Success = true,
                    RawResponse = responseData
                };
            }
            catch (Exception e)
            {
                return new EdmaResponse
                {
                    Success = false,
                    Message = e.Message,
                    RawResponse = e.ToString()
                };
            }
        }

        private async Task<string> PostXmlAsync(string endpoint, string xmlBody)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
                var token = Convert.ToBase64String(byteArray);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

                var content = new StringContent(xmlBody, Encoding.GetEncoding("ISO-8859-1"), "text/xml");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
