using System.Configuration;

namespace PortaleRegione.C102.ImportazioneDatiAlfresco
{
    public static class AppsettingsConfiguration
    {
        internal static readonly string MASTER_KEY = ConfigurationManager.AppSettings["master_key"];
        internal static readonly string CONNECTIONSTRING = ConfigurationManager.AppSettings["connection_string"];
    }
}