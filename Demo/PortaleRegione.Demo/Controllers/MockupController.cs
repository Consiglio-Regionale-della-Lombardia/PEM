using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PortaleRegione.Demo.Controllers
{
    [RoutePrefix("mockup")]
    public class MockupController : Controller
    {
        // GET: Mockup
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult ViewAtto()
        {
            return View("ViewAtto");
        }
        
        public ActionResult SessionViewerAdmin()
        {
            return View("SessionViewerAdmin");
        }
    }
}