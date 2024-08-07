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
using PortaleRegione.Common;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request.Public;

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

        private async Task<IQueryable<ATTI_DASI>> GetQuery(Filter<ATTI_DASI> filtro,
            CercaRequest request)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && item.Pubblicato);
            filtro?.BuildExpression(ref query);

            if (request.id_tipo.Any())
            {
                query = query.Where(a => request.id_tipo.Contains(a.Tipo));
            }

            if (request.stati.Any())
            {
                query = query.Where(a => request.stati.Contains(a.IDStato)
                || request.stati.Contains(a.TipoChiusuraIter.Value));
            }
            
            if (request.id_proponente.Any())
            {
                var view_proponenti = await PRContext
                    .View_UTENTI
                    .Where(u => request.id_proponente.Contains(u.id_persona))
                    .Select(u => u.UID_persona.Value)
                    .ToListAsync();
                if (view_proponenti.Any())
                {
                    query = query.Where(a => view_proponenti.Contains(a.UIDPersonaProponente.Value));
                }
                else
                {
                    query = query.Where(a => false);
                }
            }

            if (request.firmatari.Any())
            {
                var view_firmatari = await PRContext
                    .View_UTENTI
                    .Where(u => request.firmatari.Contains(u.id_persona))
                    .Select(u => u.UID_persona.Value)
                    .ToListAsync();

                if (view_firmatari.Any())
                {
                    var atti_firmatari = PRContext
                        .ATTI_FIRME
                        .Where(f => view_firmatari.Contains(f.UID_persona))
                        .Select(f => f.UIDAtto)
                        .Distinct();

                    query = query.GroupJoin(atti_firmatari,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, docs) => new { atto, docs })
                        .Where(g => !g.docs.Any())
                        .Select(g => g.atto);
                }
                else
                {
                    query = query.Where(a => false);
                }
            }

            if (request.id_commissione.Any())
            {
                var attiCommissioni = PRContext
                    .ATTI_COMMISSIONI
                    .Where(c => request.id_commissione.Contains(c.id_organo))
                    .Select(c => c.UIDAtto);

                query = query.GroupJoin(attiCommissioni,
                        atto => atto.UIDAtto,
                        doc => doc,
                        (atto, docs) => new { atto, docs })
                    .Where(g => !g.docs.Any())
                    .Select(g => g.atto);

                var attiRisposta = PRContext
                    .ATTI_RISPOSTE
                    .Where(c => request.id_commissione.Contains(c.IdOrgano))
                    .Select(c => c.UIDAtto);

                query = query.GroupJoin(attiRisposta,
                        atto => atto.UIDAtto,
                        doc => doc,
                        (atto, docs) => new { atto, docs })
                    .Where(g => !g.docs.Any())
                    .Select(g => g.atto);
            }

            return query;
        }

        public async Task<List<ATTI_DASI>> GetAll(int page, int size, Filter<ATTI_DASI> filtro,
           CercaRequest request)
        {
            var query = await GetQuery(filtro, request);

            return await query
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(Filter<ATTI_DASI> filtro,
            CercaRequest request)
        {
            var query = await GetQuery(filtro, request);

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
                .Select(s => new KeyValueDto
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

        public async Task<List<ATTI_DOCUMENTI>> GetDocumenti(Guid uidAtto)
        {
            var dataFromDb = await PRContext
                .ATTI_DOCUMENTI
                .Where(r => r.UIDAtto == uidAtto && r.Pubblica)
                .ToListAsync();

            return dataFromDb;
        }

        public async Task<List<AttiAbbinamentoDto>> GetAbbinamenti(Guid uidAtto)
        {
            var abbinamentiInDB = await PRContext
                .ATTI_ABBINAMENTI
                .Where(a => a.UIDAtto.Equals(uidAtto))
                .ToListAsync();

            var res = new List<AttiAbbinamentoDto>();
            foreach (var attiAbbinamenti in abbinamentiInDB)
            {
                var abbinata = new AttiAbbinamentoDto
                {
                    UidAbbinamento = attiAbbinamenti.Uid,
                    Data = attiAbbinamenti.Data
                };

                if (!string.IsNullOrEmpty(attiAbbinamenti.TipoAttoAbbinato)
                    || !string.IsNullOrEmpty(attiAbbinamenti.NumeroAttoAbbinato)
                    || !string.IsNullOrEmpty(attiAbbinamenti.OggettoAttoAbbinato))
                {
                    abbinata.OggettoAttoAbbinato = attiAbbinamenti.OggettoAttoAbbinato;
                    abbinata.TipoAttoAbbinato = attiAbbinamenti.TipoAttoAbbinato;
                    abbinata.NumeroAttoAbbinato = attiAbbinamenti.NumeroAttoAbbinato;
                }
                else if (attiAbbinamenti.UIDAttoAbbinato.HasValue)
                {
                    var attoAbbinato = await PRContext
                        .VIEW_ATTI
                        .Where(a => a.UIDAtto.Equals(attiAbbinamenti.UIDAttoAbbinato.Value))
                        .FirstOrDefaultAsync();

                    if (attoAbbinato == null)
                        continue;

                    abbinata.OggettoAttoAbbinato = attoAbbinato.Oggetto;
                    abbinata.TipoAttoAbbinato = Utility.GetText_Tipo(attoAbbinato.Tipo);
                    abbinata.NumeroAttoAbbinato = attoAbbinato.NAtto;
                }

                res.Add(abbinata);
            }

            return res;
        }
    }
}