using System;
using System.Threading.Tasks;
using System.Web.Http;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.API.Controllers
{
    [Authorize(Roles = RuoliEnum.Amministratore_PEM + "," + RuoliEnum.Segreteria_Assemblea)]
    [Authorize]
    [RoutePrefix("report")]
    public class ReportController : BaseApiController
    {
        private readonly ReportLogic _logic;

        public ReportController(ReportLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> GetReport(ReportRequest req)
        {
            var result = await _logic.GetReport(req, Request.RequestUri);
            return Ok(result);
        }
    }
}