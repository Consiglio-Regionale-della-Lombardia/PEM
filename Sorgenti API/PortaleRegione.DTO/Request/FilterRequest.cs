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
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.DTO.Request
{
    public class FilterRequest
    {
        public List<FilterItem> filters { get; set; } = new List<FilterItem>();
        public int page { get; set; }
        public int size { get; set; }
        public int viewMode { get; set; } = (int)ViewModeEnum.GRID;
        
        public int clientMode { get; set; } = (int)ClientModeEnum.GRUPPI;
        public List<SortingInfo> sort_settings { get; set; } = new List<SortingInfo>();
        public List<string> columns_settings { get; set; } = new List<string>();
    }
}