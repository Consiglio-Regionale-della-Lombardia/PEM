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
using System.Threading.Tasks;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PortaleRegioneDbContext _context;

        public UnitOfWork(PortaleRegioneDbContext context)
        {
            _context = context;

            Sedute = new SeduteRepository(_context);
            Persone = new PersoneRepository(_context);
            Legislature = new LegislatureRepository(_context);
            Atti = new AttiRepository(_context);
            Ruoli = new RuoliRepository(_context);
            Gruppi = new GruppiRepository(_context);
            Emendamenti = new EmendamentiRepository(_context);
            Stampe = new StampeRepository(_context);
            Articoli = new ArticoliRepository(_context);
            Commi = new CommiRepository(_context);
            Lettere = new LettereRepository(_context);
            Firme = new FirmeRepository(_context);
            Notifiche = new NotificheRepository(_context);
            Notifiche_Destinatari = new Notifiche_DestinatariRepository(context);
            DASI = new DASIRepository(context);
            Atti_Firme = new AttiFirmeRepository(context);
        }

        public ISeduteRepository Sedute { get; }
        public IPersoneRepository Persone { get; }
        public ILegislatureRepository Legislature { get; }
        public IAttiRepository Atti { get; }
        public IArticoliRepository Articoli { get; }
        public ICommiRepository Commi { get; }
        public ILettereRepository Lettere { get; }
        public IRuoliRepository Ruoli { get; }
        public IGruppiRepository Gruppi { get; }
        public IEmendamentiRepository Emendamenti { get; }
        public IStampeRepository Stampe { get; }
        public IFirmeRepository Firme { get; }
        public INotificheRepository Notifiche { get; }
        public INotifiche_DestinatariRepository Notifiche_Destinatari { get; }
        public IDASIRepository DASI { get; }
        public IAttiFirmeRepository Atti_Firme { get; }

        public async Task<int> CompleteAsync()
        {

            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}