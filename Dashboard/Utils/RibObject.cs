using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class RibDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string CodeDescription { get; set; }
        public string StateCode { get; set; }
        public string TokenOption { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class RibSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalPendingTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class RibStateSummary
    {
        public string TransactionStateCode { get; set; }
        public string TransactionStateDesc { get; set; }
        public string CodeDescription { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TotalTransaction { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TotalVolume { get; set; }
        public string TransactionStateInd { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }

    public class RibTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int ReversedCount { get; set; }
        public int PendingCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class RibVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal ReversedCount { get; set; }
        public decimal PendingCount { get; set; }
        public decimal FailureCount { get; set; }
    }

    public class RibFailureTrend
    {
        public string DateTime { get; set; }
        public decimal FailureCount { get; set; }
        public decimal Error98Count { get; set; }
        public decimal Error99Count { get; set; }
        public decimal Error21Count { get; set; }
        public decimal ErrorNewCount { get; set; }
    }
}