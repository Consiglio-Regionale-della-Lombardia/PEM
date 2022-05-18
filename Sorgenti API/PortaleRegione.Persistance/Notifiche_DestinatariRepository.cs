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

        public async Task<bool> ExistDestinatarioNotifica(Guid emendamentoUId, Guid personaUId)
        {
            var listaDestinatariEM = await PRContext
                .NOTIFICHE_DESTINATARI
                .Where(nd => nd.NOTIFICHE.UIDEM == emendamentoUId && nd.UIDPersona == personaUId)
                .ToListAsync();

            return listaDestinatariEM.Any();
        }

        public async Task<bool> ExistDestinatarioNotificaDASI(Guid attoUId, Guid personaUId)
        {
            var listaDestinatariDASI = await PRContext
                .NOTIFICHE_DESTINATARI
                .Where(nd => nd.NOTIFICHE.UIDAtto == attoUId && nd.UIDPersona == personaUId)
                .ToListAsync();

            return listaDestinatariDASI.Any();
        }

        public async Task SetSeen_DestinatarioNotifica(Guid emendamentoUId, Guid personaUId)
        {
            var listaDestinatariEM = await PRContext
                .NOTIFICHE_DESTINATARI
                .Where(nd => nd.NOTIFICHE.UIDEM == emendamentoUId && nd.UIDPersona == personaUId)
                .ToListAsync();
            foreach (var notificheDestinatari in listaDestinatariEM)
            {
                notificheDestinatari.Visto = true;
                notificheDestinatari.DataVisto = DateTime.Now;
            }

            await PRContext.SaveChangesAsync();
        }
    }
}