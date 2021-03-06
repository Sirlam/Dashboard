﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class NapsOutgoingDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        public string CodeDescription { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string StateCode { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class NapsOutgoingSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalPendingTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class NapsOutgoingStateSummary
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

    public class NapsOutgoingTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int PendingCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class NapsOutgoingVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal PendingCount { get; set; }
        public decimal FailureCount { get; set; }
    }
}