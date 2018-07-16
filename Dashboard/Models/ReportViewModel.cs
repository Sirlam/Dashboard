using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dashboard.Models
{
    public class ReportViewModel
    {
        public List<string> ApplicationList;
        public SelectList Application { get; set; }
        public String SelectedApplication { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

    }
}