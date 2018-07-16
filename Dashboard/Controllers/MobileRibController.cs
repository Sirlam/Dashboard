using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class MobileRibController : Controller
    {
        //
        // GET: /MobileRib/

        public ActionResult Index()
        {
            var mobileRibUtil = new MobileRibUtil();

            ViewBag.MobileRib = mobileRibUtil.GetMobileRibTodayStateSummary();
            return View();
        }

    }
}
