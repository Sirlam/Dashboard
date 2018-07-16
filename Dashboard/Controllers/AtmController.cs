using System.Web.Mvc;
using Dashboard.Models.DataAccess;

namespace Dashboard.Controllers
{
    public class AtmController : Controller
    {
        //
        // GET: /Atm/

        public ActionResult Index()
        {
            var source = new AtmDataSource();
            var summaries = source.GetAtmClassSummayList();
            return View(summaries);
        }
        public ActionResult Details(string atmclass)
        {
            var source = new AtmDataSource();
            var summaries = source.GetDetail(atmclass);
            return View(summaries);
        }

        public JsonResult SummaryTrends()
        {
            var source = new AtmDataSource();
            var summaries = source.GetAtmClassSummaryTrend();
            return Json(summaries, JsonRequestBehavior.AllowGet);
        }
    }
}
