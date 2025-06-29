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

using PortaleRegione.DTO.Domain;
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

        public Guid? UIDAtto { get; set; }

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

        public string Query { get; set; }

        public bool Notifica { get; set; }

        public bool DASI { get; set; } = false;

        public Guid? UIDFascicolo { get; set; }

        public int NumeroFascicolo { get; set; } = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ATTI ATTI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual EM EM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual UTENTI_NoCons UTENTI_NoCons { get; set; }

        public static implicit operator STAMPE(StampaDto stampaDto)
        {
            var result = new STAMPE
            {
                UIDAtto = stampaDto.UIDAtto,
                Query = stampaDto.Query,
                UIDEM = stampaDto.UIDEM,
                CurrentRole = stampaDto.CurrentRole,
                DASI = stampaDto.DASI,
                Da = stampaDto.Da,
                A = stampaDto.A,
                DataFineEsecuzione = stampaDto.DataFineEsecuzione,
                DataInizioEsecuzione = stampaDto.DataInizioEsecuzione,
                DataInvio = stampaDto.DataInvio,
                DataLock = stampaDto.DataLock,
                DataRichiesta = stampaDto.DataRichiesta,
                Invio = stampaDto.Invio,
                Lock = stampaDto.Lock,
                MessaggioErrore = stampaDto.MessaggioErrore,
                Notifica = stampaDto.Notifica,
                Ordine = stampaDto.Ordine,
                PathFile = stampaDto.PathFile,
                Scadenza = stampaDto.Scadenza,
                Tentativi = stampaDto.Tentativi,
                UIDStampa = stampaDto.UIDStampa,
                UIDUtenteRichiesta = stampaDto.UIDUtenteRichiesta,
                UIDFascicolo = stampaDto.UIDFascicolo,
                NumeroFascicolo = stampaDto.NumeroFascicolo
            };

            return result;
        }

        public StampaDto ToDto()
        {
            var result = new StampaDto
            {
                UIDAtto = UIDAtto,
                Query = Query,
                UIDEM = UIDEM,
                CurrentRole = CurrentRole.Value,
                DASI = DASI,
                Da = Da,
                A = A,
                DataFineEsecuzione = DataFineEsecuzione,
                DataInizioEsecuzione = DataInizioEsecuzione,
                DataInvio = DataInvio,
                DataLock = DataLock,
                DataRichiesta = DataRichiesta,
                Invio = Invio,
                Lock = Lock,
                MessaggioErrore = MessaggioErrore,
                Notifica = Notifica,
                Ordine = Ordine,
                PathFile = PathFile,
                Scadenza = Scadenza,
                Tentativi = Tentativi,
                UIDStampa = UIDStampa,
                UIDUtenteRichiesta = UIDUtenteRichiesta,
                UIDFascicolo = UIDFascicolo,
                NumeroFascicolo = NumeroFascicolo
            };

            return result;
        }
    }
}
