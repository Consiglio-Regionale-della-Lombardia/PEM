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
using System.Text;
using System.Xml.Linq;
using PortaleRegione.SDK.EDMA.Models;

namespace PortaleRegione.SDK.EDMA.Helpers
{
    public static class EdmaXmlHelper
    {
        /// <summary>
        ///     Converte un file in blocchi base64 da 1024 bytes come richiesto da EDMA
        /// </summary>
        public static XElement CreaFileXml(byte[] fileBytes, string estensione)
        {
            var fileElement = new XElement("file", new XAttribute("suffix", estensione));

            var blockSize = 1024;
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
        ///     Genera l'XML per la creazione di un documento EDMA
        /// </summary>
        public static string GeneraCreaDocumentoXml(DocumentoBase documento, byte[] fileBytes, string estensioneFile)
        {
            var doc = new XElement("it.lispa.edma.documenti.po.DocumentoFile",
                new XAttribute("id", "1"),
                new XElement("documentoBase",
                    new XAttribute("id", "7"),
                    new XElement("oggetto", documento.Oggetto ?? string.Empty),
                    new XElement("codAutore", documento.CodAutore ?? "SYSTEM_"),
                    new XElement("perfetto", documento.Perfetto.ToString().ToLower()),
                    new XElement("protocollato", documento.Protocollato.ToString().ToLower()),
                    new XElement("riservato", documento.Riservato.ToString().ToLower()),
                    new XElement("fascicolo", documento.Fascicolo.ToString().ToLower()),
                    new XElement("classificatore", documento.Classificatore.ToString().ToLower()),
                    new XElement("cartaceo", documento.Cartaceo.ToString().ToLower()),
                    new XElement("firmato", documento.Firmato.ToString().ToLower()),
                    new XElement("metamodulo", documento.Metamodulo),
                    new XElement("metadocumento",
                        new XAttribute("id", "15"),
                        new XElement("id", documento.MetaDocumento?.Id ?? 0),
                        new XElement("codice", documento.MetaDocumento?.Codice ?? string.Empty)
                    ),
                    new XElement("figli", new XAttribute("id", "23")),
                    new XElement("padri", new XAttribute("id", "24")),
                    new XElement("filesCM",
                        new XAttribute("id", "25"),
                        fileBytes != null ? CreaFileXml(fileBytes, estensioneFile) : null
                    ),
                    new XElement("attributi", new XAttribute("id", "26")),
                    new XElement("vaFirmato", documento.VaFirmato.ToString().ToLower()),
                    new XElement("classificazioni", new XAttribute("id", "29")),
                    new XElement("statoMetaclassificazione", new XAttribute("id", "30")),
                    new XElement("segnature", new XAttribute("id", "31")),
                    new XElement("voceInMetaclassificazioni", new XAttribute("id", "32")),
                    new XElement("codiciEcDestinatarie", new XAttribute("id", "33")),
                    new XElement("scansito", documento.Scansito.ToString().ToLower()),
                    new XElement("privato", documento.Privato.ToString().ToLower())
                )
            );

            return doc.ToString();
        }

        /// <summary>
        ///     Genera l'XML per l'invio al protocollo
        /// </summary>
        public static string GeneraProtocollaXml(SchedaProtocollo scheda)
        {
            var mittenteXml = new XElement("it.lispa.edma.telemaco.po.MittenteInterno",
                new XAttribute("id", "4"),
                new XElement("confermaRicezione", scheda.Mittente?.ConfermaRicezione ?? "no"),
                new XElement("registrazioneProtocollo", new XAttribute("reference", "2")),
                new XElement("emailPec", (scheda.Mittente?.EmailPec ?? false).ToString().ToLower()),
                new XElement("codiceEnteComp", scheda.Mittente?.CodiceEnteComp ?? string.Empty)
            );

            var destinatariElements = new List<XElement>();
            if (scheda.Destinatari != null)
            {
                for (var i = 0; i < scheda.Destinatari.Length; i++)
                {
                    var dest = scheda.Destinatari[i];
                    var destXml = new XElement("it.lispa.edma.telemaco.po.DestinatarioEsterno",
                        new XAttribute("id", (16 + i).ToString()),
                        new XElement("manuale", dest.Manuale.ToString().ToLower()),
                        new XElement("registrazioneProtocollo", new XAttribute("reference", "2")),
                        new XElement("tuttiDestinatario", dest.TuttiDestinatario.ToString().ToLower()),
                        new XElement("emailPec", dest.EmailPec.ToString().ToLower()),
                        new XElement("descrizione", dest.Descrizione ?? string.Empty),
                        new XElement("ordineInserimento", dest.OrdineInserimento),
                        new XElement("tuttiDestinatario",
                            new XAttribute("defined-in", "it.lispa.edma.telemaco.po.Destinatario"),
                            dest.TuttiDestinatario.ToString().ToLower()),
                        new XElement("codiceEc", dest.CodiceEc ?? string.Empty),
                        new XElement("principale", dest.Principale.ToString().ToLower()),
                        new XElement("tipologia", dest.Tipologia)
                    );
                    destinatariElements.Add(destXml);
                }
            }

            var documentoBaseXml = new XElement("documentoBase",
                new XAttribute("id", "25"),
                new XElement("oggetto", scheda.DocumentoBase?.Oggetto ?? string.Empty),
                new XElement("perfetto", "false"),
                new XElement("protocollato", "false"),
                new XElement("riservato", (scheda.DocumentoBase?.Riservato ?? false).ToString().ToLower()),
                new XElement("fascicolo", "false"),
                new XElement("classificatore", "false"),
                new XElement("cartaceo", (scheda.DocumentoBase?.Cartaceo ?? false).ToString().ToLower()),
                new XElement("firmato", "false"),
                new XElement("figli", new XAttribute("id", "26")),
                new XElement("padri", new XAttribute("id", "27")),
                new XElement("filesCM", new XAttribute("id", "28")),
                new XElement("attributi", new XAttribute("id", "29")),
                new XElement("vaFirmato", "false"),
                new XElement("classificazioni", new XAttribute("id", "31")),
                new XElement("statoMetaclassificazione", new XAttribute("id", "32")),
                new XElement("segnature", new XAttribute("id", "33")),
                new XElement("voceInMetaclassificazioni", new XAttribute("id", "34")),
                new XElement("codiciEcDestinatarie", new XAttribute("id", "35")),
                new XElement("scansito", "false"),
                new XElement("privato", (scheda.DocumentoBase?.Privato ?? false).ToString().ToLower())
            );

            var schedaXml = new XElement("schedaProtocollo",
                new XAttribute("id", "2"),
                new XElement("mittente", new XAttribute("id", "3"), mittenteXml),
                new XElement("destinatari", new XAttribute("id", "15"), destinatariElements.ToArray()),
                new XElement("numeroAllegati", scheda.NumeroAllegati),
                new XElement("tipoAllegati", scheda.TipoAllegati ?? string.Empty),
                new XElement("allegati", new XAttribute("id", "22")),
                new XElement("mezzoSpedizione", scheda.MezzoSpedizione ?? string.Empty),
                new XElement("tipoDocumento", scheda.TipoDocumento ?? string.Empty),
                new XElement("flagRiscontro", scheda.FlagRiscontro),
                new XElement("listaAssegnatariAccesso", new XAttribute("id", "23")),
                new XElement("assegnatari", new XAttribute("id", "24")),
                documentoBaseXml
            );

            var root = new XElement("it.lispa.edma.mercurioNew.dto.ParametriAccessoProtocollo",
                new XAttribute("id", "1"),
                schedaXml,
                new XElement("protocollazioneAutomatica", scheda.ProtocollazioneAutomatica.ToString().ToLower())
            );

            return root.ToString();
        }
    }
}
