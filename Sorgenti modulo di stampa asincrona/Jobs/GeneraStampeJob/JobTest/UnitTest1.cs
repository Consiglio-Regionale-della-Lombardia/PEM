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
                    Password = "**",
                    Username = "**",
                    RootRepository = @"D:\Regione Lombardia\Emendamenti",
                    UrlAPI = "http://localhost:52415",
                    UrlCLIENT = "http://localhost:58019",
                    PDF_LICENSE = "***",
                    ConnectionString = "data source=DESKTOP-FJ2A7DR\\GAM01;initial catalog=dbEmendamenti_TestImport;Trusted_Connection=True;App=EntityFramework",
                    masterKey = "***"
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
