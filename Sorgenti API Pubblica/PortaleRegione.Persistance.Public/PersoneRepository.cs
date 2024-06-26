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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Model;
using Z.EntityFramework.Plus;

namespace PortaleRegione.Persistance.Public
{
    /// <summary>
    ///     Repository per la gestione delle persone
    /// </summary>
    public class PersoneRepository : IPersoneRepository
    {
        /// <summary>
        ///     Contesto del database utilizzato per l'accesso ai dati.
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        ///     Proprietà di utilità per accedere al contesto del database specifico di PortaleRegione come tipizzato DbContext.
        /// </summary>
        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public PersoneRepository(DbContext context)
        {
            Context = context;
        }

        public async Task<List<KeyValueDto>> GetCariche(int idLegislatura)
        {
            var result = await PRContext
                .View_cariche_assessori_per_legislatura
                .Where(c => c.id_legislatura == idLegislatura)
                .OrderBy(item => item.ordine)
                .ThenBy(item => item.nome_carica)
                .ToListAsync();
            return result.Select(c => new KeyValueDto
            {
                id = c.id_carica,
                descr = c.nome_carica
            }).ToList();
        }

        public async Task<List<KeyValueDto>> GetCommissioni(int idLegislatura)
        {
            var result = await PRContext
                .View_Commissioni_per_legislatura
                .Where(c => c.id_legislatura == idLegislatura)
                .OrderBy(item => item.ordinamento)
                .ThenBy(item => item.nome_organo)
                .ToListAsync();

            return result.Select(c => new KeyValueDto
            {
                id = c.id_organo,
                descr = c.nome_organo
            }).ToList();
        }

        public async Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura)
        {
            var query = PRContext
                .JOIN_GRUPPO_AD
                .Where(j => j.id_legislatura == idLegislatura)
                .Join(PRContext
                        .gruppi_politici,
                    p => p.id_gruppo,
                    g => g.id_gruppo,
                    (p, g) => g);
            var lstGruppi = await query
                .Select(g => new KeyValueDto
                {
                    id = g.id_gruppo,
                    descr = g.nome_gruppo,
                    sigla = g.codice_gruppo
                })
                .ToListAsync();
            return lstGruppi;
        }

        public async Task<List<PersonaPublicDto>> GetFirmatariByLegislatura(int idLegislatura)
        {
            PRContext.View_consiglieri_in_carica.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();

            var consiglieri = await PRContext
                .View_consiglieri_per_legislatura
                .Where(p => p.id_legislatura == idLegislatura && p.id_persona > 0)
                .Select(p => new PersonaPublicDto
                {
                    id = p.id_persona,
                    DisplayName = p.DisplayName
                })
                .ToListAsync();
            return consiglieri;
        }

        public async Task<List<View_UTENTI>> GetAll()
        {
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();

            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty)
                .OrderByDescending(u => u.id_persona)
                .ThenBy(u => u.cognome)
                .ThenBy(u => u.nome);

            return await query
                .ToListAsync();
        }
    }
}