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
using System.Threading.Tasks;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Contracts
{
    public interface IAttiFirmeRepository : IRepository<ATTI_FIRME>
    {
        Task Firma(Guid attoUId, Guid personaUId, string firmaCert, string dataFirmaCert, bool ufficio = false);
        Task<int> CountFirme(Guid attoUId);
        Task<IEnumerable<ATTI_FIRME>> GetFirmatari(ATTI_DASI atto, FirmeTipoEnum tipo);
        Task CancellaFirme(Guid attoUId);
        Task<bool> CheckFirmato(Guid attoUId, Guid personaUId);
        Task<bool> CheckIfFirmabile(AttoDASIDto atto, PersonaDto persona);
        Task<bool> CheckFirmatoDaUfficio(Guid attoUId);
        Task<ATTI_FIRME> Get(Guid attoUId, Guid personaUId);
    }
}