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
        /// <param name="em"></param>
        /// <param name="personaUId"></param>
        /// <param name="firmaCert"></param>
        /// <param name="dataFirmaCert"></param>
        public async Task Firma(Guid attoUId, Guid personaUId, string firmaCert, string dataFirmaCert,
            bool ufficio = false)
        {
            PRContext
                .ATTI_FIRME
                .Add(new ATTI_FIRME
                {
                    UIDAtto = attoUId,
                    UID_persona = personaUId,
                    FirmaCert = firmaCert,
                    Data_firma = dataFirmaCert,
                    Timestamp = DateTime.Now,
                    ufficio = ufficio
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
                .CountAsync(f => f.UIDAtto == attoUId && string.IsNullOrEmpty(f.Data_ritirofirma));
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
        public async Task<bool> CheckIfFirmabile(AttoDASIDto atto, PersonaDto persona)
        {
            if (atto.IDStato > (int)StatiAttoEnum.PRESENTATO)
            {
                return false;
            }

            var firma_personale = await Get(atto.UIDAtto, persona.UID_persona);
            var firma_proponente = await CheckFirmato(atto.UIDAtto, atto.UIDPersonaProponente.Value);

            if (firma_personale == null
                && (firma_proponente || atto.UIDPersonaProponente == persona.UID_persona)
                && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                    persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta ||
                    persona.CurrentRole == RuoliIntEnum.Presidente_Regione))
            {
                return true;
            }

            if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM &&
                persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea)
            {
                return false;
            }

            var firmatoUfficio = await CheckFirmatoDaUfficio(atto.UIDAtto);
            return !firmatoUfficio;
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
        public async Task<bool> CheckFirmato(Guid attoUId, Guid personaUId)
        {
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
            var firmaProponente = await PRContext
                .ATTI_FIRME
                .SingleOrDefaultAsync(f =>
                    f.UIDAtto == atto.UIDAtto
                    && f.UID_persona == atto.UIDPersonaProponente);

            var query = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == atto.UIDAtto
                            && f.UID_persona != atto.UIDPersonaProponente);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (atto.IDStato >= (int)StatiEnum.Depositato)
                    {
                        query = query.Where(f => f.Timestamp < atto.Timestamp);
                    }

                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
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

            if (firmaProponente != null && tipo != FirmeTipoEnum.DOPO_DEPOSITO)
            {
                lst.Insert(0, firmaProponente);
            }

            return lst;
        }

        /// <summary>
        /// Ritorna singola firma
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