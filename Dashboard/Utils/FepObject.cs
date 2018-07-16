using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class FepDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string TransactionResponseCode { get; set; }
        public string TransactionType { get; set; }
        public string ChannelType { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class FepSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalIncompleteTransaction { get; set; }
        public string TransactionDateTime { get; set; }
        public int TotalPendingTransaction { get; set; }
    }

    public class FepStateSummary
    {
        public string TransactionStateCode { get; set; }
        public string TransactionStateDesc { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TotalTransaction { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TotalTransactionVolume { get; set; }
        public string TransactionStateInd { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }
    public class FepTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class FepVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal FailureCount { get; set; }
    }
}