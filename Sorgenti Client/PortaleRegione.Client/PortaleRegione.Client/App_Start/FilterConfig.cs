using System.Web;
using System.Web.Mvc;
using PortaleRegione.Client.Helpers;

namespace PortaleRegione.Client
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
            // ACT44: Security Headers Filter
            filters.Add(new SecurityHeadersAttribute());
        }
    }
}
