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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;

namespace PortaleRegione.Persistance
{
    public class DASIRepository : Repository<ATTI_DASI>, IDASIRepository
    {
        public DASIRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;
        
        public async Task<ATTI_DASI> Get(Guid attoUId)
        {
            var result = await PRContext
                .DASI
                .SingleOrDefaultAsync(a => a.UIDAtto == attoUId);
            return result;
        }

        public async Task<List<Guid>> GetAll(PersonaDto persona, int page, int size, Filter<ATTI_DASI> filtro=null)
        {
            var query = PRContext
                .DASI
                .Where(item=>!item.Eliminato);

            filtro?.BuildExpression(ref query);

            return await query
                .OrderByDescending(item=>item.OrdineVisualizzazione)
                .Select(item => item.UIDAtto)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(PersonaDto persona, Filter<ATTI_DASI> filtro)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            filtro?.BuildExpression(ref query);

            return await query
                .CountAsync();
        }
    }
}