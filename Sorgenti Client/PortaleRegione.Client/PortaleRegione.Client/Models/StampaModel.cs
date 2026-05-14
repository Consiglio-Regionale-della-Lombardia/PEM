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
using PortaleRegione.DTO.Request;
using System;
using System.Collections.Generic;

namespace PortaleRegione.Client.Models
{
    public class StampaModel
    {
        public int da { get; set; }
        public int a { get; set; }
        public string client_mode { get; set; }
        public List<string> filters { get; set; }
        public string uid_atto { get; set; }
        public string ordine { get; set; }

        public ICollection<Guid> Lista { get; set; }
        public bool Tutti { get; set; } = false;

        public List<FilterItem> filters_dasi { get; set; } = new List<FilterItem>();
        public List<SortingInfo> sort_settings_dasi { get; set; } = new List<SortingInfo>();

    }
}