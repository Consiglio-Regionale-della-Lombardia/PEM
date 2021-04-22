using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

namespace Scheduler.BusinessLogic
{
    public abstract class LogicBase
    {
        protected bool IsValidConfigPath(string path)
        {
            return File.Exists(path);
        }

        protected async Task<LoginResponse> Init()
        {
            var result = await PersoneGate.Login(
                ConfigurationManager.AppSettings["ServiceUsername"],
                ConfigurationManager.AppSettings["ServicePassword"]);
            BaseGateway.access_token = result.jwt;
            return result;
        }
    }
}