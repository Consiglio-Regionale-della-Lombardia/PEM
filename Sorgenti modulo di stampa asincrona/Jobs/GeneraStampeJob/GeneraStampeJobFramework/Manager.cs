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
using log4net;

namespace GeneraStampeJobFramework
{
    public class Manager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Manager));
        private ThreadWorkerModel _model;

        public Manager(ThreadWorkerModel model)
        {
            _model = model;
        }

        public event EventHandler<bool> OnManagerFinish;

        public async Task Run()
        {
            log.Info("Manager.Run() - Avvio procedura stampe...");
            
            try
            {
                BaseGateway.apiUrl = _model.UrlAPI;
                var apiGateway = new ApiGateway();
                log.Info("Richiesta autenticazione tramite ApiGateway...");
                var auth = await apiGateway.Persone.Login(new LoginRequest
                {
                    Username = _model.Username,
                    Password = _model.Password
                });

                log.Info("Autenticazione eseguita con successo.");
                
                using (var context = new PortaleRegioneDbContext(_model.ConnectionString))
                {
                    using (var unitOfWork = new UnitOfWork(context))
                    {
                        log.Info("Recupero e lock delle stampe con PickAndLockStampe...");
                        var stampeList = await unitOfWork.Stampe.PickAndLockStampe(5, 5);
                        log.Info($"Numero stampe trovate e lockate: {stampeList.Count}");
                        
                        var worker = new Worker(auth.jwt, unitOfWork, ref _model);
                        foreach (var stampa in stampeList)
                        {
                            log.Info($"Elaborazione stampa UID={stampa.UIDStampa}");
                            await worker.ExecuteAsync(stampa.ToDto());
                            log.Info($"Stampa UID={stampa.UIDStampa} completata.");
                        }
                    }
                }

                log.Info("Manager.Run() - Tutte le stampe sono state processate.");
                OnManagerFinish?.Invoke(this, true);
            }
            catch (Exception e)
            {
                log.Error("Errore in Manager.Run()", e);
                OnManagerFinish?.Invoke(this, false);
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}