using System.Web.Mvc;
using System.Web.Routing;

namespace PortaleRegione.Client
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;
            
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Sedute", action = "RiepilogoSedute", id = UrlParameter.Optional}
            );
        }
    }
}