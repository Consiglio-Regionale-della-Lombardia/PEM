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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;

namespace PortaleRegione.Persistance
{
    public class SessionsRepository: Repository<Sessioni>, ISessionsRepository
    {
        public SessionsRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task NuovaSessione(Sessioni sessioni)
        {
            var currentSession = await PRContext.Sessioni.FirstOrDefaultAsync(s => s.uidUtente.Equals(sessioni.uidUtente) && !s.DataUscita.HasValue);
            if(currentSession != null)
            {
                PRContext.Sessioni.Remove(currentSession);
            }

            PRContext.Sessioni.Add(sessioni);
            await PRContext.SaveChangesAsync();
        }

        public async Task<List<SessioniDto>> Get()
        {
            var resultFromDb = await PRContext.Sessioni.OrderByDescending(s => s.DataIngresso).ToListAsync();

            var result = new List<SessioniDto>();
            foreach (var sessioni in resultFromDb)
            {
                var stato = (sessioni.DataUscita.HasValue) ? "Chiuso" : "Attivo";
                var persona = await PRContext.View_UTENTI.FindAsync(sessioni.uidUtente);
                result.Add(new SessioniDto
                {
                    uidSessione = sessioni.uidSessione,
                    Persona = new PersonaLightDto(persona.cognome, persona.nome),
                    ChiusuraCorretta = sessioni.ChiusuraCorretta,
                    DataIngresso = sessioni.DataIngresso,
                    DataUscita = sessioni.DataUscita,
                    Stato = stato
                });
            }

            return result;
        }

        public async Task ChiudiSessione(Guid currentUid)
        {
            var sessioneCorrente = await PRContext.Sessioni.FirstOrDefaultAsync(s => s.uidUtente.Equals(currentUid));
            if (sessioneCorrente != null)
            {
                sessioneCorrente.DataUscita = DateTime.Now;
                sessioneCorrente.ChiusuraCorretta = true;

                await PRContext.SaveChangesAsync();
            }
        }
    }
}