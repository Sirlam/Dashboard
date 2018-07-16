using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class PayArenaOnAtmController : Controller
    {
        //
        // GET: /PayArenaOnAtm/

        public ActionResult Index()
        {
            var payArenaOnAtmUtil = new PayArenaOnAtmUtil();
            ViewBag.PayArenaOnAtm = payArenaOnAtmUtil.GetPayArenaOnAtmTodayStateSummary();

            return View();
        }

    }
}
