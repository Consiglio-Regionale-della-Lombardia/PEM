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
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Common;
using ExpressionBuilder.Interfaces;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class EmendamentiRepository : Repository<EM>, IEmendamentiRepository
    {
        public EmendamentiRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        /// <summary>
        ///     Conteggio emendamenti nell'atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public async Task<int> Count(Guid attoUId, PersonaDto persona, CounterEmendamentiEnum counter_emendamenti,
            int CLIENT_MODE,
            Filter<EM> filtro = null, List<Guid> firmatari = null, List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == attoUId && !em.Eliminato);
            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
            {
                query = query.Where(em => em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
            }
            else
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                          || em.IDStato == (int)StatiEnum.Bozza_Riservata
                                          && (em.UIDPersonaCreazione == persona.UID_persona
                                              || em.UIDPersonaProponente == persona.UID_persona));

                if (persona.IsGiunta)
                {
                    query = query
                        .Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                }
                else if (!persona.IsSegreteriaAssemblea
                         && !persona.IsPresidente)
                {
                    query = query
                        .Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);
                }

                if (persona.IsSegreteriaAssemblea)
                {
                    query = query.Where(em =>
                        !string.IsNullOrEmpty(em.DataDeposito) ||
                        em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);
                }
            }

            filtro?.BuildExpression(ref query);

            if (firmatari != null)
            {
                if (firmatari.Count > 0)
                {
                    //Avvio ricerca firmatari
                    var firme = await PRContext
                        .FIRME
                        .Where(f => firmatari.Contains(f.UID_persona))
                        .Select(f => f.UIDEM)
                        .ToListAsync();
                    query = query
                        .Where(em => firme.Contains(em.UIDEM));
                }
            }

            if (proponenti != null)
            {
                if (proponenti.Count > 0)
                {
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(em => proponenti.Contains(em.UIDPersonaProponente));
                }
            }

            if (gruppi != null)
            {
                if (gruppi.Count > 0)
                {
                    //Avvio ricerca gruppi;
                    query = query
                        .Where(em => gruppi.Contains(em.id_gruppo));
                }
            }
            
            if (stati != null)
            {
                if (stati.Count > 0)
                {
                    //Avvio ricerca stati;
                    query = query
                        .Where(em => stati.Contains(em.IDStato));
                }
            }

            switch (counter_emendamenti)
            {
                case CounterEmendamentiEnum.NONE:
                    {
                        return await query.CountAsync();
                    }
                case CounterEmendamentiEnum.EM:
                    if (persona.IsSegreteriaAssemblea)
                    {
                        return await query.CountAsync(e =>
                            !string.IsNullOrEmpty(e.N_EM) && string.IsNullOrEmpty(e.N_SUBEM));
                    }
                    else
                    {
                        return await query.CountAsync(e => string.IsNullOrEmpty(e.N_SUBEM));
                    }

                case CounterEmendamentiEnum.SUB_EM:
                    if (persona.IsSegreteriaAssemblea)
                    {
                        return await query.CountAsync(e => string.IsNullOrEmpty(e.N_EM));
                    }
                    else
                    {
                        return await query.CountAsync(e =>
                            string.IsNullOrEmpty(e.N_EM) && !string.IsNullOrEmpty(e.N_SUBEM));
                    }

                default:
                    return 0;
            }
        }

        public async Task<int> Count(string query)
        {
            return await PRContext
                .EM
                .SqlQuery(query)
                .CountAsync();
        }

        public async Task<EM> GetEMInProiezione(Guid emUidAtto, int ordine)
        {
            if (ordine < 0)
            {
                return null;
            }

            var result = await PRContext
                .EM
                .Include(em => em.ATTI)
                .Include(em => em.PARTI_TESTO)
                .Include(em => em.TIPI_EM)
                .Include(em => em.ARTICOLI)
                .Include(em => em.COMMI)
                .Include(em => em.LETTERE)
                .Include(em => em.EM2)
                .Include(em => em.STATI_EM)
                .Where(em =>
                    em.UIDAtto == emUidAtto &&
                    em.IDStato >= (int)StatiEnum.Depositato)
                .ToListAsync();

            if (result.Any(em => em.OrdineVotazione == ordine))
            {
                return result.FirstOrDefault(em => em.OrdineVotazione == ordine);
            }

            return null;
        }

        public async Task<IEnumerable<EM>> GetAll_RichiestaPropriaFirma(Guid id, PersonaDto persona,
            OrdinamentoEnum ordine, int page, int size, int mode)
        {
            var emendamenti_da_firmare = new List<Guid>();

            var notifiche = await PRContext
                .NOTIFICHE
                .Where(n => n.UIDAtto == id && !n.Chiuso && n.EM.IDStato <= (int)StatiEnum.Depositato)
                .Select(n => n.UIDNotifica)
                .ToListAsync();
            if (notifiche.Any())
            {
                var my_em_notifiche = await PRContext
                    .NOTIFICHE_DESTINATARI
                    .Include(n => n.NOTIFICHE)
                    .Where(n => n.UIDPersona == persona.UID_persona && !n.Chiuso && notifiche.Contains(n.UIDNotifica))
                    .ToListAsync();

                foreach (var emUid in my_em_notifiche.Select(n => n.NOTIFICHE.UIDEM))
                {
                    if (emUid == null) continue;
                    var check_firmato = await PRContext.FIRME.AnyAsync(f =>
                        f.UIDEM == emUid
                        && f.UID_persona == persona.UID_persona
                        && string.IsNullOrEmpty(f.Data_ritirofirma));
                    if (!check_firmato)
                    {
                        emendamenti_da_firmare.Add(emUid.Value);
                    }
                }
            }

            var my_em_proponente = await PRContext
                .EM
                .Where(em => em.UIDPersonaProponente == persona.UID_persona
                             && !em.UIDPersonaPrimaFirma.HasValue
                             && em.UIDAtto == id
                             && em.IDStato <= (int)StatiEnum.Bozza
                             && !em.Eliminato)
                .Select(em => em.UIDEM)
                .ToListAsync();
            foreach (var emUid in my_em_proponente)
            {
                emendamenti_da_firmare.Add(emUid);
            }

            var result = await GetEmendamentiByArray(emendamenti_da_firmare);

            return result
                .Skip((page - 1) * size)
                .Take(size);
        }

        public async Task<EM> GetCurrentEMInProiezione(Guid attoUId)
        {
            var result = await PRContext
                .EM
                .Where(e => e.UIDAtto == attoUId && e.Proietta)
                .Include(em => em.ATTI)
                .Include(em => em.PARTI_TESTO)
                .Include(em => em.TIPI_EM)
                .Include(em => em.ARTICOLI)
                .Include(em => em.COMMI)
                .Include(em => em.LETTERE)
                .Include(em => em.EM2)
                .Include(em => em.STATI_EM)
                .ToListAsync();
            if (!result.Any())
            {
                try
                {
                    var allEM = await GetAll(null, OrdinamentoEnum.Votazione, 1, 1,
                        (int)ClientModeEnum.TRATTAZIONE);
                    if (allEM.Any())
                    {
                        var em = allEM.First();
                        var emdto = await Get(em, false);
                        emdto.Proietta = true;
                        await Context.SaveChangesAsync();
                        return emdto;
                    }
                }
                catch
                {
                }
            }

            return result.FirstOrDefault();
        }

        public async Task<EM> GetByQR(Guid id)
        {
            return await PRContext
                .EM
                .FirstOrDefaultAsync(em => em.UID_QRCode == id);
        }

        /// <summary>
        ///     Riepilogo emendamenti
        /// </summary>
        /// <param name="persona"></param>
        /// <param name="ordine"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="CLIENT_MODE"></param>
        /// <param name="filtro"></param>
        /// <param name="firmatari"></param>
        /// <param name="proponenti"></param>
        /// <param name="gruppi"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Guid>> GetAll(PersonaDto persona, OrdinamentoEnum ordine, int? page,
            int? size, int CLIENT_MODE, Filter<EM> filtro = null, List<Guid> firmatari = null, List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null)
        {
            var query = PRContext
                .EM
                .Where(em => !em.Eliminato);

            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
            {
                var filter_value = filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(AttiDto.UIDAtto)).Value;
                var uidAtto = new Guid(filter_value.ToString());
                var atto = await PRContext
                    .ATTI
                    .SingleAsync(a => a.UIDAtto == uidAtto);
                if (atto.OrdinePresentazione == false && ordine == OrdinamentoEnum.Presentazione)
                {
                    return new List<Guid>();
                }

                if (atto.OrdineVotazione == false && ordine == OrdinamentoEnum.Votazione)
                {
                    return new List<Guid>();
                }

                query = query.Where(em => em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
            }
            else
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                          || em.IDStato == (int)StatiEnum.Bozza_Riservata
                                          && (em.UIDPersonaCreazione == persona.UID_persona
                                              || em.UIDPersonaProponente == persona.UID_persona
                                              || persona.IsCapoGruppo));

                if (persona.IsGiunta)
                {
                    query = query
                        .Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                }
                else if (!persona.IsSegreteriaAssemblea
                         && !persona.IsPresidente)
                {
                    query = query
                        .Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);
                }

                if (persona.IsSegreteriaAssemblea)
                {
                    query = query.Where(em =>
                        !string.IsNullOrEmpty(em.DataDeposito) ||
                        em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);
                }
            }
            
            filtro?.BuildExpression(ref query);

            if (firmatari != null)
            {
                if (firmatari.Count > 0)
                {
                    //Avvio ricerca firmatari
                    var firme = await PRContext
                        .FIRME
                        .Where(f => firmatari.Contains(f.UID_persona))
                        .Select(f => f.UIDEM)
                        .ToListAsync();
                    query = query
                        .Where(em => firme.Contains(em.UIDEM));
                }
            }
            
            if (proponenti != null)
            {
                if (proponenti.Count > 0)
                {
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(em => proponenti.Contains(em.UIDPersonaProponente));
                }
            }
            
            if (gruppi != null)
            {
                if (gruppi.Count > 0)
                {
                    //Avvio ricerca gruppi;
                    query = query
                        .Where(em => gruppi.Contains(em.id_gruppo));
                }
            }

            if (stati != null)
            {
                if (stati.Count > 0)
                {
                    //Avvio ricerca stati;
                    query = query
                        .Where(em => stati.Contains(em.IDStato));
                }
            }

            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE ||
                (persona.IsSegreteriaAssemblea
                 || persona.IsPresidente))
            {
                switch (ordine)
                {
                    case OrdinamentoEnum.Presentazione:
                        query = query.OrderBy(em => em.OrdinePresentazione);
                        break;
                    case OrdinamentoEnum.Votazione:
                        query = query.OrderBy(em => em.OrdineVotazione);
                        break;
                    default:
                        query = query.OrderBy(em => em.IDStato).ThenByDescending(em => em.DataCreazione);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(em => em.IDStato).ThenBy(em => em.Timestamp).ThenBy(em => em.Progressivo).ThenBy(em => em.SubProgressivo);
            }
            
            return await query
                .Select(em => em.UIDEM)
                .Skip((page.Value - 1) * size.Value)
                .Take(size.Value)
                .ToListAsync();
        }

        /// <summary>
        ///     Esegue query emendamenti per le stampe
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IEnumerable<EM> GetAll(ByQueryModel model)
        {
            var query = PRContext
                .EM
                .SqlQuery(model.Query)
                .Skip((model.page - 1) * 100)
                .Take(100);

            return query.ToList();
        }

        /// <summary>
        ///     Ritorna la query emednamenti da stampare
        /// </summary>
        /// <param name="filtro"></param>
        /// <param name="ordinamentoEnum"></param>
        /// <param name="attoUId"></param>
        /// <param name="persona"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        public async Task<string> GetAll_Query(Filter<EM> filtro, OrdinamentoEnum ordinamentoEnum, List<Guid> firmatari = null, List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null)
        {
            var query = PRContext
                .EM
                .Where(a => !a.Eliminato);
            filtro?.BuildExpression(ref query);

            if (firmatari != null)
            {
                if (firmatari.Count > 0)
                {
                    //Avvio ricerca firmatari
                    var firme = await PRContext
                        .FIRME
                        .Where(f => firmatari.Contains(f.UID_persona))
                        .Select(f => f.UIDEM)
                        .ToListAsync();
                    query = query
                        .Where(em => firme.Contains(em.UIDEM));
                }
            }

            if (proponenti != null)
            {
                if (proponenti.Count > 0)
                {
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(em => proponenti.Contains(em.UIDPersonaProponente));
                }
            }

            if (gruppi != null)
            {
                if (gruppi.Count > 0)
                {
                    //Avvio ricerca gruppi;
                    query = query
                        .Where(em => gruppi.Contains(em.id_gruppo));
                }
            }

            if (stati != null)
            {
                if (stati.Count > 0)
                {
                    //Avvio ricerca stati;
                    query = query
                        .Where(em => stati.Contains(em.IDStato));
                }
            }

            switch (ordinamentoEnum)
            {
                case OrdinamentoEnum.Presentazione:
                    query = query.OrderBy(em => em.OrdinePresentazione);
                    break;
                case OrdinamentoEnum.Votazione:
                    query = query.OrderBy(em => em.OrdineVotazione);
                    break;
                default:
                    query = query.OrderBy(em => em.IDStato).ThenByDescending(em => em.DataCreazione);
                    break;
            }

            var sql = query.ToTraceQuery();
            return sql;
        }

        public async Task<int> UltimoInVotazione(Guid uidAtto)
        {
            var result = await PRContext
                .EM
                .Where(em => em.UIDAtto == uidAtto)
                .OrderByDescending(em => em.OrdineVotazione)
                .Select(em => em.OrdineVotazione)
                .FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        ///     Singolo emendamento
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<EM> Get(Guid emendamentoUId, bool includes = true)
        {
            var query = PRContext.EM.AsQueryable();
            if (includes)
            {
                query = query.Include(em => em.ARTICOLI)
                    .Include(em => em.COMMI)
                    .Include(em => em.LETTERE)
                    .Include(em => em.PARTI_TESTO)
                    .Include(em => em.STATI_EM)
                    .Include(em => em.TIPI_EM);
            }

            var result = await query.SingleOrDefaultAsync(em => em.UIDEM == emendamentoUId);

            return result;
        }

        /// <summary>
        ///     Singolo emendamento
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<EM> Get(string emendamentoUId)
        {
            var guidId = new Guid(emendamentoUId);
            return await Get(guidId);
        }

        /// <summary>
        ///     Etichetta di deposito
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="sub"></param>
        /// <returns></returns>
        public async Task<int> GetEtichetta(Guid attoUId, bool sub)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == attoUId
                             && em.Eliminato == false);
            return sub
                ? await query.CountAsync(e => string.IsNullOrEmpty(e.N_EM) && !string.IsNullOrEmpty(e.N_SUBEM))
                : await query.CountAsync(e => !string.IsNullOrEmpty(e.N_EM) && string.IsNullOrEmpty(e.N_SUBEM));
        }

        /// <summary>
        ///     Riepilogo inviti
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetInvitati(Guid emendamentoUId)
        {
            try
            {
                var query = PRContext
                    .NOTIFICHE
                    .Where(n => n.UIDEM == emendamentoUId)
                    .Join(PRContext.NOTIFICHE_DESTINATARI,
                        n => n.UIDNotifica,
                        nd => nd.UIDNotifica,
                        (n, nd) => nd);

                var result = await query.ToListAsync();
                if (result.Any())
                {
                    var notificaUId = result.First().UIDNotifica;
                    var notifica = await PRContext
                        .NOTIFICHE
                        .SingleAsync(n => n.UIDNotifica == notificaUId);
                    var new_result = new List<NOTIFICHE_DESTINATARI>();
                    foreach (var destinatario in result)
                    {
                        destinatario.NOTIFICHE = notifica;
                        new_result.Add(destinatario);
                    }

                    return new_result;
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Progressivo per atto e gruppo
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="gruppo"></param>
        /// <param name="sub"></param>
        /// <returns></returns>
        public async Task<int> GetProgressivo(Guid attoUId, int gruppo, bool sub)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == attoUId
                             && em.id_gruppo == gruppo
                             && em.Eliminato == false);
            if (sub)
            {
                query = query.OrderByDescending(em => em.SubProgressivo)
                    .Take(1);
            }
            else
            {
                query = query.OrderByDescending(em => em.Progressivo)
                    .Take(1);
            }

            var list = await query.ToListAsync();
            if (list.Count == 0)
            {
                return 1;
            }

            if (sub)
            {
                if (list[0].SubProgressivo.HasValue)
                {
                    return list[0].SubProgressivo.Value + 1;
                }
            }
            else
            {
                if (list[0].Progressivo.HasValue)
                {
                    return list[0].Progressivo.Value + 1;
                }
            }

            return 1;
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PARTI_TESTO>> GetPartiEmendabili()
        {
            PRContext.PARTI_TESTO.FromCache(DateTimeOffset.Now.AddDays(100)).ToList();

            return await PRContext.PARTI_TESTO.ToListAsync();
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TIPI_EM>> GetTipiEmendamento()
        {
            PRContext.TIPI_EM.FromCache(DateTimeOffset.Now.AddDays(100)).ToList();

            return await PRContext.TIPI_EM.ToListAsync();
        }

        public async Task<List<TAGS>> GetTags()
        {
            return await PRContext.TAGS.ToListAsync();
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MISSIONI>> GetMissioniEmendamento()
        {
            return await PRContext.MISSIONI
                .Where(m => DateTime.Now > m.DAL && DateTime.Now <= m.AL || !m.AL.HasValue).OrderBy(m => m.Ordine)
                .ToListAsync();
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TITOLI_MISSIONI>> GetTitoliMissioneEmendamento()
        {
            return await PRContext.TITOLI_MISSIONI.ToListAsync();
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili per gli stati in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<STATI_EM>> GetStatiEmendamento()
        {
            PRContext.STATI_EM.FromCache(DateTimeOffset.Now.AddDays(100)).ToList();

            return await PRContext
                .STATI_EM
                .ToListAsync();
        }

        /// <summary>
        ///     Controlla che l'emendamento sia eliminabile
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public bool CheckIfEliminabile(EmendamentiDto em, PersonaDto persona)
        {
            if (persona.Gruppo == null)
            {
                return false;
            }

            if (em.id_gruppo != persona.Gruppo.id_gruppo)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(em.DataDeposito))
            {
                return false;
            }

            return persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                   || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                   || persona.UID_persona == em.UIDPersonaCreazione;
        }

        /// <summary>
        ///     Controlla che l'emendamento sia ritirabile
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public bool CheckIfRitirabile(EmendamentiDto em, PersonaDto persona)
        {
            if (persona.Gruppo == null)
            {
                return false;
            }

            if (em.id_gruppo != persona.Gruppo.id_gruppo)
            {
                return false;
            }

            if (em.DataRitiro.HasValue)
            {
                return false;
            }

            if (em.IDStato != (int)StatiEnum.Depositato)
            {
                return false;
            }

            return persona.UID_persona == em.UIDPersonaProponente
                   || em.ATTI.UIDAssessoreRiferimento == persona.UID_persona
                   || persona.CurrentRole == RuoliIntEnum.Presidente_Regione
                   && em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID;
        }

        /// <summary>
        ///     Controlla che l'emendamento sia depositabile
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public bool CheckIfDepositabile(EmendamentiDto em, PersonaDto persona)
        {
            if (!string.IsNullOrEmpty(em.DataDeposito))
            {
                return false;
            }

            if (persona.IsSegreteriaAssemblea)
            {
                if (em.Firma_da_ufficio)
                {
                    return true;
                }
            }

            // Se proponente non ha firmato non è possibile depositare
            if (!em.Firmato_Dal_Proponente)
            {
                return false;
            }

            switch (persona.CurrentRole)
            {
                case RuoliIntEnum.Consigliere_Regionale:
                case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                case RuoliIntEnum.Presidente_Regione:
                    return em.UIDPersonaProponente == persona.UID_persona;
                case RuoliIntEnum.Amministratore_PEM:
                case RuoliIntEnum.Segreteria_Assemblea:
                    return true;
            }

            if (persona.Gruppo != null)
            {
                return em.id_gruppo == persona.Gruppo.id_gruppo;
            }

            return false;
        }

        /// <summary>
        ///     Controlla che l'emendamento sia modificabile dall'utente
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public bool CheckIfModificabile(EmendamentiDto em, PersonaDto persona)
        {
            if (string.IsNullOrEmpty(em.EM_Certificato))
            {
                return em.UIDPersonaProponente == persona.UID_persona
                       || em.UIDPersonaCreazione == persona.UID_persona
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta;
            }

            return (em.UIDPersonaProponente == persona.UID_persona || em.UIDPersonaCreazione == persona.UID_persona)
                   && (em.IDStato == (int)StatiEnum.Bozza || em.IDStato == (int)StatiEnum.Bozza_Riservata)
                   && em.ConteggioFirme == 1;
        }

        public async Task<int> GetOrdinePresentazione(Guid uidAtto)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == uidAtto
                             && em.IDStato >= (int)StatiEnum.Depositato
                             && em.Eliminato == false);
            return await query.CountAsync();
        }

        /// <summary>
        ///     Controlla che il progressivo sia unico all'interno dell'atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="encrypt_progressivo"></param>
        /// <param name="counter_emendamenti"></param>
        /// <returns></returns>
        public async Task<bool> CheckProgressivo(Guid attoUId, string encrypt_progressivo,
            CounterEmendamentiEnum counter_emendamenti)
        {
            var query = PRContext
                .EM
                .Where(e => true);

            switch (counter_emendamenti)
            {
                case CounterEmendamentiEnum.NONE:
                    return false;
                case CounterEmendamentiEnum.EM:
                    return !await query.AnyAsync(e => e.UIDAtto == attoUId && e.N_EM == encrypt_progressivo);
                case CounterEmendamentiEnum.SUB_EM:
                    return !await query.AnyAsync(e => e.UIDAtto == attoUId && e.N_SUBEM == encrypt_progressivo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(counter_emendamenti), counter_emendamenti, null);
            }
        }

        public async Task ORDINA_EM_TRATTAZIONE(Guid attoUId)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec ORDINA_EM_TRATTAZIONE @UIDAtto='{attoUId}'");
        }

        public async Task UP_EM_TRATTAZIONE(Guid emendamentoUId)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec UP_EM_TRATTAZIONE @UIDEM='{emendamentoUId}'");
        }

        public async Task DOWN_EM_TRATTAZIONE(Guid emendamentoUId)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec DOWN_EM_TRATTAZIONE @UIDEM='{emendamentoUId}'");
        }

        public async Task SPOSTA_EM_TRATTAZIONE(Guid emendamentoUId, int pos)
        {
            await PRContext.Database.ExecuteSqlCommandAsync(
                $"exec SPOSTA_EM_TRATTAZIONE @UIDEM='{emendamentoUId}',@Pos={pos}");
        }

        private async Task<IEnumerable<EM>> GetEmendamentiByArray(List<Guid> listaEmendamenti)
        {
            return await PRContext
                .EM
                .Include(em => em.ATTI)
                .Include(em => em.PARTI_TESTO)
                .Include(em => em.TIPI_EM)
                .Include(em => em.ARTICOLI)
                .Include(em => em.COMMI)
                .Include(em => em.LETTERE)
                .Include(em => em.EM2)
                .Include(em => em.STATI_EM)
                .Where(em => listaEmendamenti.Contains(em.UIDEM))
                .ToListAsync();
        }
    }
}
