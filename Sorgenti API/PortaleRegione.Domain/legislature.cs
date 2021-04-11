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
    [Table("legislature")]
    public partial class legislature
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public legislature()
        {
            join_gruppi_politici_legislature = new HashSet<join_gruppi_politici_legislature>();
            join_persona_assessorati = new HashSet<join_persona_assessorati>();
            join_persona_gruppi_politici = new HashSet<join_persona_gruppi_politici>();
            join_persona_organo_carica = new HashSet<join_persona_organo_carica>();
            organi = new HashSet<organi>();
            SEDUTE = new HashSet<SEDUTE>();
        }

        [Key]
        public int id_legislatura { get; set; }

        [Required]
        [StringLength(4)]
        public string num_legislatura { get; set; }

        public DateTime durata_legislatura_da { get; set; }

        public DateTime? durata_legislatura_a { get; set; }

        public bool attiva { get; set; }

        public int? id_causa_fine { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_gruppi_politici_legislature> join_gruppi_politici_legislature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_persona_assessorati> join_persona_assessorati { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_persona_gruppi_politici> join_persona_gruppi_politici { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_persona_organo_carica> join_persona_organo_carica { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<organi> organi { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SEDUTE> SEDUTE { get; set; }
    }
}
