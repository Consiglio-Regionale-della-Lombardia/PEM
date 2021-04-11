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
    /// <summary>
    ///     Interfaccia Firme
    /// </summary>
    public interface IFirmeRepository : IRepository<FIRME>
    {
        void Firma(Guid emendamentoUId, Guid personaUId, string firmaCert, string dataFirmaCert, bool ufficio = false);
        Task<int> CountFirme(Guid emendamentoUId);
        Task<IEnumerable<FIRME>> GetFirmatari(EM em, FirmeTipoEnum tipo);
        Task CancellaFirme(Guid emendamentoUId);
        Task<bool> CheckFirmato(Guid emendamentoUId, Guid personaUId);
        Task<bool> CheckIfFirmabile(EmendamentiDto em, PersonaDto persona);
        Task<bool> CheckFirmatoDaUfficio(Guid emedamentoUId);
    }
}