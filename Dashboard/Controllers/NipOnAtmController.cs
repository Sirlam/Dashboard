using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class NipOnAtmController : Controller
    {
        //
        // GET: /NipOnAtm/

        public ActionResult Index()
        {
            var nipOnAtmUtil = new NipOnAtmUtil();
            ViewBag.NipOnAtm = nipOnAtmUtil.GetNipOnAtmTodayStateSummary();

            return View();
        }

    }
}
