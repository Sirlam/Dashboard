using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class NeftOutgoingDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string CodeDescription { get; set; }
        public string TransactionCompleteCode { get; set; }
        public string Category { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class NeftOutgoingSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalIncompleteTransaction { get; set; }
        public int TotalPendingTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class NeftOutgoingStateSummary
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

    public class NeftTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int PendingCount { get; set; }
        public int IncompleteCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class NeftVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal PendingCount { get; set; }
        public decimal IncompleteCount { get; set; }
        public decimal FailureCount { get; set; }
    }
}