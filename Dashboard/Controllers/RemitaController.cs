using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class RemitaController : Controller
    {
        //
        // GET: /Remita/

        public ActionResult Index()
        {
            var RemitaUtil = new RemitaUtil();

            ViewBag.Remita = RemitaUtil.GetRemitaTodayStateSummary();
            return View();
        }

    }
}
