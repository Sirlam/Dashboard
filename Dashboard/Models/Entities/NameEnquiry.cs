using System;

namespace Dashboard.Models.Entities
{
    public class NameEnquiry
    {
        public string Category { get; set; }
        public DateTime SpooledTime { get; set; }
        public string Time { get; set; }
        public int RequestCount { get; set; }
    }

    public class NameEnquirySummary
    {
        public int Success { get; set; }
        public int Pending { get; set; }
        public int Failed { get; set; }
        public string Time { get; set; }
    }
}