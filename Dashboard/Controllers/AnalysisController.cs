using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class AnalysisController : Controller
    {
        //
        // GET: /Analysis/
        public String Days;
        public String SelectedApplication;

        [HttpGet]
        public ActionResult Index(String days, String selectedApplication)
        {
            ViewBag.Days = days ?? "Weekly";
            ViewBag.SelectedApplication = selectedApplication ?? "RIB";
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection formCollection)
        {
            SelectedApplication = formCollection["SelectedApplication"];
            Days = formCollection["Days"];

            return View();
        }

        [HttpGet]
        public JsonResult AnalysisTrend(String days, String selectedApplication)
        {
            AnalysisUtil analysis = new AnalysisUtil();
            var trendingData = analysis.GetMonthlyAnalysisTrend(days, selectedApplication);

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
    }
}
