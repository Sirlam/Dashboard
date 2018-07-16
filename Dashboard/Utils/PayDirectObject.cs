﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class PayDirectDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string StateCode { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class PayDirectStateSummary
    {
        public string StateCode { get; set; }
        public int TotalTransaction { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TotalTransactionVolume { get; set; }
        public string TransactionStateDesc { get; set; }
        public string TransactionStateInd { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }
}