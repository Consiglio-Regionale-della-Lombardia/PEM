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
    public class LegislatureRepository : Repository<legislature>, ILegislatureRepository
    {
        public LegislatureRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<int> Legislatura_Attiva()
        {
            var result = await PRContext.legislature.Where(l => l.attiva).OrderBy(l => l.id_legislatura)
                .FirstOrDefaultAsync();
            return result?.id_legislatura ?? 0;
        }

        public async Task<IEnumerable<legislature>> GetLegislature()
        {
            var query = PRContext
                .legislature
                .Where(l => l.durata_legislatura_da >= new DateTime(2010,01,01))
                .OrderByDescending(l => l.durata_legislatura_da);

            return await query.ToListAsync();
        }

        public async Task<legislature> Get(int legislaturaId)
        {
            var result = await PRContext.legislature.SingleOrDefaultAsync(a => a.id_legislatura == legislaturaId);

            return result;
        }
    }
}