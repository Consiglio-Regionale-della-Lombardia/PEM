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
    [Table("LETTERE")]
    public partial class LETTERE
    {
        [Key]
        public Guid UIDLettera { get; set; }

        public Guid? UIDComma { get; set; }

        [StringLength(5)]
        public string Lettera { get; set; }

        public string TestoLettera { get; set; }

        public int Ordine { get; set; }
        
        public Guid? UIDUtenteModifica { get; set; }
        public DateTime? DataModifica { get; set; }
        public bool Eliminato { get; set; } = false;

        public virtual COMMI COMMI { get; set; }
    }
}
