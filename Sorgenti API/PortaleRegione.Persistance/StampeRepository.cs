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
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class StampeRepository : Repository<STAMPE>, IStampeRepository
    {
        public StampeRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<IEnumerable<STAMPE>> GetAll(PersonaDto persona, int? page, int? size,
            Filter<STAMPE> filtro = null)
        {
            await PRContext.ATTI
                .Include(a => a.SEDUTE)
                .Include(a => a.TIPI_ATTO)
                .Where(a => a.Eliminato == false)
                .LoadAsync();

            var query = PRContext.STAMPE.Where(s => true);

            if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                && persona.CurrentRole != RuoliIntEnum.Amministratore_Giunta)
            {
                query = PRContext.STAMPE.Where(s => s.UIDUtenteRichiesta == persona.UID_persona);
            }

            filtro?.BuildExpression(ref query);

            return await query
                .OrderByDescending(s => s.DataRichiesta)
                .Skip((page.Value - 1) * size.Value)
                .Take(size.Value)
                .ToListAsync();
        }

        public async Task<IEnumerable<STAMPE>> GetAll(int page, int size)
        {
            return await PRContext
                .STAMPE
                .Where(s => !s.Lock)
                .OrderBy(s => s.Query.Length)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<IEnumerable<STAMPE>> GetStampeFascicolo(Guid uidFascicolo)
        {
            return await PRContext
                .STAMPE
                .Where(s => s.UIDFascicolo.Value.Equals(uidFascicolo))
                .OrderBy(s => s.NumeroFascicolo)
                .ToListAsync();
        }

        public async Task<int> Count(PersonaDto persona, Filter<STAMPE> filtro = null)
        {
            await PRContext.ATTI
                .Include(a => a.SEDUTE)
                .Include(a => a.TIPI_ATTO)
                .Where(a => a.Eliminato == false)
                .LoadAsync();

            var query = PRContext.STAMPE.Where(s => true);

            if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                && persona.CurrentRole != RuoliIntEnum.Amministratore_Giunta)
            {
                query = PRContext.STAMPE.Where(s => s.UIDUtenteRichiesta == persona.UID_persona);
            }

            filtro?.BuildExpression(ref query);

            return await query
                .CountAsync();
        }

        public async Task<int> Count()
        {
            return await PRContext
                .STAMPE
                .CountAsync(s => !s.Lock);
        }

        public async Task<STAMPE> Get(Guid stampaUId)
        {
            return await PRContext.STAMPE.FindAsync(stampaUId);
        }

        public void AddInfo(Guid stampaUId, string messaggio)
        {
            PRContext
                .STAMPE_INFO
                .Add(new STAMPE_INFO
                {
                    Id = Guid.NewGuid(),
                    UIDStampa = stampaUId,
                    Message = messaggio
                });
        }

        public async Task<IEnumerable<STAMPE_INFO>> GetInfo(Guid stampaUId)
        {
            var query = PRContext
                .STAMPE_INFO
                .Where(s => s.UIDStampa == stampaUId)
                .OrderByDescending(i => i.Date);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<STAMPE_INFO>> GetInfo()
        {
            return await PRContext
                .STAMPE_INFO
                .OrderByDescending(i => i.Date)
                .Take(1000)
                .ToListAsync();
        }

        public async Task<STAMPE_INFO> GetLastInfo(Guid stampaUId)
        {
            var query = PRContext
                .STAMPE_INFO
                .Where(s => s.UIDStampa == stampaUId)
                .OrderByDescending(i => i.Date);
            return await query.FirstOrDefaultAsync();
        }
    }
}