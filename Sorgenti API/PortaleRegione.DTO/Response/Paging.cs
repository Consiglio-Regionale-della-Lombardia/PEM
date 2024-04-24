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
using Newtonsoft.Json;

namespace PortaleRegione.DTO.Response
{
    public class Paging
    {
        public int Page { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Entities { get; set; }
        public bool Has_Next { get; set; }
        public bool Has_Prev { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri Next_Url { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri Prev_Url { get; set; }
        public int Last_Page { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri Last_Url { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri First_Url { get; set; }
    }
}