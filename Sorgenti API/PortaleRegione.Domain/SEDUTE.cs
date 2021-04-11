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
    [Table("SEDUTE")]
    public partial class SEDUTE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SEDUTE()
        {
            ATTI = new HashSet<ATTI>();
        }

        [Key]
        public Guid UIDSeduta { get; set; }

        public DateTime Data_seduta { get; set; }

        public DateTime? Data_apertura { get; set; }

        public DateTime? Data_effettiva_inizio { get; set; }

        public DateTime? Data_effettiva_fine { get; set; }

        public int? IDOrgano { get; set; }

        public DateTime? Scadenza_presentazione { get; set; }

        public int id_legislatura { get; set; }

        public string Intervalli { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public DateTime? DataModifica { get; set; }

        public bool? Eliminato { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ATTI> ATTI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual legislature legislature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual organi organi { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual UTENTI_NoCons UTENTI_NoCons { get; set; }
    }
}
