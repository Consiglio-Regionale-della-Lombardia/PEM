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
using System.Threading.Tasks;
using PortaleRegione.Domain;

namespace PortaleRegione.Contracts.Public
{
    /// <summary>
    ///     Definisce le operazioni di repository per la gestione degli ATTI,
    ///     consentendo l'accesso e la manipolazione dei dati degli atti.
    /// </summary>
    public interface IAttiRepository
    {
        /// <summary>
        ///     Recupera un singolo atto utilizzando il suo identificativo unico.
        /// </summary>
        /// <param name="attoUId">L'identificativo unico dell'atto da recuperare.</param>
        /// <returns>L'atto corrispondente all'identificativo fornito.</returns>
        Task<ATTI> Get(Guid attoUId);
    }
}