using System;
using System.ComponentModel.DataAnnotations;

namespace Dashboard.Utils
{
    public class NipOutgoingDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string TransactionCompleteCode { get; set; }
        public string CodeDescription { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class NipOutgoingSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalIncompleteTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class NipOutgoingStateSummary
    {
        public string TransactionStateCode { get; set; }
        public string TransactionStateDesc { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TotalTransaction { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TotalVolume { get; set; }
        public string TransactionStateInd { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }
    public class NipOutgoingTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int ReversedCount { get; set; }
        public int IncompleteCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class NipOutgoingVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal ReversedCount { get; set; }
        public decimal IncompleteCount { get; set; }
        public decimal FailureCount { get; set; }
    }
}