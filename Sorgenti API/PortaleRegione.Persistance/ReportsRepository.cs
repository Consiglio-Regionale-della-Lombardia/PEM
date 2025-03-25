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
    public class ReportsRepository : Repository<REPORTS>, IReportsRepository
    {
        public ReportsRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;
        
        public async Task<List<REPORTS>> GetByUser(Guid uidPersona)
        {
            var res = await PRContext
                .REPORTS
                .Where(f => f.UId_persona.Equals(uidPersona))
                .OrderBy(f => f.Nome)
                .ToListAsync();
            return res;
        }

        public async Task<REPORTS> Get(string nome, Guid UidPersona)
        {
            var res = await PRContext
                .REPORTS
                .FirstOrDefaultAsync(f => f.UId_persona.Equals(UidPersona) && f.Nome.Equals(nome));
            return res;
        }
    }
}