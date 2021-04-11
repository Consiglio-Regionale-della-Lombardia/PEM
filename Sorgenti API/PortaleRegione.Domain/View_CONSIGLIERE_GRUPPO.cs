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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortaleRegione.Domain
{
    public partial class View_CONSIGLIERE_GRUPPO
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id_persona { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string nome { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string cognome { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id_gruppo { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(50)]
        public string codice_gruppo { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(255)]
        public string nome_gruppo { get; set; }

        [Key]
        [Column(Order = 6)]
        public bool attivo { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id_legislatura { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(4)]
        public string num_legislatura { get; set; }
    }
}
