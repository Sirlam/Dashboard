using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NeftOutgoingController : Controller
    {
        //
        // GET: /NeftOutgoing/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var neftOutgoingUtil = new NeftOutgoingUtil();

            ViewBag.NeftOutgoing = neftOutgoingUtil.GetNeftOutgoingTodayStateSummary();

            return View();
        }

    }
}
