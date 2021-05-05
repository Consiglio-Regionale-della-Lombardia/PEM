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
using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Interfaccia Atti
    /// </summary>
    public interface IAttiRepository : IRepository<ATTI>
    {
        Task<int> Count(Guid sedutaUId, Filter<ATTI> filtro = null);
        Task<int> CountEM(Guid id, bool sub_em, PersonaDto persona, int gruppo);
        Task<ATTI> Get(Guid attoUId);
        Task<ATTI> Get(string attoUId);

        Task<IEnumerable<ATTI>> GetAll(Guid sedutaUId, int pageIndex, int pageSize, Filter<ATTI> filtro = null);
        Task SalvaRelatori(Guid attoUId, IEnumerable<Guid> persone);
        Task<int> PrioritaAtto(Guid sedutaUId);
        Task SPOSTA_UP(Guid attoUId);
        Task SPOSTA_DOWN(Guid attoUId);
        bool CanMoveUp(int currentPriorita);
        Task<bool> CanMoveDown(Guid sedutaUId, int currentPriorita);

        Task RimuoviFascicoliObsoleti(Guid attoUId, OrdinamentoEnum ordinamento);
    }
}