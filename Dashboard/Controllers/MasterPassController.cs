using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class MasterPassController : Controller
    {
        //
        // GET: /MasterPass/

        public ActionResult Index()
        {
            var masterPassSummary = new MasterPassUtil();
            ViewBag.MerchantCountSummary = masterPassSummary.GetMerchantTodayPerformance();
            ViewBag.SalesSummary = masterPassSummary.GetSalesTodayPerformance();
            return View();
        }
        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Details(FormCollection formCollection)
        {
            string strApplication = formCollection["SelectedApplication"].ToString();
            string strStart = formCollection["StartDate"].ToString();
            string strEnd = formCollection["EndDate"].ToString();

            MasterPassUtil masterPassUtil = new MasterPassUtil();

            if (strApplication == "Transaction Details")
            {
                ViewBag.Report = masterPassUtil.GetSalesDetails(strStart, strEnd);
            }
            else if (strApplication == "Merchant Onboarding")
            {
                ViewBag.Report2 = masterPassUtil.GetMerchantOnboardingDetails(strStart, strEnd);
            }
            
            return View();
        }

        public void LoadData()
        {
            var masterPass = new MasterPassUtil();
            masterPass.StageMerchantOnboardingData();
            masterPass.StageSalesData();
        }

        public ActionResult Refresh()
        {
            LoadData();
            ViewBag.LastRefresh = DateTime.Now.ToString("F");
            return View();
        }

        public JsonResult MerchantOnboardingTrend()
        {
            MasterPassUtil masterPassUtil = new MasterPassUtil();
            var trendingData = masterPassUtil.GetMerchantTodayPerformance();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult WeeklySalesTrend()
        {
            MasterPassUtil masterPassUtil = new MasterPassUtil();
            var trendingData = masterPassUtil.GetSalesTodayPerformance();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        //private DateTime parseToDateTime(string date)
        //{
        //    DateTime dateTime;
        //    try
        //    {
        //        dateTime = DateTime.ParseExact(date, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
        //    }
        //    catch (Exception e)
        //    {
        //        dateTime = DateTime.Now;
        //    }

        //    return dateTime;
        //}
    }
}
