using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Gateway;

namespace PortaleRegione.Client.Controllers
{
    [Authorize]
    [RoutePrefix("report")]
    public class ReportController : BaseController
    {
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult> Index(Guid id, ReportTypeEnum type = ReportTypeEnum.NOI, int page = 1,
            int size = 50)
        {
            var result = await EMGate.GetReport(id, type, page, size);
            return View(result);
        }
    }
}