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

using Newtonsoft.Json;
using System;

namespace PortaleRegione.DTO.Request.Public
{
    public class CercaRequest
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id_legislatura { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] id_tipo { get; set; } = Array.Empty<int>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] id_proponente { get; set; } = Array.Empty<int>();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id_gruppo { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] id_tipo_risposta { get; set; } = Array.Empty<int>();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] organi { get; set; } = Array.Empty<int>();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public int[] stati { get; set; } = Array.Empty<int>();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public int[] firmatari { get; set; } = Array.Empty<int>();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string oggetto { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? data_presentazione_da { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? data_presentazione_a { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string n_atto { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string burl { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? dcr { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? dccr { get; set; }
        
        public int page { get; set; } = 1;
        public int size { get; set; } = 20;
    }
}