﻿/*
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
    public class SeduteRepository : Repository<SEDUTE>, ISeduteRepository
    {
        public SeduteRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<SEDUTE> Get(Guid sedutaUId)
        {
            var result = await PRContext.SEDUTE.Include(s => s.legislature)
                .SingleOrDefaultAsync(a => a.UIDSeduta == sedutaUId);
            return result;
        }

        public async Task<SEDUTE> Get(DateTime dataSeduta)
        {
            return await PRContext.SEDUTE.FirstOrDefaultAsync(item =>
                item.Data_seduta.Day == dataSeduta.Day
                && item.Data_seduta.Month == dataSeduta.Month
                && item.Data_seduta.Year == dataSeduta.Year);
        }

        public async Task<IEnumerable<SEDUTE>> GetAll(int legislaturaId, int pageIndex, int pageSize,
            Filter<SEDUTE> filtro = null)
        {
            var query = PRContext.SEDUTE.Include(s => s.legislature)
                .Where(c => c.Eliminato == false || !c.Eliminato.HasValue);

            if (filtro == null)
                query = query.Where(s => s.id_legislatura == legislaturaId);
            else
                filtro.BuildExpression(ref query);

            if (pageIndex == 0 && pageSize == 0)
                return await query.OrderByDescending(c => c.Data_seduta)
                    .ToListAsync();

            return await query.OrderByDescending(c => c.Data_seduta)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<SEDUTE>> GetAttiveDashboard()
        {
            var query = PRContext.SEDUTE.Include(s => s.legislature)
                .Where(c => (c.Eliminato == false
                             || !c.Eliminato.HasValue)
                            && !c.Data_effettiva_fine.HasValue
                            && c.Data_apertura < DateTime.Now);

            return await query.OrderBy(c => c.Data_seduta)
                .ToListAsync();
        }

        public async Task<int> Count(int legislaturaId, Filter<SEDUTE> filtro = null)
        {
            var query = PRContext.SEDUTE
                .Where(c => c.Eliminato == false || !c.Eliminato.HasValue);

            if (filtro == null)
                query = query.Where(s => s.id_legislatura == legislaturaId);
            else
                filtro.BuildExpression(ref query);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<SEDUTE>> GetAttive(bool riservato_dasi = false, bool convocata = false)
        {
            var query = PRContext.SEDUTE.Include(s => s.legislature).Where(c => (c.Eliminato == false
                    || !c.Eliminato.HasValue)
                && !c.Data_effettiva_fine.HasValue);
            if (convocata) query = query.Where(c => c.Data_apertura <= DateTime.Now);
            if (riservato_dasi) query = query.Where(c => c.Riservato_DASI);

            return await query.OrderBy(c => c.Data_seduta)
                .ToListAsync();
        }
    }
}