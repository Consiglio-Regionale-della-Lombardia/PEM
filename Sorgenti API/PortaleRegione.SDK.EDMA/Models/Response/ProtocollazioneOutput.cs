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

namespace PortaleRegione.SDK.EDMA.Models.Response
{
    /// <summary>
    ///     Esito di una protocollazione (protocolla o protocollazioneApplicativa).
    ///     La segnatura per il Consiglio Regionale e' la concatenazione di
    ///     <see cref="AooProtocollo"/>, <see cref="AnnoProtocollo"/> e
    ///     <see cref="NumeroProtocollo"/> separati da ".", con il numero
    ///     formattato a sette cifre. La proprieta' <see cref="Segnatura"/>
    ///     contiene gia' la stringa formattata.
    /// </summary>
    public class ProtocollazioneOutput
    {
        public string IdScheda { get; set; }
        public string AooProtocollo { get; set; }
        public string AnnoProtocollo { get; set; }
        public string NumeroProtocollo { get; set; }
        public string EnteProtocollo { get; set; }
        public string Segnatura { get; set; }
    }
}
