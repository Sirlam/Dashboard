//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class onlineuser
    {
        public int id { get; set; }
        public string username { get; set; }
        public string created_by { get; set; }
        public System.DateTime creation_date { get; set; }
        public string verified_by { get; set; }
        public Nullable<System.DateTime> verification_date { get; set; }
        public string udisabled { get; set; }
    }
}
