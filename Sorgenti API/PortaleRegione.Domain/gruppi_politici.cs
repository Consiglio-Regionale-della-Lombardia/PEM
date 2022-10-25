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

namespace PortaleRegione.Domain
{
    public partial class gruppi_politici
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public gruppi_politici()
        {
            EM = new HashSet<EM>();
            join_gruppi_politici_legislature = new HashSet<join_gruppi_politici_legislature>();
            JOIN_GRUPPO_AD = new HashSet<JOIN_GRUPPO_AD>();
            join_persona_gruppi_politici = new HashSet<join_persona_gruppi_politici>();
            NOTIFICHE_DESTINATARI = new HashSet<NOTIFICHE_DESTINATARI>();
            NOTIFICHE = new HashSet<NOTIFICHE>();
        }

        [Key]
        public int id_gruppo { get; set; }

        [Required]
        [StringLength(50)]
        public string codice_gruppo { get; set; }

        [Required]
        [StringLength(255)]
        public string nome_gruppo { get; set; }

        public DateTime data_inizio { get; set; }

        public DateTime? data_fine { get; set; }

        public bool attivo { get; set; }

        public int? id_causa_fine { get; set; }
        public int TipoArea { get; set; } = 0;

        public bool deleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EM> EM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_gruppi_politici_legislature> join_gruppi_politici_legislature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JOIN_GRUPPO_AD> JOIN_GRUPPO_AD { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<join_persona_gruppi_politici> join_persona_gruppi_politici { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICHE_DESTINATARI> NOTIFICHE_DESTINATARI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICHE> NOTIFICHE { get; set; }
    }
}
