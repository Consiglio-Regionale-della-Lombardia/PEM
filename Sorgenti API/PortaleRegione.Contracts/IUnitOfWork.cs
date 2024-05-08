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

namespace PortaleRegione.Contracts
{
    /// <summary>
    /// Interfaccia di comunicazione tra i vari repository
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ISeduteRepository Sedute { get; }
        IPersoneRepository Persone { get; }
        ILegislatureRepository Legislature { get; }
        IAttiRepository Atti { get; }
        IArticoliRepository Articoli { get; }
        ICommiRepository Commi { get; }
        ILettereRepository Lettere { get; }
        IRuoliRepository Ruoli { get; }
        IGruppiRepository Gruppi { get; }
        IEmendamentiRepository Emendamenti { get; }
        IStampeRepository Stampe { get; }
        IFirmeRepository Firme { get; }
        INotificheRepository Notifiche { get; }
        INotifiche_DestinatariRepository Notifiche_Destinatari { get; }
        IDASIRepository DASI { get; }
        IAttiFirmeRepository Atti_Firme { get; }
        IFiltriRepository Filtri { get; }
        IReportsRepository Reports { get; }

        /// <summary>
        /// Salva le operazioni fatte finora nel Context
        /// </summary>
        /// <returns></returns>
        Task<int> CompleteAsync();
    }
}