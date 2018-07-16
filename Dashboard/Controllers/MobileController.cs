using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class MobileController : Controller
    {
        //
        // GET: /Mobile/

        public ActionResult Index()
        {
            var mobileUtil = new MobileUtil();

            ViewBag.XpressAccount = mobileUtil.GetMobileStateSummaries("XPRESS ACCOUNT ONBOARDING");
            ViewBag.TopUp = mobileUtil.GetMobileStateSummaries("TOPUP");
            ViewBag.BillPayment = mobileUtil.GetMobileStateSummaries("BILL PAYMENT");
            ViewBag.InterBankTransfer = mobileUtil.GetMobileStateSummaries("INTER-BANK TRANSFERS");
            ViewBag.InterAffiliateTransfer = mobileUtil.GetMobileStateSummaries("INTER-AFFILIATE TRANSFERS");
            ViewBag.MasterPassMvisa = mobileUtil.GetMobileStateSummaries("MASTERPASS-MVISA");
            ViewBag.MerchantP2P = mobileUtil.GetMobileStateSummaries("MERCHANT-P2P");
            ViewBag.AgencyBanking = mobileUtil.GetMobileStateSummaries("AGENCY-BANKING");
            ViewBag.MobileIncoming = mobileUtil.GetMobileStateSummaries("INCOMING-TO-XPRESS-ACCOUNT");
            return View();
        }

    }
}
