using System;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class CollectionsController : Controller
    {
        public ActionResult Index()
        {
            var collectionsSummary = new CollectionsSummaryUtil();
            ViewBag.Summary = collectionsSummary.GetTodayPerformanceSummary();
            //ViewBag.FaultyProduct = collectionsSummary.CheckProductInProblem();
            return View();
        }

        public void LoadData()
        {
            var basisUtil = new BasisUtil();
            basisUtil.StageBasisData();
            var payDirectUtil = new PayDirectUtil();
            payDirectUtil.StagePayDirectData();
            var swiftMt101Util = new SwiftMT101Util();
            swiftMt101Util.StageSwiftMt101Data();
            var remita = new RemitaUtil();
            remita.StageRemitaData();
            var fcy = new FcyUtil();
            fcy.StageFcyData();
            var qtInflow = new QtInflowUtil();
            qtInflow.StageQtInflowData();
            var nipOnXpress = new NipOnXpressUtil();
            nipOnXpress.StageNipOnXpressData();
            var nipOnAtm = new NipOnAtmUtil();
            nipOnAtm.StageNipOnAtmData();
            var payArenaOnAtm = new PayArenaOnAtmUtil();
            payArenaOnAtm.StagePayAraneOnAtmData();
            var xcashOnAtm = new XcashOnAtmUtil();
            xcashOnAtm.StageXcashOnAtmData();
            var pinSelection = new PinSelectionUtil();
            pinSelection.StagePinSelectionData();
        }

        public ActionResult Refresh()
        {
            LoadData();
            ViewBag.LastRefresh = DateTime.Now.ToString("F");
            return View();
        }
    }
}