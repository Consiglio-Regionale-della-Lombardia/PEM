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
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using PortaleRegione.BAL;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Routes;
using Z.EntityFramework.Plus;

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

        public async Task<List<Guid>> GetAll(PersonaDto currentUser,
            int page,
            int size,
            ClientModeEnum mode,
            Filter<ATTI_DASI> filtro,
            QueryExtendedRequest queryExtended,
            List<SortingInfo> dettaglioOrdinamento)
        {
            var ordinamento_numero_atto = filtro.Statements.Any(f => f.PropertyId == nameof(ATTI_DASI.NAtto));
            List<int> userOrder = null;
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            query = ApplyFilters(query, filtro.Statements, queryExtended, mode, currentUser, ref userOrder);

            // #990
            if (dettaglioOrdinamento != null && dettaglioOrdinamento.Any())
            {
                var properties = typeof(AttiDASISorting).GetProperties();
                IOrderedQueryable<ATTI_DASI> orderedQuery = null;

                foreach (var dettaglio in dettaglioOrdinamento)
                {
                    // Cercare la proprietà corrispondente
                    var propertyInfo = properties.FirstOrDefault(p => p.Name == dettaglio.propertyName);
                    if (propertyInfo != null)
                    {
                        var parameter = Expression.Parameter(typeof(ATTI_DASI), "item");
                        var property = Expression.Property(parameter, dettaglio.propertyName);
                        var lambda = Expression.Lambda(property, parameter);

                        // Ordinamento crescente o decrescente
                        if (dettaglio.sortDirection == 1) // Crescente
                        {
                            orderedQuery = orderedQuery == null
                                ? Queryable.OrderBy((dynamic)query, (dynamic)lambda)
                                : Queryable.ThenBy((dynamic)orderedQuery, (dynamic)lambda);
                        }
                        else // Decrescente
                        {
                            orderedQuery = orderedQuery == null
                                ? Queryable.OrderByDescending((dynamic)query, (dynamic)lambda)
                                : Queryable.ThenByDescending((dynamic)orderedQuery, (dynamic)lambda);
                        }

                        // Se la proprietà corrente è "DCR", aggiungi l'ordinamento su "DCCR"
                        if (dettaglio.propertyName == nameof(ATTI_DASI.DCR))
                        {
                            var secondaryProperty = Expression.Property(parameter, nameof(ATTI_DASI.DCCR));
                            var secondaryLambda = Expression.Lambda(secondaryProperty, parameter);

                            if (dettaglio.sortDirection == 1) // Crescente
                            {
                                orderedQuery = Queryable.ThenBy((dynamic)orderedQuery, (dynamic)secondaryLambda);
                            }
                            else // Decrescente
                            {
                                orderedQuery =
                                    Queryable.ThenByDescending((dynamic)orderedQuery, (dynamic)secondaryLambda);
                            }
                        }
                    }
                }

                if (orderedQuery != null)
                {
                    query = orderedQuery;

                    if (page > 0 && size > 0)
                        return await query
                            .Select(item => item.UIDAtto)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .ToListAsync();

                    return await query
                        .Select(item => item.UIDAtto)
                        .ToListAsync();
                }
            }

            if (queryExtended.Stati.Any())
                if (queryExtended.Stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                {
                    if (page > 0 && size > 0)
                        return await query
                            .OrderBy(item => item.Tipo)
                            .ThenByDescending(item => item.DataCreazione)
                            .Select(item => item.UIDAtto)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .ToListAsync();

                    return await query
                        .OrderBy(item => item.Tipo)
                        .ThenByDescending(item => item.DataCreazione)
                        .Select(item => item.UIDAtto)
                        .ToListAsync();
                }

            if (ordinamento_numero_atto)
            {
                if (page > 0 && size > 0)
                {
                    var resultReorderPaginated = await query
                        .OrderBy(a => a.Eliminato)
                        .Select(item => new { item.UIDAtto, item.NAtto_search })
                        .Skip((page - 1) * size)
                        .Take(size)
                        .ToListAsync();

                    return resultReorderPaginated.OrderBy(r => userOrder.IndexOf(r.NAtto_search)).Select(r => r.UIDAtto)
                        .ToList();
                }

                var result = await query
                    .OrderBy(a => a.Eliminato)
                    .Select(item => new { item.UIDAtto, item.NAtto_search })
                    .ToListAsync();

                return result.OrderBy(r => userOrder.IndexOf(r.NAtto_search)).Select(r => r.UIDAtto).ToList();
            }

            if (page > 0 && size > 0)
                return await query
                    .OrderBy(item => item.Tipo)
                    .ThenByDescending(item => item.NAtto_search)
                    .Select(item => item.UIDAtto)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

            return await query
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Select(item => item.UIDAtto)
                .ToListAsync();
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

        public async Task<ATTI_DASI_CONTATORI> GetContatore(int tipo, int tipo_risposta)
        {
            if (tipo == (int)TipoAttoEnum.MOZ
                || tipo == (int)TipoAttoEnum.ODG)
                tipo_risposta = 0;

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
            if (commissioni.Any())
                PRContext.ATTI_COMMISSIONI.RemoveRange(commissioni);
        }

        public async Task RimuoviCommissioniProponenti(Guid UidAtto)
        {
            var commissioniProponenti = await PRContext
                .ATTI_PROPONENTI
                .Where(item => item.UIDAtto == UidAtto)
                .ToListAsync();
            PRContext.ATTI_PROPONENTI.RemoveRange(commissioniProponenti);
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

        public async Task AggiungiCommissioneProponente(Guid UidAtto, KeyValueDto organo)
        {
            var newObject = new ATTI_PROPONENTI
            {
                Uid = Guid.NewGuid(),
                UIDAtto = UidAtto
            };

            if (organo.id > 0)
            {
                newObject.IdOrgano = organo.id;
                var organoInDb = await PRContext.View_Commissioni_attive.FindAsync(organo.id);
                newObject.DescrizioneOrgano = organoInDb.nome_organo;
            }
            else
            {
                newObject.UidPersona = new Guid(organo.uid);
            }

            PRContext.ATTI_PROPONENTI.Add(newObject);
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
                .SqlQuery(model.Query);

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
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
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

        public async Task<List<AttiRisposteDto>> GetRisposte(Guid uidAtto)
        {
            var dataFromDb = await PRContext
                .ATTI_RISPOSTE
                .Where(r => r.UIDAtto == uidAtto)
                .OrderByDescending(r => r.Data)
                .ToListAsync();

            var res = new List<AttiRisposteDto>();
            foreach (var r in dataFromDb.Where(risp =>
                         !risp.UIDRispostaAssociata.HasValue || risp.UIDRispostaAssociata == Guid.Empty))
            {
                var risposta = new AttiRisposteDto
                {
                    UIDAtto = r.UIDAtto,
                    Data = r.Data,
                    DataTrasmissione = r.DataTrasmissione,
                    DataTrattazione = r.DataTrattazione,
                    DataRevoca = r.DataRevoca,
                    DescrizioneOrgano = r.DescrizioneOrgano,
                    IdOrgano = r.IdOrgano,
                    Uid = r.Uid,
                    Tipo = r.Tipo,
                    DisplayTipo = Utility.GetText_TipoRispostaDASI(r.Tipo),
                    TipoOrgano = r.TipoOrgano,
                    DisplayTipoOrgano = Utility.GetText_TipoOrganoDASI(r.TipoOrgano),
                    UIDDocumento = r.UIDDocumento,
                    UIDRispostaAssociata = r.UIDRispostaAssociata
                };

                var risposteAssociate = dataFromDb.Where(risp => risp.UIDRispostaAssociata == r.Uid).ToList();

                if (risposteAssociate.Any())
                {
                    foreach (var rispostaAssociata in risposteAssociate)
                    {
                        risposta.RisposteAssociate.Add(new AttiRisposteDto
                        {
                            UIDAtto = rispostaAssociata.UIDAtto,
                            DescrizioneOrgano = rispostaAssociata.DescrizioneOrgano,
                            IdOrgano = rispostaAssociata.IdOrgano,
                            Uid = rispostaAssociata.Uid,
                            Tipo = rispostaAssociata.Tipo,
                            DisplayTipo = Utility.GetText_TipoRispostaDASI(rispostaAssociata.Tipo),
                            TipoOrgano = rispostaAssociata.TipoOrgano,
                            DisplayTipoOrgano = Utility.GetText_TipoOrganoDASI(rispostaAssociata.TipoOrgano)
                        });
                    }
                }

                res.Add(risposta);
            }

            return res;
        }

        public async Task<List<AttiMonitoraggioDto>> GetMonitoraggi(Guid uidAtto)
        {
            var dataFromDb = await PRContext
                .ATTI_MONITORAGGIO
                .Where(r => r.UIDAtto == uidAtto)
                .ToListAsync();

            return dataFromDb.Select(r => new AttiMonitoraggioDto
            {
                UIDAtto = r.UIDAtto,
                DescrizioneOrgano = r.DescrizioneOrgano,
                IdOrgano = r.IdOrgano,
                TipoOrgano = r.TipoOrgano,
                DisplayTipoOrgano = Utility.GetText_TipoOrganoDASI(r.TipoOrgano)
            }).ToList();
        }

        public async Task<List<AttiDocumentiDto>> GetDocumenti(Guid uidAtto)
        {
            var docInDB = await PRContext
                .ATTI_DOCUMENTI
                .Where(d => d.UIDAtto == uidAtto)
                .OrderBy(d => d.Data)
                .ToListAsync();

            return docInDB
                .Select(d => new AttiDocumentiDto
                {
                    Uid = d.Uid,
                    Tipo = ((TipoDocumentoEnum)d.Tipo).ToString(),
                    Titolo = d.Titolo,
                    Pubblico = d.Pubblica,
                    Link = $"{AppSettingsConfiguration.URL_API}/{ApiRoutes.DASI.DownloadDoc}?path={d.Path}",
                    TipoEnum = (TipoDocumentoEnum)d.Tipo
                })
                .ToList();
        }

        public async Task<List<KeyValueDto>> GetCommissioniProponenti(Guid uidAtto)
        {
            var proponentiInDb = await PRContext
                .ATTI_PROPONENTI
                .Where(d => d.UIDAtto == uidAtto)
                .ToListAsync();
            var res = new List<KeyValueDto>();
            foreach (var item in proponentiInDb)
            {
                if (item.IdOrgano.HasValue)
                    res.Add(new KeyValueDto()
                    {
                        descr = item.DescrizioneOrgano,
                        id = item.IdOrgano.Value
                    });
                else
                {
                    var persona = await PRContext.View_UTENTI.FindAsync(item.UidPersona.Value);
                    res.Add(new KeyValueDto()
                    {
                        descr = $"{persona.cognome} {persona.nome}",
                        uid = persona.UID_persona.Value.ToString()
                    });
                }
            }

            return res.OrderBy(p => p.descr).ToList();
        }

        public void AggiungiAbbinamento(Guid requestUidAbbinamento, Guid requestUidAttoAbbinato)
        {
            PRContext.ATTI_ABBINAMENTI.Add(new ATTI_ABBINAMENTI
            {
                Uid = Guid.NewGuid(),
                Data = DateTime.Now,
                UIDAtto = requestUidAbbinamento,
                UIDAttoAbbinato = requestUidAttoAbbinato
            });
        }

        public async Task<ATTI_ABBINAMENTI> GetAbbinamento(Guid requestUidAbbinamento, Guid requestUidAttoAbbinato)
        {
            return await PRContext.ATTI_ABBINAMENTI.FirstOrDefaultAsync(a => a.UIDAtto == requestUidAbbinamento
                                                                             && a.UIDAttoAbbinato ==
                                                                             requestUidAttoAbbinato);
        }

        public void RimuoviAbbinamento(ATTI_ABBINAMENTI abbinamentoInDb)
        {
            PRContext.ATTI_ABBINAMENTI.Remove(abbinamentoInDb);
        }

        public void AggiungiRisposta(ATTI_RISPOSTE risposta)
        {
            PRContext.ATTI_RISPOSTE.Add(risposta);
        }

        public void RimuoviRisposta(ATTI_RISPOSTE risposta)
        {
            PRContext.ATTI_RISPOSTE.Remove(risposta);
        }

        public async Task<ATTI_RISPOSTE> GetRisposta(Guid requestUid)
        {
            return await PRContext.ATTI_RISPOSTE.FirstOrDefaultAsync(r => r.Uid == requestUid);
        }

        public async Task<ATTI_MONITORAGGIO> GetMonitoraggio(Guid requestUid)
        {
            return await PRContext.ATTI_MONITORAGGIO.FirstOrDefaultAsync(m => m.Uid == requestUid);
        }

        public void AggiungiMonitoraggio(ATTI_MONITORAGGIO monitoraggio)
        {
            PRContext.ATTI_MONITORAGGIO.Add(monitoraggio);
        }

        public void RimuoviMonitoraggio(ATTI_MONITORAGGIO monitoraggioInDb)
        {
            PRContext.ATTI_MONITORAGGIO.Remove(monitoraggioInDb);
        }

        public async Task<ATTI_NOTE> GetNota(Guid requestUidAtto, TipoNotaEnum requestTipoEnum)
        {
            return await PRContext.ATTI_NOTE.FirstOrDefaultAsync(n => n.UIDAtto == requestUidAtto
                                                                      && n.Tipo == (int)requestTipoEnum);
        }

        public void RimuoviNota(ATTI_NOTE notaInDb)
        {
            PRContext.ATTI_NOTE.Remove(notaInDb);
        }

        public void AggiungiNota(ATTI_NOTE notaInDb)
        {
            PRContext.ATTI_NOTE.Add(notaInDb);
        }

        public void AggiungiDocumento(ATTI_DOCUMENTI doc)
        {
            PRContext.ATTI_DOCUMENTI.Add(doc);
        }

        public async Task<ATTI_DOCUMENTI> GetDocumento(Guid requestUid)
        {
            return await PRContext.ATTI_DOCUMENTI.FirstAsync(d => d.Uid.Equals(requestUid));
        }

        public async Task<List<ATTI_DOCUMENTI>> GetDocumento(Guid UIdAtto, TipoDocumentoEnum tipoDocumento)
        {
            return await PRContext.ATTI_DOCUMENTI
                .Where(d => d.UIDAtto.Equals(UIdAtto) 
                            && d.Tipo.Equals((int)tipoDocumento))
                .ToListAsync();
        }

        public void RimuoviDocumento(ATTI_DOCUMENTI doc)
        {
            PRContext.ATTI_DOCUMENTI.Remove(doc);
        }

        public async Task<ATTI_DASI> GetByEtichetta(string etichettaProgressiva)
        {
            return await PRContext.DASI.FirstOrDefaultAsync(atto => atto.Etichetta.Equals(etichettaProgressiva));
        }

        public async Task<Guid> GetByQR(Guid id)
        {
            var res = await PRContext
                .DASI
                .FirstAsync(atto => atto.UID_QRCode == id);

            return res.UIDAtto;
        }

        public async Task<bool> CheckDCR(string dcrl, string dcr, string dccr)
        {
            var query = PRContext
                .DASI
                .Where(a => a.DCRL.Equals(dcrl));

            var intDcr = int.Parse(dcr);
            var intDccr = int.Parse(dccr);

            if (intDcr > 0)
            {
                query = query.Where(a => a.DCR.Value.Equals(intDcr));
            }

            if (intDccr > 0)
            {
                query = query.Where(a => a.DCCR.Value.Equals(intDccr));
            }

            var res = await query.AnyAsync();
            return res;
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

        public async Task<List<NoteDto>> GetNote(Guid uidAtto)
        {
            var noteInDB = await PRContext
                .ATTI_NOTE
                .Where(d => d.UIDAtto == uidAtto)
                .OrderByDescending(d => d.Data)
                .ToListAsync();
            var res = new List<NoteDto>();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();

            foreach (var nota in noteInDB)
            {
                var persona = await PRContext.View_UTENTI.FindAsync(nota.UIDPersona);
                res.Add(new NoteDto
                {
                    Tipo = Utility.GetText_TipoNotaDASI(nota.Tipo),
                    TipoEnum = (TipoNotaEnum)nota.Tipo,
                    Data = nota.Data,
                    Uid = nota.Uid,
                    Nota = nota.Nota,
                    Persona = new PersonaLightDto(persona.cognome, persona.nome)
                });
            }

            return res;
        }

        public async Task<List<AttiAbbinamentoDto>> GetAbbinamenti(Guid uidAtto)
        {
            var abbinamentiInDB = await PRContext
                .ATTI_ABBINAMENTI
                .Where(a => a.UIDAtto.Equals(uidAtto))
                .OrderBy(a => a.Data)
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
                    abbinata.UidAttoAbbinato = attoAbbinato.UIDAtto;
                }

                res.Add(abbinata);
            }

            return res;
        }

        public async Task<List<AttoLightDto>> GetAbbinamentiDisponibili(int legislaturaId, int page, int size)
        {
            var query = PRContext
                .VIEW_ATTI
                .Where(a => a.Legislatura.Equals(legislaturaId))
                .Select(a => new AttoLightDto
                {
                    tipo = a.Tipo.ToString(),
                    natto = a.NAtto,
                    uidAtto = a.UIDAtto,
                    oggetto = a.Oggetto
                });

            return await query
                .OrderBy(abb => abb.tipo)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<GruppiDto>> GetGruppiDisponibili(int legislaturaId, int page, int size)
        {
            var query = PRContext
                .View_gruppi_politici_ws
                .Where(a => a.id_legislatura.Equals(legislaturaId))
                .Distinct()
                .Select(a => new GruppiDto()
                {
                    id_gruppo = a.id_gruppo,
                    nome_gruppo = a.nome_gruppo,
                    codice_gruppo = a.codice_gruppo,
                    data_inizio = a.data_inizio,
                    data_fine = a.data_fine
                });

            return await query
                .OrderBy(abb => abb.nome_gruppo)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<OrganoDto>> GetOrganiDisponibili(int legislaturaId)
        {
            var result = new List<OrganoDto>();

            var queryCommissioni = PRContext
                .View_Commissioni_per_legislatura
                .Where(a => a.id_legislatura.Equals(legislaturaId))
                .Distinct()
                .Select(a => new OrganoDto
                {
                    tipo_organo = TipoOrganoEnum.COMMISSIONE,
                    nome_organo = a.nome_organo,
                    id_organo = a.id_organo
                });

            result.AddRange(await queryCommissioni.ToListAsync());

            var queryOrgani = PRContext
                .View_cariche_assessori_per_legislatura
                .Where(a => a.id_legislatura.Equals(legislaturaId))
                .Distinct()
                .Select(a => new OrganoDto
                {
                    tipo_organo = TipoOrganoEnum.GIUNTA,
                    nome_organo = a.nome_carica,
                    id_organo = a.id_carica
                });

            result.AddRange(await queryOrgani.ToListAsync());

            return result.Distinct().ToList();
        }

        public async Task<int> CountByTipo(PersonaDto persona, TipoAttoEnum tipo)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (persona.Gruppo != null)
            {
                if (persona.Gruppo.id_gruppo > 0)
                {
                    query = query.Where(item => item.id_gruppo.Equals(persona.Gruppo.id_gruppo));
                }
            }

            if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo.Equals((int)tipo));

            return await query
                .CountAsync();
        }

        public async Task<int> CountByStato(PersonaDto persona, List<int> tipo, StatiAttoEnum stato)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (persona.Gruppo != null)
            {
                if (persona.Gruppo.id_gruppo > 0)
                {
                    query = query.Where(item => item.id_gruppo.Equals(persona.Gruppo.id_gruppo));
                }
            }

            if (stato != StatiAttoEnum.TUTTI) query = query.Where(item => item.IDStato.Equals((int)stato));
            if (tipo.Any()) query = query.Where(item => tipo.Contains(item.Tipo));

            return await query
                .CountAsync();
        }

        public async Task<int> Count(PersonaDto currentUser, TipoAttoEnum tipo, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended)
        {
            var userOrder = new List<int>();
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (!currentUser.IsSegreteriaAssemblea)
            {
                queryExtended.Tipi.Clear();
                queryExtended.Stati.Clear();
            }

            query = ApplyFilters(query, queryFilter.Statements, queryExtended, clientMode, currentUser, ref userOrder);

            if (tipo != TipoAttoEnum.TUTTI) query = query.Where(item => item.Tipo.Equals((int)tipo));

            var res = await query.CountAsync();
            return res;
        }

        public async Task<int> Count(PersonaDto currentUser, StatiAttoEnum stato, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended)
        {
            var userOrder = new List<int>();
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (!currentUser.IsSegreteriaAssemblea)
            {
                queryExtended.Stati.Clear();
            }

            query = ApplyFilters(query, queryFilter.Statements, queryExtended, clientMode, currentUser, ref userOrder);
            if (stato != StatiAttoEnum.TUTTI) query = query.Where(item => item.IDStato.Equals((int)stato));

            return await query
                .CountAsync();
        }

        public async Task<int> Count(PersonaDto currentUser, ClientModeEnum clientMode, Filter<ATTI_DASI> queryFilter,
            QueryExtendedRequest queryExtended)
        {
            var userOrder = new List<int>();
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            query = ApplyFilters(query, queryFilter.Statements, queryExtended, clientMode, currentUser, ref userOrder);

            return await query
                .CountAsync();
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
                                                     && !atto.TipoChiusuraIter.HasValue);

            if (tipoMoz != TipoMOZEnum.ORDINARIA) query = query.Where(atto => atto.TipoMOZ == (int)tipoMoz);

            return await query.ToListAsync();
        }

        public async Task<List<ATTI_DASI>> GetProposteAtti(string dataRichiesta, TipoAttoEnum tipo, TipoMOZEnum tipoMoz)
        {
            var query = PRContext.DASI.Where(atto => !atto.Eliminato
                                                     && atto.DataRichiestaIscrizioneSeduta.Equals(dataRichiesta)
                                                     && atto.Tipo == (int)tipo
                                                     && atto.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                                     && !atto.TipoChiusuraIter.HasValue);

            if (tipoMoz != TipoMOZEnum.ORDINARIA) query = query.Where(atto => atto.TipoMOZ == (int)tipoMoz);

            return await query.ToListAsync();
        }

        public async Task<int> CountODGByAttoPEM(Guid uidAtto)
        {
            return await PRContext
                .DASI
                .CountAsync(item => !item.Eliminato
                                    && item.IDStato >= (int)StatiAttoEnum.PRESENTATO
                                    && item.UID_Atto_ODG == uidAtto
                                    && item.DataIscrizioneSeduta.HasValue);
        }

        public async Task<bool> CheckIscrizioneSedutaIQT(string dataRichiesta, Guid uidPersona)
        {
            var res = true;

            var atti_proposti_in_seduta = await PRContext.DASI
                .Where(i => i.DataRichiestaIscrizioneSeduta.Equals(dataRichiesta)
                            && i.Tipo == (int)TipoAttoEnum.IQT
                            && i.IDStato >= (int)StatiAttoEnum.PRESENTATO
                            && !i.TipoChiusuraIter.HasValue)
                .ToListAsync();

            foreach (var attiDasi in atti_proposti_in_seduta)
            {
                var firmatari = await PRContext.ATTI_FIRME
                    .Where(i => i.UIDAtto == attiDasi.UIDAtto && string.IsNullOrEmpty(i.Data_ritirofirma) &&
                                i.Prioritario)
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
                            && !i.TipoChiusuraIter.HasValue
                            && (i.UIDSeduta == seduta.UIDSeduta
                                || i.DataRichiestaIscrizioneSeduta == dataSedutaEncrypt)
                            && i.Tipo == (int)TipoAttoEnum.MOZ
                            && i.TipoMOZ == (int)TipoMOZEnum.URGENTE
                            && !i.MOZU_Capigruppo)
                .ToListAsync();

            foreach (var attiDasi in atti_proposti_in_seduta)
            {
                var firmatari = await PRContext.ATTI_FIRME
                    .Where(i => i.UIDAtto == attiDasi.UIDAtto && string.IsNullOrEmpty(i.Data_ritirofirma) &&
                                i.Prioritario)
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

        private IQueryable<ATTI_DASI> ApplyFilters(IQueryable<ATTI_DASI> query,
            IEnumerable<IFilterStatement> statements, QueryExtendedRequest queryExtended, ClientModeEnum mode,
            PersonaDto currentUser, ref List<int> userOrder)
        {
            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in statements)
                if (f.PropertyId == nameof(ATTI_DASI.Oggetto))
                {
                    query = query.Where(item => item.Oggetto.Contains(f.Value.ToString())
                                                || item.Oggetto_Modificato.Contains(f.Value.ToString())
                                                || item.Oggetto_Approvato.Contains(f.Value.ToString())
                                                || item.Richiesta_Modificata.Contains(f.Value.ToString())
                                                || item.Premesse.Contains(f.Value.ToString())
                                                || item.Premesse_Modificato.Contains(f.Value.ToString())
                                                || item.Richiesta.Contains(f.Value.ToString()));
                }
                else if (f.PropertyId == nameof(AttoDASIDto.Note))
                {
                    var noteQuery = PRContext.ATTI_NOTE
                        .Where(nota => nota.Nota.Contains(f.Value.ToString()))
                        .Select(nota => nota.UIDAtto)
                        .Distinct(); // #1405
                    
                    query = query
                        .Join(
                            noteQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, doc) => atto
                        );
                }
                else if (f.PropertyId == nameof(ATTI_DASI.NAtto))
                {
                    var nAttoFilters = f.Value.ToString().Split(',');
                    var singleNumbers = new List<int>();
                    userOrder = new List<int>();
                    Expression<Func<ATTI_DASI, bool>> combinedRangePredicate = null;

                    foreach (var nAttoFilter in nAttoFilters)
                    {
                        if (string.IsNullOrEmpty(nAttoFilter))
                            continue;

                        if (nAttoFilter.Contains('-'))
                        {
                            var parts = nAttoFilter.Split('-').Select(int.Parse).ToArray();
                            var start = Math.Min(parts[0], parts[1]);
                            var end = Math.Max(parts[0], parts[1]);

                            for (var i = start; i <= end; i++) userOrder.Add(i);

                            // Crea un'espressione per l'intervallo
                            Expression<Func<ATTI_DASI, bool>> rangePredicate = item =>
                                item.NAtto_search >= start && item.NAtto_search <= end;

                            // Combina l'espressione corrente con le espressioni precedenti
                            if (combinedRangePredicate == null)
                                combinedRangePredicate = rangePredicate;
                            else
                                combinedRangePredicate =
                                    ExpressionExtensions.CombineExpressions(combinedRangePredicate, rangePredicate);
                        }
                        else
                        {
                            userOrder.Add(int.Parse(nAttoFilter));
                            singleNumbers.Add(int.Parse(nAttoFilter));
                        }
                    }

                    if (singleNumbers.Any())
                    {
                        Expression<Func<ATTI_DASI, bool>> singleNumbersPredicate =
                            item => singleNumbers.Contains(item.NAtto_search);
                        combinedRangePredicate = combinedRangePredicate == null
                            ? singleNumbersPredicate
                            : ExpressionExtensions.CombineExpressions(combinedRangePredicate, singleNumbersPredicate);
                    }

                    if (combinedRangePredicate != null)
                        query = query.Where(combinedRangePredicate);
                }
                else if (f.PropertyId == nameof(ATTI_DASI.DCR) || f.PropertyId == nameof(ATTI_DASI.DCCR))
                {
                    var values = f.Value.ToString().Split(';');
                    var dcrFilters = values.Length > 0 ? values[0].Split(',') : Array.Empty<string>();
                    var dccrFilters = values.Length > 1 ? values[1].Split(',') : Array.Empty<string>();

                    // Creiamo i predicati per DCR
                    Expression<Func<ATTI_DASI, bool>> dcrPredicate = null;
                    var dcrSingleNumbers = new List<int>();

                    foreach (var filter in dcrFilters)
                    {
                        if (string.IsNullOrEmpty(filter)) continue;

                        if (filter.Contains('-'))
                        {
                            var rangeParts = filter.Split('-').Select(int.Parse).ToArray();
                            var start = Math.Min(rangeParts[0], rangeParts[1]);
                            var end = Math.Max(rangeParts[0], rangeParts[1]);

                            Expression<Func<ATTI_DASI, bool>> rangePredicate = item =>
                                item.DCR >= start && item.DCR <= end;

                            dcrPredicate = dcrPredicate == null
                                ? rangePredicate
                                : ExpressionExtensions.CombineExpressions(dcrPredicate, rangePredicate);
                        }
                        else
                        {
                            dcrSingleNumbers.Add(int.Parse(filter));
                        }
                    }

                    if (dcrSingleNumbers.Any())
                    {
                        Expression<Func<ATTI_DASI, bool>> singlePredicate = item =>
                            dcrSingleNumbers.Contains((int)item.DCR);
                        dcrPredicate = dcrPredicate == null
                            ? singlePredicate
                            : ExpressionExtensions.CombineExpressions(dcrPredicate, singlePredicate);
                    }

                    if (dcrPredicate != null)
                        query = query.Where(dcrPredicate);

                    // Creiamo i predicati per DCCR
                    Expression<Func<ATTI_DASI, bool>> dccrPredicate = null;
                    var dccrSingleNumbers = new List<int>();

                    foreach (var filter in dccrFilters)
                    {
                        if (string.IsNullOrEmpty(filter)) continue;

                        if (filter.Contains('-'))
                        {
                            var rangeParts = filter.Split('-').Select(int.Parse).ToArray();
                            var start = Math.Min(rangeParts[0], rangeParts[1]);
                            var end = Math.Max(rangeParts[0], rangeParts[1]);

                            Expression<Func<ATTI_DASI, bool>> rangePredicate = item =>
                                item.DCCR >= start && item.DCCR <= end;

                            dccrPredicate = dccrPredicate == null
                                ? rangePredicate
                                : ExpressionExtensions.CombineExpressions(dccrPredicate, rangePredicate);
                        }
                        else
                        {
                            dccrSingleNumbers.Add(int.Parse(filter));
                        }
                    }

                    if (dccrSingleNumbers.Any())
                    {
                        Expression<Func<ATTI_DASI, bool>> singlePredicate = item =>
                            dccrSingleNumbers.Contains((int)item.DCCR);
                        dccrPredicate = dccrPredicate == null
                            ? singlePredicate
                            : ExpressionExtensions.CombineExpressions(dccrPredicate, singlePredicate);
                    }

                    if (dccrPredicate != null)
                        query = query.Where(dccrPredicate);
                }

                else
                {
                    filtro2._statements.Add(f);
                }

            filtro2.BuildExpression(ref query);

            #region Filtri semplici

            if (mode == ClientModeEnum.GRUPPI)
            {
                query = query.Where(atto => atto.IDStato != (int)StatiAttoEnum.BOZZA_RISERVATA
                                            || (atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA
                                                && (atto.UIDPersonaCreazione == currentUser.UID_persona
                                                    || atto.UIDPersonaProponente == currentUser.UID_persona)));

                if (!currentUser.IsSegreteriaAssemblea
                    && !currentUser.IsPresidente)
                    query = query.Where(item => item.id_gruppo == currentUser.Gruppo.id_gruppo);

                if (currentUser.IsSegreteriaAssemblea && !queryExtended.Stati.Any())
                {
                    query = query.Where(atto => atto.IDStato == (int)StatiAttoEnum.PRESENTATO
                                                || atto.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE
                                                || atto.IDStato == (int)StatiAttoEnum.COMPLETATO);
                }
            }
            else
            {
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
            }

            if (queryExtended.Stati.Any())
                query = query.Where(i => queryExtended.Stati.Contains(i.IDStato));

            if (queryExtended.GruppiProponenti.Any())
                query = query.Where(i => queryExtended.GruppiProponenti.Contains(i.id_gruppo));

            if (queryExtended.AreaPolitica.Any())
                query = query.Where(i => queryExtended.AreaPolitica.Contains(i.AreaPolitica));

            if (queryExtended.Tipi.Any())
                query = query.Where(i => queryExtended.Tipi.Contains(i.Tipo));

            if (queryExtended.TipiRispostaRichiesta.Any())
                query = query.Where(i => queryExtended.TipiRispostaRichiesta.Contains(i.IDTipo_Risposta));

            if (queryExtended.TipiChiusura.Any())
                query = query.Where(i => queryExtended.TipiChiusura.Contains(i.TipoChiusuraIter.Value));

            if (queryExtended.TipiVotazione.Any())
                query = query.Where(i => queryExtended.TipiVotazione.Contains(i.TipoVotazioneIter.Value));

            if (queryExtended.DataChiusuraIterIsNull)
                query = query
                    .Where(atto => atto.DataChiusuraIter == null);

            if (queryExtended.DataComunicazioneAssembleaIsNull)
                query = query
                    .Where(atto => atto.DataComunicazioneAssemblea == null);

            if (queryExtended.DataPresentazioneIsNull)
                query = query
                    .Where(atto => atto.Timestamp == null);

            if (queryExtended.DataAnnunzioIsNull)
                query = query
                    .Where(atto => atto.DataAnnunzio == null);

            if (queryExtended.AttiDaFirmare.Any())
                query = query.Where(i => queryExtended.AttiDaFirmare.Contains(i.UIDAtto));

            #endregion

            #region Filtri complessi

            if (queryExtended.GruppiFirmatari.Any())
            {
                query = query.Where(atto => PRContext.ATTI_FIRME
                    .Any(firma => queryExtended.GruppiFirmatari.Contains(firma.id_gruppo) && firma.UIDAtto == atto.UIDAtto));
            }

            if (queryExtended.Firmatari.Any())
            {
                query = query.Where(atto => PRContext.ATTI_FIRME
                    .Any(firma => queryExtended.Firmatari.Contains(firma.UID_persona) && firma.UIDAtto == atto.UIDAtto));
            }

            if (queryExtended.DocumentiMancanti && queryExtended.TipiDocumento.Any())
            {
                query = query.Where(atto => !PRContext.ATTI_DOCUMENTI
                    .Any(f => queryExtended.TipiDocumento.Contains(f.Tipo) && f.UIDAtto == atto.UIDAtto));
            }
            else if (queryExtended.TipiDocumento.Any() && !queryExtended.DocumentiMancanti)
            {
                query = query.Where(atto => PRContext.ATTI_DOCUMENTI
                    .Any(f => queryExtended.TipiDocumento.Contains(f.Tipo) && f.UIDAtto == atto.UIDAtto));
            }

            if (queryExtended.RispostaMancante)
            {
                query = query.Where(atto => PRContext.ATTI_RISPOSTE
                    .Any(risposta => risposta.UIDAtto == atto.UIDAtto));
            }
            else if (queryExtended.Risposte.Any())
            {
                query = query.Where(atto => PRContext.ATTI_RISPOSTE
                    .Any(f => queryExtended.Risposte.Contains(f.Tipo) && f.UIDAtto == atto.UIDAtto) || queryExtended.Risposte.Contains(atto.IDTipo_Risposta_Effettiva.Value));
            }

            if (queryExtended.Proponenti.Any())
            {
                query = query.Where(atto => PRContext.ATTI_PROPONENTI
                    .Any(proponente => queryExtended.Proponenti.Contains(proponente.UidPersona.Value) && proponente.UIDAtto == atto.UIDAtto) || queryExtended.Proponenti.Contains(atto.UIDPersonaProponente.Value));
            }

            if (queryExtended.Provvedimenti.Any())
            {
                query = query.Where(atto => PRContext.ATTI_ABBINAMENTI
                    .Any(abb => queryExtended.Provvedimenti.Contains(abb.UIDAttoAbbinato.Value) && abb.UIDAtto == atto.UIDAtto) || queryExtended.Provvedimenti.Contains(atto.UID_Atto_ODG.Value));
            }

            if (queryExtended.OrganiIsNull)
            {
                query = query.Where(atto => PRContext.ATTI_COMMISSIONI
                    .Any(f => f.UIDAtto == atto.UIDAtto));
            }
            else if (queryExtended.Organi.Any())
            {
                query = query.Where(atto => PRContext.ATTI_COMMISSIONI
                    .Where(organo => queryExtended.Organi.Contains(organo.id_organo) && organo.UIDAtto == atto.UIDAtto)
                    .Any() || PRContext.ATTI_RISPOSTE
                    .Where(organo => queryExtended.Organi.Contains(organo.IdOrgano) && organo.UIDAtto == atto.UIDAtto)
                    .Any() || PRContext.ATTI_MONITORAGGIO
                    .Where(organo => queryExtended.Organi.Contains(organo.IdOrgano) && organo.UIDAtto == atto.UIDAtto)
                    .Any());
            }

            if (queryExtended.Organi_Commissione.Any())
            {
                query = query.Where(atto => PRContext.ATTI_RISPOSTE
                    .Any(organo => queryExtended.Organi_Commissione.Contains(organo.IdOrgano)
                                   && organo.TipoOrgano.Equals((int)TipoOrganoEnum.COMMISSIONE) && organo.UIDAtto == atto.UIDAtto));
            }

            if (queryExtended.Organi_Giunta.Any())
            {
                query = query.Where(atto => PRContext.ATTI_RISPOSTE
                    .Any(organo => queryExtended.Organi_Giunta.Contains(organo.IdOrgano)
                                   && organo.TipoOrgano.Equals((int)TipoOrganoEnum.GIUNTA) && organo.UIDAtto == atto.UIDAtto));
            }

            if (queryExtended.DataSeduta.Any())
            {
                if (queryExtended.DataSeduta.Count > 1)
                {
                    var startDate = queryExtended.DataSeduta[0];
                    var endDate = queryExtended.DataSeduta[1];

                    var seduteQuery = PRContext
                        .SEDUTE
                        .Where(seduta => seduta.Data_seduta >= startDate && seduta.Data_seduta <= endDate)
                        .Distinct()
                        .AsQueryable();

                    var identificativiSedute = seduteQuery.Select(seduta => seduta.UIDSeduta).ToList();
                    // #1341
                    var listaDateSedutaCrypt = new List<string>();
                    foreach (var seduta in seduteQuery)
                    {
                        var dataSedutaCrypt = BALHelper.EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"), AppSettingsConfiguration.masterKey);
                        listaDateSedutaCrypt.Add(dataSedutaCrypt);
                    }

                    query = query
                        .Where(atto => identificativiSedute.Contains(atto.UIDSeduta.Value)
                        || listaDateSedutaCrypt.Contains(atto.DataRichiestaIscrizioneSeduta));
                }
                else
                {
                    var singleDate = queryExtended.DataSeduta[0];

                    var seduteQuery = PRContext
                        .SEDUTE
                        .Where(seduta => singleDate.Date == seduta.Data_seduta.Date)
                        .Select(seduta => seduta.UIDSeduta)
                        .Distinct()
                        .AsQueryable();

                    query = query
                        .Where(atto => seduteQuery.Contains(atto.UIDSeduta.Value));
                }
            }

            if (queryExtended.DataTrasmissione.Any())
            {
                if (queryExtended.DataTrasmissione.Count > 1)
                {
                    var startDate = queryExtended.DataTrasmissione[0];
                    var endDate = queryExtended.DataTrasmissione[1];

                    var risposteTrasmesseQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta => risposta.DataTrasmissione >= startDate
                                           && risposta.DataTrasmissione <= endDate)
                        .Select(risposta => risposta.UIDAtto)
                        .Distinct()
                        .AsQueryable();

                    query = query
                        .Where(atto => risposteTrasmesseQuery.Contains(atto.UIDAtto));
                }
                else
                {
                    var singleDate = queryExtended.DataTrasmissione[0];

                    var risposteTrasmesseQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta =>
                            singleDate.Date == risposta.DataTrasmissione.Value.Date)
                        .Select(risposta => risposta.UIDAtto)
                        .Distinct()
                        .AsQueryable();

                    query = query
                        .Where(atto => risposteTrasmesseQuery.Contains(atto.UIDAtto));
                }
            }

            if (queryExtended.DataTrasmissioneIsNull)
            {
                var risposteTrasmesseNulleQuery = PRContext.ATTI_RISPOSTE
                    .Where(risposta => risposta.DataTrasmissione == null)
                    .Select(risposta => risposta.UIDAtto)
                    .Distinct()
                    .AsQueryable();

                query = query
                    .Where(atto => risposteTrasmesseNulleQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.DataTrattazione.Any())
            {
                if (queryExtended.DataTrattazione.Count > 1)
                {
                    var startDate = queryExtended.DataTrattazione[0];
                    var endDate = queryExtended.DataTrattazione[1];

                    var risposteTrasmesseQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta => risposta.DataTrattazione >= startDate
                                           && risposta.DataTrattazione <= endDate)
                        .Select(risposta => risposta.UIDAtto)
                        .Distinct()
                        .AsQueryable();

                    query = query
                        .Where(atto => risposteTrasmesseQuery.Contains(atto.UIDAtto));
                }
                else
                {
                    var singleDate = queryExtended.DataTrattazione[0];

                    var risposteTrasmesseQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta =>
                            singleDate.Date == risposta.DataTrattazione.Value.Date)
                        .Select(risposta => risposta.UIDAtto)
                        .Distinct()
                        .AsQueryable();

                    query = query
                        .Where(atto => risposteTrasmesseQuery.Contains(atto.UIDAtto));
                }
            }

            if (queryExtended.DataTrattazioneIsNull)
            {
                var risposteTrattateNulleQuery = PRContext.ATTI_RISPOSTE
                    .Where(risposta => !risposta.DataTrattazione.HasValue)
                    .Select(risposta => risposta.UIDAtto);

                query = query
                    .Where(atto => risposteTrattateNulleQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.DataChiusuraIter.Any())
            {
                if (queryExtended.DataChiusuraIter.Count > 1)
                {
                    var startDate = queryExtended.DataChiusuraIter[0];
                    var endDate = queryExtended.DataChiusuraIter[1];
                    query = query
                        .Where(atto => atto.DataChiusuraIter >= startDate
                                       && atto.DataChiusuraIter <= endDate);
                }
                else
                {
                    var singleDate = queryExtended.DataChiusuraIter[0];
                    query = query
                        .Where(atto => singleDate.Date == atto.DataChiusuraIter.Value.Date);
                }
            }

            if (queryExtended.DataComunicazioneAssemblea.Any())
            {
                if (queryExtended.DataComunicazioneAssemblea.Count > 1)
                {
                    var startDate = queryExtended.DataComunicazioneAssemblea[0];
                    var endDate = queryExtended.DataComunicazioneAssemblea[1];
                    query = query
                        .Where(atto => atto.DataComunicazioneAssemblea >= startDate
                                       && atto.DataComunicazioneAssemblea <= endDate);
                }
                else
                {
                    var singleDate = queryExtended.DataComunicazioneAssemblea[0];
                    query = query
                        .Where(atto => singleDate.Date == atto.DataComunicazioneAssemblea.Value.Date);
                }
            }

            if (queryExtended.DataPresentazione.Any())
            {
                if (queryExtended.DataPresentazione.Count > 1)
                {
                    var startDate = queryExtended.DataPresentazione[0];
                    var endDate = queryExtended.DataPresentazione[1];
                    query = query
                        .Where(atto => atto.Timestamp >= startDate && atto.Timestamp <= endDate);
                }
                else
                {
                    var singleDate = queryExtended.DataPresentazione[0];
                    query = query
                        .Where(atto => singleDate.Date == atto.Timestamp.Value.Date);
                }
            }

            if (queryExtended.DataAnnunzio.Any())
            {
                if (queryExtended.DataAnnunzio.Count > 1)
                {
                    var startDate = queryExtended.DataAnnunzio[0];
                    var endDate = queryExtended.DataAnnunzio[1];
                    query = query
                        .Where(atto => atto.DataAnnunzio >= startDate
                                       && atto.DataAnnunzio <= endDate);
                }
                else
                {
                    var singleDate = queryExtended.DataAnnunzio[0];
                    query = query
                        .Where(atto => singleDate.Date == atto.DataAnnunzio.Value.Date);
                }
            }

            if (queryExtended.DataRisposta.Any())
            {
                if (queryExtended.DataRisposta.Count > 1)
                {
                    var startDate = queryExtended.DataRisposta[0];
                    var endDate = queryExtended.DataRisposta[1];

                    var risposteDateQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta => risposta.Data >= startDate
                                           && risposta.Data <= endDate)
                        .Select(risposta => risposta.UIDAtto);

                    query = query
                        .Where(atto => risposteDateQuery.Contains(atto.UIDAtto));
                }
                else
                {
                    var singleDate = queryExtended.DataRisposta[0];

                    var risposteDateQuery = PRContext.ATTI_RISPOSTE
                        .Where(risposta =>
                            singleDate.Date == risposta.Data.Value.Date)
                        .Select(risposta => risposta.UIDAtto);

                    query = query
                        .Where(atto => risposteDateQuery.Contains(atto.UIDAtto));
                }
            }

            if (queryExtended.DataRispostaIsNull)
            {
                var risposteDateNulleQuery = PRContext.ATTI_RISPOSTE
                    .Where(risposta =>
                        !risposta.Data.HasValue)
                    .Select(risposta => risposta.UIDAtto);

                query = query
                    .Where(atto => risposteDateNulleQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.Ritardo.HasValue)
            {
                if (queryExtended.Ritardo.Value)
                    query = query.Where(atto => atto.Ritardo > 0);
                else
                    query = query.Where(atto => atto.Ritardo.Equals(0));
            }

            #endregion

            return query;
        }
    }
}