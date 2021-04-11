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

namespace PortaleRegione.Domain
{
    public partial class PINS
    {
        [Key]
        public Guid UIDPIN { get; set; }

        public Guid UID_persona { get; set; }

        [Required]
        [StringLength(255)]
        public string PIN { get; set; }

        public DateTime Dal { get; set; }

        public DateTime? Al { get; set; }

        public bool FIRMA_e_DEPOSITO { get; set; }

        public bool RichiediModificaPIN { get; set; }

        public virtual UTENTI_NoCons UTENTI_NoCons { get; set; }
    }
}
