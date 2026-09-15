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
                connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=dbEmendamenti;Integrated Security=True;"
            });

            await manager.Run();
            Assert.IsTrue(true);
        }
    }
}
