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
        [TestCase("matteo.c", "Opencast88!")]
        public async Task Stampa(string username, string password)
        {
            await Init(username, password);
            try
            {
                var stampa = await apiGateway.Stampe.Get(new Guid("43fd198c-1638-4c70-b24c-43f1975e236b"));
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
                    UrlAPI = "http://localhost:52417",
                    UrlCLIENT = "http://localhost:58020",
                    PDF_LICENSE = "IRONPDF.DEVTEAM.IRO231017.6714.98141B-AABBF41E63-CUID3XETD2IFM3N-LNILMOQXMDRE-RBQPZQXCIWXT-AUOEGNSNGNJU-SA2YPICMLHAF-CUD4Y4-LFT3WEY5OLGNUA-IRONPDF.DOTNET.LITE.SUB-XOOCEJ.RENEW.SUPPORT.16.OCT.2024"
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