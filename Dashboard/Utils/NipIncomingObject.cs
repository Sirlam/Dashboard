using System;
using System.ComponentModel.DataAnnotations;

namespace Dashboard.Utils
{
    public class NipIncomingDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string TransactionCompleteCode { get; set; }
        public string CodeDescription { get; set; }
        public string TransactionStateCode { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class NipIncomingSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalIncompleteTransaction { get; set; }
        public string TransactionDateTime { get; set; }
        public int TotalPendingTransaction { get; set; }
    }

    public class NipIncomingStateSummary
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
    public class NipIncomingTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int ReversedCount { get; set; }
        public int IncompleteCount { get; set; }
        public int FailureCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class NipIncomingVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal ReversedCount { get; set; }
        public decimal IncompleteCount { get; set; }
        public decimal FailureCount { get; set; }
        public decimal PendingCount { get; set; }
    }
}