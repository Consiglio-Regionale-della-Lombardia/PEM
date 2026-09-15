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
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    public class AttiDASIColumnsConsiglieri
    {
        [DisplayName("Tipo atto")] public int Tipo { get; set; }
        [DisplayName("Etichetta")] public string Etichetta { get; set; }
        [DisplayName("Oggetto")] public string Oggetto { get; set; }
        [DisplayName("Proponente")] public Guid? UIDPersonaProponente { get; set; }
        [DisplayName("Firmatari")] public string Firme { get; set; }
        [DisplayName("Data presentazione")] public DateTime Timestamp { get; set; }
        [DisplayName("Gruppo politico")] public int id_gruppo { get; set; }
        [DisplayName("Area politica")] public int AreaPolitica { get; set; }
        [DisplayName("Stato dell'atto")] public int IDStato { get; set; }
    }
}