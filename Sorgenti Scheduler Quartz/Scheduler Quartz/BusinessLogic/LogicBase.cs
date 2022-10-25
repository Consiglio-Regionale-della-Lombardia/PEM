using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace Scheduler.BusinessLogic
{
    public abstract class LogicBase
    {
        internal LoginResponse currentServiceUser { get; set; }

        protected bool IsValidConfigPath(string path)
        {
            return File.Exists(path);
        }

        protected async Task<LoginResponse> Init()
        {
            BaseGateway.apiUrl = ConfigurationManager.AppSettings["UrlApi"];
            var apiGateway = new ApiGateway();
            var result = await apiGateway.Persone.Login(
                ConfigurationManager.AppSettings["ServiceUsername"],
                ConfigurationManager.AppSettings["ServicePassword"]);
            currentServiceUser = result;
            return result;
        }
    }
}