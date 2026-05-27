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

namespace PortaleRegione.DTO.Response
{
    /// <summary>
    ///     Esito sintetico della protocollazione di un atto DASI verso EDMA,
    ///     restituito al client al termine del flusso a cinque step. Quando
    ///     <see cref="Success"/> e' vero la pratica risulta creata e il pdf
    ///     dell'atto protocollato; gli altri campi descrivono l'esito dei
    ///     singoli step in modo che la segreteria possa capire se la chiamata
    ///     ha completato tutto o se serve un nuovo tentativo.
    /// </summary>
    public class EdmaProtocollazioneEsitoDto
    {
        public bool Success { get; set; }
        public string Messaggio { get; set; }

        public string IdPratica { get; set; }
        public string NumeroPratica { get; set; }
        public string IdDocumento { get; set; }
        public string IdProtocollo { get; set; }
        public string Segnatura { get; set; }
        public DateTime? DataInvioAlProtocollo { get; set; }

        public int TentativiInvio { get; set; }
        public string UltimoErrore { get; set; }
    }
}
