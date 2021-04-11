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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortaleRegione.Domain
{
    [Table("organi")]
    public partial class organi
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public organi()
        {
            join_persona_organo_carica = new HashSet<join_persona_organo_carica>();
            SEDUTE = new HashSet<SEDUTE>();
        }

        [Key]
        public int id_organo { get; set; }

        public int id_legislatura { get; set; }

        [Required]
        [StringLength(255)]
        public string nome_organo { get; set; }

        public DateTime data_inizio { get; set; }

        public DateTime? data_fine { get; set; }

        public bool deleted { get; set; }

        public int? ordinamento { get; set; }

        public int? id_tipo_organo { get; set; }

        [StringLength(30)]
        public string nome_organo_breve { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_persona_organo_carica> join_persona_organo_carica { get; set; }

        public virtual legislature legislature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SEDUTE> SEDUTE { get; set; }
    }
}
