using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NipOutgoingController : Controller
    {
        //
        // GET: /NipOutgoing/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var nipOutgoingUtil = new NipOutgoingUtil();

            ViewBag.NipOutgoing = nipOutgoingUtil.GetNipOutgoingTodayStateSummary();
            return View();
        }

    }
}
