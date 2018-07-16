using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NipOnXpressController : Controller
    {
        //
        // GET: /NipOnXpress/

        public ActionResult Index()
        {
            var nipOnXpressUtil = new NipOnXpressUtil();

            ViewBag.NipOnXpress = nipOnXpressUtil.GetNipOnXpressTodayStateSummary();

            return View();
        }

    }
}
