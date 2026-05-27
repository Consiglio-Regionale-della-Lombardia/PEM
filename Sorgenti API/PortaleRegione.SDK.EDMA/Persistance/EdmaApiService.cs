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
using PortaleRegione.SDK.EDMA.Contracts;
using PortaleRegione.SDK.EDMA.Helpers;
using PortaleRegione.SDK.EDMA.Models;
using PortaleRegione.SDK.EDMA.Models.Response;

namespace PortaleRegione.SDK.EDMA.Persistance
{
    /// <summary>
    ///     Implementazione del client EDMA. Le chiamate sono POST XML in
    ///     ISO-8859-1 con autenticazione Basic, secondo quanto previsto dalla
    ///     specifica RSI3. L'<see cref="HttpClient"/> e' un singleton statico
    ///     condiviso fra tutte le istanze del servizio: ASP.NET le ricrea
    ///     spesso e il pattern <c>using new HttpClient()</c> esaurirebbe le
    ///     socket disponibili sotto carico.
    /// </summary>
    public class EdmaApiService : IEdmaApiService
    {
        private static readonly HttpClient SharedHttpClient = new HttpClient
        {
            // Il timeout effettivo viene poi sovrascritto per ogni chiamata
            // tramite CancellationToken se servisse un valore piu' restrittivo.
            Timeout = TimeSpan.FromMinutes(15)
        };

        private static readonly Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");

        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _noSession;
        private readonly Action<string, Exception> _logger;

        public EdmaApiService(string url, string username, string password,
            bool noSession = true,
            Action<string, Exception> logger = null)
        {
            _url = (url ?? string.Empty).TrimEnd('/');
            _username = username ?? string.Empty;
            _password = password ?? string.Empty;
            _noSession = noSession;
            _logger = logger;
        }

        public async Task<EdmaResponse<int>> CaricaMetaDocumentoIdAsync(string codiceMetadocumento)
        {
            var body = EdmaXmlHelper.GeneraCaricaMetadocumentoXml(codiceMetadocumento);
            var endpoint = BuildEndpoint("Metadocumento/caricaMetadocumento");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<int>.Fail(raw.Errore, raw.Body);
            var id = EdmaXmlParser.ParseMetadocumentoId(raw.Body);
            return EdmaResponse<int>.Ok(id, raw.Body);
        }

        public async Task<EdmaResponse<FascicoloPraticaOutput>> CreaInserisciPraticaAsync(
            string idSottoFascicoloPadre,
            string codiceMetadocPadre,
            int metamoduloPadre,
            FascicoloPratica pratica)
        {
            var body = EdmaXmlHelper.GeneraCreaInserisciPraticaXml(idSottoFascicoloPadre, codiceMetadocPadre,
                metamoduloPadre, pratica);
            var endpoint = BuildEndpoint("FascicoloPratica/creaInserisciDocumento");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<FascicoloPraticaOutput>.Fail(raw.Errore, raw.Body);
            var parsed = EdmaXmlParser.ParseFascicoloPratica(raw.Body);
            return EdmaResponse<FascicoloPraticaOutput>.Ok(parsed, raw.Body);
        }

        public async Task<EdmaResponse<DocumentoFileOutput>> CreaDocumentoAsync(
            DocumentoBase documento, string nomeFile, byte[] fileBytes, string estensioneFile)
        {
            var body = EdmaXmlHelper.GeneraCreaDocumentoXml(documento, nomeFile, fileBytes, estensioneFile);
            var endpoint = BuildEndpoint("DocumentoFile/creaDocumento");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<DocumentoFileOutput>.Fail(raw.Errore, raw.Body);
            var parsed = EdmaXmlParser.ParseDocumentoFile(raw.Body);
            return EdmaResponse<DocumentoFileOutput>.Ok(parsed, raw.Body);
        }

        public async Task<EdmaResponse<DocumentoFileOutput>> CreaInserisciDocumentoFiglioAsync(
            string idDocumentoPadre, string codiceMetadocPadre,
            DocumentoBase figlio, string nomeFile, byte[] fileBytes, string estensioneFile)
        {
            var body = EdmaXmlHelper.GeneraCreaInserisciDocumentoFiglioXml(idDocumentoPadre, codiceMetadocPadre,
                figlio, nomeFile, fileBytes, estensioneFile);
            var endpoint = BuildEndpoint("DocumentoFile/creaInserisciDocumento");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<DocumentoFileOutput>.Fail(raw.Errore, raw.Body);
            var parsed = EdmaXmlParser.ParseDocumentoFile(raw.Body);
            return EdmaResponse<DocumentoFileOutput>.Ok(parsed, raw.Body);
        }

        public async Task<EdmaResponse<bool>> AssociaDocumentiAsync(
            string idPratica, string idDocumento, int metamoduloDocumento)
        {
            var body = EdmaXmlHelper.GeneraAssociaDocumentiXml(idPratica,
                new[] { (IdDocumento: idDocumento, Metamodulo: metamoduloDocumento) });
            var endpoint = BuildEndpoint($"FascicoloPratica/{idPratica}/associaDocumenti");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<bool>.Fail(raw.Errore, raw.Body);
            var esito = EdmaXmlParser.ParseBoolean(raw.Body);
            return EdmaResponse<bool>.Ok(esito, raw.Body);
        }

        public async Task<EdmaResponse<ProtocollazioneOutput>> ProtocollazioneApplicativaAsync(
            string idDocumento, ParametriProtocollazioneApplicativa parametri)
        {
            var body = EdmaXmlHelper.GeneraProtocollazioneApplicativaXml(parametri);
            var endpoint = BuildEndpoint($"DocumentoFile/{idDocumento}/protocollazioneApplicativa");
            var raw = await PostXmlAsync(endpoint, body).ConfigureAwait(false);
            if (raw.Errore != null)
                return EdmaResponse<ProtocollazioneOutput>.Fail(raw.Errore, raw.Body);
            var parsed = EdmaXmlParser.ParseProtocollazione(raw.Body);
            return EdmaResponse<ProtocollazioneOutput>.Ok(parsed, raw.Body);
        }

        // ----------------------------------------------------------------
        // Trasporto HTTP
        // ----------------------------------------------------------------

        private string BuildEndpoint(string servicePath)
        {
            var url = $"{_url}/EdmaWeb/service/{servicePath}";
            if (_noSession)
                url += url.Contains("?") ? "&no-session=true" : "?no-session=true";
            return url;
        }

        private async Task<(string Body, string Errore)> PostXmlAsync(string endpoint, string xmlBody)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

                    var content = new StringContent(xmlBody, Iso88591, "text/xml");
                    // Forziamo il charset corretto sull'header (StringContent
                    // a volte aggiunge utf-8 anche con l'Encoding settato).
                    content.Headers.ContentType.CharSet = "ISO-8859-1";
                    request.Content = content;

                    using (var response = await SharedHttpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            var msg = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} su {endpoint}";
                            _logger?.Invoke(msg + Environment.NewLine + responseBody, null);
                            return (responseBody, msg);
                        }

                        return (responseBody, null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Eccezione su {endpoint}", ex);
                return (null, ex.Message);
            }
        }
    }
}
