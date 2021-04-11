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
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PortaleRegione.DTO.Domain.Essentials
{
    public class EmendamentoLightDto
    {
        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        [Required(ErrorMessage = "E' obbligatorio indicare il modo")]
        public int IDTipo_EM { get; set; }

        [Required(ErrorMessage = "E' obbligatorio indicare l'elemento da emendare")]
        public int IDParte { get; set; }

        [StringLength(5)]
        public string NTitolo { get; set; }

        [StringLength(5)]
        public string NCapo { get; set; }

        public Guid? UIDArticolo { get; set; }

        public Guid? UIDComma { get; set; }

        public Guid? UIDLettera { get; set; }

        [StringLength(5)]
        public string NLettera { get; set; }

        [StringLength(5)] 
        public string NNumero { get; set; }

        public int? NMissione { get; set; }

        public int? NProgramma { get; set; }

        public int? NTitoloB { get; set; }

        [AllowHtml] 
        public string TestoEM_originale { get; set; }
        
        [AllowHtml]
        public string TestoREL_originale { get; set; }

        public string PATH_AllegatoGenerico { get; set; }

        public string PATH_AllegatoTecnico { get; set; }

        public int? EffettiFinanziari { get; set; }

        [AllowHtml] public string NOTE_EM { get; set; }

        [AllowHtml] public string NOTE_Griglia { get; set; }
    }
}