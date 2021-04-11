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
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class GruppiRepository : Repository<gruppi_politici>, IGruppiRepository
    {
        public GruppiRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<GruppiDto> GetGruppoPersona(List<string> LGruppi, bool IsGiunta = false)
        {
            ///TODO: Controllo se ci sono più di due gruppi rilanciare errore di sistema

            var query = PRContext.JOIN_GRUPPO_AD
                .Where(p => LGruppi.Contains(p.GruppoAD) && p.GiuntaRegionale == IsGiunta)
                .Join(PRContext.View_gruppi_politici_con_giunta,
                    p => p.id_gruppo,
                    g => g.id_gruppo,
                    (p, g) => new GruppiDto
                    {
                        id_gruppo = g.id_gruppo,
                        nome_gruppo = g.nome_gruppo,
                        codice_gruppo = g.codice_gruppo,
                        giunta = p.GiuntaRegionale,
                        abilita_em_privati = p.AbilitaEMPrivati,
                        id_legislatura = p.id_legislatura
                    });

            var lstGruppi = await query.OrderByDescending(g => g.id_gruppo).ToListAsync();

            if (lstGruppi.Count == 2)
            {
                if (!lstGruppi.Any(g => g.giunta))
                    throw new Exception("Contatta SUBITO l'amministratore del sistema PEM.");
            }
            else if (lstGruppi.Count > 1)
            {
                throw new Exception("Contatta SUBITO l'amministratore del sistema PEM.");
            }


            return lstGruppi.Any() ? lstGruppi[0] : null;
        }

        public async Task<IEnumerable<KeyValueDto>> GetAll(int id_legislatura)
        {
            var query = PRContext
                .JOIN_GRUPPO_AD
                .Where(j => j.id_legislatura == id_legislatura)
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

        public async Task<gruppi_politici> Get(int gruppoId)
        {
            return await PRContext
                .gruppi_politici
                .SingleOrDefaultAsync(g => g.attivo && g.id_gruppo == gruppoId);
        }

        public async Task<View_UTENTI> GetCapoGruppo(int gruppoId)
        {
            var result = PRContext
                .View_UTENTI
                .SqlQuery(
                    $"Select * from View_UTENTI where UID_Persona = dbo.get_GUIDCapogruppo_from_idGruppo({gruppoId})");

            return await result.AnyAsync() ? await result.FirstAsync() : null;
        }

        public async Task<IEnumerable<View_UTENTI>> GetConsiglieriGruppo(int id_legislatura, int id_gruppo)
        {
            var query = PRContext
                .join_persona_gruppi_politici
                .Where(jpoc =>
                    jpoc.deleted == false
                    && jpoc.id_legislatura == id_legislatura
                    && jpoc.id_gruppo == id_gruppo
                    && jpoc.data_inizio <= DateTime.Now
                    && (jpoc.data_fine >= DateTime.Now || jpoc.data_fine == null))
                .Join(PRContext.join_persona_AD,
                    p => p.id_persona,
                    a => a.id_persona,
                    (p, a) => a.UID_persona)
                .Join(PRContext.View_UTENTI,
                    u => u,
                    p => p.UID_persona,
                    (u, p) => p);
            return (await query.ToListAsync())
                .Distinct()
                .OrderBy(p => p.cognome)
                .ThenBy(p => p.nome)
                .ToList();
        }

        public async Task<IEnumerable<UTENTI_NoCons>> GetSegreteriaPolitica(int id, bool notifica_firma,
            bool notifica_deposito)
        {
            var query = PRContext
                .UTENTI_NoCons
                .SqlQuery($"Select * from UTENTI_NoCons where id_gruppo_politico_rif={id}");

            if (notifica_firma)
                return query.Where(u => u.notifica_firma).ToList();
            if (notifica_deposito)
                return query.Where(u => u.notifica_deposito).ToList();

            return await query.ToListAsync();
        }

        public async Task<View_gruppi_politici_con_giunta> GetGruppoAttuale(PersonaDto persona, bool isGiunta)
        {
            var resultViewGruppi = PRContext
                .View_gruppi_politici_con_giunta
                .SqlQuery(
                    $"Select * from View_gruppi_politici_con_giunta where id_gruppo = dbo.get_IDgruppoAttuale_from_persona('{persona.UID_persona}', {(isGiunta ? 1 : 0)})");

            return await resultViewGruppi.AnyAsync() ? await resultViewGruppi.FirstAsync() : null;
        }

        public async Task<IEnumerable<JOIN_GRUPPO_AD>> GetGruppiPoliticiAD(int id_legislatura, bool soloRuoliGiunta)
        {
            var query = PRContext
                .JOIN_GRUPPO_AD
                .Where(j => j.id_legislatura == id_legislatura);

            if (soloRuoliGiunta)
                query = query.Where(j => j.GiuntaRegionale);

            return await query.ToListAsync();
        }
    }
}