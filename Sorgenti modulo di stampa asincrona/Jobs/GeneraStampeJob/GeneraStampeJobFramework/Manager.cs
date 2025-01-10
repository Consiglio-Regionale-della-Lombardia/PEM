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
using PortaleRegione.DataBase;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.Gateway;
using PortaleRegione.Persistance;

namespace GeneraStampeJobFramework
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
                BaseGateway.apiUrl = _model.UrlAPI;
                var apiGateway = new ApiGateway();
                var auth = await apiGateway.Persone.Login(new LoginRequest
                {
                    Username = _model.Username,
                    Password = _model.Password
                });

                using (var context = new PortaleRegioneDbContext(_model.ConnectionString))
                {
                    using (var unitOfWork = new UnitOfWork(context))
                    {
                        // Recupera i dati dal database
                        var stampeList = await unitOfWork.Stampe.GetAll(1, 5);

                        // LOCK STAMPE
                        foreach (var stampa in stampeList)
                        {
                            stampa.Lock = true;
                            stampa.DataLock = DateTime.Now;
                            stampa.DataInizioEsecuzione = DateTime.Now;
                            stampa.Tentativi += 1;

                            await unitOfWork.CompleteAsync();
                        }

                        var worker = new Worker(auth.jwt, unitOfWork, ref _model);
                        foreach (var stampa in stampeList)
                        {
                            await worker.ExecuteAsync(stampa.ToDto());
                        }
                    }
                }

                OnManagerFinish?.Invoke(this, true);
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