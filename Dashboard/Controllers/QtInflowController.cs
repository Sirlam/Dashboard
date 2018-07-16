using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class QtInflowController : Controller
    {
        //
        // GET: /QtInflow/

        public ActionResult Index()
        {
            var qtInflowUtil = new QtInflowUtil();
            ViewBag.QtInflow = qtInflowUtil.GetQtInflowTodayStateSummary();

            return View();
        }

    }
}
