using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class OnBoardingController : Controller
    {
        //

        public ActionResult Index()
        {
            var masterPassSummary = new MasterPassUtil();
            ViewBag.MerchantCountSummary = masterPassSummary.GetMerchantTodayPerformance();
            var onBoardingSummary = new OnBoardingUtil();
            ViewBag.CardSummary = onBoardingSummary.GetCardIssuedFor7Days();
            ViewBag.RibSummary = onBoardingSummary.GetRibFor7Days();

            ViewBag.CasaSummary = onBoardingSummary.GetCasaSummary();
            return View();
        }
        

        public void LoadData()
        {
            var onBoardingSummary = new OnBoardingUtil();
            onBoardingSummary.StageCardIssuedData();
            onBoardingSummary.StageRibData();
            onBoardingSummary.StageCasaDeposits();
        }

        public ActionResult Refresh()
        {
            LoadData();
            ViewBag.LastRefresh = DateTime.Now.ToString("F");
            return View();
        }

        public JsonResult CardOnboardingTrend()
        {
            var onBoardingSummary = new OnBoardingUtil();
            var trendingData = onBoardingSummary.GetCardIssuedFor7Days();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RibOnboardingTrend()
        {
            var onBoardingSummary = new OnBoardingUtil();
            var trendingData = onBoardingSummary.GetRibFor7Days();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CasaDepositsTrend()
        {
            var onBoardingSummary = new OnBoardingUtil();
            var trendingData = onBoardingSummary.GetCasaSummary();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
    }
}
