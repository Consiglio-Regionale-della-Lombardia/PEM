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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using PortaleRegione.SDK.EDMA.Models;

namespace PortaleRegione.SDK.EDMA.Helpers
{
    /// <summary>
    ///     Generatori dei body XML per le chiamate ai servizi EDMA. Tutti i
    ///     payload seguono lo schema XStream-style usato dal framework RSI3 di
    ///     EDMA: tag con i nomi delle classi Java (it.lispa.edma...) e
    ///     attributi <c>id</c> progressivi sui sotto-oggetti.
    /// </summary>
    public static class EdmaXmlHelper
    {
        private static readonly CultureInfo Italian = new CultureInfo("it-IT");

        /// <summary>
        ///     Costruisce l'elemento &lt;file&gt; con il payload spezzato in
        ///     blocchi base64 da 1024 byte come richiesto dalla specifica.
        /// </summary>
        public static XElement CreaFileXml(byte[] fileBytes, string estensione)
        {
            var fileElement = new XElement("file", new XAttribute("suffix", estensione ?? string.Empty));

            const int blockSize = 1024;
            for (var i = 0; i < fileBytes.Length; i += blockSize)
            {
                var length = Math.Min(blockSize, fileBytes.Length - i);
                var block = new byte[length];
                Array.Copy(fileBytes, i, block, 0, length);
                fileElement.Add(new XElement("base64", Convert.ToBase64String(block)));
            }

            return fileElement;
        }

        /// <summary>
        ///     Genera il body per <c>DocumentoFile/creaDocumento</c>: crea un
        ///     documento principale (non figlio di altro) con il pdf dell'atto.
        ///     Il tag &lt;fileCM&gt; include &lt;nome&gt; come da specifica
        ///     SIS.EDMA - DocumentoFile v1.
        /// </summary>
        public static string GeneraCreaDocumentoXml(DocumentoBase documento, string nomeFile, byte[] fileBytes,
            string estensioneFile)
        {
            var fileCm = new XElement("fileCM", new XAttribute("id", "25"));
            if (!string.IsNullOrEmpty(nomeFile))
                fileCm.Add(new XElement("nome", nomeFile));
            if (fileBytes != null && fileBytes.Length > 0)
                fileCm.Add(CreaFileXml(fileBytes, estensioneFile));

            var doc = new XElement("it.lispa.edma.documenti.po.DocumentoFile",
                new XAttribute("id", "1"),
                new XElement("documentoBase",
                    new XAttribute("id", "7"),
                    new XElement("oggetto", documento.Oggetto ?? string.Empty),
                    new XElement("codAutore", documento.CodAutore ?? "SYSTEM_"),
                    new XElement("perfetto", ToXmlBool(documento.Perfetto)),
                    new XElement("protocollato", ToXmlBool(documento.Protocollato)),
                    new XElement("riservato", ToXmlBool(documento.Riservato)),
                    new XElement("fascicolo", ToXmlBool(documento.Fascicolo)),
                    new XElement("classificatore", ToXmlBool(documento.Classificatore)),
                    new XElement("cartaceo", ToXmlBool(documento.Cartaceo)),
                    new XElement("firmato", ToXmlBool(documento.Firmato)),
                    new XElement("metamodulo", documento.Metamodulo),
                    new XElement("metadocumento",
                        new XAttribute("id", "15"),
                        new XElement("codice", documento.MetaDocumento?.Codice ?? string.Empty)
                    ),
                    new XElement("figli", new XAttribute("id", "23")),
                    new XElement("padri", new XAttribute("id", "24")),
                    fileCm,
                    new XElement("attributi", new XAttribute("id", "26")),
                    new XElement("vaFirmato", ToXmlBool(documento.VaFirmato)),
                    new XElement("classificazioni", new XAttribute("id", "29")),
                    new XElement("statoMetaclassificazione", new XAttribute("id", "30")),
                    new XElement("segnature", new XAttribute("id", "31")),
                    new XElement("voceInMetaclassificazioni", new XAttribute("id", "32")),
                    new XElement("codiciEcDestinatarie", new XAttribute("id", "33")),
                    new XElement("scansito", ToXmlBool(documento.Scansito)),
                    new XElement("privato", ToXmlBool(documento.Privato))
                )
            );

            return WithXmlDeclaration(doc);
        }

