using System;

namespace Dashboard.Utils
{
    public class MobileRibDetail
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Device { get; set; }
        public DateTime DateGenerated { get; set; }
        public DateTime DateExpiring { get; set; }
        public string InitialPayload { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class MobileRibSummary
    {
        public int TotalTransaction { get; set; }
        public int TotalSuccessfulTransaction { get; set; }
        public int TotalPendingTransaction { get; set; }
        public int TotalReversedTransaction { get; set; }
        public int TotalFailedTransaction { get; set; }
        public string TransactionDateTime { get; set; }
    }

    public class MobileRibStateSummary
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Device { get; set; }
        public DateTime DateGenerated { get; set; }
        public string InitialPayload { get; set; }
        public string TransactionStateInd { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class MobileRibTrend
    {
        
    }
}