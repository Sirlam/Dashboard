using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class PinSelectionController : Controller
    {
        //
        // GET: /PinSelection/

        public ActionResult Index()
        {
            var pinSelectionUtil = new PinSelectionUtil();
            ViewBag.PinSelection = pinSelectionUtil.GetPinSelectionTodayStateSummary();

            return View();
        }

    }
}
