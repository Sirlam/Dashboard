using System;
using System.Globalization;
using System.Web.Mvc;
using Dashboard.Models.DataAccess;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            var summary = new SummaryUtil();
            ViewBag.Summary = summary.GetTodayPerformanceSummary();
            ViewBag.FaultyProduct=summary.CheckProductInProblem();
            return View();
        }

        public void LoadData()
        {
            var mobileUtil = new MobileUtil();
            mobileUtil.StageXpressAccountData();
            mobileUtil.StageTopUpData();
            mobileUtil.StageBillPaymentData();
            mobileUtil.StageInterBankTransferData();
            mobileUtil.StageInterAffiliateTransferData();
            mobileUtil.StageMasterPassMvisaData();
            mobileUtil.StageMerchantP2PData();
            mobileUtil.StageAgencyBankingData();
            mobileUtil.StageMobileIncomingData();

            var nipOutgoingUtil = new NipOutgoingUtil();
            nipOutgoingUtil.StageNipOutgoingData();

            var neftOutgoingUtil = new NeftOutgoingUtil();
            neftOutgoingUtil.StageNeftOutgoingData();

            var ribUtil = new RibUtil();
            ribUtil.StageRibData();

            var nipIncomingUtil = new NipIncomingUtil();
            nipIncomingUtil.StageNipIncomingData();

            //var macallaUtil = new MacallaUtil();
            //macallaUtil.StageMacallaData();
            var napsUtil = new NapsUtil();
            napsUtil.StageNapsData();

            var ebillsUtil = new EbillsUtil();
            ebillsUtil.StageEbillsData();

            var flexbranchUtil = new FlexBranchUtil();
            flexbranchUtil.StageFLexBranchData();
            flexbranchUtil.StageAtmTransactionsData();

            var napsOutgoingUtil = new NapsOutgoingUtil();
            napsOutgoingUtil.StageNapsOutgoingData();

            var bulkNapsOutgoingUtil = new BulkNapsOutgoingUtil();
            bulkNapsOutgoingUtil.StageBulkNapsOutgoingData();

            NameEnquiryDataSource enquiryDataSource = new NameEnquiryDataSource();
            enquiryDataSource.StageNameEnquiryData();

//            var mobileRibUtil = new MobileRibUtil();
//            mobileRibUtil.StageMobileRibData();

            var fepUtil = new FepUtil();
            fepUtil.StageFepData();
        }

        public ActionResult Refresh()
        {
            LoadData();
            ViewBag.LastRefresh = DateTime.Now.ToString("F");
            return View();
        }

        public ActionResult MacallaReportUpload()
        {
            var macallaUtil = new MacallaUtil();
            macallaUtil.StageMacallaReport();

            ViewBag.LastRefresh = DateTime.Now.ToString("F");

            return View();
        }

        public ActionResult Detail(String InputDate)
        {
            ViewBag.DetailDate = InputDate ?? DateTime.Now.ToString("dd-MMM-yyyy");
            return View();
        }

        [HttpGet]
        public ActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reports(FormCollection formCollection)
        {
            string strApplication = formCollection["SelectedApplication"];
            string strStart = formCollection["StartDate"];
            string strEnd = formCollection["EndDate"];

            ReportUtil reportUtil = new ReportUtil();
            if (strApplication == "RIB")
            {
                ViewBag.Report = reportUtil.RibReport(strStart, strEnd);
            }
            else if (strApplication == "NIP-Outgoing")
            {
                ViewBag.Report = reportUtil.NipOutgoingReport(strStart, strEnd);
            }
            else if (strApplication == "NEFT-Outgoing")
            {
                ViewBag.Report = reportUtil.NeftOutgoingReport(strStart, strEnd);
            }
            else if (strApplication == "NIP-Incoming")
            {
                ViewBag.Report = reportUtil.NipIncomingReport(strStart, strEnd);
            }
            else if(strApplication == "NAPS")
            {
                ViewBag.Report = reportUtil.NapsReport(strStart, strEnd);
            }
            else if (strApplication == "EBILLS")
            {
                ViewBag.Report = reportUtil.EbillsReport(strStart, strEnd);
            }
            else if (strApplication == "Macalla")
            {
                ViewBag.Report = reportUtil.MacallaReport(strStart, strEnd);
            }
            else if(strApplication == "FEP")
            {
                ViewBag.Report = reportUtil.FepReport(strStart, strEnd);
            }
            return View();
        }

        [HttpGet]
        public ActionResult MacallaReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MacallaReport(FormCollection formCollection)
        {
            var macallaReport = new MacallaUtil();

            string trandate = formCollection["TranDate"];
            ViewBag.MacallaReport = macallaReport.GetMobileReports(trandate);

            return View();
        }

        public JsonResult SuccessTrend()
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodaySuccessTrending();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummSuccessTrend()
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodaySuccessCummTrend();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummMacallaTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayMacallaSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CummMacallaInterAccDcJsonResultTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayMacallaInterAccDCSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CummMobileTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayMobileSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CummTopUpTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayTopupSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CummInterBankTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayInterBankSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CummXpressBoardingTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayXpressBoardingSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummRibTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayRibSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummNeftTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNeftSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummNipOutgoingTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNipOutgoingSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummNipIncomingTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNipIncomingSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummAtmTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessCummTrend(parseToDateTime(detailDate), "ATM");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummPosTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessCummTrend(parseToDateTime(detailDate), "POS");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummWebTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFepSuccessCummTrend(parseToDateTime(detailDate), "WEB");

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummNapsTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayNapsSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummEbillsTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayEbillsSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummFlexBranchTrend(string detailDate)
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFlexBranchSuccessCummTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CummNameEnquiryTrend(string detailDate)
        {
            NameEnquiryDataSource dataSource=new NameEnquiryDataSource();
            var trendingData = dataSource.GetTodayNameEnquiryTrend(parseToDateTime(detailDate));

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FailureTrend()
        {
            SummaryUtil summaryUtil = new SummaryUtil();
            var trendingData = summaryUtil.GetTodayFailureTrending();

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

        public ActionResult MonthlyFailureTrend()
        {
            return View();
        }

        public JsonResult MonthlyRibFailureCummTrend()
        {
            FailureUtil failureUtil = new FailureUtil();
            var trendingData = failureUtil.GetMonthlyRibFailureCummTrend();

            return Json(trendingData, JsonRequestBehavior.AllowGet);
        }
    }
}
