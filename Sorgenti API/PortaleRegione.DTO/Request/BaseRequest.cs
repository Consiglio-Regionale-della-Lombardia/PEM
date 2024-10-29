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

using ExpressionBuilder.Generics;
using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;

namespace PortaleRegione.DTO.Request
{
    public class BaseRequest<T, T2> where T : class
    {
        public Guid id { get; set; }
        public int page { get; set; }

        public int size { get; set; }
        public List<FilterStatement<T>> filtro { get; set; }

        /// <summary>
        ///     Usato per ordinare gli emendamenti in presentazione o votazione
        /// </summary>
        public OrdinamentoEnum ordine { get; set; } = OrdinamentoEnum.Default;

        public IDictionary<string, object> param { get; set; }
        public T2 entity { get; set; }
    }
    
    public class BaseRequest<T> where T : class
    {
        public BaseRequest()
        {
            filtro = new List<FilterStatement<T>>();
            dettagliOrdinamento = new List<SortingInfo>();
        }

        public Guid id { get; set; }
        public int page { get; set; }

        public int size { get; set; }
        public List<FilterStatement<T>> filtro { get; set; }

        /// <summary>
        ///     Usato per ordinare gli emendamenti in presentazione o votazione
        /// </summary>
        public OrdinamentoEnum ordine { get; set; } = OrdinamentoEnum.Default;
        
        public IDictionary<string, object> param { get; set; }
        
        public List<SortingInfo> dettagliOrdinamento { get; set; }
    }
}