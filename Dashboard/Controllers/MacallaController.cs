using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dashboard.Utils;

namespace Dashboard.Controllers
{
    public class MacallaController : Controller
    {
        //
        // GET: /Macala/
        [AllowAnonymous]
        public ActionResult Index()
        {
            var macallaUtil = new MacallaUtil();

            ViewBag.MacallaQuickTopup = macallaUtil.GetMacallaTodayStateSummary("QuickTopup");
            ViewBag.MacallaTopup = macallaUtil.GetMacallaTodayStateSummary("Topup");
            ViewBag.MacallaInterAccountTransfer = macallaUtil.GetMacallaTodayStateSummary("InterAccountTransfer");
            ViewBag.MacallaDepositMoney = macallaUtil.GetMacallaTodayStateSummary("DepositMoney");
            ViewBag.MacallaInterAccountDebitCredit = macallaUtil.GetMacallaTodayStateSummary("InterAccountDebitCredit");
            ViewBag.MacallaMerchantPaymentTransfer = macallaUtil.GetMacallaTodayStateSummary("MerchantPaymentTransfer");
            ViewBag.MacallaUtilityBillPayment = macallaUtil.GetMacallaTodayStateSummary("UtilityBillPayment");
            ViewBag.MacallaWithdrawMoney = macallaUtil.GetMacallaTodayStateSummary("WithdrawMoney");
            ViewBag.MacallaTransferMoney = macallaUtil.GetMacallaTodayStateSummary("TransferMoney");
            ViewBag.MacallaTransfer = macallaUtil.GetMacallaTodayStateSummary("Transfer");
            ViewBag.MacallaQuickBalance = macallaUtil.GetMacallaTodayStateSummary("QuickBalance");
//            LoadData();
            return View();
        }
       
    }
}
