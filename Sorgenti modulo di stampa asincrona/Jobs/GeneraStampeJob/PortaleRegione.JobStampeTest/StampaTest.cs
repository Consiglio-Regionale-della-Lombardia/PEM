using GeneraStampeJob;
using NUnit.Framework;
using PortaleRegione.DTO.Autenticazione;
using System;
using System.Threading.Tasks;

namespace PortaleRegione.JobStampeTest
{
    public class Tests : BaseTest
    {
        [Test]
        [TestCase("matteo.c", "Opencast88")]
        public async Task Stampa(string username, string password)
        {
            await Init(username, password);
            try
            {
                var stampa = await apiGateway.Stampe.Get(new Guid("2b6d63d0-c980-4ba5-9e1a-30c94bcdb5a6"));
                var model = new ThreadWorkerModel
                {
                    CartellaLavoroStampe = @"D:\Regione Lombardia\Stampe",
                    CartellaLavoroTemporanea = @"D:\Regione Lombardia\Temp",
                    PercorsoCompatibilitaDocumenti = @"D:\DocumentiPEM",
                    EmailFrom = "",
                    NumMaxTentativi = "3",
                    Password = "passWD01",
                    Username = "servizio_pem",
                    RootRepository = @"D:\Regione Lombardia\Emendamenti",
                    UrlAPI = "http://localhost:52415",
                    UrlCLIENT = "http://localhost:58019",
                    PDF_LICENSE = ""
                };

                var auth = await apiGateway.Persone.Login(new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password
                });

                var worker = new Worker(auth, ref model);
                var result_of_work = false;
                worker.OnWorkerFinish += (sender, args) =>
                {
                    result_of_work = args;
                    Assert.That(args, Is.True);
                };
                await worker.ExecuteAsync(stampa);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}