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

namespace PortaleRegione.Gateway
{
    public class ApiGateway : IApiGateway
    {
        private string _token;

        public ApiGateway()
        {
            Persone = new PersoneGateway();
            Emendamento_Pubblico = new EMGateway_Pubblico();
            DASI_Pubblico = new DASIGateway_Pubblico();
        }

        public ApiGateway(string token)
        {
            _token = token;

            Sedute = new SeduteGateway(_token);
            Persone = new PersoneGateway(_token);
            Notifiche = new NotificheGateway(_token);
            Stampe = new StampeGateway(_token);
            Esporta = new EsportaGateway(_token);
            Emendamento = new EMGateway(_token);
            Atti = new AttiGateway(_token);
            Admin = new AdminGateway(_token);
            DASI = new DASIGateway(_token);
            Legislature = new LegislatureGateway(_token);
            Templates = new TemplatesGateway(_token);
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public ISeduteGateway Sedute { get; }
        public IPersoneGateway Persone { get; }
        public INotificheGateway Notifiche { get; }
        public IStampeGateway Stampe { get; }
        public IEsportaGateway Esporta { get; }
        public IEMGateway_Pubblico Emendamento_Pubblico { get; }
        public IDASIGateway_Pubblico DASI_Pubblico { get; }
        public IEMGateway Emendamento { get; }
        public IAttiGateway Atti { get; }
        public IAdminGateway Admin { get; }
        public IDASIGateway DASI { get; set; }
        public ILegislatureGateway Legislature { get; set; }
        public ITemplatesGateway Templates { get; set; }
    }
}