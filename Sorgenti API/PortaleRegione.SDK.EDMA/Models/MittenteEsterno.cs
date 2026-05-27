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
    /// <summary>
    ///     Mittente esterno per la protocollazione in arrivo. Per gli atti DASI
    ///     la <see cref="Descrizione"/> contiene i nomi dei consiglieri
    ///     firmatari separati da virgola (senza il gruppo politico).
    /// </summary>
    public class MittenteEsterno
    {
        public bool Manuale { get; set; } = true;
        public string Descrizione { get; set; }
        public IndirizzoPostale IndirizzoPostale { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Telefono { get; set; }
        public string PartitaIva { get; set; }
        public string CodFisc { get; set; }
        public bool EmailPec { get; set; }
    }
}
