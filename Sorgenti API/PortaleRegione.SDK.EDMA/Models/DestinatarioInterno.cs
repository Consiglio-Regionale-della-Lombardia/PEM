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
    ///     Destinatario interno (entita' competente all'interno dell'AOO della
    ///     struttura protocollante). Per gli atti DASI il destinatario per
    ///     competenza e' CRA0060102 (UO Procedure d'aula, atti e resoconti).
    /// </summary>
    public class DestinatarioInterno
    {
        public string CodiceEc { get; set; }
        /// <summary>1 = per conoscenza, 2 = per competenza (default).</summary>
        public int Tipologia { get; set; } = 2;
        public bool Principale { get; set; } = true;
    }
}
