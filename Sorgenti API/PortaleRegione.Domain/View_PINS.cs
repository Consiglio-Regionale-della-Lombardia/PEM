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
    public partial class View_PINS
    {
        [Key]
        [Column(Order = 0)]
        public Guid UIDPIN { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid UID_persona { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(255)]
        public string PIN { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime Dal { get; set; }

        public DateTime? Al { get; set; }

        [Key]
        [Column(Order = 4)]
        public bool FIRMA_e_DEPOSITO { get; set; }

        [Key]
        [Column(Order = 5)]
        public bool RichiediModificaPIN { get; set; }
    }
}
