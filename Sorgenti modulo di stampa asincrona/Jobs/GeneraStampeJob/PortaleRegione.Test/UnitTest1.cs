using GeneraStampeJob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortaleRegione.Gateway;
using System;
using System.Threading.Tasks;

namespace PortaleRegione.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {

            var _model = new ThreadWorkerModel
            {
                UrlCLIENT = "http://localhost:58019",
                UrlAPI = "http://localhost:52415",
                CartellaLavoroStampe = "",
                CartellaLavoroTemporanea = "",
                EmailFrom = "",
                NumMaxTentativi = "3",
                RootRepository = "",
                Username = "",
                Password = ""
            };
            BaseGateway.apiUrl = _model.UrlAPI;
            var apiGateway = new ApiGateway();
            var auth = await apiGateway.Persone.Login(_model.Username, _model.Password);
            var work = new Worker(auth, ref _model);
            var stampa = await apiGateway.Stampe.Get(new Guid(""));
            await work.ExecuteAsync(stampa);
        }
    }
}
