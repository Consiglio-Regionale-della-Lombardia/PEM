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
    public class EmendamentiColumns
    {
        [DisplayName("Numero EM")] public string N_EM { get; set; }
        [DisplayName("Data Deposito")] public string DataDeposito { get; set; }
        [DisplayName("Stato")] public int IDStato { get; set; }
        [DisplayName("Tipo")] public int IDTipo_EM { get; set; }
        [DisplayName("Parte")] public int IDParte { get; set; }
        [DisplayName("Articolo")] public Guid? UIDArticolo { get; set; }
        [DisplayName("Comma")] public Guid? UIDComma { get; set; }
        [DisplayName("Lettera")] public Guid? UIDLettera { get; set; }
        [DisplayName("Titolo")] public string NTitolo { get; set; }
        [DisplayName("Capo")] public string NCapo { get; set; }
        [DisplayName("Missione")] public int? NMissione { get; set; }
        [DisplayName("Programma")] public int? NProgramma { get; set; }
        [DisplayName("TitoloM")] public int? NTitoloB { get; set; }
        [DisplayName("Area politica")] public int? AreaPolitica { get; set; }
        [DisplayName("Firmatari")] public string Firme { get; set; }
        [DisplayName("Proponente")] public Guid UIDPersonaProponente { get; set; }
        [DisplayName("Gruppi")] public int id_gruppo { get; set; }
        [DisplayName("Effetti finanziari")] public int EffettiFinanziari { get; set; }
        [DisplayName("Tags")] public string Tags { get; set; }
    }
}