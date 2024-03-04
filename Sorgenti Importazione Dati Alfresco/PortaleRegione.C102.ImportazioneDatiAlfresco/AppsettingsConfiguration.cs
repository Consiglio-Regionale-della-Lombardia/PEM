using System.Configuration;

namespace PortaleRegione.C102.ImportazioneDatiAlfresco
{
    public static class AppsettingsConfiguration
    {
        internal static readonly string URL_API = ConfigurationManager.AppSettings["url_api"];
        internal static readonly string USER_API = ConfigurationManager.AppSettings["user_api"];
        internal static readonly string PASSWORD_API = ConfigurationManager.AppSettings["password_api"];
        internal static readonly string MASTER_KEY = ConfigurationManager.AppSettings["master_key"];
        internal static readonly string CONNECTIONSTRING = ConfigurationManager.AppSettings["connection_string"];
    }
}