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

namespace PortaleRegione.SDK.EDMA.Models
{
    public class MittenteInterno
    {
        public string ConfermaRicezione { get; set; } = "no";
        public bool EmailPec { get; set; }
        public string CodiceEnteComp { get; set; }
    }

    public class IndirizzoPostale
    {
        public string Indirizzo { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        public string Cap { get; set; }
    }

    public class DestinatarioEsterno
    {
        public bool Manuale { get; set; } = true;
        public bool TuttiDestinatario { get; set; }
        public bool EmailPec { get; set; }
        public string Descrizione { get; set; }
        public IndirizzoPostale IndirizzoPostale { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Telefono { get; set; }
        public string PartitaIva { get; set; }
        public string CodFisc { get; set; }
        public int OrdineInserimento { get; set; }
        public string CodiceEc { get; set; }
        public bool Principale { get; set; } = true;
        public int Tipologia { get; set; } = 2;
    }

    public class SchedaProtocollo
    {
        public MittenteInterno Mittente { get; set; }
        public DestinatarioEsterno[] Destinatari { get; set; }
        public int NumeroAllegati { get; set; }
        public string TipoAllegati { get; set; }
        public string MezzoSpedizione { get; set; }
        public string TipoDocumento { get; set; }
        public int FlagRiscontro { get; set; } = 1;
        public DocumentoBase DocumentoBase { get; set; }
        public bool ProtocollazioneAutomatica { get; set; }
    }
}
