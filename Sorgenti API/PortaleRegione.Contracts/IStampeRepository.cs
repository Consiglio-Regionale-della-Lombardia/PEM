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

using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Interfaccia Stampe
    /// </summary>
    public interface IStampeRepository : IRepository<STAMPE>
    {
        Task<IEnumerable<STAMPE>> GetAll(PersonaDto persona, int? page, int? size, Filter<STAMPE> filtro = null);
        Task<IEnumerable<STAMPE>> GetAll(int page, int size);
        Task<IEnumerable<STAMPE>> GetStampeFascicolo(Guid uidFascicolo);
        Task<int> Count(PersonaDto persona, Filter<STAMPE> filtro = null);
        Task<int> Count();
        Task<STAMPE> Get(Guid stampaUId);
        void AddInfo(Guid stampaUId, string messaggio);
        Task<IEnumerable<STAMPE_INFO>> GetInfo(Guid stampaUId);
        Task<IEnumerable<STAMPE_INFO>> GetInfo();
        Task<STAMPE_INFO> GetLastInfo(Guid stampaUId);
    }
}