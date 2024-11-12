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
using System.Diagnostics.CodeAnalysis;

namespace PortaleRegione.Domain
{
    [Table("ATTI_RISPOSTE")]
    public class ATTI_RISPOSTE
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ATTI_RISPOSTE()
        {
            Uid = Guid.NewGuid();
        }

        [Key] public Guid Uid { get; set; }
        public Guid UIDAtto { get; set; }
        public int TipoOrgano { get; set; } = 0;
        public int Tipo { get; set; } = 0;
        public DateTime? Data { get; set; }
        public DateTime? DataTrasmissione { get; set; }
        public DateTime? DataTrattazione { get; set; }
        public int IdOrgano { get; set; } = 0;
        public string DescrizioneOrgano { get; set; }
    }
}