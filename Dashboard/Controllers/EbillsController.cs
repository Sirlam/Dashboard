using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class EbillsController : Controller
    {
        //
        // GET: /Rib/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var ebillsUtil = new EbillsUtil();

            ViewBag.Ebills = ebillsUtil.GetEbillsTodayStateSummary();
            return View();
        }

    }
}
