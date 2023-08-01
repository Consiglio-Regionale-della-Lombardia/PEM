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

using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Contracts
{
    public interface IAttiFirmeRepository : IRepository<ATTI_FIRME>
    {
        Task Firma(Guid attoUId, Guid personaUId, int gruppoIdGruppo, string firmaCert, string dataFirmaCert, DateTime timestamp,
            bool ufficio = false,
            bool primoFirmatario = false,
            bool valida = true,
            bool capogruppo = false);
        Task<int> CountFirme(Guid attoUId);
        Task<IEnumerable<ATTI_FIRME>> GetFirmatari(ATTI_DASI atto, FirmeTipoEnum tipo);
        Task<IEnumerable<ATTI_FIRME>> GetFirmatari(Guid attoUId);
        Task<List<ATTI_FIRME>> GetFirmatari(List<Guid> guids, int max_result);
        Task CancellaFirme(Guid attoUId);
        Task<bool> CheckFirmato(Guid attoUId, Guid? personaUId);
        Task<bool> CheckIfFirmabile(AttoDASIDto atto, PersonaDto persona, bool firma_ufficio = false);
        Task<bool> CheckFirmatoDaUfficio(Guid attoUId);
        Task<ATTI_FIRME> FindInCache(Guid attoUId, Guid personaUId);
        Task<ATTI_FIRME> Get(Guid attoUId, Guid personaUId);
    }
}