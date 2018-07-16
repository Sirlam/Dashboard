using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class RibController : Controller
    {
        //
        // GET: /Rib/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var ribUtil = new RibUtil();

            ViewBag.Rib = ribUtil.GetRibTodayStateSummary();
            return View();
        }

    }
}
