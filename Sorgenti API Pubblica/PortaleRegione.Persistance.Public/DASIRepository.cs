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
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Persistance.Public
{
    /// <summary>
    ///     Repository per la gestione degli ATTI_DASI, fornendo operazioni di lettura sui dati degli atti.
    /// </summary>
    public class DASIRepository : IDASIRepository
    {
        /// <summary>
        ///     Contesto del database utilizzato per l'accesso ai dati.
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        ///     Proprietà di utilità per accedere al contesto del database specifico di PortaleRegione come tipizzato DbContext.
        /// </summary>
        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public DASIRepository(DbContext context)
        {
            Context = context;
        }

        public async Task<ATTI_DASI> Get(Guid attoUId)
        {
            var atto = await PRContext
                .DASI
                .FirstOrDefaultAsync(item => !item.Eliminato
                                             && item.Pubblicato
                                             && item.UIDAtto == attoUId);
            return atto;
        }

        public async Task<List<ATTI_DASI>> GetAll(int page, int size, Filter<ATTI_DASI> filtro = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && item.Pubblicato);
            filtro?.BuildExpression(ref query);
            return await query
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(Filter<ATTI_DASI> filtro = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && item.Pubblicato);

            filtro?.BuildExpression(ref query);
            return await query.CountAsync();
        }

        public async Task<List<ATTI_FIRME>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO && tipo == FirmeTipoEnum.DOPO_DEPOSITO)
                return new List<ATTI_FIRME>();

            var firmaProponente = await PRContext
                .ATTI_FIRME
                .SingleOrDefaultAsync(f =>
                    f.UIDAtto == atto.UIDAtto
                    && f.UID_persona == atto.UIDPersonaProponente
                    && f.Valida);

            var query = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == atto.UIDAtto
                            && f.UID_persona != atto.UIDPersonaProponente
                            && f.Valida);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp <= atto.Timestamp);
                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp > atto.Timestamp);
                    break;
                case FirmeTipoEnum.ATTIVI:
                    query = query.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            query = query.OrderBy(f => f.Timestamp);

            var lst = await query
                .ToListAsync();

            if (firmaProponente != null && tipo != FirmeTipoEnum.DOPO_DEPOSITO) lst.Insert(0, firmaProponente);

            return lst;
        }

        public async Task<List<KeyValueDto>> GetCommissioniPerAtto(Guid uidAtto)
        {
            var commissioniTable = await PRContext
                .ATTI_COMMISSIONI
                .Where(item => item.UIDAtto == uidAtto)
                .Select(item => item.id_organo)
                .ToListAsync();
            var query = PRContext
                .View_Commissioni
                .Where(item => commissioniTable.Contains(item.id_organo));
            var res = await query
                .Select(s=>new KeyValueDto
                {
                    id = s.id_organo,
                    descr = s.nome_organo
                })
                .ToListAsync();

            return res;
        }

        public async Task<List<ATTI_RISPOSTE>> GetRisposte(Guid uidAtto)
        {
            var dataFromDb = await PRContext
                .ATTI_RISPOSTE
                .Where(r => r.UIDAtto == uidAtto)
                .ToListAsync();

            return dataFromDb;
        }
    }
}