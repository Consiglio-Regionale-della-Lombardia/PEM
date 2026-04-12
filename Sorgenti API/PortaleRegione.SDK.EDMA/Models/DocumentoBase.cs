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
    public class DocumentoBase
    {
        public string Oggetto { get; set; }
        public string CodAutore { get; set; } = "SYSTEM_";
        public bool Perfetto { get; set; }
        public bool Protocollato { get; set; }
        public string AooProtocollo { get; set; }
        public string Protocollo { get; set; }
        public string EnteProtocollo { get; set; }
        public string AnnoProtocollo { get; set; }
        public bool Riservato { get; set; }
        public bool Fascicolo { get; set; }
        public bool Classificatore { get; set; }
        public bool Cartaceo { get; set; }
        public bool Firmato { get; set; }
        public int Metamodulo { get; set; }
        public MetaDocumento MetaDocumento { get; set; }
        public bool VaFirmato { get; set; }
        public bool Scansito { get; set; }
        public bool Privato { get; set; }
        public string Id { get; set; }
    }
}
