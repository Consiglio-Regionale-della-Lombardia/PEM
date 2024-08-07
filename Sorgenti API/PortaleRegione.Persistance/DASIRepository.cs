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

using ExpressionBuilder.Generics;
using PortaleRegione.Common;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PortaleRegione.Persistance
{
    public class DASIRepository : Repository<ATTI_DASI>, IDASIRepository
    {
        public DASIRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<ATTI_DASI> Get(Guid attoUId)
        {
            var result = await PRContext
                .DASI
                .SingleOrDefaultAsync(a => a.UIDAtto == attoUId);
            return result;
        }

        public async Task<List<Guid>> GetAll(PersonaDto persona, int page, int size, 
            OrdinamentoEnum ordinamento,
            ClientModeEnum mode,
            Filter<ATTI_DASI> filtro = null,
            List<int> soggetti = null, List<Guid> proponenti = null, List<Guid> provvedimenti = null,
            List<int> stati = null, List<Guid> atti_da_firmare = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in filtro.Statements)
            {
                if (f.PropertyId == nameof(ATTI_DASI.DataIscrizioneSeduta))
                {
                    var data_iscrizione = Convert.ToDateTime(f.Value.ToString());
                    query = query.Where(item => item.DataIscrizioneSeduta.Value.Year == data_iscrizione.Year
                                                && item.DataIscrizioneSeduta.Value.Month == data_iscrizione.Month
                                                && item.DataIscrizioneSeduta.Value.Day == data_iscrizione.Day);
                }
                else if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                {
                    query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                || item.Premesse.Contains(f.Value.ToString())
                                                || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta.Contains(f.Value.ToString()));
                }
                else
                    filtro2._statements.Add(f);
            }

            filtro2.BuildExpression(ref query);

            if (mode == ClientModeEnum.GRUPPI)
            {
                query = query.Where(atto => atto.IDStato != (int)StatiAttoEnum.BOZZA_RISERVATA
                                            || (atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA
                                                && (atto.UIDPersonaCreazione == persona.UID_persona
                                                    || atto.UIDPersonaProponente == persona.UID_persona)));

                if (!persona.IsSegreteriaAssemblea
                    && !persona.IsPresidente)
                    query = query.Where(item => item.id_gruppo == persona.Gruppo.id_gruppo);
            }
            else
            {
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);
            }

            if (stati != null)
            {
                if (stati.Any())
                    query = query.Where(i => stati.Contains(i.IDStato));
            }

            if (soggetti != null)
                if (soggetti.Count > 0)
                {
                    //Avvio ricerca soggetti
                    var list = await PRContext
                        .ATTI_SOGGETTI_INTERROGATI
                        .Where(f => soggetti.Contains(f.id_carica))
                        .Select(f => f.UIDAtto)
                        .ToListAsync();
                    query = query
                        .Where(item => list.Contains(item.UIDAtto));
                }

            if (proponenti != null)
                if (proponenti.Count > 0)
                    //Avvio ricerca proponenti;
                    query = query
                        .Where(atto => proponenti.Contains(atto.UIDPersonaProponente.Value));

            if (provvedimenti != null)
                if (provvedimenti.Count > 0)
                    // #541 Avvio ricerca provvedimenti;
                    query = query
                        .Where(atto => provvedimenti.Contains(atto.UID_Atto_ODG.Value));

            if (atti_da_firmare != null)
                if (atti_da_firmare.Any())
                {
                    query = query.Where(i => atti_da_firmare.Contains(i.UIDAtto));
                }

            if (stati != null)
            {
                if (stati.Any())
                {
                    if (stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                    {
                        // #946

                        if (ordinamento == OrdinamentoEnum.Crescente)
                        {
                            query = query
                                .OrderBy(item => item.Tipo)
                                .ThenBy(item => item.DataCreazione);
                        }
                        else
                        {
                            query = query
                                .OrderBy(item => item.Tipo)
                                .ThenByDescending(item => item.DataCreazione);
                        }

                        return await query
                            .Select(item => item.UIDAtto)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .ToListAsync();
                    }
                }
            }

            // #946

            if (ordinamento == OrdinamentoEnum.Crescente)
            {
                query = query
                    .OrderBy(item => item.Tipo)
                    .ThenBy(item => item.NAtto_search);
            }
            else
            {
                query = query
                    .OrderBy(item => item.Tipo)
                    .ThenByDescending(item => item.NAtto_search);
            }

            return await query
                .Select(item => item.UIDAtto)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(PersonaDto persona, ClientModeEnum mode, Filter<ATTI_DASI> filtro,
            List<int> soggetti, List<int> stati, List<Guid> atti_da_firmare = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato && item.IDStato != (int)StatiAttoEnum.BOZZA_CARTACEA);

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in filtro.Statements)
                if (f.PropertyId == nameof(ATTI_DASI.DataIscrizioneSeduta))
                {
                    var data_iscrizione = Convert.ToDateTime(f.Value.ToString());
                    query = query.Where(item => item.DataIscrizioneSeduta.Value.Year == data_iscrizione.Year
                                                && item.DataIscrizioneSeduta.Value.Month == data_iscrizione.Month
                                                && item.DataIscrizioneSeduta.Value.Day == data_iscrizione.Day);
                }
                else if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                {
                    query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                || item.Premesse.Contains(f.Value.ToString())
                                                || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta.Contains(f.Value.ToString()));
                }
                else
                    filtro2._statements.Add(f);

            filtro2.BuildExpression(ref query);

            if (mode == ClientModeEnum.GRUPPI)
            {
                query = query.Where(atto => atto.IDStato != (int)StatiAttoEnum.BOZZA_RISERVATA
                                            || (atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA
                                                && (atto.UIDPersonaCreazione == persona.UID_persona
                                                    || atto.UIDPersonaProponente == persona.UID_persona)));

                if (!persona.IsSegreteriaAssemblea
                    && !persona.IsPresidente)
                    query = query.Where(item => item.id_gruppo == persona.Gruppo.id_gruppo);
            }
            else
            {
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);
            }

            if (stati.Any())
                query = query.Where(i => stati.Contains(i.IDStato));

            if (soggetti != null)
                if (soggetti.Count > 0)
                {
                    //Avvio ricerca soggetti
                    var list = await PRContext
                        .ATTI_SOGGETTI_INTERROGATI
                        .Where(f => soggetti.Contains(f.id_carica))
                        .Select(f => f.UIDAtto)
                        .ToListAsync();
                    query = query
                        .Where(item => list.Contains(item.UIDAtto));
                }

            if (atti_da_firmare == null)
                return await query
                    .CountAsync();

            if (atti_da_firmare.Any())
            {
                query = query.Where(i => atti_da_firmare.Contains(i.UIDAtto));
            }

            return await query
                .CountAsync();
        }

        public async Task<int> Count(Filter<ATTI_DASI> filtro)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in filtro.Statements)
                if (f.PropertyId != nameof(ATTI_DASI.Oggetto))
                    filtro2._statements.Add(f);
                else if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                    query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                || item.Premesse.Contains(f.Value.ToString())
                                                || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta.Contains(f.Value.ToString()));

            filtro2?.BuildExpression(ref query);

            return await query
                .CountAsync();
        }

        public async Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, StatiAttoEnum stato, Guid? sedutaId,
            ClientModeEnum clientMode, Filter<ATTI_DASI> filtro, List<int> soggetti, List<Guid> proponenti, List<Guid> atti_da_firmare)
        {
            try
            {
                var query = PRContext
                    .DASI
                    .Where(item => !item.Eliminato && item.IDStato != (int)StatiAttoEnum.BOZZA_CARTACEA);

                var filtro2 = new Filter<ATTI_DASI>();
                foreach (var f in filtro.Statements)
                    if (f.PropertyId == nameof(ATTI_DASI.DataIscrizioneSeduta))
                    {
                        var data_iscrizione = Convert.ToDateTime(f.Value.ToString());
                        query = query.Where(item => item.DataIscrizioneSeduta.Value.Year == data_iscrizione.Year
                                                    && item.DataIscrizioneSeduta.Value.Month == data_iscrizione.Month
                                                    && item.DataIscrizioneSeduta.Value.Day == data_iscrizione.Day);
                    }
                    else if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                    {
                        query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                    || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                    || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                    || item.Premesse.Contains(f.Value.ToString())
                                                    || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                    || item.Richiesta.Contains(f.Value.ToString()));
                    }
                    else
                        filtro2._statements.Add(f);

                filtro2?.BuildExpression(ref query);

                if (clientMode == ClientModeEnum.GRUPPI)
                {
                    query = query.Where(atto => atto.IDStato != (int)StatiAttoEnum.BOZZA_RISERVATA
                                                || (atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA
                                                    && (atto.UIDPersonaCreazione == persona.UID_persona
                                                        || atto.UIDPersonaProponente == persona.UID_persona)));

                    if (!persona.IsSegreteriaAssemblea
                        && !persona.IsPresidente)
                        query = query.Where(item => item.id_gruppo == persona.Gruppo.id_gruppo);

                    if (stato != StatiAttoEnum.TUTTI)
                    {
                        if (stato == StatiAttoEnum.CHIUSO)
                            query = query.Where(FilterByClosed());
                        else
                            query = query.Where(item => item.IDStato == (int)stato);
                    }
                    else
                    {
                        if (persona.IsSegreteriaAssemblea)
                            query = query.Where(item => !Utility.statiNonVisibili_Segreteria.Contains(item.IDStato));
                    }

                    if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo == (int)tipo);
                }
                else
                {
                    query = query.Where(item => item.DataIscrizioneSeduta.HasValue
                                                && item.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                                && item.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);
                    if (stato != StatiAttoEnum.TUTTI) query = query.Where(item => item.IDStato == (int)stato);
                    if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo == (int)tipo);
                }

                if (soggetti != null)
                    if (soggetti.Count > 0)
                    {
                        //Avvio ricerca soggetti
                        var list = await PRContext
                            .ATTI_SOGGETTI_INTERROGATI
                            .Where(f => soggetti.Contains(f.id_carica))
                            .Select(f => f.UIDAtto)
                            .ToListAsync();
                        query = query
                            .Where(item => list.Contains(item.UIDAtto));
                    }

                if (proponenti.Any())
                {
                    query = query.Where(i => proponenti.Contains(i.UIDPersonaProponente.Value));
                }

                if (atti_da_firmare == null)
                    return await query
                        .CountAsync();

                if (atti_da_firmare.Any())
                {
                    query = query.Where(i => atti_da_firmare.Contains(i.UIDAtto));
                }

                return await query
                    .CountAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<ATTI_DASI_CONTATORI> GetContatore(int tipo, int tipo_risposta)
        {
            if (tipo == (int)TipoAttoEnum.MOZ
                || tipo == (int)TipoAttoEnum.ODG)
            {
                tipo_risposta = 0;
            }

            var query = PRContext
                .DASI_CONTATORI
                .Where(atto => atto.Tipo == tipo
                               && atto.Risposta == tipo_risposta);
            var result = await query.FirstAsync();

            return result;
        }

        public bool CheckIfPresentabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (!string.IsNullOrEmpty(dto.DataPresentazione)) return false;

            if (persona.IsSegreteriaAssemblea)
                if (dto.Firma_da_ufficio)
                    return true;

            // Se proponente non ha firmato non è possibile depositare
            if (!dto.Firmato_Dal_Proponente) return false;

            switch (persona.CurrentRole)
            {
                case RuoliIntEnum.Consigliere_Regionale:
                    {
                        if (persona.IsCapoGruppo) return dto.id_gruppo == persona.Gruppo.id_gruppo;

                        return dto.UIDPersonaProponente == persona.UID_persona;
                    }
                case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                case RuoliIntEnum.Presidente_Regione:
                case RuoliIntEnum.Amministratore_PEM:
                case RuoliIntEnum.Segreteria_Assemblea:
                    return false;
            }

            if (persona.Gruppo != null) return dto.id_gruppo == persona.Gruppo.id_gruppo;

            return false;
        }

        public bool CheckIfRitirabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (dto.IsChiuso) return false;
            if (persona.Gruppo == null) return false;

            if (dto.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (dto.DataRitiro.HasValue) return false;

            if (dto.IDStato == (int)StatiAttoEnum.BOZZA
                || dto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA)
                return false;

            return persona.UID_persona == dto.UIDPersonaProponente;
        }

        public bool CheckIfEliminabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (persona.Gruppo == null) return false;

            if (dto.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (!string.IsNullOrEmpty(dto.DataPresentazione)) return false;

            return persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                   || persona.CurrentRole == RuoliIntEnum.Segreteria_Politica
                   || persona.UID_persona == dto.UIDPersonaCreazione
                   || persona.UID_persona == dto.UIDPersonaProponente;
        }

        public bool CheckIfModificabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (string.IsNullOrEmpty(dto.Atto_Certificato))
                return dto.UIDPersonaProponente == persona.UID_persona
                       || dto.UIDPersonaCreazione == persona.UID_persona
                       || persona.IsSegreteriaPolitica
                       || persona.IsSegreteriaGiunta;

            return (dto.UIDPersonaProponente == persona.UID_persona ||
                    dto.UIDPersonaCreazione == persona.UID_persona)
                   && (dto.IDStato == (int)StatiAttoEnum.BOZZA ||
                       dto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA) && dto.ConteggioFirme == 1 &&
                   dto.Firmato_Dal_Proponente;
        }

        public async Task<int> GetProgressivo(TipoAttoEnum tipo, int gruppoId, int legislatura)
        {
            var query = PRContext.DASI
                .Where(atto => atto.id_gruppo == gruppoId
                               && atto.Tipo == (int)tipo
                               && atto.Legislatura == legislatura);
            query = query.OrderByDescending(em => em.Progressivo)
                .Take(1);

            var list = await query.ToListAsync();
            if (list.Count == 0) return 1;

            if (list[0].Progressivo.HasValue) return list[0].Progressivo.Value + 1;

            return 1;
        }

        public async Task<bool> CheckProgressivo(string etichettaEncrypt)
        {
            var query = PRContext
                .DASI
                .Where(e => true);
            return !await query.AnyAsync(e => e.NAtto == etichettaEncrypt);
        }

        public void IncrementaContatore(ATTI_DASI_CONTATORI contatore, int salto = 1)
        {
            var result = contatore.Contatore + salto;

            if (contatore.Fine.HasValue)
                if (result > contatore.Fine)
                    throw new Exception(
                        $"Limite superato. Attuali [{contatore.Contatore}], Limite [{contatore.Fine}], Richiesti [{salto}] - Disponibili [{contatore.Fine - contatore.Contatore}]");

            contatore.Contatore = result;
        }

        public async Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogabili()
        {
            var result = await PRContext
                .View_cariche_assessori_in_carica
                .OrderBy(item => item.ordine)
                .ThenBy(item => item.nome_carica)
                .ToListAsync();
            return result;
        }

        public async Task<List<View_Commissioni_attive>> GetCommissioniAttive()
        {
            var result = await PRContext
                .View_Commissioni_attive
                .ToListAsync();
            return result;
        }

        public async Task RimuoviCommissioni(Guid UidAtto)
        {
            var commissioni = await PRContext
                .ATTI_COMMISSIONI
                .Where(item => item.UIDAtto == UidAtto)
                .ToListAsync();
            PRContext.ATTI_COMMISSIONI.RemoveRange(commissioni);
        }

        public void AggiungiCommissione(Guid UidAtto, int organo)
        {
            PRContext.ATTI_COMMISSIONI.Add(new ATTI_COMMISSIONI
            {
                Uid = Guid.NewGuid(),
                UIDAtto = UidAtto,
                id_organo = organo
            });
        }

        public async Task<List<View_Commissioni_attive>> GetCommissioni(Guid uidAtto)
        {
            var result = await PRContext
                .ATTI_COMMISSIONI
                .Where(item => item.UIDAtto == uidAtto)
                .Select(item => item.id_organo)
                .ToListAsync();
            var query = PRContext
                .View_Commissioni_attive
                .Where(item => result.Contains(item.id_organo));
            return await query.ToListAsync();
        }

        public async Task RimuoviSoggetti(Guid UidAtto)
        {
            var soggetti = await PRContext
                .ATTI_SOGGETTI_INTERROGATI
                .Where(item => item.UIDAtto == UidAtto)
                .ToListAsync();
            PRContext.ATTI_SOGGETTI_INTERROGATI.RemoveRange(soggetti);
        }

        public void AggiungiSoggetto(Guid UidAtto, int soggetto)
        {
            PRContext.ATTI_SOGGETTI_INTERROGATI.Add(new ATTI_SOGGETTI_INTERROGATI
            {
                Uid = Guid.NewGuid(),
                UIDAtto = UidAtto,
                id_carica = soggetto
            });
        }

        public async Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogati(Guid uidAtto)
        {
            var result = await PRContext
                .ATTI_SOGGETTI_INTERROGATI
                .Where(item => item.UIDAtto == uidAtto)
                .Select(item => item.id_carica)
                .ToListAsync();
            var query = PRContext
                .View_cariche_assessori_in_carica
                .Where(item => result.Contains(item.id_carica));
            return await query.ToListAsync();
        }

        /// <summary>
        ///     Riepilogo inviti
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetInvitati(Guid attoUId)
        {
            try
            {
                var query = PRContext
                    .NOTIFICHE
                    .Where(n => n.UIDAtto == attoUId && n.IDTipo == (int)TipoNotificaEnum.INVITO)
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
                        .SingleAsync(n => n.UIDNotifica == notificaUId && n.IDTipo == (int)TipoNotificaEnum.INVITO);
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

        public async Task<int> CountByQuery(ByQueryModel model)
        {
            var query = await PRContext
                .DASI
                .SqlQuery(model.Query)
                .ToListAsync();

            return query.Count;
        }

        public List<Guid> GetByQuery(ByQueryModel model)
        {
            var query = PRContext
                .DASI
                .SqlQuery(model.Query)
                .Skip((model.page - 1) * 100)
                .Take(100);

            return query
                .Select(item => item.UIDAtto)
                .ToList();
        }

        public async Task<string> GetAll_Query(PersonaDto persona, ClientModeEnum mode, Filter<ATTI_DASI> filtro,
            List<int> soggetti,
            List<int> stati)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in filtro.Statements)
                if (f.PropertyId != nameof(ATTI_DASI.Oggetto))
                    filtro2._statements.Add(f);
                else if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                    query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                || item.Premesse.Contains(f.Value.ToString())
                                                || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta.Contains(f.Value.ToString()));

            filtro2.BuildExpression(ref query);

            if (mode == ClientModeEnum.GRUPPI)
            {
                query = query.Where(atto => atto.IDStato != (int)StatiAttoEnum.BOZZA_RISERVATA
                                            || (atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA
                                                && (atto.UIDPersonaCreazione == persona.UID_persona
                                                    || atto.UIDPersonaProponente == persona.UID_persona)));

                if (persona.IsSegreteriaAssemblea)
                    query = query.Where(item => item.IDStato >= (int)StatiAttoEnum.PRESENTATO);
                else if (!persona.IsSegreteriaAssemblea
                         && !persona.IsPresidente)
                    query = query.Where(item => item.id_gruppo == persona.Gruppo.id_gruppo);
            }
            else
            {
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                            && item.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);
            }

            if (stati.Any())
                query = query.Where(i => stati.Contains(i.IDStato));

            if (soggetti != null)
                if (soggetti.Count > 0)
                {
                    //Avvio ricerca soggetti
                    var list = await PRContext
                        .ATTI_SOGGETTI_INTERROGATI
                        .Where(f => soggetti.Contains(f.id_carica))
                        .Select(f => f.UIDAtto)
                        .ToListAsync();
                    query = query
                        .Where(item => list.Contains(item.UIDAtto));
                }

            var statoFilter = filtro.Statements.FirstOrDefault(i => i.PropertyId == nameof(ATTI_DASI.IDStato));
            if (statoFilter != null)
                if (Convert.ToInt16(statoFilter.Value) == (int)StatiAttoEnum.BOZZA)
                    return query
                        .OrderBy(item => item.Tipo)
                        .ThenByDescending(item => item.DataCreazione)
                        .ToTraceQuery();

            return query
                .OrderBy(item => item.Tipo)
                .ThenBy(item => item.NAtto_search)
                .ToTraceQuery();
        }

        public async Task<List<Guid>> GetAbbinamentiMozione(Guid uidAtto)
        {
            return await PRContext
                .DASI
                .Where(atto => atto.UID_MOZ_Abbinata == uidAtto
                               && atto.IDStato >= (int)StatiAttoEnum.PRESENTATO
                               && !atto.Eliminato)
                .Select(atto => atto.UIDAtto)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetAllCartacei(int legislatura)
        {
            return await PRContext
                .DASI
                .Where(atto => atto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA
                               && atto.Legislatura == legislatura
                               && !atto.Eliminato)
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Select(atto => atto.UIDAtto)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetAttiProponente(Guid personaUid)
        {
            return await PRContext
                .DASI
                .Where(dasi => dasi.UIDPersonaProponente == personaUid
                               && !dasi.UIDPersonaPrimaFirma.HasValue
                               && dasi.IDStato < (int)StatiAttoEnum.PRESENTATO
                               && !dasi.Eliminato)
                .Select(dasi => dasi.UIDAtto)
                .ToListAsync();
        }

        public async Task<List<ATTI_DASI>> GetMOZAbbinabili(Guid sedutaUId)
        {
            var query = PRContext
                .DASI
                .Where(a => !a.Eliminato
                            && a.IDStato >= (int)StatiAttoEnum.PRESENTATO
                            && a.Tipo == (int)TipoAttoEnum.MOZ
                            && a.TipoMOZ != (int)TipoMOZEnum.URGENTE
                            && a.DataIscrizioneSeduta.HasValue
                            && a.UIDSeduta == sedutaUId);
            return await query.ToListAsync();
        }

        public async Task<List<ATTI_DASI>> GetAttiBySeduta(Guid uidSeduta, TipoAttoEnum tipo, TipoMOZEnum tipoMoz)
        {
            var query = PRContext.DASI.Where(atto => !atto.Eliminato
                                                     && atto.UIDSeduta == uidSeduta
                                                     && atto.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                                     && atto.DataIscrizioneSeduta.HasValue
                                                     && atto.Tipo == (int)tipo);

            if (tipoMoz != TipoMOZEnum.ORDINARIA) query = query.Where(atto => atto.TipoMOZ == (int)tipoMoz);

            return await query.ToListAsync();
        }

        public async Task<List<ATTI_DASI>> GetProposteAtti(int gruppoId, TipoAttoEnum tipo, TipoMOZEnum tipoMoz)
        {
            //Matteo Cattapan #460
            //Aggiunta clausola per rimuovere gli atti in stato "CHIUSO"
            var query = PRContext.DASI.Where(atto => !atto.Eliminato
                                                     && atto.id_gruppo == gruppoId
                                                     && atto.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                                     && atto.Tipo == (int)tipo
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);

            if (tipoMoz != TipoMOZEnum.ORDINARIA) query = query.Where(atto => atto.TipoMOZ == (int)tipoMoz);

            return await query.ToListAsync();
        }

        public async Task<List<ATTI_DASI>> GetProposteAtti(string dataRichiesta, TipoAttoEnum tipo, TipoMOZEnum tipoMoz)
        {
            var query = PRContext.DASI.Where(atto => !atto.Eliminato
                                                     && atto.DataRichiestaIscrizioneSeduta.Equals(dataRichiesta)
                                                     && atto.Tipo == (int)tipo
                                                     && atto.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                                     && atto.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);

            if (tipoMoz != TipoMOZEnum.ORDINARIA)
            {
                query = query.Where(atto => atto.TipoMOZ == (int)tipoMoz);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountODGByAttoPEM(Guid uidAtto)
        {
            return await PRContext
                .DASI
                .CountAsync(item => !item.Eliminato
                                    && item.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                    && item.UID_Atto_ODG == uidAtto
                                    && item.DataIscrizioneSeduta.HasValue
                                    && item.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                                    && item.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO);
        }

        public async Task<bool> CheckIscrizioneSedutaIQT(string dataRichiesta, Guid uidPersona)
        {
            var res = true;

            var atti_proposti_in_seduta = await PRContext.DASI
                .Where(i => i.DataRichiestaIscrizioneSeduta.Equals(dataRichiesta)
                            && i.Tipo == (int)TipoAttoEnum.IQT
                            && i.IDStato >= (int)StatiAttoEnum.PRESENTATO
                            && i.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                            && i.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO)
                .ToListAsync();

            foreach (var attiDasi in atti_proposti_in_seduta)
            {
                var firmatari = await PRContext.ATTI_FIRME
                    .Where(i => i.UIDAtto == attiDasi.UIDAtto && string.IsNullOrEmpty(i.Data_ritirofirma) && i.Prioritario)
                    .ToListAsync();
                if (firmatari.Any(i => i.UID_persona == uidPersona)) res = false;
            }

            return res;
        }

        public async Task<bool> CheckMOZUrgente(SEDUTE seduta, string dataSedutaEncrypt, Guid personaUID)
        {
            var res = true;
            var atti_proposti_in_seduta = await PRContext.DASI
                .Where(i => !i.Eliminato
                            && i.IDStato >= (int)StatiAttoEnum.PRESENTATO
                            && i.IDStato != (int)StatiAttoEnum.CHIUSO_RITIRATO
                            && i.IDStato != (int)StatiAttoEnum.CHIUSO_DECADUTO
                            && (i.UIDSeduta == seduta.UIDSeduta
                                || i.DataRichiestaIscrizioneSeduta == dataSedutaEncrypt)
                            && i.Tipo == (int)TipoAttoEnum.MOZ
                            && i.TipoMOZ == (int)TipoMOZEnum.URGENTE
                            && !i.MOZU_Capigruppo)
                .ToListAsync();

            foreach (var attiDasi in atti_proposti_in_seduta)
            {
                var firmatari = await PRContext.ATTI_FIRME
                    .Where(i => i.UIDAtto == attiDasi.UIDAtto && string.IsNullOrEmpty(i.Data_ritirofirma) && i.Prioritario)
                    .ToListAsync();
                if (firmatari.Any(i => i.UID_persona == personaUID))
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        public async Task<bool> CheckIfFirmatoDaiCapigruppo(Guid uidAtto)
        {
            //Matteo Cattapan #501
            //Vengono controllati tutti i capigruppo presenti nella vista View_CAPIGRUPPO
            //Nel caso non sia presente la firma di uno dei capigruppo viene restituito FALSE
            var firme = await PRContext.ATTI_FIRME
                .Where(i => i.UIDAtto == uidAtto && string.IsNullOrEmpty(i.Data_ritirofirma)).ToListAsync();
            var capigruppo = await PRContext.View_CAPIGRUPPO.ToListAsync();

            foreach (var capogruppo in capigruppo)
            {
                var firma_capogruppo_presente =
                    firme.FirstOrDefault(firma => firma.UID_persona == capogruppo.UID_persona);
                if (firma_capogruppo_presente == null) return false;
            }

            return true;
        }

        private Expression<Func<ATTI_DASI, bool>> FilterByClosed()
        {
            return x => x.IDStato == (int)StatiAttoEnum.CHIUSO
                        || x.IDStato == (int)StatiAttoEnum.CHIUSO_RITIRATO
                        || x.IDStato == (int)StatiAttoEnum.CHIUSO_DECADUTO;
        }
    }
}