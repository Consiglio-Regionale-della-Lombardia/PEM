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
using PortaleRegione.DTO.Request;

namespace PortaleRegione.Domain
{
    public partial class UTENTI_NoCons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UTENTI_NoCons()
        {
            NOTIFICHE = new HashSet<NOTIFICHE>();
            NOTIFICHE_DESTINATARI = new HashSet<NOTIFICHE_DESTINATARI>();
            PINS = new HashSet<PINS>();
            PINS_NoCons = new HashSet<PINS_NoCons>();
            RUOLI_UTENTE = new HashSet<RUOLI_UTENTE>();
            SEDUTE = new HashSet<SEDUTE>();
            STAMPE = new HashSet<STAMPE>();
            FIRME = new HashSet<FIRME>();
        }

        [Key]
        public Guid UID_persona { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_persona { get; set; }

        [Required]
        [StringLength(50)]
        public string cognome { get; set; }

        [StringLength(50)]
        public string nome { get; set; }

        [StringLength(250)]
        public string email { get; set; }

        [StringLength(250)]
        public string foto { get; set; }

        [StringLength(50)]
        public string UserAD { get; set; }

        public int? id_gruppo_politico_rif { get; set; }

        public bool notifica_firma { get; set; }

        public bool notifica_deposito { get; set; }

        public bool RichiediModificaPWD { get; set; }

        public DateTime? Data_ultima_modifica_PWD { get; set; }

        [StringLength(100)]
        public string pass_locale_crypt { get; set; }

        public bool attivo { get; set; }

        public bool? deleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICHE> NOTIFICHE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICHE_DESTINATARI> NOTIFICHE_DESTINATARI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PINS> PINS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PINS_NoCons> PINS_NoCons { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RUOLI_UTENTE> RUOLI_UTENTE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SEDUTE> SEDUTE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STAMPE> STAMPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FIRME> FIRME { get; set; }


        public static implicit operator UTENTI_NoCons(PersonaUpdateRequest persona)
        {
            return new UTENTI_NoCons
            {
                UID_persona = persona.UID_persona,
                cognome = persona.cognome,
                nome = persona.nome,
                email = persona.email,
                RichiediModificaPWD = false,
                UserAD = persona.userAD,
                notifica_deposito = persona.notifica_deposito,
                notifica_firma = persona.notifica_firma,
                id_gruppo_politico_rif = persona.id_gruppo_politico_rif,
                deleted = false,
                attivo = true
            };
        }
    }
}
