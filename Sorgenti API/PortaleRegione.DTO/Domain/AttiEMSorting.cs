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

namespace PortaleRegione.DTO.Domain;

/// <summary>
///     Elenco ammesso delle proprieta' per l'ordinamento dinamico degli Emendamenti
///     veicolato da SortingInfo. I nomi devono corrispondere esattamente a quelli dell'entita'
///     EF "EM"; la riflessione applicata in EmendamentiRepository scarta qualsiasi propertyName
///     non presente in questa classe.
/// </summary>
public class AttiEMSorting
{
    [DisplayName("Numero emendamento")] public int? Progressivo { get; set; }
    [DisplayName("Sub-progressivo")] public int? SubProgressivo { get; set; }
    [DisplayName("Stato")] public int IDStato { get; set; }
    [DisplayName("Tipo emendamento")] public int IDTipo_EM { get; set; }
    [DisplayName("Parte")] public int IDParte { get; set; }
    [DisplayName("Gruppo")] public int id_gruppo { get; set; }
    [DisplayName("Ordine presentazione")] public int OrdinePresentazione { get; set; }
    [DisplayName("Ordine votazione")] public int OrdineVotazione { get; set; }
    [DisplayName("Data deposito")] public DateTime? Timestamp { get; set; }
}
