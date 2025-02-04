using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using CalcoloRitardoAttoJob;

namespace TestJob
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task Run()
        {
            var manager = new Manager(new ThreadWorkerModel
            {
                connectionString = @"Data Source=DESKTOP-FJ2A7DR\GAM01;Initial Catalog=dbEmendamenti_TestImport;User ID=admin_test_import;Password=123456;"
            });

            await manager.Run();
            Assert.IsTrue(true);
        }
    }
}