        /// <summary>
        ///     Genera il body per <c>DocumentoFile/creaInserisciDocumento</c>:
        ///     crea un DocumentoFile (l'allegato) gia' collegato come figlio
        ///     del DocumentoFile padre (il pdf principale dell'atto).
        /// </summary>
        public static string GeneraCreaInserisciDocumentoFiglioXml(string idDocumentoPadre, string codiceMetadocPadre,
            DocumentoBase figlio, string nomeFile, byte[] fileBytes, string estensioneFile)
        {
            var padre = new XElement("padre",
                new XAttribute("class", "it.lispa.edma.erato.po.DocumentoBase"),
                new XElement("id", idDocumentoPadre),
                new XElement("metaDocumento",
                    new XElement("codice", codiceMetadocPadre ?? string.Empty)
                ),
                new XElement("metamodulo", figlio.Metamodulo)
            );

            var fileCm = new XElement("fileCM", new XAttribute("id", "25"));
            if (!string.IsNullOrEmpty(nomeFile))
                fileCm.Add(new XElement("nome", nomeFile));
            if (fileBytes != null && fileBytes.Length > 0)
                fileCm.Add(CreaFileXml(fileBytes, estensioneFile));

            var documento = new XElement("documento",
                new XAttribute("class", "it.lispa.edma.documenti.po.DocumentoFile"),
                new XElement("documentoBase",
                    new XElement("oggetto", figlio.Oggetto ?? string.Empty),
                    new XElement("codAutore", figlio.CodAutore ?? "SYSTEM_"),
                    new XElement("metamodulo", figlio.Metamodulo),
                    new XElement("metadocumento",
                        new XElement("codice", figlio.MetaDocumento?.Codice ?? string.Empty)
                    ),
                    fileCm
                )
            );

            var doc = new XElement("it.lispa.edma.documenti.dto.CreaInserisci", padre, documento);
            return WithXmlDeclaration(doc);
        }

