using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class FlexBranchController : Controller
    {
        //
        // GET: /FlexBranch/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var flexbranchUtil = new FlexBranchUtil();

            ViewBag.FlexBranch = flexbranchUtil.GetFlexBranchTodayStateSummary();
            return View();
        }

    }
}
