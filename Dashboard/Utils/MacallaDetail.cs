using System;
using System.ComponentModel.DataAnnotations;

namespace Dashboard.Utils
{
    public class MacallaDetail
    {
        public string TransactionType { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string TransactionStateCode { get; set; }
        public DateTime SpooledTime { get; set; }
    }
}