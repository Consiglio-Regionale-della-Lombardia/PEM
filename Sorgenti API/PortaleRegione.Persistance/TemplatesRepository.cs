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
using PortaleRegione.DTO.Enum;
using Z.EntityFramework.Plus;

namespace PortaleRegione.Persistance
{
    public class TemplatesRepository: Repository<TEMPLATES>, ITemplatesRepository
    {
        public TemplatesRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<List<TEMPLATES>> GetAll(bool viewAll = false)
        {
            PRContext.TEMPLATES.FromCache(DateTimeOffset.Now.AddHours(1)).ToList();
            var query = PRContext
                .TEMPLATES
                .Where(t => t.Eliminato == false);

            if (!viewAll)
            {
                query = query.Where(t => t.Visibile);
            }
            return await query
                .ToListAsync();
        }

        public async Task<List<TEMPLATES>> GetAllByType(TemplateTypeEnum type)
        {
            PRContext.TEMPLATES.FromCache(DateTimeOffset.Now.AddHours(1)).ToList();

            return await PRContext
                .TEMPLATES
                .Where(t => t.Eliminato == false
                && t.Tipo.Equals((int)type)
                && t.Visibile)
                .ToListAsync();
        }

        public async Task<TEMPLATES> Get(Guid uid)
        {
            PRContext.TEMPLATES.FromCache(DateTimeOffset.Now.AddHours(1)).ToList();

            return await PRContext
                .TEMPLATES
                .FirstOrDefaultAsync(t => t.Uid.Equals(uid));
        }
    }
}