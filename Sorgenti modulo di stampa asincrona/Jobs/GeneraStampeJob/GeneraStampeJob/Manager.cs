using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.Gateway;
using System;
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

using System.Threading.Tasks;

namespace GeneraStampeJob
{
    public class Manager
    {
        private ThreadWorkerModel _model;

        public Manager(ThreadWorkerModel model)
        {
            _model = model;
        }

        public event EventHandler<bool> OnManagerFinish;

        public async Task Run()
        {
            try
            {
                //Log.Debug($"URL API [{_model.UrlAPI}]");
                BaseGateway.apiUrl = _model.UrlAPI;
                var apiGateway = new ApiGateway();
                //Log.Debug($"User service [{_model.Username}][{_model.Password}]");
                var auth = await apiGateway.Persone.Login(new LoginRequest
                {
                    Username = _model.Username,
                    Password = _model.Password
                });
                //Log.Debug($"User service [{auth.id}][{auth.jwt}]");
                apiGateway = new ApiGateway(auth.jwt);
                var stampe = await apiGateway.Stampe.JobGetStampe(1, 5);
                //Log.Debug($"Stampe da elaborare [{stampe.Results.Count()}]");

                var work = new Worker(auth, ref _model);
                foreach (var stampa in stampe.Results)
                {
                    await work.ExecuteAsync(stampa);
                }
                OnManagerFinish?.Invoke(this, true);
                //Log.Debug("### FINE ###");
            }
            catch (Exception e)
            {
                OnManagerFinish?.Invoke(this, false);
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}