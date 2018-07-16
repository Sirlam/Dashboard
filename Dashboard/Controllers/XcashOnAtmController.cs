using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class XcashOnAtmController : Controller
    {
        //
        // GET: /XcashOnAtm/

        public ActionResult Index()
        {
            var xcashOnAtmUtil = new XcashOnAtmUtil();
            ViewBag.XcashOnAtm = xcashOnAtmUtil.GetXcashOnAtmTodayStateSummary();

            return View();
        }

    }
}
