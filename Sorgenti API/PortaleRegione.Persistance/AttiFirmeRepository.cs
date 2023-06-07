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

using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

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
            bool ufficio = false, bool primoFirmatario = false, bool valida = true, bool capogruppo = false)
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
                    Capogruppo = capogruppo
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
        ///     Riepilogo firmatari
        /// </summary>
        /// <param name="attoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ATTI_FIRME>> GetFirmatari(Guid attoUId)
        {
            var result = await PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == attoUId && f.Valida)
                .OrderBy(f => f.Timestamp)
                .ToListAsync();

            return result;
        }

        public async Task<List<ATTI_FIRME>> GetFirmatari(List<Guid> guids, int max_result)
        {
            var result = new List<ATTI_FIRME>();
            foreach (var guid in guids)
            {
                result.AddRange(await PRContext
                    .ATTI_FIRME
                    .Where(f => guid == f.UIDAtto && f.Valida && string.IsNullOrEmpty(f.Data_ritirofirma))
                    .OrderBy(f => f.Timestamp)
                    .Take(max_result)
                    .ToListAsync());
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
                .SingleOrDefaultAsync(f =>
                    f.UIDAtto == atto.UIDAtto
                    && f.UID_persona == atto.UIDPersonaProponente
                    && f.Valida);

            var query = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == atto.UIDAtto
                            && f.UID_persona != atto.UIDPersonaProponente
                            && f.Valida);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp < atto.Timestamp);

                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp > atto.Timestamp);
                    break;
                case FirmeTipoEnum.ATTIVI:
                    query = query.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            query = query.OrderBy(f => f.Timestamp);

            var lst = await query
                .ToListAsync();

            if (firmaProponente != null && tipo != FirmeTipoEnum.DOPO_DEPOSITO) lst.Insert(0, firmaProponente);

            return lst;
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
                .FindAsync(attoUId, personaUId);
        }
    }
}