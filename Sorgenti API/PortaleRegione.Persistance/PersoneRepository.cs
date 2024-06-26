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
using Z.EntityFramework.Plus;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class PersoneRepository : Repository<View_UTENTI>, IPersoneRepository
    {
        public PersoneRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<View_UTENTI> Get(string login_windows)
        {
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();
            var result = await PRContext.View_UTENTI.SingleOrDefaultAsync(a => a.userAD == login_windows);
            return result;
        }

        public async Task<View_UTENTI> Get(Guid personaUId)
        {
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();
            var result = await PRContext.View_UTENTI.FindAsync(personaUId);
            return result;
        }

        public async Task<UTENTI_NoCons> Get_NoCons(Guid personaUId)
        {
            var result = await PRContext.UTENTI_NoCons.SingleOrDefaultAsync(p => p.UID_persona == personaUId);
            return result;
        }

        public async Task<View_UTENTI> Get(int personaId)
        {
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();
            var result = await PRContext.View_UTENTI.SingleOrDefaultAsync(p => p.id_persona == personaId);
            return result;
        }

        public async Task<IEnumerable<View_UTENTI>> GetAll(int page, int size, PersonaDto persona = null,
            Filter<View_UTENTI> filtro = null)
        {
            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty);

            if (persona != null)
            {
                if (persona.IsCapoGruppo || persona.IsResponsabileSegreteriaPolitica)
                {
                    query = query.Where(u => u.deleted == false);
                }
                else if (persona.IsGiunta)
                {
                    query = query.Where(u => u.No_Cons == 1
                                             && u.id_gruppo_politico_rif >= 10000
                                             && u.deleted == false);
                }
            }

            filtro?.BuildExpression(ref query);

            query = query.OrderBy(u => u.cognome)
                .ThenBy(u => u.nome);

            var result = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            if (filtro._statements.Any(i => i.PropertyId == nameof(PersonaDto.nome)))
            {
                var q_nome = filtro._statements.First(i => i.PropertyId == nameof(PersonaDto.nome)).Value.ToString()
                    .ToLower();
                result = result.Where(i => i.nome.ToLower().Contains(q_nome)
                                           || i.cognome.ToLower().Contains(q_nome)
                                           || i.userAD.ToLower().Contains(q_nome))
                    .ToList();
            }

            return result;
        }

        public async Task<IEnumerable<View_UTENTI>> GetAll(int page, int size, PersonaDto persona = null,
            Filter<View_UTENTI> filtro = null, List<string> userAd = null)
        {
            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty);

            if (persona != null)
            {
                if (persona.IsCapoGruppo || persona.IsResponsabileSegreteriaPolitica)
                {
                    query = query.Where(u => u.deleted == false);
                }
                else if (persona.IsGiunta)
                {
                    query = query.Where(u => u.No_Cons == 1
                                             && u.id_gruppo_politico_rif >= 10000
                                             && u.deleted == false);
                }
            }

            filtro?.BuildExpression(ref query);

            query = query.OrderBy(u => u.cognome)
                .ThenBy(u => u.nome);

            if (userAd != null)
            {
                if (userAd.Any())
                {
                    query = query.Where(u => userAd.Contains(u.userAD));
                }
            }

            var result = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            if (filtro._statements.Any(i => i.PropertyId == nameof(PersonaDto.nome)))
            {
                var q_nome = filtro._statements.First(i => i.PropertyId == nameof(PersonaDto.nome)).Value.ToString()
                    .ToLower();
                result = result.Where(i => i.nome.ToLower().Contains(q_nome)
                                           || i.cognome.ToLower().Contains(q_nome)
                                           || i.userAD.ToLower().Contains(q_nome))
                    .ToList();
            }

            return result;
        }

        public async Task<IEnumerable<View_UTENTI>> GetAllByGiunta(int page, int size,
            Filter<View_UTENTI> filtro = null)
        {
            var gruppi_giunta = await PRContext
                .JOIN_GRUPPO_AD
                .Where(g => g.GiuntaRegionale)
                .Select(g => g.id_gruppo)
                .ToListAsync();
            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty
                            && u.No_Cons == 1
                            && gruppi_giunta.Contains(u.id_gruppo_politico_rif.Value));

            filtro?.BuildExpression(ref query);

            query = query.OrderBy(u => u.cognome)
                .ThenBy(u => u.nome);

            return await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> CountAll(PersonaDto persona = null, Filter<View_UTENTI> filtro = null)
        {
            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty);
            if (persona != null)
            {
                if (persona.IsCapoGruppo || persona.IsResponsabileSegreteriaPolitica)
                {
                    query = query.Where(u => u.deleted == false);
                }
                else if (persona.IsGiunta)
                {
                    query = query.Where(u => u.No_Cons == 1 && u.id_gruppo_politico_rif >= 10000);
                }
            }

            filtro?.BuildExpression(ref query);

            var result = await query
                .ToListAsync();

            if (filtro._statements.Any(i => i.PropertyId == nameof(PersonaDto.nome)))
            {
                var q_nome = filtro._statements.First(i => i.PropertyId == nameof(PersonaDto.nome)).Value.ToString()
                    .ToLower();
                result = result.Where(i => i.nome.ToLower().Contains(q_nome)
                                           || i.cognome.ToLower().Contains(q_nome)
                                           || i.userAD.ToLower().Contains(q_nome))
                    .ToList();
            }

            return result.Count;
        }

        public async Task<int> CountAllByGiunta(Filter<View_UTENTI> filtro)
        {
            var gruppi_giunta = await PRContext
                .JOIN_GRUPPO_AD
                .Where(g => g.GiuntaRegionale)
                .Select(g => g.id_gruppo)
                .ToListAsync();
            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty
                            && u.No_Cons == 1
                            && gruppi_giunta.Contains(u.id_gruppo_politico_rif.Value));

            filtro?.BuildExpression(ref query);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<View_UTENTI>> GetAll()
        {
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();

            var query = PRContext
                .View_UTENTI
                .Where(u => u.UID_persona != Guid.Empty)
                .OrderBy(u => u.id_gruppo_politico_rif)
                .ThenBy(u => u.cognome)
                .ThenBy(u => u.nome);

            return await query
                .ToListAsync();
        }

        public async Task<IEnumerable<View_UTENTI>> GetAssessoriRiferimento(int id_legislatura)
        {
            PRContext.View_assessori_in_carica.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();

            var assessori = await PRContext
                .View_assessori_in_carica
                .ToListAsync();
            var result = new List<View_UTENTI>();
            foreach (var assessoriInCarica in assessori.Distinct())
            {
                try
                {
                    var persona = await PRContext.View_UTENTI.FindAsync(assessoriInCarica.UID_persona);
                    result.Add(persona);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }

        public async Task<bool> IsRelatore(Guid personaUId, Guid attoUId)
        {
            return await PRContext
                .ATTI_RELATORI
                .AnyAsync(ar => ar.UIDAtto == attoUId && ar.UIDPersona == personaUId);
        }

        public async Task<bool> IsAssessore(Guid personaUId, Guid attoUId)
        {
            return await PRContext
                .ATTI
                .AnyAsync(a => a.UIDAtto == attoUId && a.UIDAssessoreRiferimento == personaUId);
        }

        public async Task<string> GetCarica(Guid personaUId)
        {
            var query = PRContext
                .Database
                .SqlQuery<string>($"SELECT dbo.get_CaricaGiunta_from_UIDpersona('{personaUId}')");
            return await query.FirstOrDefaultAsync();
        }

        public async Task<View_PINS> GetPin(Guid personaUId)
        {
            return await PRContext
                .View_PINS
                .SingleOrDefaultAsync(p => p.UID_persona == personaUId
                                           && !p.Al.HasValue);
        }

        public async Task<IEnumerable<View_UTENTI>> GetConsiglieri(int id_legislatura)
        {
            PRContext.View_consiglieri_in_carica.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();

            var consiglieri = await PRContext
                .View_consiglieri_in_carica
                .ToListAsync();
            var result = new List<View_UTENTI>();
            foreach (var consigliere in consiglieri.Distinct())
            {
                var persona = await PRContext.View_UTENTI.FindAsync(consigliere.UID_persona);
                result.Add(persona);
            }

            return result;
        }

        public async Task SavePin(Guid personaUId, string nuovo_pin, bool reset)
        {
            var persona = await Get(personaUId);
            var no_cons = Convert.ToBoolean(persona.No_Cons);
            var table = no_cons ? "PINS_NoCons" : "PINS";
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"UPDATE {table} SET Al=GETDATE() WHERE Al IS NULL AND UID_persona='{personaUId}'");
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"INSERT INTO {table} (UID_persona,PIN,RichiediModificaPIN) VALUES ('{personaUId}','{nuovo_pin}',{(reset ? 1 : 0)})");
        }

        public async Task DeletePersona(int id)
        {
            var user = await PRContext
                .UTENTI_NoCons
                .FirstOrDefaultAsync(u => u.id_persona == id);
            if (user != null)
            {
                user.deleted = true;
            }
        }

        public async Task<IEnumerable<View_Composizione_GiuntaRegionale>> GetGiuntaRegionale()
        {
            var query = PRContext.View_Composizione_GiuntaRegionale;
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<UTENTI_NoCons>> GetSegreteriaGiuntaRegionale(bool notifica_firma,
            bool notifica_deposito)
        {
            var query = PRContext
                .UTENTI_NoCons
                .SqlQuery(
                    $"Select * from UTENTI_NoCons where id_gruppo_politico_rif>=10000 AND notifica_firma={Convert.ToInt16(notifica_firma)} AND notifica_deposito={Convert.ToInt16(notifica_deposito)}");

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<View_UTENTI>> GetRelatori(Guid? attoUId)
        {
            if (attoUId == Guid.Empty)
            {
                var query = PRContext
                    .View_UTENTI
                    .Where(u =>
                        u.legislatura_attuale == true
                        && u.No_Cons == 0
                        && u.attivo == true);

                return await query.ToListAsync();
            }
            else
            {
                var query = PRContext
                    .ATTI_RELATORI
                    .Where(a => a.UIDAtto == attoUId)
                    .Join(
                        PRContext.View_UTENTI,
                        a => a.UIDPersona,
                        p => p.UID_persona,
                        (a, p) => p);

                return await query.ToListAsync();
            }
        }

        public async Task UpdateUtente_NoCons(Guid uid_persona, int id_persona, string userAd)
        {
            try
            {
                var effrow =
                    await PRContext.Database.ExecuteSqlCommandAsync(
                        $"UPDATE join_persona_AD set userAD='{userAd}' where UID_persona='{uid_persona}'");
                if (effrow < 1)
                {
                    await PRContext.Database.ExecuteSqlCommandAsync(
                        $"INSERT INTO join_persona_AD(userAD,UID_persona,id_persona) values ('{userAd}','{uid_persona}', {id_persona})");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Add(UTENTI_NoCons newUser)
        {
            PRContext.UTENTI_NoCons.Add(newUser);
        }

        public async Task<bool> Autentica(string username, string password)
        {
            var user = await PRContext
                .View_UTENTI
                .Where(item => item.userAD.EndsWith(username)
                               && item.pass_locale_crypt == password)
                .FirstOrDefaultAsync();
            return user != null;
        }

        public async Task<List<View_consiglieri>> GetProponentiFirmatari(string legislaturaId)
        {
            if (int.TryParse(legislaturaId, out int idLegislatura))
            {
                return await PRContext
                    .View_consiglieri
                    .Where(c => c.id_legislatura == idLegislatura)
                    .ToListAsync();
            }

            throw new ArgumentException($"L'ID [{legislaturaId}] della legislatura fornito non è valido.");
        }
    }
}