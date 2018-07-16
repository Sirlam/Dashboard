using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{



    public class OnBoarding
    {
        public string SegmentCode { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public int CardCount { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime SpoonDateTime { get; set; }
    }

    public class CasaDeposits
    {
        public int Count { get; set; }
        public decimal Volume { get; set; }
        public String AccountType { get; set; }
        public String SegementCode { get; set; }
        public String BusinessSegment { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public DateTime SpooledTime { get; set; }

    }

    public class CasaSummary
    {
        public int Count { get; set; }
        public decimal Volume { get; set; }
        public string TransactionDate { get; set; }
        public int SavingsCount { get; set; }
        public int CurrentCount { get; set; }
        public int OthersCount { get; set; }
        public decimal SavingsVolume { get; set; }
        public decimal CurrentVolume { get; set; }
        public decimal OthersVolume { get; set; }
    }
}