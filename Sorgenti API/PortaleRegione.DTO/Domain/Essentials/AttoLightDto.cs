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
using System.Collections.Generic;
using Newtonsoft.Json;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.DTO.Domain.Essentials
{
    public class AttoLightDto
    {
        public Guid uidAtto { get; set; }
        public string oggetto { get; set; }
        public string display { get; set; }
        public string natto { get; set; }
        public string tipo { get; set; }
        public string tipo_esteso { get; set; }
    }

    public class AttoDasiPublicDto
    {
        public Guid uidAtto { get; set; }
        public string oggetto { get; set; }
        public string premesse { get; set; }
        public string richiesta { get; set; }
        public string tipo { get; set; }
        public string tipo_esteso { get; set; }
        public string n_atto { get; set; }
        public string data_presentazione { get; set; }
        public PersonaPublicDto proponente { get; set; } = new PersonaPublicDto();
        public string tipo_risposta { get; set; }
        public string stato { get; set; }
        public string area_politica { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string data_ritiro { get; set; }

        public List<AttiFirmePublicDto> firme { get; set; } = new List<AttiFirmePublicDto>();
        public KeyValueDto gruppo { get; set; } = new KeyValueDto();
        public string data_iscrizione { get; set; }
        public string display { get; set; }
        public List<AttiDocumentiPublicDto> documenti { get; set; } = new List<AttiDocumentiPublicDto>();
        public object atto_odg { get; set; } = null;
        public bool non_passaggio_in_esame { get; set; }
        public string tipo_mozione { get; set; }
        public object abbinata { get; set; } = null;
        public List<KeyValueDto> commissioni { get; set; } = new List<KeyValueDto>();
        public List<AttiRispostePublicDto> risposte { get; set; } = new List<AttiRispostePublicDto>();
        public int id_stato { get; set; }
        public int id_tipo { get; set; }
        public Guid uid_proponente { get; set; }
        public string data_annunzio { get; set; }
        public string stato_iter { get; set; }
        public string dcrl { get; set; }
        public int? dcr { get; set; }
        public int? dcrc { get; set; }
        public List<AttiAbbinamentoDto> abbinamenti { get; set; } = new List<AttiAbbinamentoDto>();
        public string burl { get; set; }
    }
}