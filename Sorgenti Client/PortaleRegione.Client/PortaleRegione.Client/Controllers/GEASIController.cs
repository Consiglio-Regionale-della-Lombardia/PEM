using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using PortaleRegione.Client.Helpers;
using PortaleRegione.Logger;

namespace PortaleRegione.Client.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("geasi")]
    public class GEASIController : BaseController
    {
        internal static string apiUrl = AppSettingsConfiguration.GEASI_URL;

        [AllowAnonymous]
        [HttpGet]
        [Route("login")]
        public async Task<ActionResult> Login()
        {
            var result = "";
            var url =
                $"{apiUrl}/api/login?u={AppSettingsConfiguration.GEASI_USERNAME}&pw={AppSettingsConfiguration.GEASI_PASSWORD}";
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                var string_response = await response.Content.ReadAsStringAsync();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(string_response);
                foreach (XmlNode node in xmlDoc.ChildNodes)
                {
                    if (node.Name == "ticket")
                    {
                        result = node.InnerText;
                        break;
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error($"Login [{url}]", e);
                throw e;
            }
            catch (Exception e)
            {
                Log.Error($"Login [{url}]", e);
                throw e;
            }
        }
        
        [AllowAnonymous]
        [HttpGet]
        [Route("search")]
        public async Task<ActionResult> Search(string ticket, string tipoatto, int natto, int idlegislatura)
        {
            var url =
                $"{apiUrl}/search?limit=1&alf_ticket={ticket}&tipoAtto={tipoatto}&numeroAtto={natto}&idLegislatura={idlegislatura}";
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                var string_response = await response.Content.ReadAsStringAsync();
                
                return Json(string_response, JsonRequestBehavior.AllowGet);
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error($"Search [{url}]", e);
                throw e;
            }
            catch (Exception e)
            {
                Log.Error($"Search [{url}]", e);
                throw e;
            }
        }
    }
}