using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class FLexBranchDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string TransactionModule { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class FlexBranchSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalPendingTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class FlexBranchStateSummary
    {
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string TransactionModule { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TotalTransaction { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }

    public class AtmTransactionsLog
    {
        public long TransactionCount { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class FlexBranchTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
    }
}