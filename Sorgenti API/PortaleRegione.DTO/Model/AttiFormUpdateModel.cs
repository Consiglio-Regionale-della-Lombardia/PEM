﻿/*
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
using PortaleRegione.DTO.Domain.Essentials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PortaleRegione.DTO.Model
{
    public class AttiFormUpdateModel
    {
        [Key] public Guid UIDAtto { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Numero atto")]
        public string NAtto { get; set; }

        public int IDTipoAtto { get; set; }

        public Guid? UIDSeduta { get; set; }

        [AllowHtml] [StringLength(500)] public string Oggetto { get; set; }

        [AllowHtml] [StringLength(255)] public string Note { get; set; }

        [Display(Name = "Data apertura")] public DateTime? Data_apertura { get; set; }

        [Display(Name = "Data chiusura")] public DateTime? Data_chiusura { get; set; }

        [Display(Name = "Abilita missioni e programmi")]
        public bool VIS_Mis_Prog { get; set; }

        [Display(Name = "Assessore di riferimento")]
        public Guid? UIDAssessoreRiferimento { get; set; }

        [Display(Name = "Abilita deposito differito")]
        public bool Notifica_deposito_differita { get; set; }

        [JsonIgnore]
        public HttpPostedFileBase DocAtto { get; set; }

        public byte[] DocAtto_Stream { get; set; }

        public string Path_Testo_Atto { get; set; }

        public IEnumerable<PersonaLightDto> Relatori { get; set; }
        [Display(Name = "Emendabile")]
        public bool Emendabile { get; set; } = false;
    }
}