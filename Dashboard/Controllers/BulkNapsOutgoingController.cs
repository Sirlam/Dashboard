using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class BulkNapsOutgoingController : Controller
    {
        //
        // GET: /BulkNapsOutgoing/

        public ActionResult Index()
        {
            var bulkNapsOutgoingUtil = new BulkNapsOutgoingUtil();

            ViewBag.BulkNapsOutgoing = bulkNapsOutgoingUtil.GetBulkNapsOutgoingTodayStateSummary();
            return View();
        }

    }
}
