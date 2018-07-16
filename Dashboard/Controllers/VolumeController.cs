using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class VolumeController : Controller
    {
        //
        // GET: /Volume/

        public ActionResult Index()
        {
            var summary = new SummaryUtil();
            ViewBag.Summary = summary.GetTodayVolumeSummary();
            return View();
        }

        public ActionResult Detail()
        {
            return View();
        }

        public JsonResult VolMacallaTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayMacallaSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolRibTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayRibSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolNeftTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNeftSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolNipOutgoingTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNipOutgoingSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolNipIncomingTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNipIncomingSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolAtmTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessVolTrend(parseToDateTime(detailDate), "ATM");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolPosTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessVolTrend(parseToDateTime(detailDate), "POS");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolWebTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessVolTrend(parseToDateTime(detailDate), "WEB");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolNapsTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNapsSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VolEbillsTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayEbillsSuccessVolTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        private DateTime parseToDateTime(string date)
        {
            DateTime dateTime;
            try
            {
                dateTime = DateTime.ParseExact(date, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                dateTime = DateTime.Now;
            }

            return dateTime;
        }
    }
}
