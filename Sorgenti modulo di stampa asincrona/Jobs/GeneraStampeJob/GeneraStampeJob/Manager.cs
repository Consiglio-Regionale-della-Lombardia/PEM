﻿using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.Gateway;
using System;
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