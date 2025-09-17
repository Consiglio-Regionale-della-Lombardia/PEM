using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeneraStampeJobFramework;
using Newtonsoft.Json;
using NUnit.Framework;
using PortaleRegione.GestioneStampe;

namespace TestProject
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task Stampa()
        {
            try
            {
                var config = JsonConvert.DeserializeObject<ThreadWorkerModel>(
                    File.ReadAllText("testsettings.json"));

                var manager = new Manager(config);
                await manager.Run();

                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [Test]
        public async Task StampaMerge()
        {
            try
            {
                var stamper = new PdfStamper_Playwright();
                var docs = Directory.GetFiles("C:\\Regione Lombardia\\Temp\\2af7f0be-01d1-44d1-8457-6746b5de2311").ToList();
                stamper.MergedPDF("C:\\Regione Lombardia\\Temp\\2af7f0be-01d1-44d1-8457-6746b5de2311\\test.pdf", docs);
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