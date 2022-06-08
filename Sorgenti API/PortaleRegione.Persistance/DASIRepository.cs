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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

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

        public async Task<List<Guid>> GetAll(PersonaDto persona, int page, int size, Filter<ATTI_DASI> filtro = null,
            List<int> soggetti = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            filtro?.BuildExpression(ref query);

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea)
            {
                query = query.Where(item => item.IDStato >= (int) StatiAttoEnum.PRESENTATO);
            }

            if (soggetti != null)
            {
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
            }

            return await query
                .OrderByDescending(item => item.OrdineVisualizzazione)
                .Select(item => item.UIDAtto)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(PersonaDto persona, Filter<ATTI_DASI> filtro, List<int> soggetti)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            filtro?.BuildExpression(ref query);

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea)
            {
                query = query.Where(item => item.IDStato >= (int)StatiAttoEnum.PRESENTATO);
            }

            if (soggetti != null)
            {
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
            }

            return await query
                .CountAsync();
        }

        public async Task<int> Count(Filter<ATTI_DASI> filtro)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato);

            filtro?.BuildExpression(ref query);

            return await query
                .CountAsync();
        }
        
        public async Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, StatiAttoEnum stato, Guid sedutaId,
            ClientModeEnum clientMode, Filter<ATTI_DASI> filtro = null, List<int> soggetti = null)
        {
            try
            {
                var query = PRContext
                    .DASI
                    .Where(item => !item.Eliminato);

                filtro?.BuildExpression(ref query);

                if (clientMode == ClientModeEnum.GRUPPI)
                {
                    if (stato != StatiAttoEnum.TUTTI)
                    {
                        if (stato == StatiAttoEnum.PRESENTATO
                            && persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale)
                            query = query.Where(item => item.IDStato >= (int)stato);
                        else
                        {
                            query = query.Where(item => item.IDStato == (int)stato);
                        }
                    }

                    if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo == (int) tipo);
                }
                else
                {
                    query = query.Where(item => item.UIDSeduta == sedutaId
                                                && item.IDStato >= (int) StatiAttoEnum.IN_TRATTAZIONE);
                    if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo == (int) tipo);
                }

                if (soggetti != null)
                {
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

        public async Task<ATTI_DASI_CONTATORI> GetContatore(TipoAttoEnum tipo, int tipo_risposta)
        {
            var query = PRContext
                .DASI_CONTATORI
                .Where(atto => atto.Tipo == (int) tipo
                               && atto.Risposta == tipo_risposta);
            var result = await query.FirstAsync();

            return result;
        }

        public async Task<int> GetOrdine(int tipo)
        {
            //TODO: CALCOLA L'ORDINE IN BASE ALLA PROGRESSIONE
            //ULTIMO DEI CONTATORI 
            var random = new Random();
            return random.Next(9, 999);
        }

        public async Task<bool> CheckIfPresentabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (!string.IsNullOrEmpty(dto.DataPresentazione)) return false;

            if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea)
                if (dto.Firma_da_ufficio)
                    return true;

            // Se proponente non ha firmato non è possibile depositare
            if (!dto.Firmato_Dal_Proponente) return false;

            switch (persona.CurrentRole)
            {
                case RuoliIntEnum.Consigliere_Regionale:
                case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                case RuoliIntEnum.Presidente_Regione:
                    return dto.UIDPersonaProponente == persona.UID_persona;
                case RuoliIntEnum.Amministratore_PEM:
                case RuoliIntEnum.Segreteria_Assemblea:
                    return true;
            }

            if (persona.Gruppo != null) return dto.id_gruppo == persona.Gruppo.id_gruppo;

            return false;
        }

        public bool CheckIfRitirabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (persona.Gruppo == null) return false;

            if (dto.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (dto.DataRitiro.HasValue) return false;

            if (dto.IDStato != (int) StatiAttoEnum.PRESENTATO) return false;

            return persona.UID_persona == dto.UIDPersonaProponente
                   || persona.CurrentRole == RuoliIntEnum.Presidente_Regione;
        }

        public bool CheckIfEliminabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (persona.Gruppo == null) return false;

            if (dto.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (!string.IsNullOrEmpty(dto.DataPresentazione)) return false;

            return persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                   || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                   || persona.UID_persona == dto.UIDPersonaCreazione;
        }

        public bool CheckIfModificabile(AttoDASIDto dto, PersonaDto persona)
        {
            if (string.IsNullOrEmpty(dto.Atto_Certificato))
                return dto.UIDPersonaProponente == persona.UID_persona
                       || dto.UIDPersonaCreazione == persona.UID_persona
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica
                       || persona.CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta;

            return (dto.UIDPersonaProponente == persona.UID_persona || dto.UIDPersonaCreazione == persona.UID_persona)
                   && (dto.IDStato == (int) StatiEnum.Bozza || dto.IDStato == (int) StatiEnum.Bozza_Riservata)
                   && dto.ConteggioFirme == 1;
        }

        public async Task<int> GetProgressivo(TipoAttoEnum tipo, int gruppoId, int legislatura)
        {
            var query = PRContext.DASI
                .Where(atto => atto.id_gruppo == gruppoId
                               && atto.Tipo == (int) tipo
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

        public void IncrementaContatore(ATTI_DASI_CONTATORI contatore)
        {
            contatore.Contatore += 1;
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
                    .Where(n => n.UIDAtto == attoUId)
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

        public async Task<int> CountByQuery(string query)
        {
            return await PRContext
                .DASI
                .SqlQuery(query)
                .CountAsync();
        }

        public List<Guid> GetByQuery(ByQueryModel model)
        {
            var query = PRContext
                .DASI
                .SqlQuery(model.Query)
                .Skip((model.page - 1) * 100)
                .Take(100);

            return query
                .Select(item=>item.UIDAtto)
                .ToList();
        }

        public string GetAll_Query(Filter<ATTI_DASI> filtro)
        {
            var query = PRContext
               .DASI
               .Where(em => !em.Eliminato);
            
            filtro?.BuildExpression(ref query);
            
            var sql = query.ToTraceQuery();
            return sql;
        }
    }
}