﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class NameEnquiryDetail
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TransactionCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal TransactionVolume { get; set; }
        public string CodeDescription { get; set; }
        public string StateCode { get; set; }
        public DateTime SpooledTime { get; set; }
    }
}