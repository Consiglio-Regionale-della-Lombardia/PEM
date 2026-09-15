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

using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using PortaleRegione.SDK.EDMA.Models.Response;

namespace PortaleRegione.SDK.EDMA.Helpers
{
    /// <summary>
    ///     Estrae i campi utili dalle risposte XML dei servizi EDMA. I servizi
    ///     restituiscono l'intero oggetto serializzato (stile XStream), ma per
    ///     l'integrazione interessa solo un sottoinsieme dei campi: id,
    ///     identificatori e dati di protocollazione.
    /// </summary>
    public static class EdmaXmlParser
    {
        /// <summary>
        ///     Da <c>DocumentoFile/creaDocumento</c>: l'id si trova in
        ///     <c>/it.lispa.edma.documenti.po.DocumentoFile/documentoBase/id</c>.
        /// </summary>
        public static DocumentoFileOutput ParseDocumentoFile(string xml)
        {
            var root = ParseRoot(xml);
            if (root == null) return null;
            var id = root.Descendants("documentoBase")
                .Select(db => (string)db.Element("id"))
                .FirstOrDefault(v => !string.IsNullOrEmpty(v));
            if (id == null)
                id = (string)root.Element("id");
            return new DocumentoFileOutput { IdDocumento = id };
        }

        /// <summary>
        ///     Da <c>FascicoloPratica/creaInserisciDocumento</c>: l'id EDMA, il
        ///     codice (numero pratica) e l'identificatore univoco della pratica.
        ///     Vanno letti SOLO come figli diretti della root: la risposta annida
        ///     referente, procedimento, documentoBase ecc., ciascuno con propri
        ///     &lt;id&gt;/&lt;codice&gt;, quindi una ricerca "ovunque" prenderebbe
        ///     per errore quelli del referente (es. l'id del referente al posto
        ///     dell'id pratica).
        /// </summary>
        public static FascicoloPraticaOutput ParseFascicoloPratica(string xml)
        {
            var root = ParseRoot(xml);
            if (root == null) return null;

            return new FascicoloPraticaOutput
            {
                IdPratica = (string)root.Element("id"),
                NumeroPratica = (string)root.Element("codice"),
                Identificatore = (string)root.Element("identificatore")
            };
        }

        /// <summary>
        ///     Da <c>protocolla</c> / <c>protocollazioneApplicativa</c>: dati
        ///     della protocollazione. La segnatura formattata viene costruita
        ///     concatenando aoo, anno e numero (a 7 cifre) separati da ".".
        /// </summary>
        public static ProtocollazioneOutput ParseProtocollazione(string xml)
        {
            var root = ParseRoot(xml);
            if (root == null) return null;

            var aoo = FirstNonEmpty(root, "aooProtocollo");
            var anno = FirstNonEmpty(root, "annoProtocollo");
            var numero = FirstNonEmpty(root, "protocollo");
            var ente = FirstNonEmpty(root, "enteProtocollo");
            var idScheda = FindFirstSchedaId(root);

            return new ProtocollazioneOutput
            {
                IdScheda = idScheda,
                AooProtocollo = aoo,
                AnnoProtocollo = anno,
                NumeroProtocollo = numero,
                EnteProtocollo = ente,
                Segnatura = FormatSegnatura(aoo, anno, numero)
            };
        }

        /// <summary>
        ///     Da <c>Metadocumento/caricaMetadocumento</c>: l'id intero del
        ///     metadocumento. Restituisce 0 se non lo trova.
        /// </summary>
        public static int ParseMetadocumentoId(string xml)
        {
            var root = ParseRoot(xml);
            if (root == null) return 0;
            var idText = (string)root.Element("id");
            if (string.IsNullOrEmpty(idText)) return 0;
            return int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : 0;
        }

        /// <summary>Da <c>associaDocumenti</c>: boolean true/false.</summary>
        public static bool ParseBoolean(string xml)
        {
            var root = ParseRoot(xml);
            if (root == null) return false;
            var text = (root.Value ?? string.Empty).Trim();
            return bool.TryParse(text, out var b) && b;
        }

        private static XElement ParseRoot(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            try
            {
                return XDocument.Parse(xml).Root;
            }
            catch
            {
                return null;
            }
        }

        private static string FirstNonEmpty(XElement root, string elementName)
        {
            return root.DescendantsAndSelf(elementName)
                .Select(e => (string)e)
                .FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        private static string FindFirstSchedaId(XElement root)
        {
            // Nella risposta della protocollazione, la scheda e' annidata: cerco
            // il primo <id> figlio di un nodo che faccia riferimento alla scheda.
            var scheda = root.DescendantsAndSelf().FirstOrDefault(e =>
                e.Name.LocalName.Equals("schedaProtocollo", System.StringComparison.OrdinalIgnoreCase));
            if (scheda == null) return null;
            return (string)scheda.Element("id");
        }

        private static string FormatSegnatura(string aoo, string anno, string numero)
        {
            if (string.IsNullOrEmpty(aoo) || string.IsNullOrEmpty(anno) || string.IsNullOrEmpty(numero))
                return null;
            if (int.TryParse(numero, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return $"{aoo}.{anno}.{n.ToString("0000000", CultureInfo.InvariantCulture)}";
            return $"{aoo}.{anno}.{numero}";
        }
    }
}
