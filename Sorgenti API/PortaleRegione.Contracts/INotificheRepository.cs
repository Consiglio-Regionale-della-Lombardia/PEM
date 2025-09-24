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
    ///     Interfaccia Notifiche
    /// </summary>
    public interface INotificheRepository : IRepository<NOTIFICHE>
    {
        Task<IEnumerable<NOTIFICHE>> GetNotificheInviate(PersonaDto currentUser, int idGruppo, bool Archivio, int pageIndex,
            int pageSize,
            Filter<NOTIFICHE> filtro = null);

        Task<int> CountInviate(PersonaDto currentUser, int idGruppo, bool Archivio, Filter<NOTIFICHE> filtro = null);
        Task<int> CountRicevute(PersonaDto currentUser, int idGruppo, bool Archivio, bool Solo_Non_Viste, Filter<NOTIFICHE> filtro = null);

        Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetDestinatariNotifica(string notificaId);

        bool CheckIfNotificabile(EmendamentiDto em, PersonaDto persona);
        bool CheckIfNotificabile(AttoDASIDto atto, PersonaDto persona);

        Task<IEnumerable<NOTIFICHE>> GetNotificheRicevute(PersonaDto currentUser, int idGruppo, bool Archivio,
            bool Solo_Non_Viste, int pageIndex, int pageSize, Filter<NOTIFICHE> filtro = null);
        Task<bool> UpdateNotificaVista(PersonaDto currentUser, Guid uid);
        Task<NOTIFICHE> Get(string id);

        Task<bool> EsisteRitiroDasi(Guid attoUId, Guid personaUId);
        Task<NOTIFICHE> GetBySync(Guid syncGuid);
    }
}