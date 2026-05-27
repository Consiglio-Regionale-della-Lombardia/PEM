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
    ///     Sede del soggetto associato a una Pratica EDMA. La specifica richiede
    ///     come obbligatori descrizione, codice fiscale e partita IVA; gli altri
    ///     campi sono facoltativi.
    /// </summary>
    public class SedeSoggetto
    {
        public string Descrizione { get; set; }
        public string CodFiscale { get; set; }
        public string Pariva { get; set; }
        public string Via { get; set; }
        public string Citta { get; set; }
        public string Provincia { get; set; }
        public string Cap { get; set; }
        public string Regione { get; set; }
        public string Stato { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Fax { get; set; }
    }
}
