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

using PortaleRegione.DTO.Model;
using System.Collections.Generic;
using System.Linq;

namespace PortaleRegione.DTO.Enum
{
    public class AreaPoliticaEnum
    {
        public const string Maggioranza = "Maggioranza";
        public const string Minoranza = "Minoranza";
        public const string Misto_Maggioranza = "Misto-maggioranza";
        public const string Misto_Minoranza = "Misto-minoranza";
        public const string Misto = "Misto";

        public static ICollection<KeyValueDto> GetItems()
        {
            var listaEnum= System.Enum.GetValues(typeof(AreaPoliticaIntEnum)).Cast<AreaPoliticaIntEnum>();
            var result = listaEnum.Select(itemArea => new KeyValueDto {id = (int) itemArea, descr = itemArea.ToString()}).ToList();
            return result
                .Where(i=>i.id != 0)
                .ToList();
        }
    }

    public enum AreaPoliticaIntEnum
    {
        Maggioranza = 1,
        Minoranza = 2,
        Misto_Maggioranza = 3,
        Misto_Minoranza = 4,
        Misto = 0
    }
}