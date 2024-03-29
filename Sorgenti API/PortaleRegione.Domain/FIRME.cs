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
    [Table("FIRME")]
    public class FIRME
    {
        [Key] [Column(Order = 0)] public Guid UIDEM { get; set; }

        [Key] [Column(Order = 1)] public Guid UID_persona { get; set; }

        public string FirmaCert { get; set; }

        [StringLength(255)] public string Data_firma { get; set; }

        [StringLength(255)] public string Data_ritirofirma { get; set; }

        public int? id_AreaPolitica { get; set; }

        
        public DateTime Timestamp { get; set; }

        public bool ufficio { get; set; }

        public virtual EM EM { get; set; }

        public virtual UTENTI_NoCons UTENTI_NoCons { get; set; }
    }
}