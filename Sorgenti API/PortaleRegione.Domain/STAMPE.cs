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
using System.ComponentModel.DataAnnotations.Schema;

namespace PortaleRegione.Domain
{
    [Table("STAMPE")]
    public partial class STAMPE
    {
        [Key]
        public Guid UIDStampa { get; set; }

        public Guid UIDAtto { get; set; }

        public Guid? UIDEM { get; set; }

        public int Da { get; set; }

        public int A { get; set; }

        public Guid UIDUtenteRichiesta { get; set; }

        public DateTime DataRichiesta { get; set; }

        public bool Invio { get; set; }

        public DateTime? DataInvio { get; set; }

        public string MessaggioErrore { get; set; }

        public bool Lock { get; set; }

        public DateTime? DataLock { get; set; }

        public string PathFile { get; set; }

        public DateTime? DataInizioEsecuzione { get; set; }

        public DateTime? DataFineEsecuzione { get; set; }

        public int Tentativi { get; set; }

        public int? CurrentRole { get; set; }

        public DateTime? Scadenza { get; set; }

        public int? Ordine { get; set; }

        public string QueryEM { get; set; }

        public bool NotificaDepositoEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ATTI ATTI { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual EM EM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual UTENTI_NoCons UTENTI_NoCons { get; set; }
    }
}
