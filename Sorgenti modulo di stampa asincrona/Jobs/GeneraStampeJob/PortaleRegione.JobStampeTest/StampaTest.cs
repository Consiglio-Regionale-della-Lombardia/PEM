using GeneraStampeJob;
using NUnit.Framework;
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
                var stampa = await apiGateway.Stampe.Get(new Guid("fc21675b-0081-48d6-baf4-dbcf92391776"));
                var model = new ThreadWorkerModel
                {
                    CartellaLavoroStampe = @"C:\Temp\Regione Lombardia\Stampe",
                    CartellaLavoroTemporanea = @"C:\Temp\Regione Lombardia\Temp",
                    EmailFrom = "",
                    NumMaxTentativi = "3",
                    Password = "passWD01",
                    Username = "servizio_pem",
                    RootRepository = @"C:\Temp\Regione Lombardia\Repository",
                    UrlAPI = "http://localhost:52415",
                    UrlCLIENT = "http://localhost:58019",
                    PDF_LICENSE = "IRONPDF.NAMIRIALSPA.IRO221014.7583.32109.410122-18A3800A31-CFV723JMBJIYZ4K-HCIT6H72EBCJ-NQYCKHU2WYVO-DUCCQUCNKAXF-MGYXL4N53BA5-UH6NWC-L23P7VJKHTGKUA-LITE.SUB-JVZX4D.RENEW.SUPPORT.14.OCT.2023"
                };

                var auth = await apiGateway.Persone.Login(model.Username, model.Password);

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