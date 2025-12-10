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

using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class LettereRepository : Repository<LETTERE>, ILettereRepository
    {
        public LettereRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<bool> CheckIfLetteraExists(Guid commaUId, string lettera)
        {
            return await PRContext
                .LETTERE
                .AnyAsync(a => a.UIDComma == commaUId && a.Lettera.Contains(lettera) && !a.Eliminato);
        }

        public async Task<LETTERE> GetLettera(Guid lettaraUId)
        {
            return await PRContext.LETTERE.FindAsync(lettaraUId);
        }

        public async Task<IEnumerable<LETTERE>> GetLettere(Guid commaUId)
        {
            return await PRContext
                .LETTERE
                .Where(l => l.UIDComma == commaUId && !l.Eliminato)
                .OrderBy(l => l.Ordine)
                .ToListAsync();
        }

        public async Task<int> OrdineLettera(Guid commaUId)
        {
            var list = await PRContext
                .LETTERE
                .Where(a => a.UIDComma == commaUId && !a.Eliminato)
                .OrderByDescending(a => a.Ordine)
                .Take(1)
                .ToListAsync();

            return list.Any() ? list[0].Ordine + 1 : 1;
        }
    }
}