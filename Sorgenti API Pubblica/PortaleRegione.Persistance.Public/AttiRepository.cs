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
using System.Data.Entity;
using System.Threading.Tasks;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;

namespace PortaleRegione.Persistance.Public
{
    /// <summary>
    ///     Repository per la gestione degli ATTI, fornendo operazioni di lettura sui dati degli atti.
    /// </summary>
    public class AttiRepository : IAttiRepository
    {
        /// <summary>
        ///     Contesto del database utilizzato per l'accesso ai dati.
        /// </summary>
        private readonly DbContext Context;

        public AttiRepository(DbContext context)
        {
            Context = context;
        }
        
        /// <summary>
        ///     Proprietà di utilità per accedere al contesto del database specifico di PortaleRegione come tipizzato DbContext.
        /// </summary>
        private PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;
        
        public async Task<ATTI> Get(Guid attoUId)
        {
            var result = await PRContext
                .ATTI
                .SingleOrDefaultAsync(a => a.UIDAtto == attoUId);
            return result;
        }
    }
}