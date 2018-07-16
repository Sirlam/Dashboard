using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dashboard.Models.Entities
{
    public class AtmPerformanceDetail
    {
        public string TerminalId { get; set; }
        public string AtmClass { get; set; }
        public string BranchCode { get; set; }
        public string Location { get; set; }
        public double InService { get; set; }
        public double OutOfService { get; set; }
        public double HardFaults { get; set; }
        public double SupplyOut { get; set; }
        public double CashOut { get; set; }
        public double Comms { get; set; }
        public double ClosedMode { get; set; }
        public double Replenishment { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime InsertTime { get; set; }
        public string Username { get; set; }
    }
    public class SummaryTrend
    {
        public string DateTime { get; set; }
        public double GoldCount { get; set; }
        public double BronzeCount { get; set; }
        public double SilverCount { get; set; }
    }
}