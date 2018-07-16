using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class FepController : Controller
    {
        //
        // GET: /Fep/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Atm()
        {
            var fepUtil = new FepUtil();

            ViewBag.Atm = fepUtil.GetFepTodayStateSummary("ATM");
            return View();
        }public ActionResult Pos()
        {
            var fepUtil = new FepUtil();

            ViewBag.Pos = fepUtil.GetFepTodayStateSummary("POS");
            return View();
        }public ActionResult Web()
        {
            var fepUtil = new FepUtil();

            ViewBag.Web = fepUtil.GetFepTodayStateSummary("WEB");
            return View();
        }
    }
}
