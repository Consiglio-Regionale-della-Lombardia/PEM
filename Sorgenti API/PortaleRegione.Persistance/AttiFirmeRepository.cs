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
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Persistance
{
    public class AttiFirmeRepository : Repository<ATTI_FIRME>, IAttiFirmeRepository
    {
        public AttiFirmeRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        /// <summary>
        ///     Firma atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="personaUId"></param>
        /// <param name="id_gruppo"></param>
        /// <param name="firmaCert"></param>
        /// <param name="dataFirmaCert"></param>
        /// <param name="ufficio"></param>
        /// <param name="primoFirmatario"></param>
        /// <param name="valida"></param>
        /// <param name="gruppoIdGruppo"></param>
        /// <param name="em"></param>
        public async Task Firma(Guid attoUId, Guid personaUId, int id_gruppo, string firmaCert,
            string dataFirmaCert, DateTime timestamp,
            bool ufficio = false, bool primoFirmatario = false, bool valida = true, bool capogruppo = false,
            bool prioritario = true,
            int ordine = 0)
        {
            PRContext
                .ATTI_FIRME
                .Add(new ATTI_FIRME
                {
                    UIDAtto = attoUId,
                    UID_persona = personaUId,
                    FirmaCert = firmaCert,
                    Data_firma = dataFirmaCert,
                    Timestamp = timestamp,
                    ufficio = ufficio,
                    PrimoFirmatario = primoFirmatario,
                    id_gruppo = id_gruppo,
                    Valida = valida,
                    Capogruppo = capogruppo,
                    Prioritario = prioritario,
                    OrdineVisualizzazione = ordine
                });
            await PRContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Conta le firme attive nell'atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<int> CountFirme(Guid attoUId)
        {
            return await PRContext
                .ATTI_FIRME
                .CountAsync(f => f.UIDAtto == attoUId && string.IsNullOrEmpty(f.Data_ritirofirma) && f.Valida);
        }

        /// <summary>
        ///     Conta le firme attive nell'atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<int> CountFirmePrioritarie(Guid attoUId)
        {
            return await PRContext
                .ATTI_FIRME
                .CountAsync(f =>
                    f.UIDAtto == attoUId && string.IsNullOrEmpty(f.Data_ritirofirma) && f.Valida && f.Prioritario);
        }

        /// <summary>
        ///     Riepilogo firmatari
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ATTI_FIRME>> GetFirmatari(Guid attoUId)
        {
            var result = await PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == attoUId && f.Valida)
                .ToListAsync();
            //#983
            return result.OrderBy(f => f.Timestamp.Date)
                .ThenBy(f => f.OrdineVisualizzazione)
                .ToList();
        }

        public async Task<List<ATTI_FIRME>> GetFirmatari(List<Guid> guids, int max_result)
        {
            var result = new List<ATTI_FIRME>();
            foreach (var guid in guids)
            {
                var res = await PRContext
                    .ATTI_FIRME
                    .Where(f => guid == f.UIDAtto && f.Valida && string.IsNullOrEmpty(f.Data_ritirofirma) &&
                                f.Prioritario)
                    .ToListAsync();
                //#983
                result = result.OrderBy(f => f.Timestamp.Date)
                    .ThenBy(f => f.OrdineVisualizzazione)
                    .ToList();

                result.AddRange(res.Take(max_result));
            }

            return result;
        }

        public async Task CancellaFirme(Guid attoUId)
        {
            var firmeDaEliminare = await PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == attoUId)
                .ToListAsync();
            PRContext
                .ATTI_FIRME
                .RemoveRange(firmeDaEliminare);
        }

        /// <summary>
        ///     Controlla che l'atto sia firmabile
        /// </summary>
        /// <param name="atto"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfFirmabile(AttoDASIDto atto, PersonaDto persona, bool firma_ufficio = false)
        {
            // #721
            if (!persona.IsConsigliereRegionale && !firma_ufficio)
                return false;
            if (atto.IsChiuso) return false;
            if (atto.DataIscrizioneSeduta.HasValue) return false;
            if (atto.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE
                && (atto.Tipo == (int)TipoAttoEnum.ITL
                    && atto.IDTipo_Risposta == (int)TipoRispostaEnum.COMMISSIONE
                    || atto.IDTipo_Risposta == (int)TipoRispostaEnum.SCRITTA
                    || atto.Tipo == (int)TipoAttoEnum.ITR
                    && atto.IDTipo_Risposta == (int)TipoRispostaEnum.COMMISSIONE
                    || atto.IDTipo_Risposta == (int)TipoRispostaEnum.SCRITTA))
            {
                return false;
            }

            var firma_personale = await Get(atto.UIDAtto, persona.UID_persona);
            var firma_proponente = await CheckFirmato(atto.UIDAtto, atto.UIDPersonaProponente);

            if (firma_personale == null && (firma_proponente || atto.UIDPersonaProponente == persona.UID_persona))
                return true;

            return false;
        }
        
        /// <summary>
        ///     Controlla che l'atto sia firmabile
        /// </summary>
        /// <param name="atto"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfFirmabile(AttoDASIDto atto, List<AttiFirmeDto> firme, PersonaDto persona, bool firma_ufficio = false)
        {
            if (persona == null)
                return false;
            // #721
            if (!persona.IsConsigliereRegionale && !firma_ufficio)
                return false;
            if (atto.IsChiuso) return false;
            if (atto.DataIscrizioneSeduta.HasValue) return false;
            if (atto.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE
                && (atto.Tipo == (int)TipoAttoEnum.ITL
                    && atto.IDTipo_Risposta == (int)TipoRispostaEnum.COMMISSIONE
                    || atto.IDTipo_Risposta == (int)TipoRispostaEnum.SCRITTA
                    || atto.Tipo == (int)TipoAttoEnum.ITR
                    && atto.IDTipo_Risposta == (int)TipoRispostaEnum.COMMISSIONE
                    || atto.IDTipo_Risposta == (int)TipoRispostaEnum.SCRITTA))
            {
                return false;
            }

            var firma_personale = firme.Any(f=> f.UID_persona.Equals(persona.UID_persona));
            var firma_proponente = atto.Firmato_Dal_Proponente;

            if (firma_personale == false && (firma_proponente || atto.UIDPersonaProponente == persona.UID_persona))
                return true;

            return false;
        }

        /// <summary>
        ///     Controlla se l'ufficio ha firmato l'atto
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<bool> CheckFirmatoDaUfficio(Guid attoUId)
        {
            return await PRContext.ATTI_FIRME.AnyAsync(f => f.UIDAtto == attoUId && f.ufficio);
        }

        /// <summary>
        ///     Controlla se la persona ha firmato l'atto
        /// </summary>
        /// <param name="atto"></param>
        /// <param name="personaUId"></param>
        /// <returns></returns>
        public async Task<bool> CheckFirmato(Guid attoUId, Guid? personaUId)
        {
            if (personaUId is null) return false;
            var firme = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == attoUId && f.UID_persona == personaUId);
            return await firme.AnyAsync();
        }

        /// <summary>
        ///     Riepilogo firmatari
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ATTI_FIRME>> GetFirmatari(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO && tipo == FirmeTipoEnum.DOPO_DEPOSITO)
                return new List<ATTI_FIRME>();

            var firmaProponente = await PRContext
                .ATTI_FIRME
                .FirstOrDefaultAsync(f =>
                    f.UIDAtto == atto.UIDAtto
                    && f.PrimoFirmatario
                    && f.Valida);

            if (firmaProponente == null)
            {
                return new List<ATTI_FIRME>();
            }

            var query = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == atto.UIDAtto
                            && f.UID_persona != firmaProponente.UID_persona
                            && f.Valida);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp <= atto.Timestamp);
                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp > atto.Timestamp);
                    break;
                case FirmeTipoEnum.ATTIVI:
                    query = query.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                    break;
                case FirmeTipoEnum.RITIRATI:
                    query = query.Where(f => !string.IsNullOrEmpty(f.Data_ritirofirma));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            // #955
            query = query.OrderBy(f =>  DbFunctions.TruncateTime(f.Timestamp))
                .ThenBy(f => f.OrdineVisualizzazione);

            var lst = await query
                .ToListAsync();

            if (firmaProponente != null 
                && tipo != FirmeTipoEnum.DOPO_DEPOSITO
                && tipo != FirmeTipoEnum.RITIRATI) 
                lst.Insert(0, firmaProponente);

            return lst;
        }

        /// <summary>
        ///     Ritorna singola firma
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="personaUId"></param>
        /// <returns></returns>
        public async Task<ATTI_FIRME> FindInCache(Guid attoUId, Guid personaUId)
        {
            return await PRContext
                .ATTI_FIRME
                .FindAsync(attoUId, personaUId);
        }

        /// <summary>
        ///     Ritorna singola firma
        /// </summary>
        /// <param name="attoUId"></param>
        /// <param name="personaUId"></param>
        /// <returns></returns>
        public async Task<ATTI_FIRME> Get(Guid attoUId, Guid personaUId)
        {
            return await PRContext
                .ATTI_FIRME
                .FirstOrDefaultAsync(i => i.UIDAtto == attoUId && i.UID_persona == personaUId);
        }
    }
}