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
    [Table("ATTI")]
    public partial class ATTI
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ATTI()
        {
            ARTICOLI = new HashSet<ARTICOLI>();
            ATTI_RELATORI = new HashSet<ATTI_RELATORI>();
            COMMI = new HashSet<COMMI>();
            EM = new HashSet<EM>();
            NOTIFICHE = new HashSet<NOTIFICHE>();
            STAMPE = new HashSet<STAMPE>();
        }

        [Key]
        public Guid UIDAtto { get; set; }

        [Required]
        [StringLength(50)]
        public string NAtto { get; set; }

        public int IDTipoAtto { get; set; }

        [StringLength(500)]
        public string Oggetto { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        public string Path_Testo_Atto { get; set; }

        public Guid? UIDSeduta { get; set; }

        public DateTime? Data_apertura { get; set; }

        public DateTime? Data_chiusura { get; set; }

        public bool VIS_Mis_Prog { get; set; }

        public Guid? UIDAssessoreRiferimento { get; set; }

        public bool Notifica_deposito_differita { get; set; }

        public bool? OrdinePresentazione { get; set; }

        public bool? OrdineVotazione { get; set; }

        public int? Priorita { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public bool? Eliminato { get; set; }

        public string LinkFascicoloPresentazione { get; set; }

        public DateTime? DataCreazionePresentazione { get; set; }

        public string LinkFascicoloVotazione { get; set; }

        public DateTime? DataCreazioneVotazione { get; set; }

        public DateTime? DataUltimaModificaEM { get; set; }

        public int TipoDibattito { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ARTICOLI> ARTICOLI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ATTI_RELATORI> ATTI_RELATORI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual SEDUTE SEDUTE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual TIPI_ATTO TIPI_ATTO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<COMMI> COMMI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EM> EM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICHE> NOTIFICHE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STAMPE> STAMPE { get; set; }

        public bool BloccoODG { get; set; } = false;
        public bool Jolly { get; set; } = false;
    }
}
