using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.DTO.Domain;
using PortaleRegione.Gateway;
using PortaleRegione.Logger;

namespace GeneraStampeJob
{
    public class Manager
    {
        private ThreadWorkerModel _model;

        public Manager(ThreadWorkerModel model)
        {
            _model = model;
            BaseGateway.apiUrl = _model.UrlAPI;
        }

        public event EventHandler<bool> OnManagerFinish;

        public async Task Run()
        {
            try
            {
                var auth = await PersoneGate.Login(_model.Username, _model.Password);

                var stampe = await StampeGate.JobGetStampe(1, 1);
                Log.Debug($"Stampe da elaborare [{stampe.Results.Count()}]");
                
                var work = new Worker(ref auth, ref _model);
                foreach (var stampa in stampe.Results)
                {
                    await work.ExecuteAsync(stampa);
                }
                OnManagerFinish?.Invoke(this, true);
                Log.Debug("### FINE ###");
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