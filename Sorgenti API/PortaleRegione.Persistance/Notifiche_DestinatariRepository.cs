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

using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class Notifiche_DestinatariRepository : Repository<NOTIFICHE_DESTINATARI>, INotifiche_DestinatariRepository
    {
        public Notifiche_DestinatariRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<NOTIFICHE_DESTINATARI> Get(long notificaId, Guid personaUId)
        {
            return await PRContext.NOTIFICHE_DESTINATARI.SingleOrDefaultAsync(nd =>
                nd.UIDNotifica == notificaId && nd.UIDPersona == personaUId);
        }

        public async Task<NOTIFICHE_DESTINATARI> ExistDestinatarioNotifica(Guid guid, Guid personaUId, bool dasi = false)
        {
            if (dasi)
            {
                var notificheDASI = await PRContext
                    .NOTIFICHE
                    .Where(nd => nd.UIDAtto == guid)
                    .Select(i => i.UIDNotifica)
                    .ToListAsync();

                var DestinatarioDASI = await PRContext
                    .NOTIFICHE_DESTINATARI
                    .Where(nd => nd.UIDPersona == personaUId && notificheDASI.Contains(nd.UIDNotifica))
                    .FirstOrDefaultAsync();
                return DestinatarioDASI;
            }

            var notifichePEM = await PRContext
                .NOTIFICHE
                .Where(nd => nd.UIDEM == guid)
                .Select(i => i.UIDNotifica)
                .ToListAsync();

            var DestinatarioEM = await PRContext
                .NOTIFICHE_DESTINATARI
                .Where(nd => nd.UIDPersona == personaUId && notifichePEM.Contains(nd.UIDNotifica))
                .FirstOrDefaultAsync();

            return DestinatarioEM;
        }

        public async Task SetSeen_DestinatarioNotifica(NOTIFICHE_DESTINATARI destinatario, Guid personaUId)
        {
            destinatario.Visto = true;
            destinatario.DataVisto = DateTime.Now;

            await PRContext.SaveChangesAsync();
        }
    }
}