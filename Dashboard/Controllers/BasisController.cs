using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class BasisController : Controller
    {
        //
        // GET: /Basis/

        public ActionResult Index()
        {
            var basisSummary = new BasisUtil();
            ViewBag.BasisSummary = basisSummary.GetBasisTodayStateSummary();
            return View();
        }

    }
}
