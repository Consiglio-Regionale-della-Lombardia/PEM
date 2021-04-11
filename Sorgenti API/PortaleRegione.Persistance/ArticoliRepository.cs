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
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class ArticoliRepository : Repository<ARTICOLI>, IArticoliRepository
    {
        public ArticoliRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<bool> CheckIfArticoloExists(Guid attoUId, string articolo)
        {
            return await PRContext
                .ARTICOLI
                .AnyAsync(a => a.UIDAtto == attoUId && a.Articolo.Contains(articolo));
        }

        public async Task<IEnumerable<ARTICOLI>> GetArticoli(Guid attoUId)
        {
            return await PRContext
                .ARTICOLI
                .Where(a => a.UIDAtto == attoUId)
                .OrderBy(a => a.Ordine)
                .ToListAsync();
        }

        public async Task<ARTICOLI> GetArticolo(Guid articoloUId)
        {
            return await PRContext.ARTICOLI.FindAsync(articoloUId);
        }

        public async Task<int> OrdineArticolo(Guid attoUId)
        {
            var list = await PRContext
                .ARTICOLI
                .Where(a => a.UIDAtto == attoUId)
                .OrderByDescending(a => a.Ordine)
                .Take(1)
                .ToListAsync();

            return list.Any() ? list[0].Ordine + 1 : 1;
        }
    }
}