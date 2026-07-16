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
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Crypto;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;

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
        ///     Conteggio emendamenti, firma legacy con liste tipizzate. Mantenuta per i flussi
        ///     non migrati; i nuovi chiamanti devono usare l'overload basato su <see cref="QueryExtendedRequestEM" />.
        /// </summary>
        [Obsolete("Usa l'overload basato su QueryExtendedRequestEM.")]
        public async Task<int> Count(Guid attoUId, PersonaDto persona, CounterEmendamentiEnum counter_emendamenti,
            int CLIENT_MODE,
            Filter<EM> filtro = null, List<Guid> firmatari = null, List<Guid> proponenti = null,
            List<int> gruppi = null, List<int> stati = null, List<TagDto> tagDtos = null)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == attoUId && !em.Eliminato);
            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
            {
                query = query.Where(em =>
                    em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
            }
            else
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                          || (em.IDStato == (int)StatiEnum.Bozza_Riservata
                                              && (em.UIDPersonaCreazione == persona.UID_persona
                                                  || em.UIDPersonaProponente == persona.UID_persona)));

                if (persona.IsGiunta)
                    query = query
                        .Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                else if (!persona.IsSegreteriaAssemblea_Vista
                         && !persona.IsPresidente)
                    query = query
                        .Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);

                if (persona.IsSoloSegreteriaAssemblea)
                    query = query.Where(em =>
                        !string.IsNullOrEmpty(em.DataDeposito) ||
                        em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);
            }

            // #956
            if (filtro != null)
            {
                if (filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(EM.N_EM)) != null)
                {
                    var filter_n_em_value = filtro.Statements.First(item => item.PropertyId == nameof(EM.N_EM))
                        .Value.ToString();

                    var tokens = filter_n_em_value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim());

                    var allNumbers = new List<int>();

                    foreach (var tok in tokens)
                    {
                        if (tok.Contains("-"))
                        {
                            // Gestione del range (es. "3-5" o "5-2")
                            var bounds = tok.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim()).ToArray();
                            if (bounds.Length == 2 && int.TryParse(bounds[0], out var a) &&
                                int.TryParse(bounds[1], out var b))
                            {
                                var start = Math.Min(a, b);
                                var end = Math.Max(a, b);

                                for (var i = start; i <= end; i++)
                                {
                                    allNumbers.Add(i);
                                }
                            }
                        }
                        else if (int.TryParse(tok, out var num))
                        {
                            allNumbers.Add(num);
                        }
                    }

                    if (allNumbers.Any())
                    {
                        // Rimuoviamo i duplicati
                        allNumbers = allNumbers.Distinct().ToList();

                        // Costruiamo la condizione OR per tutti i numeri
                        Expression<Func<EM, bool>> combinedPredicate = null;

                        foreach (var numero in allNumbers)
                        {
                            var encryt_nem = CryptoHelper.EncryptString(numero.ToString(),
                                AppSettingsConfiguration.masterKey);

                            // Condizione per questo numero specifico
                            Expression<Func<EM, bool>> singleCondition = e =>
                                (!e.Timestamp.HasValue && e.Progressivo == numero) || e.N_EM == encryt_nem;

                            // Combiniamo con OR
                            combinedPredicate = combinedPredicate == null
                                ? singleCondition
                                : ExpressionExtensions.CombineExpressions(combinedPredicate, singleCondition);
                        }

                        if (combinedPredicate != null)
                            query = query.Where(combinedPredicate);
                    }

                    var n_em_request =
                        filtro._statements.First(statement => statement.PropertyId == nameof(EM.N_EM));
                    filtro._statements.Remove(n_em_request);
                }
                if (filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(EM.N_SUBEM)) != null)
                {
                    var filter_n_subem_value = filtro.Statements.First(item => item.PropertyId == nameof(EM.N_SUBEM))
                        .Value.ToString();

                    var tokens = filter_n_subem_value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim());

                    var allNumbers = new List<int>();

                    foreach (var tok in tokens)
                    {
                        if (tok.Contains("-"))
                        {
                            // Gestione del range (es. "3-5" o "5-2")
                            var bounds = tok.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim()).ToArray();
                            if (bounds.Length == 2 && int.TryParse(bounds[0], out var a) &&
                                int.TryParse(bounds[1], out var b))
                            {
                                var start = Math.Min(a, b);
                                var end = Math.Max(a, b);

                                for (var i = start; i <= end; i++)
                                {
                                    allNumbers.Add(i);
                                }
                            }
                        }
                        else if (int.TryParse(tok, out var num))
                        {
                            allNumbers.Add(num);
                        }
                    }

                    if (allNumbers.Any())
                    {
                        // Rimuoviamo i duplicati
                        allNumbers = allNumbers.Distinct().ToList();

                        // Costruiamo la condizione OR per tutti i numeri
                        Expression<Func<EM, bool>> combinedPredicate = null;

                        foreach (var numero in allNumbers)
                        {
                            var encryt_nem = CryptoHelper.EncryptString(numero.ToString(),
                                AppSettingsConfiguration.masterKey);

                            // Condizione per questo numero specifico
                            Expression<Func<EM, bool>> singleCondition = e =>
                                (!e.Timestamp.HasValue && e.SubProgressivo == numero) || e.N_SUBEM == encryt_nem;

                            // Combiniamo con OR
                            combinedPredicate = combinedPredicate == null
                                ? singleCondition
                                : ExpressionExtensions.CombineExpressions(combinedPredicate, singleCondition);
                        }

                        if (combinedPredicate != null)
                            query = query.Where(combinedPredicate);
                    }

                    var n_subem_request =
                        filtro._statements.First(statement => statement.PropertyId == nameof(EM.N_SUBEM));
                    filtro._statements.Remove(n_subem_request);
                }
            }

            filtro?.BuildExpression(ref query);

            if (firmatari != null)
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

            if (tagDtos != null)
                if (tagDtos.Count > 0)
                {
                    //Avvio ricerca tags;
                    var tag_em = new List<Guid>();
                    foreach (var t in tagDtos)
                    {
                        var arr = await PRContext.EM
                            .Where(em => em.Tags.Contains(t.tag)).Select(em => em.UIDEM)
                            .ToListAsync();
                        tag_em.AddRange(arr.Where(item => !tag_em.Contains(item)));
                    }

                    query = query.Where(em => tag_em.Contains(em.UIDEM));
                }

            if (proponenti != null)
                if (proponenti.Count > 0)
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(em => proponenti.Contains(em.UIDPersonaProponente));

            if (gruppi != null)
                if (gruppi.Count > 0)
                    //Avvio ricerca gruppi;
                    query = query
                        .Where(em => gruppi.Contains(em.id_gruppo));

            if (stati != null)
                if (stati.Count > 0)
                    //Avvio ricerca stati;
                    query = query
                        .Where(em => stati.Contains(em.IDStato));

            switch (counter_emendamenti)
            {
                case CounterEmendamentiEnum.NONE:
                {
                    return await query.CountAsync();
                }
                case CounterEmendamentiEnum.EM:
                    if (persona.IsSegreteriaAssemblea_Vista)
                        return await query.CountAsync(e =>
                            !string.IsNullOrEmpty(e.N_EM) && string.IsNullOrEmpty(e.N_SUBEM));
                    return await query.CountAsync(e => string.IsNullOrEmpty(e.N_SUBEM));

                case CounterEmendamentiEnum.SUB_EM:
                    if (persona.IsSegreteriaAssemblea_Vista)
                        return await query.CountAsync(e =>
                            string.IsNullOrEmpty(e.N_EM) && !string.IsNullOrEmpty(e.N_SUBEM));
                    return await query.CountAsync(e => !string.IsNullOrEmpty(e.N_SUBEM));

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
            if (ordine < 0) return null;

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
                return result.FirstOrDefault(em => em.OrdineVotazione == ordine);

            return null;
        }

        public async Task<IEnumerable<EM>> GetAll_RichiestaPropriaFirma(Guid id, PersonaDto persona,
            OrdinamentoEnum ordine, int page, int size, int mode)
        {
            var allowStates = new List<int>
            {
                (int)StatiEnum.Bozza,
                (int)StatiEnum.Bozza_Riservata,
                (int)StatiEnum.Depositato
            };
            var emendamenti_da_firmare = new List<Guid>();

            var notifiche = await PRContext
                .NOTIFICHE
                .Where(n => n.UIDAtto == id && !n.Chiuso && allowStates.Contains(n.EM.IDStato))
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
                    if (!check_firmato) emendamenti_da_firmare.Add(emUid.Value);
                }
            }

            var my_em_proponente = await PRContext
                .EM
                .Where(em => em.UIDPersonaProponente == persona.UID_persona
                             && !em.UIDPersonaPrimaFirma.HasValue
                             && em.UIDAtto == id
                             && allowStates.Contains(em.IDStato)
                             && !em.Eliminato)
                .Select(em => em.UIDEM)
                .ToListAsync();
            foreach (var emUid in my_em_proponente) emendamenti_da_firmare.Add(emUid);

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

            return result.FirstOrDefault();
        }

        public async Task<EM> GetByQR(Guid id)
        {
            return await PRContext
                .EM
                .FirstOrDefaultAsync(em => em.UID_QRCode == id);
        }

        /// <summary>
        ///     Riepilogo emendamenti, firma legacy con liste tipizzate. Mantenuta per export Word,
        ///     stampe e richiesta firma; i nuovi chiamanti devono usare l'overload basato su
        ///     <see cref="QueryExtendedRequestEM" />.
        /// </summary>
        [Obsolete("Usa l'overload basato su QueryExtendedRequestEM.")]
        public async Task<IEnumerable<Guid>> GetAll(PersonaDto persona, OrdinamentoEnum ordine, int? page,
            int? size, int CLIENT_MODE, Filter<EM> filtro = null, List<Guid> firmatari = null,
            List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null, List<TagDto> tagDtos = null)
        {
            var query = PRContext
                .EM
                .Where(em => !em.Eliminato);
            
                if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
                {
                    var filter_value = filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(AttiDto.UIDAtto))
                        .Value;
                    var uidAtto = new Guid(filter_value.ToString());
                    var atto = await PRContext
                        .ATTI
                        .SingleAsync(a => a.UIDAtto == uidAtto);
                    if (atto.OrdinePresentazione == false && ordine == OrdinamentoEnum.Presentazione)
                        return new List<Guid>();

                    if (atto.OrdineVotazione == false && ordine == OrdinamentoEnum.Votazione) return new List<Guid>();

                    query = query.Where(em =>
                        em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
                }
                else
                {
                    query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                              || (em.IDStato == (int)StatiEnum.Bozza_Riservata
                                                  && (em.UIDPersonaCreazione == persona.UID_persona
                                                      || em.UIDPersonaProponente == persona.UID_persona
                                                      || (persona.IsCapoGruppo && em.UIDPersonaPrimaFirma.HasValue))));

                    if (persona.IsGiunta)
                        query = query
                            .Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                    else if (!persona.IsSegreteriaAssemblea_Vista
                             && !persona.IsPresidente)
                        query = query
                            .Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);

                    if (persona.IsSoloSegreteriaAssemblea)
                        query = query.Where(em =>
                            !string.IsNullOrEmpty(em.DataDeposito) ||
                            em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);
                }

                // #956
                if (filtro != null)
                {
                    if (filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(EM.N_EM)) != null)
                    {
                        var filter_n_em_value = filtro.Statements.First(item => item.PropertyId == nameof(EM.N_EM))
                            .Value.ToString();

                        var tokens = filter_n_em_value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim());

                        var allNumbers = new List<int>();

                        foreach (var tok in tokens)
                        {
                            if (tok.Contains("-"))
                            {
                                // Gestione del range (es. "3-5" o "5-2")
                                var bounds = tok.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim()).ToArray();
                                if (bounds.Length == 2 && int.TryParse(bounds[0], out var a) &&
                                    int.TryParse(bounds[1], out var b))
                                {
                                    var start = Math.Min(a, b);
                                    var end = Math.Max(a, b);

                                    for (var i = start; i <= end; i++)
                                    {
                                        allNumbers.Add(i);
                                    }
                                }
                            }
                            else if (int.TryParse(tok, out var num))
                            {
                                allNumbers.Add(num);
                            }
                        }

                        if (allNumbers.Any())
                        {
                            // Rimuoviamo i duplicati
                            allNumbers = allNumbers.Distinct().ToList();

                            // Costruiamo la condizione OR per tutti i numeri
                            Expression<Func<EM, bool>> combinedPredicate = null;

                            foreach (var numero in allNumbers)
                            {
                                var encryt_nem = CryptoHelper.EncryptString(numero.ToString(),
                                    AppSettingsConfiguration.masterKey);

                                // Condizione per questo numero specifico
                                Expression<Func<EM, bool>> singleCondition = e =>
                                    (!e.Timestamp.HasValue && e.Progressivo == numero) || e.N_EM == encryt_nem;

                                // Combiniamo con OR
                                combinedPredicate = combinedPredicate == null
                                    ? singleCondition
                                    : ExpressionExtensions.CombineExpressions(combinedPredicate, singleCondition);
                            }

                            if (combinedPredicate != null)
                                query = query.Where(combinedPredicate);
                        }

                        var n_em_request =
                            filtro._statements.First(statement => statement.PropertyId == nameof(EM.N_EM));
                        filtro._statements.Remove(n_em_request);
                    }

                    if (filtro.Statements.FirstOrDefault(item => item.PropertyId == nameof(EM.N_SUBEM)) != null)
                    {
                        var filter_n_subem_value = filtro.Statements
                            .First(item => item.PropertyId == nameof(EM.N_SUBEM))
                            .Value.ToString();

                        var tokens = filter_n_subem_value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim());

                        var allNumbers = new List<int>();

                        foreach (var tok in tokens)
                        {
                            if (tok.Contains("-"))
                            {
                                // Gestione del range (es. "3-5" o "5-2")
                                var bounds = tok.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim()).ToArray();
                                if (bounds.Length == 2 && int.TryParse(bounds[0], out var a) &&
                                    int.TryParse(bounds[1], out var b))
                                {
                                    var start = Math.Min(a, b);
                                    var end = Math.Max(a, b);

                                    for (var i = start; i <= end; i++)
                                    {
                                        allNumbers.Add(i);
                                    }
                                }
                            }
                            else if (int.TryParse(tok, out var num))
                            {
                                allNumbers.Add(num);
                            }
                        }

                        if (allNumbers.Any())
                        {
                            // Rimuoviamo i duplicati
                            allNumbers = allNumbers.Distinct().ToList();

                            // Costruiamo la condizione OR per tutti i numeri
                            Expression<Func<EM, bool>> combinedPredicate = null;

                            foreach (var numero in allNumbers)
                            {
                                var encryt_nem = CryptoHelper.EncryptString(numero.ToString(),
                                    AppSettingsConfiguration.masterKey);

                                // Condizione per questo numero specifico
                                Expression<Func<EM, bool>> singleCondition = e =>
                                    (!e.Timestamp.HasValue && e.SubProgressivo == numero) || e.N_SUBEM == encryt_nem;

                                // Combiniamo con OR
                                combinedPredicate = combinedPredicate == null
                                    ? singleCondition
                                    : ExpressionExtensions.CombineExpressions(combinedPredicate, singleCondition);
                            }

                            if (combinedPredicate != null)
                                query = query.Where(combinedPredicate);
                        }

                        var n_subem_request =
                            filtro._statements.First(statement => statement.PropertyId == nameof(EM.N_SUBEM));
                        filtro._statements.Remove(n_subem_request);
                    }
                }

                filtro?.BuildExpression(ref query);

                if (firmatari != null)
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

                if (proponenti != null)
                    if (proponenti.Count > 0)
                        //Avvio ricerca proponenti;
                        query = query
                            .Where(em => proponenti.Contains(em.UIDPersonaProponente));

                if (tagDtos != null)
                    if (tagDtos.Count > 0)
                    {
                        //Avvio ricerca tags;
                        var tag_em = new List<Guid>();
                        foreach (var t in tagDtos)
                        {
                            var arr = await PRContext.EM
                                .Where(em => em.Tags.Contains(t.tag)).Select(em => em.UIDEM)
                                .ToListAsync();
                            tag_em.AddRange(arr.Where(item => !tag_em.Contains(item)));
                        }

                        query = query.Where(em => tag_em.Contains(em.UIDEM));
                    }

                if (gruppi != null)
                    if (gruppi.Count > 0)
                        //Avvio ricerca gruppi;
                        query = query
                            .Where(em => gruppi.Contains(em.id_gruppo));

                if (stati != null)
                    if (stati.Count > 0)
                        //Avvio ricerca stati;
                        query = query
                            .Where(em => stati.Contains(em.IDStato));

                if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE ||
                    persona.IsSegreteriaAssemblea_Vista
                    || persona.IsPresidente)
                    switch (ordine)
                    {
                        case OrdinamentoEnum.Presentazione:
                            query = query.OrderBy(em => em.SubEM).ThenBy(em => em.OrdinePresentazione);
                            break;
                        case OrdinamentoEnum.Votazione:
                            query = query.OrderBy(em => em.OrdineVotazione);
                            break;
                        default:
                            query = query.OrderBy(em => em.IDStato).ThenByDescending(em => em.DataCreazione);
                            break;
                    }
                else
                    query = query.OrderBy(em => em.IDStato).ThenBy(em => em.Timestamp).ThenBy(em => em.Progressivo)
                        .ThenBy(em => em.SubProgressivo);

                if (size == -1)
                    return await query
                        .Select(em => em.UIDEM)
                        .ToListAsync();

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
                .Skip((model.page - 1) * model.size)
                .Take(model.size);

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
        public async Task<string> GetAll_Query(PersonaDto persona, int CLIENT_MODE, Filter<EM> filtro,
            OrdinamentoEnum ordinamentoEnum,
            List<Guid> firmatari = null, List<Guid> proponenti = null, List<int> gruppi = null, List<int> stati = null)
        {
            var query = PRContext
                .EM
                .Where(a => !a.Eliminato);

            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
            {
                query = query.Where(em =>
                    em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
            }
            else
            {
                query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                          || (em.IDStato == (int)StatiEnum.Bozza_Riservata
                                              && (em.UIDPersonaCreazione == persona.UID_persona
                                                  || em.UIDPersonaProponente == persona.UID_persona)));

                if (persona.IsGiunta)
                    query = query
                        .Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
                else if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                         && persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea
                         && persona.CurrentRole != RuoliIntEnum.Presidente_Regione)
                    query = query
                        .Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);

                if (persona.IsSegreteriaAssemblea_Vista)
                    query = query.Where(em =>
                        !string.IsNullOrEmpty(em.DataDeposito) ||
                        em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);
            }

            filtro?.BuildExpression(ref query);

            if (firmatari != null)
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

            if (proponenti != null)
                if (proponenti.Count > 0)
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(em => proponenti.Contains(em.UIDPersonaProponente));

            if (gruppi != null)
                if (gruppi.Count > 0)
                    //Avvio ricerca gruppi;
                    query = query
                        .Where(em => gruppi.Contains(em.id_gruppo));

            if (stati != null)
                if (stati.Count > 0)
                    //Avvio ricerca stati;
                    query = query
                        .Where(em => stati.Contains(em.IDStato));

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

        /// <summary>
        ///     Singolo emendamento
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<EM> Get(Guid emendamentoUId, bool includes = true)
        {
            var query = PRContext.EM.AsQueryable();
            if (includes)
                query = query.Include(em => em.ARTICOLI)
                    .Include(em => em.COMMI)
                    .Include(em => em.LETTERE)
                    .Include(em => em.PARTI_TESTO)
                    .Include(em => em.STATI_EM)
                    .Include(em => em.TIPI_EM);

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
                query = query.OrderByDescending(em => em.SubProgressivo)
                    .Take(1);
            else
                query = query.OrderByDescending(em => em.Progressivo)
                    .Take(1);

            var list = await query.ToListAsync();
            if (list.Count == 0) return 1;

            if (sub)
            {
                if (list[0].SubProgressivo.HasValue) return list[0].SubProgressivo.Value + 1;
            }
            else
            {
                if (list[0].Progressivo.HasValue) return list[0].Progressivo.Value + 1;
            }

            return 1;
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PARTI_TESTO>> GetPartiEmendabili()
        {
            return await PRContext.PARTI_TESTO.ToListAsync();
        }

        /// <summary>
        ///     Ritorna tutti i valori disponibili in tabella
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TIPI_EM>> GetTipiEmendamento()
        {
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
                .Where(m => (DateTime.Now > m.DAL && DateTime.Now <= m.AL) || !m.AL.HasValue).OrderBy(m => m.Ordine)
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
            // #1607
            if (persona.Gruppo == null)
            {
                if (persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea 
                    && persona.CurrentRole != RuoliIntEnum.Amministratore_PEM)
                {
                    return false;
                }

                return string.IsNullOrEmpty(em.DataDeposito);
            }

            if (em.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (!string.IsNullOrEmpty(em.DataDeposito)) return false;

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
            if (persona.Gruppo == null) return false;

            if (em.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (em.DataRitiro.HasValue) return false;

            if (em.IDStato != (int)StatiEnum.Depositato) return false;

            return persona.UID_persona == em.UIDPersonaProponente
                   || em.ATTI.UIDAssessoreRiferimento == persona.UID_persona
                   || (persona.CurrentRole == RuoliIntEnum.Presidente_Regione
                       && em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
        }

        /// <summary>
        ///     Controlla che l'emendamento sia depositabile
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public bool CheckIfDepositabile(EmendamentiDto em, PersonaDto persona)
        {
            if (!string.IsNullOrEmpty(em.DataDeposito)) return false;

            if (persona.IsSegreteriaAssemblea)
                if (em.Firma_da_ufficio)
                    return true;

            // Se proponente non ha firmato non è possibile depositare
            if (!em.Firmato_Dal_Proponente) return false;

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

            if (persona.Gruppo != null) return em.id_gruppo == persona.Gruppo.id_gruppo;

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
                return em.UIDPersonaProponente == persona.UID_persona
                       || em.UIDPersonaCreazione == persona.UID_persona
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta;

            return (em.UIDPersonaProponente == persona.UID_persona || em.UIDPersonaCreazione == persona.UID_persona)
                   && (em.IDStato == (int)StatiEnum.Bozza || em.IDStato == (int)StatiEnum.Bozza_Riservata);
        }

        public async Task<int> GetOrdinePresentazione(Guid uidAtto)
        {
            var query = PRContext.EM
                .Where(em => em.UIDAtto == uidAtto
                             && em.IDStato >= (int)StatiEnum.Depositato
                             && em.Eliminato == false);
            return await query.CountAsync();
        }

        public async Task<bool> TagExists(string tag)
        {
            return await PRContext.TAGS.AnyAsync(item => tag.ToLower().Equals(item.tag.ToLower()));
        }

        public void AddTag(string tag)
        {
            PRContext.TAGS.Add(new TAGS
            {
                tag = tag
            });
        }

        public async Task<List<View_Conteggi_EM_Gruppi_Politici>> GetConteggiGruppi(Guid uidAtto)
        {
            var result = await PRContext
                .View_Conteggi_EM_Gruppi_Politici
                .Where(o => o.UIDAtto == uidAtto)
                .OrderByDescending(o => o.num_em)
                .ToListAsync();

            var emendamenti_atto_by_giunta = await PRContext
                .EM
                .Where(em => em.UIDAtto == uidAtto
                             && em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID
                             && em.IDStato >= (int)StatiEnum.Depositato
                             && !em.Eliminato).ToListAsync();

            if (emendamenti_atto_by_giunta.Any())
                result.Add(new View_Conteggi_EM_Gruppi_Politici
                {
                    UIDAtto = uidAtto,
                    id_gruppo = AppSettingsConfiguration.GIUNTA_REGIONALE_ID,
                    nome_gruppo = "GIUNTA REGIONALE",
                    num_em = emendamenti_atto_by_giunta.Count
                });

            return result.OrderByDescending(i => i.num_em).ToList();
        }

        public async Task<List<View_Conteggi_EM_Area_Politica>> GetConteggiAreePolitiche(Guid uidAtto)
        {
            return await PRContext
                .View_Conteggi_EM_Area_Politica
                .Where(o => o.UIDAtto == uidAtto)
                .OrderByDescending(o => o.num_em)
                .ToListAsync();
        }

        public async Task<List<EM>> GetByLettera(Guid uGuid)
        {
            return await PRContext
                .EM
                .Where(em => em.UIDLettera == uGuid && !em.Eliminato)
                .ToListAsync();
        }

        public async Task<List<EM>> GetByComma(Guid guid)
        {
            return await PRContext
                .EM
                .Where(em => em.UIDComma == guid && !em.UIDLettera.HasValue && !em.Eliminato)
                .ToListAsync();
        }

        public async Task<List<EM>> GetByArticolo(Guid guid)
        {
            return await PRContext
                .EM
                .Where(em => em.UIDArticolo == guid && !em.UIDComma.HasValue && !em.Eliminato)
                .ToListAsync();
        }

        public async Task<List<EM>> GetGrigliaOrdinamento(Guid id)
        {
            return await PRContext
                .EM
                .Where(em => em.UIDAtto == id
                             && em.IDStato >= (int)StatiEnum.Depositato
                             && !em.Eliminato)
                .OrderBy(em => em.OrdineVotazione)
                .ToListAsync();
        }

        public async Task SetOrdineVotazione(Guid uidem, int pos)
        {
            var em = await Get(uidem);
            em.OrdineVotazione = pos;
        }

        public async Task<bool> TryAcquireDepositoLock(Guid userId)
        {
            var timeout = TimeSpan.FromMinutes(10); // durata lock valida
            var now = DateTime.Now;

            var existingLock = PRContext.DepositoLock.FirstOrDefault(x => x.Id == 1);

            if (existingLock != null)
            {
                // Se il lock è scaduto, lo forzo a mano
                if (existingLock.LockTime < now.Subtract(timeout))
                {
                    PRContext.DepositoLock.Remove(existingLock);
                    await PRContext.SaveChangesAsync(); // elimina il vecchio lock

                    // Ora provo a creare il mio lock
                    var lockEntity = new DepositoLock
                    {
                        Id = 1,
                        LockedBy = userId,
                        LockTime = now
                    };
                    PRContext.DepositoLock.Add(lockEntity);
                    await PRContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // C'è già un lock attivo
                    return false;
                }
            }
            else
            {
                // Nessun lock, posso inserirlo
                var lockEntity = new DepositoLock
                {
                    Id = 1,
                    LockedBy = userId,
                    LockTime = now
                };
                PRContext.DepositoLock.Add(lockEntity);
                await PRContext.SaveChangesAsync();
                return true;
            }
        }
        
        public async Task ReleaseDepositoLock(Guid userId)
        {
            var row = await PRContext.DepositoLock.FirstOrDefaultAsync(l => l.LockedBy == userId);
            if (row != null)
            {
                PRContext.DepositoLock.Remove(row);
                await PRContext.SaveChangesAsync();
            }
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

        public async Task<bool> CheckOrdinePresentazione(Guid attoUId, int ordine)
        {
            var res = await PRContext
                .EM
                .AnyAsync(e => e.UIDAtto == attoUId && e.OrdinePresentazione == ordine);
            return !res;
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

        #region Riepilogo basato su QueryExtendedRequestEM

        /// <summary>
        ///     Riepilogo emendamenti, pattern unificato: filtri specializzati in <see cref="QueryExtendedRequestEM" />,
        ///     statement residui nel <paramref name="filtro" />.
        ///     Costruisce un'unica IQueryable: scoping per ruolo + range N_EM/N_SUBEM + statement
        ///     promossi + sub-query EF per Firmatari/MyEM/EMDaFirmare + ordinamento primario
        ///     per ruolo + paginazione. Niente materializzazione
        ///     intermedia (eccetto Tags, dove il pattern di traduzione EF6 dell'OR su lista non
        ///     e' garantito).
        /// </summary>
        public async Task<IEnumerable<Guid>> GetAll(PersonaDto persona, int? page, int? size, int CLIENT_MODE,
            OrdinamentoEnum ordine, Filter<EM> filtro, QueryExtendedRequestEM queryExtended)
        {
            if (queryExtended == null) queryExtended = new QueryExtendedRequestEM();

            // 1. Promozione dei filtri specializzati a statement del Filter<EM> + estrazione
            //    delle liste tipizzate (firmatari, proponenti, gruppi, stati, tag).
            var filtroPromosso = PromuoviFiltriEM(filtro, queryExtended);
            var firmatari = queryExtended.Firmatari != null && queryExtended.Firmatari.Count > 0
                ? queryExtended.Firmatari : null;
            var proponenti = queryExtended.Proponenti != null && queryExtended.Proponenti.Count > 0
                ? queryExtended.Proponenti : null;
            var gruppi = queryExtended.GruppiProponenti != null && queryExtended.GruppiProponenti.Count > 0
                ? queryExtended.GruppiProponenti : null;
            var stati = PromuoviStatiEM(queryExtended.Stati);
            var tags = queryExtended.Tags != null && queryExtended.Tags.Count > 0
                ? queryExtended.Tags : null;

            // 2. Base + scoping per ruolo/modalita'. In ricerca trasversale (#1626) lo scoping
            //    non e' legato a un singolo atto: si parte da tutti i depositati.
            IQueryable<EM> query;
            if (queryExtended.RicercaGlobale)
            {
                query = ApplicaScopingGlobaleEM();
            }
            else
            {
                query = await ApplicaScopingBaseEM(filtroPromosso, persona, CLIENT_MODE, ordine, queryExtended.UIDAtto);
                if (query == null) return new List<Guid>();
            }

            // 3. Range N_EM/N_SUBEM (token "1,3-5,7") estratti dagli statement e applicati come
            //    OR di Where; gli statement consumati vengono rimossi dal Filter<EM>.
            query = ApplicaRangeNEM(query, filtroPromosso);
            query = ApplicaRangeNSubEM(query, filtroPromosso);

            // 4. Statement residui (UIDAtto, IDTipo_EM, IDParte, UIDArticolo, UIDComma, ecc.).
            filtroPromosso?.BuildExpression(ref query);

            // 5. Liste tipizzate inline come sub-query EF.
            query = ApplicaFiltriEstesi(query, firmatari, proponenti, gruppi, stati, tags,
                queryExtended, persona);

            // 5-bis. Filtri specifici della ricerca trasversale (legislatura, area politica, EM/SUBEM).
            if (queryExtended.RicercaGlobale)
                query = ApplicaFiltriGlobaliEM(query, queryExtended);

            // 6. Ordinamento primario per ruolo (in ricerca trasversale gli EM appartengono
            //    ad atti diversi: si raggruppa per atto).
            var ordered = queryExtended.RicercaGlobale
                ? ApplicaOrdinamentoGlobaleEM(query)
                : ApplicaOrdinamentoEM(query, ordine, persona, CLIENT_MODE);

            // 7. Paginazione.
            if (!size.HasValue || size.Value == -1)
                return await ordered.Select(em => em.UIDEM).ToListAsync();

            return await ordered.Select(em => em.UIDEM)
                .Skip(((page ?? 1) - 1) * size.Value)
                .Take(size.Value)
                .ToListAsync();
        }

        /// <summary>
        ///     Conteggio emendamenti, pattern unificato. Replica lo stesso schema di GetAll
        ///     (scoping + range + Filter + sub-query) ma termina con un CountAsync,
        ///     differenziando EM / SUB_EM / NONE.
        /// </summary>
        public async Task<int> Count(Guid attoUId, PersonaDto persona, CounterEmendamentiEnum counter,
            int CLIENT_MODE, Filter<EM> filtro, QueryExtendedRequestEM queryExtended)
        {
            if (queryExtended == null) queryExtended = new QueryExtendedRequestEM();

            var filtroPromosso = PromuoviFiltriEM(filtro, queryExtended);
            var firmatari = queryExtended.Firmatari != null && queryExtended.Firmatari.Count > 0
                ? queryExtended.Firmatari : null;
            var proponenti = queryExtended.Proponenti != null && queryExtended.Proponenti.Count > 0
                ? queryExtended.Proponenti : null;
            var gruppi = queryExtended.GruppiProponenti != null && queryExtended.GruppiProponenti.Count > 0
                ? queryExtended.GruppiProponenti : null;
            var stati = PromuoviStatiEM(queryExtended.Stati);
            var tags = queryExtended.Tags != null && queryExtended.Tags.Count > 0
                ? queryExtended.Tags : null;

            // Per il count l'UIDAtto e' sempre quello richiesto: lo applichiamo subito sulla base.
            var query = PRContext.EM.Where(em => em.UIDAtto == attoUId && !em.Eliminato);
            query = ApplicaScopingRuolo(query, persona, CLIENT_MODE);

            query = ApplicaRangeNEM(query, filtroPromosso);
            query = ApplicaRangeNSubEM(query, filtroPromosso);

            filtroPromosso?.BuildExpression(ref query);

            query = ApplicaFiltriEstesi(query, firmatari, proponenti, gruppi, stati, tags,
                queryExtended, persona);

            switch (counter)
            {
                case CounterEmendamentiEnum.NONE:
                    return await query.CountAsync();
                case CounterEmendamentiEnum.EM:
                    return persona.IsSegreteriaAssemblea_Vista
                        ? await query.CountAsync(e =>
                            !string.IsNullOrEmpty(e.N_EM) && string.IsNullOrEmpty(e.N_SUBEM))
                        : await query.CountAsync(e => string.IsNullOrEmpty(e.N_SUBEM));
                case CounterEmendamentiEnum.SUB_EM:
                    return persona.IsSegreteriaAssemblea_Vista
                        ? await query.CountAsync(e =>
                            string.IsNullOrEmpty(e.N_EM) && !string.IsNullOrEmpty(e.N_SUBEM))
                        : await query.CountAsync(e => !string.IsNullOrEmpty(e.N_SUBEM));
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Base query per il riepilogo: scoping per ruolo/atto (TRATTAZIONE: visibili solo i
        ///     depositati; GRUPPI: nasconde le bozze riservate altrui, restringe a giunta /
        ///     gruppo / segreteria a seconda del ruolo). In TRATTAZIONE early-return su atti che
        ///     non hanno OrdinePresentazione/Votazione attivi.
        /// </summary>
        private async Task<IQueryable<EM>> ApplicaScopingBaseEM(Filter<EM> filtroPromosso,
            PersonaDto persona, int CLIENT_MODE, OrdinamentoEnum ordine, Guid? uidAttoExtended)
        {
            var query = PRContext.EM.Where(em => !em.Eliminato);

            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
            {
                var uidAtto = uidAttoExtended ?? Guid.Empty;
                if (uidAtto == Guid.Empty && filtroPromosso != null)
                {
                    var attoStmt = filtroPromosso._statements
                        .FirstOrDefault(s => s.PropertyId == nameof(EM.UIDAtto));
                    if (attoStmt?.Value != null)
                        Guid.TryParse(attoStmt.Value.ToString(), out uidAtto);
                }

                if (uidAtto != Guid.Empty)
                {
                    var atto = await PRContext.ATTI.SingleOrDefaultAsync(a => a.UIDAtto == uidAtto);
                    if (atto != null)
                    {
                        if (atto.OrdinePresentazione == false && ordine == OrdinamentoEnum.Presentazione)
                            return null;
                        if (atto.OrdineVotazione == false && ordine == OrdinamentoEnum.Votazione)
                            return null;
                    }
                }

                query = query.Where(em =>
                    em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));
                return query;
            }

            return ApplicaScopingRuolo(query, persona, CLIENT_MODE);
        }

        private IQueryable<EM> ApplicaScopingRuolo(IQueryable<EM> query, PersonaDto persona, int CLIENT_MODE)
        {
            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE)
                return query.Where(em =>
                    em.IDStato >= (int)StatiEnum.Depositato && !string.IsNullOrEmpty(em.DataDeposito));

            query = query.Where(em => em.IDStato != (int)StatiEnum.Bozza_Riservata
                                      || (em.IDStato == (int)StatiEnum.Bozza_Riservata
                                          && (em.UIDPersonaCreazione == persona.UID_persona
                                              || em.UIDPersonaProponente == persona.UID_persona
                                              || (persona.IsCapoGruppo && em.UIDPersonaPrimaFirma.HasValue))));

            if (persona.IsGiunta)
                query = query.Where(em => em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);
            else if (!persona.IsSegreteriaAssemblea_Vista && !persona.IsPresidente)
                query = query.Where(em => em.id_gruppo == persona.Gruppo.id_gruppo);

            if (persona.IsSoloSegreteriaAssemblea)
                query = query.Where(em => !string.IsNullOrEmpty(em.DataDeposito)
                                          || em.idRuoloCreazione == (int)RuoliIntEnum.Segreteria_Assemblea);

            return query;
        }

        /// <summary>
        ///     #1626 - Base per la ricerca trasversale EM/SUBEM (Area Aula): tutti gli emendamenti
        ///     non eliminati e depositati, senza vincolo di singolo atto ne' di gruppo. La
        ///     visibilita' "depositati da tutti i gruppi" e' garantita dalla regola sullo stato,
        ///     coerente con la modalita' TRATTAZIONE.
        /// </summary>
        private IQueryable<EM> ApplicaScopingGlobaleEM()
        {
            return PRContext.EM.Where(em =>
                !em.Eliminato
                && em.IDStato >= (int)StatiEnum.Depositato
                && !string.IsNullOrEmpty(em.DataDeposito));
        }

        /// <summary>
        ///     #1626 - Filtri specifici della ricerca trasversale: legislatura (join su ATTI),
        ///     area politica (campo EM.AreaPolitica) e tipo EM/SUBEM (presenza di Rif_UIDEM).
        /// </summary>
        private IQueryable<EM> ApplicaFiltriGlobaliEM(IQueryable<EM> query, QueryExtendedRequestEM qx)
        {
            if (qx.Legislature != null && qx.Legislature.Count > 0)
            {
                var legislature = qx.Legislature;
                query = query.Where(em => PRContext.ATTI.Any(a =>
                    a.UIDAtto == em.UIDAtto
                    && a.Legislatura.HasValue
                    && legislature.Contains(a.Legislatura.Value)));
            }

            if (qx.AreePolitiche != null && qx.AreePolitiche.Count > 0)
            {
                var aree = qx.AreePolitiche;
                query = query.Where(em => em.AreaPolitica.HasValue && aree.Contains(em.AreaPolitica.Value));
            }

            switch ((TipoRicercaEmendamentiEnum)qx.TipoRicerca)
            {
                case TipoRicercaEmendamentiEnum.SoloEM:
                    query = query.Where(em => !em.Rif_UIDEM.HasValue);
                    break;
                case TipoRicercaEmendamentiEnum.SoloSubEM:
                    query = query.Where(em => em.Rif_UIDEM.HasValue);
                    break;
            }

            return query;
        }

        /// <summary>
        ///     #1626 - Ordinamento per la ricerca trasversale: gli EM provengono da atti diversi,
        ///     quindi si raggruppa per atto e poi per EM/SUBEM e ordine di presentazione.
        /// </summary>
        private IOrderedQueryable<EM> ApplicaOrdinamentoGlobaleEM(IQueryable<EM> query)
        {
            return query
                .OrderBy(em => em.UIDAtto)
                .ThenBy(em => em.SubEM)
                .ThenBy(em => em.OrdinePresentazione);
        }

        /// <summary>
        ///     #1626 - Conteggio per la ricerca trasversale: replica la pipeline di GetAll in
        ///     modalita' globale (scoping depositati + filtri estesi + filtri globali) e termina
        ///     con un CountAsync.
        /// </summary>
        public async Task<int> CountGlobale(PersonaDto persona, Filter<EM> filtro, QueryExtendedRequestEM queryExtended)
        {
            if (queryExtended == null) queryExtended = new QueryExtendedRequestEM();
            queryExtended.RicercaGlobale = true;

            var filtroPromosso = PromuoviFiltriEM(filtro, queryExtended);
            var firmatari = queryExtended.Firmatari != null && queryExtended.Firmatari.Count > 0
                ? queryExtended.Firmatari : null;
            var proponenti = queryExtended.Proponenti != null && queryExtended.Proponenti.Count > 0
                ? queryExtended.Proponenti : null;
            var gruppi = queryExtended.GruppiProponenti != null && queryExtended.GruppiProponenti.Count > 0
                ? queryExtended.GruppiProponenti : null;
            var stati = PromuoviStatiEM(queryExtended.Stati);
            var tags = queryExtended.Tags != null && queryExtended.Tags.Count > 0
                ? queryExtended.Tags : null;

            var query = ApplicaScopingGlobaleEM();
            query = ApplicaRangeNEM(query, filtroPromosso);
            query = ApplicaRangeNSubEM(query, filtroPromosso);
            filtroPromosso?.BuildExpression(ref query);
            query = ApplicaFiltriEstesi(query, firmatari, proponenti, gruppi, stati, tags,
                queryExtended, persona);
            query = ApplicaFiltriGlobaliEM(query, queryExtended);

            return await query.CountAsync();
        }

        /// <summary>
        ///     Estrae lo statement N_EM (tokens "1,3-5,7") dal <paramref name="filtro" />, lo applica
        ///     come OR di condizioni cifrate sul campo cifrato e rimuove lo statement consumato.
        /// </summary>
        private IQueryable<EM> ApplicaRangeNEM(IQueryable<EM> query, Filter<EM> filtro)
        {
            if (filtro?._statements == null) return query;
            var stmt = filtro._statements.FirstOrDefault(s => s.PropertyId == nameof(EM.N_EM));
            if (stmt?.Value == null) return query;

            var numeri = ParseTokensRange(stmt.Value.ToString());
            if (numeri.Count > 0)
            {
                Expression<Func<EM, bool>> combined = null;
                foreach (var n in numeri)
                {
                    var encryt = CryptoHelper.EncryptString(n.ToString(), AppSettingsConfiguration.masterKey);
                    Expression<Func<EM, bool>> piece = e =>
                        (!e.Timestamp.HasValue && e.Progressivo == n) || e.N_EM == encryt;
                    combined = combined == null
                        ? piece
                        : ExpressionExtensions.CombineExpressions(combined, piece);
                }
                if (combined != null) query = query.Where(combined);
            }

            filtro._statements.Remove(stmt);
            return query;
        }

        private IQueryable<EM> ApplicaRangeNSubEM(IQueryable<EM> query, Filter<EM> filtro)
        {
            if (filtro?._statements == null) return query;
            var stmt = filtro._statements.FirstOrDefault(s => s.PropertyId == nameof(EM.N_SUBEM));
            if (stmt?.Value == null) return query;

            var numeri = ParseTokensRange(stmt.Value.ToString());
            if (numeri.Count > 0)
            {
                Expression<Func<EM, bool>> combined = null;
                foreach (var n in numeri)
                {
                    var encryt = CryptoHelper.EncryptString(n.ToString(), AppSettingsConfiguration.masterKey);
                    Expression<Func<EM, bool>> piece = e =>
                        (!e.Timestamp.HasValue && e.SubProgressivo == n) || e.N_SUBEM == encryt;
                    combined = combined == null
                        ? piece
                        : ExpressionExtensions.CombineExpressions(combined, piece);
                }
                if (combined != null) query = query.Where(combined);
            }

            filtro._statements.Remove(stmt);
            return query;
        }

        /// <summary>
        ///     Parser dei range numerici accettati dalle chip N_EM / N_SUBEM: "1,3-5,7" ->
        ///     [1, 3, 4, 5, 7]. Tollerante: token non parsabili vengono ignorati.
        /// </summary>
        private static List<int> ParseTokensRange(string raw)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(raw)) return result;

            var tokens = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim());
            foreach (var tok in tokens)
            {
                if (tok.Contains("-"))
                {
                    var bounds = tok.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToArray();
                    if (bounds.Length == 2 && int.TryParse(bounds[0], out var a) && int.TryParse(bounds[1], out var b))
                    {
                        var start = Math.Min(a, b);
                        var end = Math.Max(a, b);
                        for (var i = start; i <= end; i++) result.Add(i);
                    }
                }
                else if (int.TryParse(tok, out var num))
                {
                    result.Add(num);
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        ///     Applica i filtri estesi (firmatari come sub-query EF, proponenti, gruppi, stati con
        ///     regola Approvato + Approvato_Con_Modifiche, tag con materializzazione fallback,
        ///     MyEM ed EMDaFirmare come sub-query inline su NOTIFICHE_DESTINATARI / FIRME).
        /// </summary>
        private IQueryable<EM> ApplicaFiltriEstesi(IQueryable<EM> query,
            List<Guid> firmatari, List<Guid> proponenti, List<int> gruppi, List<int> stati,
            List<TagDto> tags, QueryExtendedRequestEM qx, PersonaDto persona)
        {
            if (firmatari != null && firmatari.Count > 0)
                query = query.Where(em => PRContext.FIRME
                    .Any(f => f.UIDEM == em.UIDEM && firmatari.Contains(f.UID_persona)));

            if (proponenti != null && proponenti.Count > 0)
                query = query.Where(em => proponenti.Contains(em.UIDPersonaProponente));

            if (gruppi != null && gruppi.Count > 0)
                query = query.Where(em => gruppi.Contains(em.id_gruppo));

            if (stati != null && stati.Count > 0)
                query = query.Where(em => stati.Contains(em.IDStato));

            // #1644 - Chip "Sub-emendamenti": presenza/assenza del riferimento all'EM padre.
            // true => solo SUBEM (Rif_UIDEM valorizzato), false => solo EM.
            if (qx.SoloSubEM.HasValue)
                query = qx.SoloSubEM.Value
                    ? query.Where(em => em.Rif_UIDEM.HasValue)
                    : query.Where(em => !em.Rif_UIDEM.HasValue);

            if (tags != null && tags.Count > 0)
            {
                // EF6 non traduce in modo affidabile un OR su lista di stringhe combinato con
                // Contains su un campo, quindi manteniamo il pattern di materializzazione
                // intermedia degli UIDEM gia' presente nella firma legacy.
                var tagEm = new List<Guid>();
                foreach (var t in tags)
                {
                    var arr = PRContext.EM
                        .Where(em => em.Tags.Contains(t.tag))
                        .Select(em => em.UIDEM)
                        .ToList();
                    foreach (var u in arr)
                        if (!tagEm.Contains(u)) tagEm.Add(u);
                }
                query = query.Where(em => tagEm.Contains(em.UIDEM));
            }

            if (qx.MyEM && qx.UIDPersonaCorrente.HasValue)
            {
                var uid = qx.UIDPersonaCorrente.Value;
                query = query.Where(em =>
                    em.UIDPersonaProponente == uid || em.UIDPersonaCreazione == uid);
            }

            if (qx.EMDaFirmare && qx.UIDPersonaCorrente.HasValue)
            {
                var uid = qx.UIDPersonaCorrente.Value;
                query = query.Where(em =>
                    (
                        PRContext.NOTIFICHE_DESTINATARI.Any(nd =>
                            nd.UIDPersona == uid && !nd.Chiuso && nd.NOTIFICHE.UIDEM == em.UIDEM)
                        || (em.UIDPersonaProponente == uid && !em.UIDPersonaPrimaFirma.HasValue)
                    )
                    && !PRContext.FIRME.Any(f =>
                        f.UIDEM == em.UIDEM && f.UID_persona == uid
                        && string.IsNullOrEmpty(f.Data_ritirofirma)));
            }

            return query;
        }

        /// <summary>
        ///     Ordinamento primario per ruolo / modalita'. Per TRATTAZIONE, segreteria assemblea
        ///     e presidente segue la regola OrdinamentoEnum (SubEM + OrdinePresentazione, oppure
        ///     OrdineVotazione, oppure IDStato + DataCreazione). Per il consigliere usa l'ordine
        ///     "naturale" della lista emendamenti.
        /// </summary>
        private IOrderedQueryable<EM> ApplicaOrdinamentoEM(IQueryable<EM> query, OrdinamentoEnum ordine,
            PersonaDto persona, int CLIENT_MODE)
        {
            // v2026.5.1 - L'ordine richiesto esplicitamente (Presentazione o Votazione)
            // prevale sul ruolo: il client cambia tab "Presentazione/Votazione" e si
            // aspetta che la griglia rispetti la scelta sia per il consigliere che per
            // l'admin/segreteria, sia in modalita' GRUPPI che TRATTAZIONE.
            // Bug precedente: lo switch era annidato nel ramo "segreteria/presidente/
            // trattazione", quindi il consigliere PEM in GRUPPI (e anche l'admin PEM
            // in GRUPPI) cadeva sempre nell'ordine "naturale" per stato, ignorando il
            // parametro ordine inviato dal client.
            switch (ordine)
            {
                case OrdinamentoEnum.Presentazione:
                    return query.OrderBy(em => em.SubEM).ThenBy(em => em.OrdinePresentazione);
                case OrdinamentoEnum.Votazione:
                    return query.OrderBy(em => em.OrdineVotazione);
            }

            // Default: fallback su ordine "naturale". Per segreteria/presidente o vista
            // TRATTAZIONE manteniamo il ramo IDStato + DataCreazione; per il consigliere
            // GRUPPI manteniamo IDStato + Timestamp + progressivi.
            if (CLIENT_MODE == (int)ClientModeEnum.TRATTAZIONE
                || persona.IsSegreteriaAssemblea_Vista
                || persona.IsPresidente)
            {
                return query.OrderBy(em => em.IDStato).ThenByDescending(em => em.DataCreazione);
            }

            return query.OrderBy(em => em.IDStato)
                .ThenBy(em => em.Timestamp)
                .ThenBy(em => em.Progressivo)
                .ThenBy(em => em.SubProgressivo);
        }

        /// <summary>
        ///     Promuove le liste specializzate del <see cref="QueryExtendedRequestEM" /> a statement
        ///     del <see cref="Filter{T}" />, cosi' BuildExpression li applica in una sola passata
        ///     dentro la query. Gli statement gia' presenti nel filtro vengono preservati.
        /// </summary>
        private Filter<EM> PromuoviFiltriEM(Filter<EM> filtroBase, QueryExtendedRequestEM qx)
        {
            var filtro = new Filter<EM>();
            if (filtroBase?._statements != null)
                foreach (var s in filtroBase._statements)
                    filtro._statements.Add(s);

            AggiungiStatementOr(filtro, nameof(EM.IDTipo_EM), qx.Tipi);
            AggiungiStatementOr(filtro, nameof(EM.IDParte), qx.Parti);
            AggiungiStatementOr(filtro, nameof(EM.UIDArticolo), qx.Articoli);
            AggiungiStatementOr(filtro, nameof(EM.UIDComma), qx.Commi);
            AggiungiStatementOr(filtro, nameof(EM.UIDLettera), qx.Lettere);
            AggiungiStatementOr(filtro, nameof(EM.NLettera), qx.LettereLegacy);
            AggiungiStatementOr(filtro, nameof(EM.NTitolo), qx.NTitoli);
            AggiungiStatementOr(filtro, nameof(EM.NCapo), qx.NCapi);
            AggiungiStatementOr(filtro, nameof(EM.NMissione), qx.NMissioni);
            AggiungiStatementOr(filtro, nameof(EM.NProgramma), qx.NProgrammi);

            // #1645 - "Effetti finanziari" e' tri-stato: Si' => solo EM con effetti (== 1),
            // No => solo EM senza effetti (== 0), chip assente (null) => nessun filtro.
            if (qx.EffettiFinanziari.HasValue)
                filtro._statements.Add(new FilterStatement<int>(
                    nameof(EM.EffettiFinanziari), Operation.EqualTo,
                    qx.EffettiFinanziari.Value ? 1 : 0));

            if (!string.IsNullOrEmpty(qx.TestoLibero1))
            {
                filtro._statements.Add(new FilterStatement<string>(
                    nameof(EM.TestoEM_originale), Operation.Contains, qx.TestoLibero1));
                if (!string.IsNullOrEmpty(qx.TestoLibero2))
                {
                    var connettore = qx.TestoLiberoConnettore == (int)FilterStatementConnector.Or
                        ? FilterStatementConnector.Or
                        : FilterStatementConnector.And;
                    filtro._statements.Add(new FilterStatement<string>(
                        nameof(EM.TestoEM_originale), Operation.Contains, qx.TestoLibero2,
                        default, connettore));
                }
            }

            if (qx.UIDAtto.HasValue
                && filtro._statements.All(s => s.PropertyId != nameof(EM.UIDAtto)))
                filtro._statements.Add(new FilterStatement<Guid>(
                    nameof(EM.UIDAtto), Operation.EqualTo, qx.UIDAtto.Value));

            return filtro;
        }

        private void AggiungiStatementOr<T>(Filter<EM> filtro, string propertyId, ICollection<T> valori)
        {
            if (valori == null || valori.Count == 0) return;
            var lista = valori.ToList();
            for (var i = 0; i < lista.Count; i++)
            {
                var connettore = i < lista.Count - 1
                    ? FilterStatementConnector.Or
                    : FilterStatementConnector.And;
                filtro._statements.Add(new FilterStatement<T>(
                    propertyId, Operation.EqualTo, lista[i], default, connettore));
            }
        }

        /// <summary>
        ///     Se l'utente filtra per stato "Approvato", include anche "Approvato con modifiche"
        ///     (a meno che non sia gia' selezionato esplicitamente).
        /// </summary>
        private List<int> PromuoviStatiEM(List<int> stati)
        {
            if (stati == null || stati.Count == 0) return null;
            var lista = stati.ToList();
            if (lista.Contains((int)StatiEnum.Approvato)
                && !lista.Contains((int)StatiEnum.Approvato_Con_Modifiche))
                lista.Add((int)StatiEnum.Approvato_Con_Modifiche);
            return lista;
        }

        #endregion
    }
}