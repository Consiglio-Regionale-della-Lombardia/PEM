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
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
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

        public async Task<List<Guid>> GetAll(PersonaDto persona,
            int page,
            int size,
            ClientModeEnum mode,
            Filter<ATTI_DASI> filtro, QueryExtendedRequest queryExtended)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

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
                else if (f.PropertyId == nameof(ATTI_DASI.NAtto))
                {
                    var nAttoFilters = f.Value.ToString().Split(',');
                    var singleNumbers = new List<int>();
                    Expression<Func<ATTI_DASI, bool>> combinedRangePredicate = null;

                    foreach (var nAttoFilter in nAttoFilters)
                    {
                        if (string.IsNullOrEmpty(nAttoFilter))
                            continue;

                        if (nAttoFilter.Contains('-'))
                        {
                            var parts = nAttoFilter.Split('-').Select(int.Parse).ToArray();
                            var start = parts[0];
                            var end = parts[1];

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
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
            }

            if (queryExtended.Stati.Any())
                query = query.Where(i => queryExtended.Stati.Contains(i.IDStato));

            if (queryExtended.Tipi.Any())
                query = query.Where(i => queryExtended.Tipi.Contains(i.Tipo));
            
            if (queryExtended.TipiRispostaRichiesta.Any())
                query = query.Where(i => queryExtended.TipiRispostaRichiesta.Contains(i.IDTipo_Risposta));

            if (queryExtended.TipiChiusura.Any())
                query = query.Where(i => queryExtended.TipiChiusura.Contains(i.TipoChiusuraIter.Value));


            if (queryExtended.TipiVotazione.Any())
                query = query.Where(i => queryExtended.TipiVotazione.Contains(i.TipoVotazioneIter.Value));
            
            if (queryExtended.TipiDocumento.Any())
            {
                var documentQuery = PRContext.ATTI_DOCUMENTI
                    .Where(f => queryExtended.TipiDocumento.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                if (queryExtended.DocumentiMancanti)
                {
                    query = query
                        .GroupJoin(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, docs) => new { atto, docs }
                        )
                        .Where(g => !g.docs.Any())
                        .Select(g => g.atto);
                }
                else
                {
                    query = query
                        .Join(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, doc) => atto
                        );
                }
            }
            
            if (queryExtended.Risposte.Any())
            {
                var risposteEffettiveQuery = PRContext.ATTI_RISPOSTE
                    .Where(f => queryExtended.Risposte.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                query = query
                    .Join(
                        risposteEffettiveQuery,
                        atto => atto.UIDAtto,
                        risp => risp,
                        (atto, risp) => atto
                    );
            }

            if (queryExtended.Proponenti.Any())
                query = query
                    .Where(atto => queryExtended.Proponenti.Contains(atto.UIDPersonaProponente.Value));

            if (queryExtended.Provvedimenti.Any())
            {
                var abbinamentiQuery = PRContext.ATTI_ABBINAMENTI
                    .Where(abb => queryExtended.Provvedimenti.Contains(abb.UIDAttoAbbinato.Value))
                    .Select(abb => abb.UIDAtto);

                query = query
                    .Where(atto => queryExtended.Provvedimenti.Contains(atto.UID_Atto_ODG.Value) || abbinamentiQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.AttiDaFirmare.Any())
            {
                query = query.Where(i => queryExtended.AttiDaFirmare.Contains(i.UIDAtto));
            }

            if (queryExtended.Stati.Any())
            {
                if (queryExtended.Stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                {
                    return await query
                        .OrderBy(item => item.Tipo)
                        .ThenByDescending(item => item.DataCreazione)
                        .Select(item => item.UIDAtto)
                        .Skip((page - 1) * size)
                        .Take(size)
                        .ToListAsync();
                }
            }

            return await query
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Select(item => item.UIDAtto)
                .Skip((page - 1) * size)
                .Take(size)
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
                .ToListAsync();

            return dataFromDb.Select(r => new AttiRisposteDto
            {
                UIDAtto = r.UIDAtto,
                Data = r.Data,
                DataTrasmissione = r.DataTrasmissione,
                DataTrattazione = r.DataTrattazione,
                DescrizioneOrgano = r.DescrizioneOrgano,
                IdOrgano = r.IdOrgano,
                Uid = r.Uid,
                Tipo = r.Tipo,
                DisplayTipo = Utility.GetText_TipoRispostaDASI(r.Tipo, false),
                TipoOrgano = r.TipoOrgano,
                DisplayTipoOrgano = Utility.GetText_TipoOrganoDASI(r.TipoOrgano)
            }).ToList();
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
                .ToListAsync();

            return docInDB
                .Select(d => new AttiDocumentiDto
                {
                    Tipo = ((TipoDocumentoEnum)d.Tipo).ToString(),
                    Titolo = d.Titolo,
                    Pubblico = d.Pubblica,
                    Link = $"{AppSettingsConfiguration.URL_API}/{ApiRoutes.DASI.DownloadDoc}?path={d.Path}",
                    TipoEnum = (TipoDocumentoEnum)d.Tipo
                })
                .ToList();
        }

        public async Task<List<NoteDto>> GetNote(Guid uidAtto)
        {
            var noteInDB = await PRContext
                .ATTI_NOTE
                .Where(d => d.UIDAtto == uidAtto)
                .ToListAsync();
            var res = new List<NoteDto>();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(2)).ToList();

            foreach (var nota in noteInDB)
            {
                var persona = await PRContext.View_UTENTI.FindAsync(nota.UIDPersona);
                res.Add(new NoteDto
                {
                    Tipo = ((TipoNotaEnum)nota.Tipo).ToString(),
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

        public async Task<List<AttoLightDto>> GetAbbinamentiDisponibili(int legislaturaId)
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

            return await query.ToListAsync();
        }

        public async Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (tipo != TipoAttoEnum.TUTTI)
            {
                query = query.Where(item => item.Tipo.Equals((int)tipo));
            }

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in queryFilter.Statements)
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
                else if (f.PropertyId == nameof(ATTI_DASI.NAtto))
                {
                    var nAttoFilters = f.Value.ToString().Split(',');
                    var singleNumbers = new List<int>();
                    Expression<Func<ATTI_DASI, bool>> combinedRangePredicate = null;

                    foreach (var nAttoFilter in nAttoFilters)
                    {
                        if (string.IsNullOrEmpty(nAttoFilter))
                            continue;

                        if (nAttoFilter.Contains('-'))
                        {
                            var parts = nAttoFilter.Split('-').Select(int.Parse).ToArray();
                            var start = parts[0];
                            var end = parts[1];

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
                else
                    filtro2._statements.Add(f);
            }

            filtro2.BuildExpression(ref query);

            if (clientMode == ClientModeEnum.GRUPPI)
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
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
            }

            if (queryExtended.Stati.Any())
                query = query.Where(i => queryExtended.Stati.Contains(i.IDStato));

            if (queryExtended.Tipi.Any())
                query = query.Where(i => queryExtended.Tipi.Contains(i.Tipo));
            
            if (queryExtended.TipiRispostaRichiesta.Any())
                query = query.Where(i => queryExtended.TipiRispostaRichiesta.Contains(i.IDTipo_Risposta));

            if (queryExtended.TipiChiusura.Any())
                query = query.Where(i => queryExtended.TipiChiusura.Contains(i.TipoChiusuraIter.Value));


            if (queryExtended.TipiVotazione.Any())
                query = query.Where(i => queryExtended.TipiVotazione.Contains(i.TipoVotazioneIter.Value));
            
            if (queryExtended.TipiDocumento.Any())
            {
                var documentQuery = PRContext.ATTI_DOCUMENTI
                    .Where(f => queryExtended.TipiDocumento.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                if (queryExtended.DocumentiMancanti)
                {
                    query = query
                        .GroupJoin(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, docs) => new { atto, docs }
                        )
                        .Where(g => !g.docs.Any())
                        .Select(g => g.atto);
                }
                else
                {
                    query = query
                        .Join(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, doc) => atto
                        );
                }
            }

            if (queryExtended.Risposte.Any())
            {
                var risposteEffettiveQuery = PRContext.ATTI_RISPOSTE
                    .Where(f => queryExtended.Risposte.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                query = query
                    .Join(
                        risposteEffettiveQuery,
                        atto => atto.UIDAtto,
                        risp => risp,
                        (atto, risp) => atto
                    );
            }

            if (queryExtended.Proponenti.Any())
                query = query
                    .Where(atto => queryExtended.Proponenti.Contains(atto.UIDPersonaProponente.Value));

            if (queryExtended.Provvedimenti.Any())
            {
                var abbinamentiQuery = PRContext.ATTI_ABBINAMENTI
                    .Where(abb => queryExtended.Provvedimenti.Contains(abb.UIDAttoAbbinato.Value))
                    .Select(abb => abb.UIDAtto);

                query = query
                    .Where(atto => queryExtended.Provvedimenti.Contains(atto.UID_Atto_ODG.Value) || abbinamentiQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.AttiDaFirmare.Any())
            {
                query = query.Where(i => queryExtended.AttiDaFirmare.Contains(i.UIDAtto));
            }

            if (queryExtended.Stati.Any())
            {
                if (queryExtended.Stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                {
                    return await query
                        .CountAsync();
                }
            }

            return await query
                .CountAsync();
        }

        public async Task<int> Count(PersonaDto persona, StatiAttoEnum stato, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            if (stato != StatiAttoEnum.TUTTI)
            {
                query = query.Where(item => item.IDStato.Equals((int)stato));
            }

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in queryFilter.Statements)
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
                else if (f.PropertyId == nameof(ATTI_DASI.NAtto))
                {
                    var nAttoFilters = f.Value.ToString().Split(',');
                    var singleNumbers = new List<int>();
                    Expression<Func<ATTI_DASI, bool>> combinedRangePredicate = null;

                    foreach (var nAttoFilter in nAttoFilters)
                    {
                        if (string.IsNullOrEmpty(nAttoFilter))
                            continue;

                        if (nAttoFilter.Contains('-'))
                        {
                            var parts = nAttoFilter.Split('-').Select(int.Parse).ToArray();
                            var start = parts[0];
                            var end = parts[1];

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
                else
                    filtro2._statements.Add(f);
            }

            filtro2.BuildExpression(ref query);

            if (clientMode == ClientModeEnum.GRUPPI)
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
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
            }

            if (queryExtended.Stati.Any())
                query = query.Where(i => queryExtended.Stati.Contains(i.IDStato));

            if (queryExtended.Tipi.Any())
                query = query.Where(i => queryExtended.Tipi.Contains(i.Tipo));
            
            if (queryExtended.TipiRispostaRichiesta.Any())
                query = query.Where(i => queryExtended.TipiRispostaRichiesta.Contains(i.IDTipo_Risposta));

            if (queryExtended.TipiChiusura.Any())
                query = query.Where(i => queryExtended.TipiChiusura.Contains(i.TipoChiusuraIter.Value));


            if (queryExtended.TipiVotazione.Any())
                query = query.Where(i => queryExtended.TipiVotazione.Contains(i.TipoVotazioneIter.Value));
            
            if (queryExtended.TipiDocumento.Any())
            {
                var documentQuery = PRContext.ATTI_DOCUMENTI
                    .Where(f => queryExtended.TipiDocumento.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                if (queryExtended.DocumentiMancanti)
                {
                    query = query
                        .GroupJoin(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, docs) => new { atto, docs }
                        )
                        .Where(g => !g.docs.Any())
                        .Select(g => g.atto);
                }
                else
                {
                    query = query
                        .Join(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, doc) => atto
                        );
                }
            }

            if (queryExtended.Risposte.Any())
            {
                var risposteEffettiveQuery = PRContext.ATTI_RISPOSTE
                    .Where(f => queryExtended.Risposte.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                query = query
                    .Join(
                        risposteEffettiveQuery,
                        atto => atto.UIDAtto,
                        risp => risp,
                        (atto, risp) => atto
                    );
            }

            if (queryExtended.Proponenti.Any())
                query = query
                    .Where(atto => queryExtended.Proponenti.Contains(atto.UIDPersonaProponente.Value));

            if (queryExtended.Provvedimenti.Any())
            {
                var abbinamentiQuery = PRContext.ATTI_ABBINAMENTI
                    .Where(abb => queryExtended.Provvedimenti.Contains(abb.UIDAttoAbbinato.Value))
                    .Select(abb => abb.UIDAtto);

                query = query
                    .Where(atto => queryExtended.Provvedimenti.Contains(atto.UID_Atto_ODG.Value) || abbinamentiQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.AttiDaFirmare.Any())
            {
                query = query.Where(i => queryExtended.AttiDaFirmare.Contains(i.UIDAtto));
            }

            if (queryExtended.Stati.Any())
            {
                if (queryExtended.Stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                {
                    return await query
                        .CountAsync();
                }
            }

            return await query
                .CountAsync();
        }

        public async Task<int> Count(PersonaDto persona, ClientModeEnum clientMode, Filter<ATTI_DASI> queryFilter,
            QueryExtendedRequest queryExtended)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato
                               && !item.IDStato.Equals((int)StatiAttoEnum.BOZZA_CARTACEA));

            var filtro2 = new Filter<ATTI_DASI>();
            foreach (var f in queryFilter.Statements)
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
                else if (f.PropertyId == nameof(ATTI_DASI.NAtto))
                {
                    var nAttoFilters = f.Value.ToString().Split(',');
                    var singleNumbers = new List<int>();
                    Expression<Func<ATTI_DASI, bool>> combinedRangePredicate = null;

                    foreach (var nAttoFilter in nAttoFilters)
                    {
                        if (string.IsNullOrEmpty(nAttoFilter))
                            continue;

                        if (nAttoFilter.Contains('-'))
                        {
                            var parts = nAttoFilter.Split('-').Select(int.Parse).ToArray();
                            var start = parts[0];
                            var end = parts[1];

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
                else
                    filtro2._statements.Add(f);
            }

            filtro2.BuildExpression(ref query);

            if (clientMode == ClientModeEnum.GRUPPI)
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
                query = query.Where(item => item.DataIscrizioneSeduta.HasValue);
            }

            if (queryExtended.Stati.Any())
                query = query.Where(i => queryExtended.Stati.Contains(i.IDStato));

            if (queryExtended.Tipi.Any())
                query = query.Where(i => queryExtended.Tipi.Contains(i.Tipo));
            
            if (queryExtended.TipiRispostaRichiesta.Any())
                query = query.Where(i => queryExtended.TipiRispostaRichiesta.Contains(i.IDTipo_Risposta));

            if (queryExtended.TipiChiusura.Any())
                query = query.Where(i => queryExtended.TipiChiusura.Contains(i.TipoChiusuraIter.Value));


            if (queryExtended.TipiVotazione.Any())
                query = query.Where(i => queryExtended.TipiVotazione.Contains(i.TipoVotazioneIter.Value));
            
            if (queryExtended.TipiDocumento.Any())
            {
                var documentQuery = PRContext.ATTI_DOCUMENTI
                    .Where(f => queryExtended.TipiDocumento.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                if (queryExtended.DocumentiMancanti)
                {
                    query = query
                        .GroupJoin(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, docs) => new { atto, docs }
                        )
                        .Where(g => !g.docs.Any())
                        .Select(g => g.atto);
                }
                else
                {
                    query = query
                        .Join(
                            documentQuery,
                            atto => atto.UIDAtto,
                            doc => doc,
                            (atto, doc) => atto
                        );
                }
            }

            if (queryExtended.Risposte.Any())
            {
                var risposteEffettiveQuery = PRContext.ATTI_RISPOSTE
                    .Where(f => queryExtended.Risposte.Contains(f.Tipo))
                    .Select(f => f.UIDAtto);

                query = query
                    .Join(
                        risposteEffettiveQuery,
                        atto => atto.UIDAtto,
                        risp => risp,
                        (atto, risp) => atto
                    );
            }

            if (queryExtended.Proponenti.Any())
                query = query
                    .Where(atto => queryExtended.Proponenti.Contains(atto.UIDPersonaProponente.Value));

            if (queryExtended.Provvedimenti.Any())
            {
                var abbinamentiQuery = PRContext.ATTI_ABBINAMENTI
                    .Where(abb => queryExtended.Provvedimenti.Contains(abb.UIDAttoAbbinato.Value))
                    .Select(abb => abb.UIDAtto);

                query = query
                    .Where(atto => queryExtended.Provvedimenti.Contains(atto.UID_Atto_ODG.Value) || abbinamentiQuery.Contains(atto.UIDAtto));
            }

            if (queryExtended.AttiDaFirmare.Any())
            {
                query = query.Where(i => queryExtended.AttiDaFirmare.Contains(i.UIDAtto));
            }

            if (queryExtended.Stati.Any())
            {
                if (queryExtended.Stati.Any(item => item.Equals((int)StatiAttoEnum.BOZZA)))
                {
                    return await query
                        .CountAsync();
                }
            }

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
                                    && !item.TipoChiusuraIter.HasValue);
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
    }
}