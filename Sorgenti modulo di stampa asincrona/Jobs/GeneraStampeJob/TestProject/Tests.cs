using System;
using System.IO;
using System.Threading.Tasks;
using GeneraStampeJobFramework;
using Newtonsoft.Json;
using NUnit.Framework;

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
    }
}