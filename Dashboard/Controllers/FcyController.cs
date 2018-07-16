using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class FcyController : Controller
    {
        //
        // GET: /Fcy/

        public ActionResult Index()
        {
            var fcyUtil = new FcyUtil();

            ViewBag.Atm = fcyUtil.GetFcyTodayStateSummary("ATM");
            ViewBag.Pos = fcyUtil.GetFcyTodayStateSummary("POS");
            ViewBag.Web = fcyUtil.GetFcyTodayStateSummary("WEB");
            ViewBag.Other_Channels = fcyUtil.GetFcyTodayStateSummary("OTHER_CHANNELS");

            return View();
        }

    }
}
