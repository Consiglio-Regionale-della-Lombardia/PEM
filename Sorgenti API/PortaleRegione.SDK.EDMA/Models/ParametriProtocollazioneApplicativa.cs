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

using System.Collections.Generic;

namespace PortaleRegione.SDK.EDMA.Models
{
    /// <summary>
    ///     Parametri per il servizio protocollazioneApplicativa. A differenza
    ///     del servizio "protocolla" semplice, qui si sceglie esplicitamente la
    ///     struttura protocollante (AOO) e si possono combinare liberamente
    ///     mittenti interni/esterni e destinatari interni/esterni. Per gli atti
    ///     DASI lo scenario e' "arrivo": mittente esterno (firmatari) +
    ///     destinatario interno (UO competente).
    /// </summary>
    public class ParametriProtocollazioneApplicativa
    {
        /// <summary>Codice AOO della struttura protocollante (es. AOO CRL).</summary>
        public string CodiceStrutturaProtocollante { get; set; }

        public MittenteEsterno Mittente { get; set; }

        public List<DestinatarioInterno> DestinatariCompetenza { get; set; } = new List<DestinatarioInterno>();
        public List<DestinatarioInterno> DestinatariConoscenza { get; set; } = new List<DestinatarioInterno>();

        public string Oggetto { get; set; }
        public bool Riservato { get; set; }
        public string MotivazioneRiservatezzaCodice { get; set; }

        public int FlagRiscontro { get; set; } = 1;
        public string TipoDocumento { get; set; }
        public string MezzoSpedizione { get; set; }
        public int NumeroAllegati { get; set; }
        public string TipoAllegati { get; set; }
    }
}
