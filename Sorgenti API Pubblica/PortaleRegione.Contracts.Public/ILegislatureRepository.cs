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

using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.Domain;

namespace PortaleRegione.Contracts.Public
{
    /// <summary>
    ///     Fornisce un'astrazione per le operazioni di repository legate alla gestione delle legislature.
    ///     Questo permette di accedere e manipolare i dati relativi alle legislature in modo indipendente dalla fonte dei
    ///     dati.
    /// </summary>
    public interface ILegislatureRepository
    {
        /// <summary>
        ///     Ottiene l'elenco completo delle legislature.
        ///     Questo metodo è tipicamente utilizzato per recuperare tutte le legislature disponibili nell'applicazione,
        ///     per esempio, per popolare elenchi o dropdown in interfacce utente.
        /// </summary>
        /// <returns>Una task che, una volta completata, restituirà un elenco di oggetti legislature.</returns>
        Task<List<legislature>> GetLegislature();
    }
}