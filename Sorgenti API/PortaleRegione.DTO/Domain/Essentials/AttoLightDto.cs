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
        public string tipo { get; set; }
        public string tipo_esteso { get; set; }
        public string n_atto { get; set; }
        public string data_presentazione { get; set; }
        public PersonaPublicDto proponente { get; set; } = new PersonaPublicDto();
        public string tipo_risposta_richiesta { get; set; }
        public string stato { get; set; }
        public string area_politica { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string data_ritiro { get; set; }

        public List<AttiFirmePublicDto> firme { get; set; } = new List<AttiFirmePublicDto>();
        public KeyValueDto gruppo { get; set; } = new KeyValueDto();
        public string display { get; set; }
        public List<AttiDocumentiPublicDto> documenti { get; set; } = new List<AttiDocumentiPublicDto>();
        public string tipo_mozione { get; set; }
        public List<KeyValueDto> commissioni { get; set; } = new List<KeyValueDto>();
        public List<AttiRispostePublicDto> risposte { get; set; } = new List<AttiRispostePublicDto>();
        public int id_stato { get; set; }
        public int id_tipo { get; set; }
        public Guid uid_proponente { get; set; }
        public string data_annunzio { get; set; }
        public string stato_iter { get; set; }
        public string dcrl { get; set; }
        public string dcr { get; set; }
        public string dcrc { get; set; }
        public List<AttiAbbinamentoPublicDto> abbinamenti { get; set; } = new List<AttiAbbinamentoPublicDto>();
        public string burl { get; set; }
        public string data_chiusura_iter { get; set; }
        public List<NoteDto> note { get; set; } = new List<NoteDto>();
        public string data_comunicazione_assemblea { get; set; }
        public string link_testo_originale { get; set; }
        public string link_testo_trattazione { get; set; }
        public List<KeyValueDto> proponenti { get; set; } = new List<KeyValueDto>();
        public PersonaPublicDto relatore1 { get; set; }
        public PersonaPublicDto relatore2 { get; set; }
        public PersonaPublicDto relatore_minoranza { get; set; }
        public string tipo_risposta_fornita { get; set; }
    }
}