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
    public class RuoliRepository : Repository<RUOLI>, IRuoliRepository
    {
        public RuoliRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<IEnumerable<RUOLI>> GetAll(bool soloRuoliGiunta)
        {
            var query = PRContext
                .RUOLI
                .Where(r => true);
            if (soloRuoliGiunta)
                query = query.Where(r => r.Ruolo_di_Giunta);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<RUOLI>> RuoliUtente(List<string> lstRuoli)
        {
            var query = PRContext.RUOLI
                .Where(c => lstRuoli.Contains(c.ADGroup))
                .OrderBy(c => c.Priorita);

            return await query.ToListAsync();
        }

        public async Task<RUOLI> Get(int ruoliId)
        {
            return await PRContext.RUOLI.FindAsync(ruoliId);
        }
    }
}