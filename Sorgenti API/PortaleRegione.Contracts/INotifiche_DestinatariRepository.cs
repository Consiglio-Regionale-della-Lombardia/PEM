﻿/*
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

using PortaleRegione.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Intefaccia Notifiche_Destinatari
    /// </summary>
    public interface INotifiche_DestinatariRepository : IRepository<NOTIFICHE_DESTINATARI>
    {
        Task<List<NOTIFICHE_DESTINATARI>> Get(string notificaId, int gruppoId);
        Task<NOTIFICHE_DESTINATARI> Get(string notificaId, Guid personaUId);
        Task<List<NOTIFICHE_DESTINATARI>> Get(Guid personaUId);
        Task<NOTIFICHE_DESTINATARI> ExistDestinatarioNotifica(Guid guid, Guid personaUId, bool dasi = false);
        Task SetSeen_DestinatarioNotifica(NOTIFICHE_DESTINATARI destinatario, Guid personaUId);
    }
}