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
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    /// <summary>
    ///     Ordinamenti secondari proposti dal modale "Ordinamento risultati".
    ///     L'ordinamento primario (Presentazione/Votazione) sta nelle tab della griglia,
    ///     quindi non viene duplicato qui.
    /// </summary>
    public class EmendamentiSorting
    {
        [DisplayName("Stato")] public int IDStato { get; set; }
        [DisplayName("Data presentazione")] public DateTime? Timestamp { get; set; }
        [DisplayName("Progressivo")] public int Progressivo { get; set; }
        [DisplayName("Sub-Progressivo")] public int? SubProgressivo { get; set; }
    }
}
