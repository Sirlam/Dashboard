using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NapsController : Controller
    {
        //
        // GET: /Rib/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var napsUtil = new NapsUtil();

            ViewBag.Naps = napsUtil.GetNapsTodayStateSummary();
            return View();
        }

    }
}
