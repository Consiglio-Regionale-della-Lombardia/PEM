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
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class FirmeRepository : Repository<FIRME>, IFirmeRepository
    {
        public FirmeRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        /// <summary>
        ///     Firma emendamento
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <param name="personaUId"></param>
        /// <param name="firmaCert"></param>
        /// <param name="dataFirmaCert"></param>
        /// <param name="tipoAreaFirma"></param>
        /// <param name="ufficio"></param>
        /// <param name="em"></param>
        public async Task Firma(Guid emendamentoUId, Guid personaUId, string firmaCert, string dataFirmaCert,
            int tipoAreaFirma = 0,
            bool ufficio = false)
        {
            PRContext
                .FIRME
                .Add(new FIRME
                {
                    UIDEM = emendamentoUId,
                    UID_persona = personaUId,
                    FirmaCert = firmaCert,
                    Data_firma = dataFirmaCert,
                    Timestamp = DateTime.Now,
                    ufficio = ufficio,
                    id_AreaPolitica = tipoAreaFirma
                });
            await PRContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Conta le firme attive nell'emendamento
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<int> CountFirme(Guid emendamentoUId)
        {
            return await PRContext
                .FIRME
                .CountAsync(f => f.UIDEM == emendamentoUId && string.IsNullOrEmpty(f.Data_ritirofirma));
        }

        public async Task CancellaFirme(Guid emendamentoUId)
        {
            var firmeDaEliminare = await PRContext
                .FIRME
                .Where(f => f.UIDEM == emendamentoUId)
                .ToListAsync();
            PRContext
                .FIRME
                .RemoveRange(firmeDaEliminare);
        }

        /// <summary>
        ///     Controlla che l'emendamento sia firmabile
        /// </summary>
        /// <param name="em"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfFirmabile(EmendamentiDto em, PersonaDto persona)
        {
            if (em.IDStato > (int)StatiEnum.Depositato)
            {
                return false;
            }

            var firma_personale = await Get(em.UIDEM, persona.UID_persona);
            var firma_proponente = await CheckFirmato(em.UIDEM, em.UIDPersonaProponente);

            if (firma_personale == null
                && (firma_proponente || em.UIDPersonaProponente == persona.UID_persona)
                && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                    persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta ||
                    persona.CurrentRole == RuoliIntEnum.Presidente_Regione))
            {
                return true;
            }

            if (!persona.IsSegreteriaAssemblea)
            {
                return false;
            }

            var firmatoUfficio = await CheckFirmatoDaUfficio(em.UIDEM);
            return !firmatoUfficio;
        }

        /// <summary>
        ///     Controlla se l'ufficio ha firmato l'emendamento
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public async Task<bool> CheckFirmatoDaUfficio(Guid emedamentoUId)
        {
            return await PRContext.FIRME.AnyAsync(f => f.UIDEM == emedamentoUId && f.ufficio);
        }

        /// <summary>
        ///     Controlla se la persona ha firmato l'emendamento
        /// </summary>
        /// <param name="em"></param>
        /// <param name="personaUId"></param>
        /// <returns></returns>
        public async Task<bool> CheckFirmato(Guid emendamentoUId, Guid personaUId)
        {
            var firme = PRContext
                .FIRME
                .Where(f => f.UIDEM == emendamentoUId && f.UID_persona == personaUId);
            return await firme.AnyAsync();
        }

        /// <summary>
        ///     Riepilogo firmatari
        /// </summary>
        /// <param name="emendamentoUId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FIRME>> GetFirmatari(EM em, FirmeTipoEnum tipo)
        {
            var firmaProponente = await PRContext
                .FIRME
                .SingleOrDefaultAsync(f =>
                    f.UIDEM == em.UIDEM
                    && f.UID_persona == em.UIDPersonaProponente);

            var query = PRContext
                .FIRME
                .Where(f => f.UIDEM == em.UIDEM
                            && f.UID_persona != em.UIDPersonaProponente);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (em.IDStato >= (int)StatiEnum.Depositato)
                    {
                        query = query.Where(f => f.Timestamp < em.Timestamp);
                    }

                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
                    query = query.Where(f => f.Timestamp > em.Timestamp);
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

        public async Task<FIRME> Get(Guid emendamentoUId, Guid personaUId)
        {
            return await PRContext
                .FIRME
                .FindAsync(emendamentoUId, personaUId);

        }

        public async Task<FIRME> GetFirmaUfficio(Guid uidEM)
        {
            var firma = await PRContext
                .FIRME
                .FirstAsync(item => item.UIDEM == uidEM && item.ufficio == true);
            return firma;
        }

        public async Task<List<FIRME>> GetFirmatariAtto(Guid attoUid)
        {
            var emendamenti = await PRContext
                .EM
                .Where(em => em.UIDAtto == attoUid)
                .Select(em => em.UIDEM)
                .ToListAsync();

            var query = PRContext
                .FIRME
                .Where(f => emendamenti.Contains(f.UIDEM));
            query = query.OrderBy(f => f.Timestamp);

            var lst = await query
                .ToListAsync();

            return lst;
        }
    }
}