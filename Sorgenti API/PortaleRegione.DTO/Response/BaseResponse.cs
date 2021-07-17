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
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortaleRegione.DTO.Response
{
    public class BaseResponse<T> where T : class
    {
        public BaseResponse()
        {
        }

        public BaseResponse(
            int current_page,
            int page_size,
            IEnumerable<T> results,
            List<FilterStatement<T>> filtri,
            int total_entities,
            Uri original_path = null)
        {
            Filters = filtri;
            Results = results;
            var port = "";
            var path = string.Empty;
            if (original_path != null)
            {
                if (original_path.Port > 0)
                {
                    port = $":{original_path.Port}";
                }

                path = $"{original_path.Scheme}://{original_path.Host}{port}{original_path.AbsolutePath}";
            }

            var max_page = (int) Math.Ceiling(total_entities / (double) page_size);
            Paging = new Paging
            {
                Page = current_page,
                Last_Page = max_page,
                Entities = results.Count(),
                Limit = page_size,
                Total = total_entities,
                Has_Prev = current_page > 1,
                Has_Next = current_page < max_page
            };

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Paging.Next_Url = new Uri(path + $"?page={current_page + 1}&size={page_size}");
            Paging.Prev_Url = new Uri(path + $"?page={current_page - 1}&size={page_size}");
        }

        public Paging Paging { get; set; }
        public IEnumerable<T> Results { get; set; }
        public List<FilterStatement<T>> Filters { get; set; }
    }
}