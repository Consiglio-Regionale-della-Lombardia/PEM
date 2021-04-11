using System;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Gateway;
using PortaleRegione.Logger;
using Quartz;

namespace GeneraStampeJob
{
    public class Genera : IJob
    {
        #region Execute

        public async Task Execute(IJobExecutionContext context)
        {
            ConvertParameters(context.JobDetail.JobDataMap);

            //LOGIN 
            BaseGateway.apiUrl = urlAPI;
            var auth = await ApiGateway.Login(username, password);
            BaseGateway.access_token = auth.jwt;

            //GET STAMPE
            var stampe = await ApiGateway.JobGetStampe(1, 20);
            Log.Debug($"Stampe da elaborare [{stampe.Results.Count()}]");

            foreach (var stampa in stampe.Results)
            {
                Log.Debug($"Stampa [{stampa.UIDStampa}]");

                var worker = new ThreadWorker(stampa, new ThreadWorkerModel
                {
                    username = username,
                    password = password,
                    urlAPI = urlAPI,
                    urlCLIENT = urlCLIENT,
                    CartellaStampeLink = CartellaStampeLink,
                    NumMaxTentativi = NumMaxTentativi,
                    CartellaLavoroTemporanea = CartellaLavoroTemporanea,
                    CartellaLavoroStampe = CartellaLavoroStampe,
                    RootRepository = RootRepository,
                    EmailFrom = EmailFrom
                });
            }

            Log.Debug("### FINE ###");
        }

        #endregion

        #region ConvertParameters

        private void ConvertParameters(JobDataMap data)
        {
            foreach (var item in data) Log.Debug($"Key: [{item.Key}], Value: [{item.Value}]");

            var error = false;

            if (data.ContainsKey("username"))
                username = data.Get("username").ToString();
            if (string.IsNullOrEmpty(username))
            {
                error = true;
                Log.Debug("Parametro [username] non configurato");
            }

            if (data.ContainsKey("password"))
                password = data.Get("password").ToString();
            if (string.IsNullOrEmpty(password))
            {
                error = true;
                Log.Debug("Parametro [password] non configurato");
            }

            if (data.ContainsKey("urlAPI"))
                urlAPI = data.Get("urlAPI").ToString();
            if (string.IsNullOrEmpty(urlAPI))
            {
                error = true;
                Log.Debug("Parametro [urlAPI] non configurato");
            }

            if (data.ContainsKey("urlCLIENT"))
                urlCLIENT = data.Get("urlCLIENT").ToString();
            if (string.IsNullOrEmpty(urlCLIENT))
            {
                error = true;
                Log.Debug("Parametro [urlCLIENT] non configurato");
            }

            if (data.ContainsKey("CartellaStampeLink"))
                CartellaStampeLink = data.Get("CartellaStampeLink").ToString();
            if (string.IsNullOrEmpty(CartellaStampeLink))
            {
                error = true;
                Log.Debug("Parametro [CartellaStampeLink] non configurato");
            }

            if (data.ContainsKey("NumMaxTentativi"))
                NumMaxTentativi = data.Get("NumMaxTentativi").ToString();
            if (string.IsNullOrEmpty(NumMaxTentativi))
            {
                error = true;
                Log.Debug("Parametro [NumMaxTentativi] non configurato");
            }

            if (data.ContainsKey("CartellaLavoroTemporanea"))
                CartellaLavoroTemporanea = data.Get("CartellaLavoroTemporanea").ToString();
            if (string.IsNullOrEmpty(CartellaLavoroTemporanea))
            {
                error = true;
                Log.Debug("Parametro [CartellaLavoroTemporanea] non configurato");
            }

            if (data.ContainsKey("CartellaLavoroStampe"))
                CartellaLavoroStampe = data.Get("CartellaLavoroStampe").ToString();
            if (string.IsNullOrEmpty(CartellaLavoroStampe))
            {
                error = true;
                Log.Debug("Parametro [CartellaLavoroStampe] non configurato");
            }

            if (data.ContainsKey("SMTP"))
                SMTP = data.Get("SMTP").ToString();
            if (string.IsNullOrEmpty(SMTP))
            {
                error = true;
                Log.Debug("Parametro [SMTP] non configurato");
            }

            if (data.ContainsKey("EmailFrom"))
                EmailFrom = data.Get("EmailFrom").ToString();
            if (string.IsNullOrEmpty(EmailFrom))
            {
                error = true;
                Log.Debug("Parametro [EmailFrom] non configurato");
            }

            if (data.ContainsKey("RootRepository"))
                RootRepository = data.Get("RootRepository").ToString();
            if (string.IsNullOrEmpty(RootRepository))
            {
                error = true;
                Log.Debug("Parametro [RootRepository] non configurato");
            }

            if (data.ContainsKey("InvioNotifiche"))
                InvioNotifiche = Convert.ToBoolean(Convert.ToInt16(data.Get("InvioNotifiche")));
            Log.Debug($"Parametro [InvioNotifiche] = {InvioNotifiche}");

            if (error)
                throw new Exception("Mancano dei parametri alla configurazione.");
        }

        #endregion

        #region FIELDS

        public string username { get; set; }
        public string password { get; set; }
        public string urlAPI { get; set; }
        public string urlCLIENT { get; set; }
        public string CartellaStampeLink { get; set; }
        public string NumMaxTentativi { get; set; }
        public string CartellaLavoroTemporanea { get; set; }
        public string CartellaLavoroStampe { get; set; }
        public string SMTP { get; set; }
        public string EmailFrom { get; set; }
        public string RootRepository { get; set; }
        public bool InvioNotifiche { get; set; }

        #endregion
    }
}