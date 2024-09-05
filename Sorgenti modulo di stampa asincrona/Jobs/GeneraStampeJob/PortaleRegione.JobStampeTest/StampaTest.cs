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
        public async Task GetStampe(string username, string password)
        {
            await Init(username, password);

            var stampa = await apiGateway.Stampe.JobGetStampe(1,1);

            Assert.IsNotNull(stampa);
        }

        [Test]
        [TestCase("matteo.c", "Opencast88!", "a8906779-da61-4628-910b-5e830d9f04c3")]
        public async Task Stampa(string username, string password, string uidStampa)
        {
            await Init(username, password);
            try
            {
                var stampa = await apiGateway.Stampe.Get(new Guid(uidStampa));
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
                    PDF_LICENSE = "IRONPDF.NAMIRIALSPA.IRO221014.7583.32109.410122-18A3800A31-CFV723JMBJIYZ4K-HCIT6H72EBCJ-NQYCKHU2WYVO-DUCCQUCNKAXF-MGYXL4N53BA5-UH6NWC-L23P7VJKHTGKUA-LITE.SUB-JVZX4D.RENEW.SUPPORT.14.OCT.2023"
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