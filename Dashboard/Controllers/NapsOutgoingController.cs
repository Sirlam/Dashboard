using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NapsOutgoingController : Controller
    {
        //
        // GET: /NapsOutgoing/

        public ActionResult Index()
        {
            var napsOutgoingUtil = new NapsOutgoingUtil();

            ViewBag.NapsOutgoing = napsOutgoingUtil.GetNapsOutgoingTodayStateSummary();

            return View();
        }

    }
}
