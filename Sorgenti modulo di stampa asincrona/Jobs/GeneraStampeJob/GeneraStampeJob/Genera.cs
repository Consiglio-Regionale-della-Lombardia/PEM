using Quartz;
using System.Threading.Tasks;

namespace GeneraStampeJob
{
    public class Genera : IJob
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UrlApi { get; set; }
        public string UrlClient { get; set; }
        public string NumMaxTentativi { get; set; }
        public string CartellaLavoroTemporanea { get; set; }
        public string CartellaLavoroStampe { get; set; }
        public string EmailFrom { get; set; }
        public string RootRepository { get; set; }
        public string PDF_LICENSE { get; set; }
        public string PercorsoCompatibilitaDocumenti { get; set; }


        public async Task Execute(IJobExecutionContext context)
        {
            ConvertParameters(context.JobDetail.JobDataMap);

            //GET STAMPE
            var manager = new Manager(new ThreadWorkerModel
            {
                UrlAPI = UrlApi,
                UrlCLIENT = UrlClient,
                Username = Username,
                Password = Password,
                EmailFrom = EmailFrom,
                NumMaxTentativi = NumMaxTentativi,
                RootRepository = RootRepository,
                CartellaLavoroStampe = CartellaLavoroStampe,
                CartellaLavoroTemporanea = CartellaLavoroTemporanea,
                PDF_LICENSE = PDF_LICENSE,
                PercorsoCompatibilitaDocumenti = PercorsoCompatibilitaDocumenti
            });
            await manager.Run();
        }

        private void ConvertParameters(JobDataMap data)
        {
            foreach (var item in data) ////Log.Debug($"Key: [{item.Key}], Value: [{item.Value}]");

                //var error = false;

                if (data.ContainsKey(nameof(ThreadWorkerModel.Username)))
                    Username = data.Get(nameof(ThreadWorkerModel.Username)).ToString();
            if (string.IsNullOrEmpty(Username))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.Username)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.Password)))
                Password = data.Get(nameof(ThreadWorkerModel.Password)).ToString();
            if (string.IsNullOrEmpty(Password))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.Password)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.UrlAPI)))
                UrlApi = data.Get(nameof(ThreadWorkerModel.UrlAPI)).ToString();
            if (string.IsNullOrEmpty(UrlApi))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.UrlAPI)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.UrlCLIENT)))
                UrlClient = data.Get(nameof(ThreadWorkerModel.UrlCLIENT)).ToString();
            if (string.IsNullOrEmpty(UrlClient))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.UrlCLIENT)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.NumMaxTentativi)))
                NumMaxTentativi = data.Get(nameof(ThreadWorkerModel.NumMaxTentativi)).ToString();
            if (string.IsNullOrEmpty(NumMaxTentativi))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.NumMaxTentativi)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.CartellaLavoroTemporanea)))
                CartellaLavoroTemporanea = data.Get(nameof(ThreadWorkerModel.CartellaLavoroTemporanea)).ToString();
            if (string.IsNullOrEmpty(CartellaLavoroTemporanea))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.CartellaLavoroTemporanea)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.CartellaLavoroStampe)))
                CartellaLavoroStampe = data.Get(nameof(ThreadWorkerModel.CartellaLavoroStampe)).ToString();
            if (string.IsNullOrEmpty(CartellaLavoroStampe))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.CartellaLavoroStampe)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.EmailFrom)))
                EmailFrom = data.Get(nameof(ThreadWorkerModel.EmailFrom)).ToString();
            if (string.IsNullOrEmpty(EmailFrom))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.EmailFrom)}] non configurato");
            }

            if (data.ContainsKey(nameof(ThreadWorkerModel.RootRepository)))
                RootRepository = data.Get(nameof(ThreadWorkerModel.RootRepository)).ToString();
            if (string.IsNullOrEmpty(RootRepository))
            {
                //error = true;
                ////Log.Debug($"Parametro [{nameof(ThreadWorkerModel.RootRepository)}] non configurato");
            }
            if (data.ContainsKey(nameof(ThreadWorkerModel.PDF_LICENSE)))
                PDF_LICENSE = data.Get(nameof(ThreadWorkerModel.PDF_LICENSE)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.PercorsoCompatibilitaDocumenti)))
                PercorsoCompatibilitaDocumenti = data.Get(nameof(ThreadWorkerModel.PercorsoCompatibilitaDocumenti)).ToString();

            //if (error)
            //    throw new Exception("Mancano dei parametri alla configurazione.");
        }
    }
}