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
    public class View_CAPIGRUPPO
    {
        [Key] public Guid? UID_persona { get; set; }

        public int id_persona { get; set; }

        [StringLength(50)] public string cognome { get; set; }

        [StringLength(50)] public string nome { get; set; }

        [StringLength(255)] public string foto { get; set; }

        [StringLength(255)] public string GruppoPolitico_attuale { get; set; }

        public int? id_gruppo_politico_rif { get; set; }
    }
}