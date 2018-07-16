using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class SwiftMt101Controller : Controller
    {
        //
        // GET: /SwiftMt101/

        public ActionResult Index()
        {
            var swiftMt101Summary = new SwiftMT101Util();
            ViewBag.SwiftMt101Summary = swiftMt101Summary.GetSwiftMt101TodayStateSummary();
            return View();
        }

    }
}
