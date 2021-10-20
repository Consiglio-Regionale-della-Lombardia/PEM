using PortaleRegione.Gateway;
using PortaleRegione.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                var apiGateway = new ApiGateway();
                var auth = await apiGateway.Persone.Login(_model.Username, _model.Password);
                var apiGateway2 = new ApiGateway(auth.jwt);
                var stampe = await apiGateway2.Stampe.JobGetStampe(1, 1);
                Log.Debug($"Stampe da elaborare [{stampe.Results.Count()}]");

                var work = new Worker(auth, ref _model);
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