using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class MobileDetail
    {
        public int TransactionCount { get; set; }
        public String TransactionCode { get; set; }
        public String TransactionType { get; set; }
        public String TransactionDescription { get; set; }
        public String ChannelType { get; set; }
        public Decimal TransactionVolume { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class MobileStateSummary
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        public String TransactionCode { get; set; }
        public String TransactionDescription { get; set; }
        public String TransactionState { get; set; }
        public String ChannelType { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public Decimal TransactionVolume { get; set; }
    }

    public class MobileTrend
    {
        public string DateTime { get; set; }
        public int SuccessCount { get; set; }
        public int ReversedCount { get; set; }
        public int PendingCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class MobileVolumeTrend
    {
        public string DateTime { get; set; }
        public decimal SuccessCount { get; set; }
        public decimal ReversedCount { get; set; }
        public decimal PendingCount { get; set; }
        public decimal FailureCount { get; set; }
    }
}