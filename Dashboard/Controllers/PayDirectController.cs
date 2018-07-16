using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class PayDirectController : Controller
    {
        //
        // GET: /PayDirect/

        public ActionResult Index()
        {
            var payDirectSummary = new PayDirectUtil();
            ViewBag.PayDirectSummary = payDirectSummary.GetPayDirectTodayStateSummary();
            return View();
        }

    }
}