        /// <summary>
        ///     Genera il body per <c>FascicoloPratica/creaInserisciDocumento</c>:
        ///     crea una Pratica EDMA sotto un sotto-fascicolo titolario gia'
        ///     esistente. Il tag &lt;padre&gt; punta al sotto-fascicolo, il tag
        ///     &lt;documento&gt; descrive la pratica da creare.
        /// </summary>
        public static string GeneraCreaInserisciPraticaXml(string idSottoFascicoloPadre,
            string codiceMetadocPadre, int metamoduloPadre, FascicoloPratica pratica)
        {
            var padre = new XElement("padre",
                new XAttribute("class", "it.lispa.edma.erato.po.DocumentoBase"),
                new XElement("metamodulo", metamoduloPadre),
                new XElement("metadocumento",
                    new XAttribute("class", "it.lispa.edma.erato.po.Metadocumento"),
                    new XElement("codice", codiceMetadocPadre ?? string.Empty)
                ),
                new XElement("id", idSottoFascicoloPadre)
            );

            var documento = new XElement("documento",
                new XAttribute("class", "it.lispa.edma.documenti.po.FascicoloPratica"),
                new XElement("titolo", pratica.Titolo ?? string.Empty),
                new XElement("codProcedimento", pratica.CodProcedimento ?? string.Empty),
                new XElement("dataApertura",
                    new XAttribute("class", "sql-date"),
                    pratica.DataApertura.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement("anniConservazione", pratica.AnniConservazione),
                new XElement("dataChiusura",
                    new XAttribute("class", "sql-date"),
                    pratica.DataChiusura.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            );

            if (!string.IsNullOrEmpty(pratica.ReferenteCodPersona))
                documento.Add(new XElement("referente",
                    new XElement("codice", pratica.ReferenteCodPersona)));

            if (!string.IsNullOrEmpty(pratica.ResponsabileCodPersona))
                documento.Add(new XElement("responsabile",
                    new XElement("codice", pratica.ResponsabileCodPersona)));

            if (pratica.SedeSoggetto != null)
                documento.Add(BuildSedeSoggetto(pratica.SedeSoggetto));

            if (!string.IsNullOrEmpty(pratica.LivelloAppartenenzaId))
                documento.Add(new XElement("livelloAppartenenza",
                    new XAttribute("class", "it.lispa.edma.documenti.po.FascicoloLivello"),
                    new XElement("id", pratica.LivelloAppartenenzaId)));

            if (!string.IsNullOrEmpty(pratica.ProcedimentoIdEdma))
                documento.Add(new XElement("procedimento",
                    new XAttribute("class", "it.lispa.edma.documenti.po.FascicoloProcedimento"),
                    new XElement("id", pratica.ProcedimentoIdEdma),
                    string.IsNullOrEmpty(pratica.ProcedimentoCodice)
                        ? null
                        : new XElement("codProcedimento", pratica.ProcedimentoCodice)));

            var documentoBase = new XElement("documentoBase",
                new XAttribute("class", "it.lispa.edma.erato.po.DocumentoBase"),
                new XElement("metamodulo", pratica.Metamodulo),
                new XElement("metadocumento",
                    new XAttribute("class", "it.lispa.edma.erato.po.Metadocumento"),
                    new XElement("codice", pratica.MetadocumentoCodice ?? string.Empty)));
            documento.Add(documentoBase);

            if (pratica.Attributi != null && pratica.Attributi.Count > 0)
            {
                var attributi = new XElement("attributi");
                foreach (var kv in pratica.Attributi)
                    attributi.Add(new XElement("entry",
                        new XElement("string", kv.Key ?? string.Empty),
                        new XElement("string", kv.Value ?? string.Empty)));
                documento.Add(attributi);
            }

            if (!string.IsNullOrEmpty(pratica.Note))
                documento.Add(new XElement("note", pratica.Note));

            var doc = new XElement("it.lispa.edma.documenti.dto.CreaInserisci", padre, documento);
            return WithXmlDeclaration(doc);
        }

        /// <summary>
        ///     Genera il body per <c>FascicoloPratica/{id}/associaDocumenti</c>:
        ///     fascicola uno o piu' documenti gia' esistenti in una pratica
        ///     gia' creata. Per il flusso DASI ci si limita al pdf principale.
        /// </summary>
        public static string GeneraAssociaDocumentiXml(string idPratica,
            IEnumerable<(string IdDocumento, int Metamodulo)> documentiDaFascicolare)
        {
            var fascicolatore = new XElement("fascicolatore",
                new XAttribute("class", "it.lispa.edma.documenti.po.FascicoloPratica"),
                new XElement("id", idPratica));

            var docs = new XElement("docsDaFascicolare");
            foreach (var d in documentiDaFascicolare)
            {
                docs.Add(new XElement("it.lispa.edma.documenti.po.DocumentoFile",
                    new XElement("documentoBase",
                        new XElement("metamodulo", d.Metamodulo)),
                    new XElement("id", d.IdDocumento)));
            }

            var doc = new XElement("it.lispa.edma.documenti.dto.AssociaDocumentiProcedimentoDTO",
                fascicolatore,
                docs);
            return WithXmlDeclaration(doc);
        }

        /// <summary>
        ///     Genera il body per <c>DocumentoFile/{id}/protocollazioneApplicativa</c>:
        ///     a differenza del servizio "protocolla" semplice, qui si specifica
        ///     esplicitamente la struttura protocollante (AOO) e si compone uno
        ///     scenario Mittente esterno + Destinatari interni (per DASI).
        /// </summary>
        public static string GeneraProtocollazioneApplicativaXml(ParametriProtocollazioneApplicativa parametri)
        {
            var scheda = new XElement("schedaProtocollo",
                new XElement("flagRiscontro", parametri.FlagRiscontro),
                new XElement("strutturaProtocollante",
                    new XAttribute("class", "it.lispa.edma.penelope.po.Aoo"),
                    new XElement("codice", parametri.CodiceStrutturaProtocollante ?? string.Empty)));

            if (parametri.Mittente != null)
                scheda.Add(new XElement("mittente", BuildMittenteEsterno(parametri.Mittente)));

            var destinatari = new XElement("destinatari");
            if (parametri.DestinatariCompetenza != null)
                foreach (var d in parametri.DestinatariCompetenza)
                    destinatari.Add(BuildDestinatarioInterno(d, tipologia: 2));
            if (parametri.DestinatariConoscenza != null)
                foreach (var d in parametri.DestinatariConoscenza)
                    destinatari.Add(BuildDestinatarioInterno(d, tipologia: 1));
            scheda.Add(destinatari);

            if (parametri.NumeroAllegati > 0)
                scheda.Add(new XElement("numeroAllegati", parametri.NumeroAllegati));
            if (!string.IsNullOrEmpty(parametri.TipoAllegati))
                scheda.Add(new XElement("tipoAllegati", parametri.TipoAllegati));
            if (!string.IsNullOrEmpty(parametri.MezzoSpedizione))
                scheda.Add(new XElement("mezzoSpedizione", parametri.MezzoSpedizione));
            if (!string.IsNullOrEmpty(parametri.TipoDocumento))
                scheda.Add(new XElement("tipoDocumento", parametri.TipoDocumento));

            var documentoBase = new XElement("documentoBase",
                new XElement("oggetto", parametri.Oggetto ?? string.Empty),
                new XElement("riservato", ToXmlBool(parametri.Riservato)));
            if (parametri.Riservato && !string.IsNullOrEmpty(parametri.MotivazioneRiservatezzaCodice))
                documentoBase.Add(new XElement("motivazioneRiservatezza",
                    new XAttribute("class", "it.lispa.edma.documenti.po.MotivazioneRiservatezza"),
                    new XElement("codice", parametri.MotivazioneRiservatezzaCodice)));
            scheda.Add(documentoBase);

            var root = new XElement("it.lispa.edma.mercurioNew.dto.ParametriAccessoProtocollo", scheda);
            return WithXmlDeclaration(root);
        }

        /// <summary>
        ///     Body per <c>Metadocumento/caricaMetadocumento</c>: input e' una
        ///     stringa contenente il codice del metadocumento. Restituisce poi
        ///     un oggetto Metadocumento da cui leggere l'id.
        /// </summary>
        public static string GeneraCaricaMetadocumentoXml(string codiceMetadocumento)
        {
            var doc = new XElement("string", codiceMetadocumento ?? string.Empty);
            return WithXmlDeclaration(doc);
        }

        private static XElement BuildSedeSoggetto(SedeSoggetto s)
        {
            var sede = new XElement("sedeSoggetto");
            if (!string.IsNullOrEmpty(s.Via)) sede.Add(new XElement("via", s.Via));
            if (!string.IsNullOrEmpty(s.Citta)) sede.Add(new XElement("citta", s.Citta));
            if (!string.IsNullOrEmpty(s.Provincia)) sede.Add(new XElement("provincia", s.Provincia));
            if (!string.IsNullOrEmpty(s.Cap)) sede.Add(new XElement("cap", s.Cap));
            if (!string.IsNullOrEmpty(s.Regione)) sede.Add(new XElement("regione", s.Regione));
            if (!string.IsNullOrEmpty(s.Stato)) sede.Add(new XElement("stato", s.Stato));
            if (!string.IsNullOrEmpty(s.Email)) sede.Add(new XElement("email", s.Email));
            if (!string.IsNullOrEmpty(s.Telefono)) sede.Add(new XElement("telefono", s.Telefono));
            if (!string.IsNullOrEmpty(s.Fax)) sede.Add(new XElement("fax", s.Fax));
            if (!string.IsNullOrEmpty(s.CodFiscale)) sede.Add(new XElement("codFiscale", s.CodFiscale));
            if (!string.IsNullOrEmpty(s.Pariva)) sede.Add(new XElement("pariva", s.Pariva));
            if (!string.IsNullOrEmpty(s.Descrizione)) sede.Add(new XElement("descrizione", s.Descrizione));
            return sede;
        }

        private static XElement BuildMittenteEsterno(MittenteEsterno m)
        {
            var x = new XElement("it.lispa.edma.telemaco.po.MittenteEsterno",
                new XElement("manuale", ToXmlBool(m.Manuale)),
                new XElement("descrizione", m.Descrizione ?? string.Empty));
            if (m.IndirizzoPostale != null)
                x.Add(new XElement("indirizzoPostale",
                    new XElement("indirizzo", m.IndirizzoPostale.Indirizzo ?? string.Empty),
                    new XElement("comune", m.IndirizzoPostale.Comune ?? string.Empty),
                    new XElement("provincia", m.IndirizzoPostale.Provincia ?? string.Empty),
                    new XElement("cap", m.IndirizzoPostale.Cap ?? string.Empty)));
            if (!string.IsNullOrEmpty(m.Email)) x.Add(new XElement("email", m.Email));
            if (!string.IsNullOrEmpty(m.Fax)) x.Add(new XElement("fax", m.Fax));
            if (!string.IsNullOrEmpty(m.Telefono)) x.Add(new XElement("telefono", m.Telefono));
            if (!string.IsNullOrEmpty(m.PartitaIva)) x.Add(new XElement("partitaIva", m.PartitaIva));
            if (!string.IsNullOrEmpty(m.CodFisc)) x.Add(new XElement("codFisc", m.CodFisc));
            x.Add(new XElement("emailPec", ToXmlBool(m.EmailPec)));
            return x;
        }

        private static XElement BuildDestinatarioInterno(DestinatarioInterno d, int tipologia)
        {
            return new XElement("it.lispa.edma.telemaco.po.DestinatarioInterno",
                new XElement("codiceEc", d.CodiceEc ?? string.Empty),
                new XElement("tipologia", d.Tipologia > 0 ? d.Tipologia : tipologia),
                new XElement("principale", ToXmlBool(d.Principale)));
        }

        private static string ToXmlBool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string WithXmlDeclaration(XElement root)
        {
            var doc = new XDocument(new XDeclaration("1.0", "ISO-8859-1", null), root);
            return doc.Declaration + Environment.NewLine + root;
        }
    }
}
