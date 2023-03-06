using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

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
            try
            {
                BaseGateway.apiUrl = ConfigurationManager.AppSettings["UrlApi"];
                var apiGateway = new ApiGateway();
                var result = await apiGateway.Persone.Login(
                    ConfigurationManager.AppSettings["ServiceUsername"],
                    ConfigurationManager.AppSettings["ServicePassword"]);
                currentServiceUser = result;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}