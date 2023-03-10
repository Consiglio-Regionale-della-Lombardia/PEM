using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using System.Threading.Tasks;

namespace PortaleRegione.JobStampeTest
{
    public class BaseTest
    {
        private string apiURL = "http://localhost:52415";
        public ApiGateway apiGateway;

        protected BaseTest()
        {
            BaseGateway.apiUrl = apiURL;
            apiGateway = new ApiGateway();
        }

        protected async Task<LoginResponse> Init(string username, string password)
        {
            var result = await apiGateway.Persone.Login(new LoginRequest
            {
                Username = username,
                Password = password
            });
            apiGateway = new ApiGateway(result.jwt);
            return result;
        }

    }
}