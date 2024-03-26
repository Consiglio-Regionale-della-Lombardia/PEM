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
using PortaleRegione.DTO.Model;

namespace PortaleRegione.DTO.Domain.Essentials
{
    public class AttoDASILightDto
    {
        public Guid uidAtto { get; set; }
        public string oggetto { get; set; }
        public string display { get; set; }
    }

    public class AttoDasiPublicDto
    {
        public Guid uidAtto { get; set; }
        public string oggetto { get; set; }
        public string premesse { get; set; }
        public string richiesta { get; set; }
        public string tipo { get; set; }
        public string data_presentazione { get; set; }
        public PersonaPublicDto proponente { get; set; } = new PersonaPublicDto();
        public string tipo_risposta { get; set; }
        public string stato { get; set; }
        public string area_politica { get; set; }
        public string data_ritiro { get; set; }
        public List<AttiFirmeDto> firme { get; set; } = new List<AttiFirmeDto>();
        public List<AttiFirmeDto> firme_dopo_deposito { get; set; } = new List<AttiFirmeDto>();
        public KeyValueDto gruppo { get; set; } = new KeyValueDto();
        public string data_iscrizione { get; set; }
        public string display { get; set; }
        public List<object> allegati { get; set; } = new List<object>();
        public object atto_odg { get; set; } = null;
        public bool non_passaggio_in_esame { get; set; }
        public string tipo_mozione { get; set; }
        public object abbinata { get; set; } = null;
        public List<KeyValueDto> commissioni { get; set; } = new List<KeyValueDto>();
        public List<object> risposte { get; set; } = new List<object>();
    }
}