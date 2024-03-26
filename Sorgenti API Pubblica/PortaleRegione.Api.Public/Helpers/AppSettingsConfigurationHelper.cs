using System.Configuration;

namespace PortaleRegione.Api.Public.Helpers
{
    public class AppSettingsConfigurationHelper
    {
        public static string masterKey => ConfigurationManager.AppSettings["masterKey"];
    }
}