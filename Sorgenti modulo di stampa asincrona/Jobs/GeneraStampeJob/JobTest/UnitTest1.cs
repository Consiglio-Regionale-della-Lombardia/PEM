using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using GeneraStampeJobFramework;

namespace JobTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task Stampa()
        {
            try
            {
                var manager = new Manager(new ThreadWorkerModel
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
                    PDF_LICENSE = "IRONPDF.DEVTEAM.IRO231017.6714.98141B-AABBF41E63-CUID3XETD2IFM3N-LNILMOQXMDRE-RBQPZQXCIWXT-AUOEGNSNGNJU-SA2YPICMLHAF-CUD4Y4-LFT3WEY5OLGNUA-IRONPDF.DOTNET.LITE.SUB-XOOCEJ.RENEW.SUPPORT.16.OCT.2024",
                    ConnectionString = "data source=DESKTOP-FJ2A7DR\\GAM01;initial catalog=dbEmendamenti_TestImport;Trusted_Connection=True;App=EntityFramework"
                });
                await manager.Run();

                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
