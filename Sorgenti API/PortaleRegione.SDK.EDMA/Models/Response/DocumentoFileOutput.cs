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
    ///     Esito della creazione di un DocumentoFile. La specifica EDMA
    ///     restituisce l'intero oggetto serializzato in XML: a noi serve
    ///     soprattutto l'id, usato per costruire le URI dei servizi successivi.
    /// </summary>
    public class DocumentoFileOutput
    {
        public string IdDocumento { get; set; }
    }
}
