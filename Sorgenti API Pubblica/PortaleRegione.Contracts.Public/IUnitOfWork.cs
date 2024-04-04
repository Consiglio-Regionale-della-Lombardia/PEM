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

namespace PortaleRegione.Contracts.Public
{
    /// <summary>
    ///     Fornisce un'interfaccia per un'unità di lavoro che raggruppa l'accesso a diverse repository.
    ///     Questo pattern è utilizzato per garantire che le operazioni su più repository siano gestite in modo coerente,
    ///     e che le modifiche siano commitate o rollbackate come un'unità atomica.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        ///     Ottiene l'accesso alla repository delle legislature.
        /// </summary>
        ILegislatureRepository Legislature { get; }

        /// <summary>
        ///     Ottiene l'accesso alla repository delle persone, inclusi ruoli, commissioni e gruppi.
        /// </summary>
        IPersoneRepository Persone { get; }

        /// <summary>
        ///     Ottiene l'accesso alla repository per la gestione degli ATTI_DASI.
        /// </summary>
        IDASIRepository DASI { get; }
    }
}