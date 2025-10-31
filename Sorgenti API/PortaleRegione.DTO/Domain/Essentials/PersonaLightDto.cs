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

namespace PortaleRegione.DTO.Domain.Essentials
{
    public class PersonaLightDto
    {
        public PersonaLightDto()
        {

        }

        public PersonaLightDto(string cognome, string nome)
        {
            this.cognome = cognome;
            this.nome = nome;
        }
        public string DisplayName => $"{cognome} {nome}";

        public Guid UID_persona { get; set; }

        public string? cognome { get; set; }
        public string? nome { get; set; }
        public string? foto { get; set; }
        public int id_persona { get; set; }
    }
    public class PersonaExtraLightDto
    {
        public PersonaExtraLightDto()
        {

        }

        public PersonaExtraLightDto(string cognome, string nome)
        {
            this.cognome = cognome;
            this.nome = nome;
        }
        public string DisplayName => $"{cognome} {nome} ({codice_gruppo})";

        public Guid UID_persona { get; set; }

        public string cognome { get; set; }
        public string nome { get; set; }
        public string foto { get; set; }
        public int id_persona { get; set; }
        public string codice_gruppo { get; set; }
    }
    
    public class PersonaPublicDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string cognome { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string nome { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int id { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid uid { get; set; }
    }
}