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

using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Common;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class AttiRepository : Repository<ATTI>, IAttiRepository
    {
        public AttiRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<ATTI> Get(Guid attoUId)
        {
            var result = await PRContext
                .ATTI
                .Include(a => a.SEDUTE)
                .SingleOrDefaultAsync(a => a.UIDAtto == attoUId);
            return result;
        }

        public async Task<ATTI> Get(string attoUId)
        {
            var newGuid = new Guid(attoUId);
            return await Get(newGuid);
        }

        public async Task<IEnumerable<ATTI>> GetAll(Guid sedutaUId, int pageIndex, int pageSize,
            int clientMode,
            PersonaDto persona,
            Filter<ATTI> filtro = null)
        {
            var query = PRContext
                .ATTI
                .Include(a => a.SEDUTE)
                .Where(c => c.Eliminato == false && c.UIDSeduta == sedutaUId);

            filtro?.BuildExpression(ref query);

            if (clientMode == (int) ClientModeEnum.GRUPPI)
            {
                if (!persona.IsSegreteriaAssemblea)
                    query = query.Where(item => item.Emendabile);
            }

            return await query
                .OrderBy(c => c.Priorita)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ATTI>> GetAllBySeduta(Guid sedutaUId)
        {
            var query = PRContext
                .ATTI
                .Where(c => c.Eliminato == false && c.UIDSeduta == sedutaUId);
            return await query
                .OrderBy(c => c.Priorita)
                .ToListAsync();
        }

        public async Task SalvaRelatori(Guid attoUId, IEnumerable<Guid> persone)
        {
            var oldRelatori = await PRContext
                .ATTI_RELATORI
                .Where(ar => ar.UIDAtto == attoUId)
                .ToListAsync();
            PRContext
                .ATTI_RELATORI
                .RemoveRange(oldRelatori);

            if (persone == null)
            {
                return;
            }

            if (!persone.Any())
            {
                return;
            }

            var newRelatori = persone.Select(persona => new ATTI_RELATORI
            { UIDAtto = attoUId, UIDPersona = persona, sycReplica = Guid.NewGuid() }).ToList();
            PRContext
                .ATTI_RELATORI
                .AddRange(newRelatori);
        }

        public async Task<int> PrioritaAtto(Guid sedutaUId)
        {
            var list = await PRContext
                .ATTI
                .Where(a => a.UIDSeduta == sedutaUId && a.Eliminato == false)
                .OrderByDescending(a => a.Priorita)
                .Take(1)
                .ToListAsync();

            return list.Any() ? list[0].Priorita.Value + 1 : 1;
        }

        public async Task<int> CountEM(Guid id, bool sub_em, PersonaDto persona, int gruppo)
        {
            var query = PRContext.EM
                .Where(em =>
                    em.UIDAtto == id
                    && !em.Eliminato);

            if (persona != null)
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza
                                          || em.IDStato == (int)StatiEnum.Bozza
                                          && (em.UIDPersonaCreazione == persona.UID_persona
                                              || em.UIDPersonaProponente == persona.UID_persona));
                if (persona.IsSegreteriaAssemblea)
                {
                    //Solo segreteria
                    query = query.Where(e => !string.IsNullOrEmpty(e.DataDeposito));
                }
                else if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM)
                {
                    //Tutti gli altri utenti visualizzano gli emendamenti del proprio gruppo
                    query = query.Where(e => e.id_gruppo == gruppo);
                }
            }
            else
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza);
            }

            if (sub_em)
            {
                query = query.Where(e => e.Rif_UIDEM != null);
            }

            return (await query.ToListAsync()).Count;
        }

        public async Task<int> Count(Guid sedutaUId, int clientMode, PersonaDto persona, Filter<ATTI> filtro = null)
        {
            var query = PRContext.ATTI.Where(c => c.UIDSeduta == sedutaUId && c.Eliminato == false);

            filtro?.BuildExpression(ref query);
            if (clientMode == (int)ClientModeEnum.GRUPPI)
            {
                if (!persona.IsSegreteriaAssemblea)
                    query = query.Where(item => item.Emendabile);
            }

            return (await query.ToListAsync()).Count;
        }

        public async Task SPOSTA_DOWN(Guid attoUId)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec DOWN_ATTO @UIDAtto='{attoUId}'");
        }

        public async Task SPOSTA_UP(Guid attoUId)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec UP_ATTO @UIDAtto='{attoUId}'");
        }

        public bool CanMoveUp(int currentPriorita)
        {
            return currentPriorita > 1;
        }

        public async Task<bool> CanMoveDown(Guid sedutaUId, int currentPriorita)
        {
            var max_priorita = await PRContext
                .ATTI
                .Where(a => a.UIDSeduta == sedutaUId)
                .MaxAsync(a => a.Priorita);
            if (currentPriorita >= max_priorita)
            {
                return false;
            }

            return true;
        }

        public async Task RimuoviFascicoliObsoleti(Guid attoUId, OrdinamentoEnum ordinamento)
        {
            var oldStampe = await PRContext
                .STAMPE
                .Where(s => s.UIDAtto == attoUId && s.Da == 0 && s.A == 0 && s.Ordine == (int)ordinamento)
                .ToListAsync();
            PRContext.STAMPE.RemoveRange(oldStampe);
        }

        public async Task<List<PersonaLightDto>> GetRelatori(Guid attoUId)
        {
            var atti_relatori = await PRContext
                .ATTI_RELATORI
                .Where(ar => ar.UIDAtto == attoUId)
                .ToListAsync();
            var result = new List<PersonaLightDto>();
            foreach (var relatore in atti_relatori)
            {
                var persona = await PRContext.View_UTENTI.FindAsync(relatore.UIDPersona);
                result.Add(new PersonaLightDto
                {
                    UID_persona = persona.UID_persona.Value,
                    cognome = persona.cognome,
                    nome = persona.nome,
                    foto = persona.foto
                });
            }

            return result;
        }

        public async Task<AttiAbbinamentoDto> GetAbbinamento(Guid attoUId)
        {
            var atto = await PRContext.VIEW_ATTI.FirstAsync(a => a.UIDAtto.Equals(attoUId));
            return new AttiAbbinamentoDto
            {
                OggettoAttoAbbinato = atto.Oggetto,
                NumeroAttoAbbinato = atto.NAtto,
                TipoAttoAbbinato = Utility.GetText_Tipo(atto.Tipo)
            };
        }
    }
}