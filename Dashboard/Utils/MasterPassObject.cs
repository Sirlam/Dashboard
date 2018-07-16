using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class MerchantOnboarding
    {
        public string NewMarchantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateofBirth { get; set; }
        public string PhoneNo { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public string CoporateName { get; set; }
        public string DateApproved { get; set; }
        public string BusinessSegment { get; set; }
        public string ExpiryDate { get; set; }
        public DateTime SpooledTime { get; set; }
    }

    public class MerchantSummary
    {
        public int MerchantCount { get; set; }
        public string DateApproved { get; set; }
        public String Segment { get; set; }
        public string Cs { get; set; }
        public string Cm { get; set; }
        public string Db { get; set; }
        public string Unknown { get; set; }
    }
    public class Sales
    {
        public int TotalTransaction { get; set; }
        public string SalesDate { get; set; }
        public string Status { get; set; }
        public decimal TotalVolume { get; set; }
        public DateTime SpooledTime { get; set; }
    }
    public class SalesSummary
    {
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int TotalTransaction { get; set; }
        public string SalesDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal FailedVolume { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal SuccessVolume { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int FailedCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int SuccessCount { get; set; }
    }
    public class MerchantOnboardingTrend
    {
        public string DateTime { get; set; }
        public int MerchantCount { get; set; }
    }
    public class SalesTrend
    {
        public string DateTime { get; set; }
        public int SalesCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class SalesDetails
    {
        public DateTime Transactiondate { get; set; }
        public string Merchantname { get; set; }
        public string Merchantid { get; set; }
        public string Terminalid { get; set; }
        public string BranchName { get; set; }
        public string Merchantphone { get; set; }
        public string MerchantAccountno { get; set; }
        public string Accountclass { get; set; }
        public string BusinessSegment { get; set; }
        public string SegmentDesc { get; set; }
        public string RmName { get; set; }
        public string RmCode { get; set; }
        public string Reference { get; set; }
        public string MasterpassTranRef { get; set; }
        public string CbaTrnRefNo { get; set; }
        public int Transactionamount { get; set; }
        public string Transactionstatus { get; set; }
        public string NameOfIssuerPayer { get; set; }
        public string Merchantlocation { get; set; }
    }

    public class MerchantOnboardingDetails
    {
        public string NewMarchantId { get; set; }
        public string TerminalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BranchCode { get; set; }
        public string PhoneNo { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public string AccountClass { get; set; }
        public string CoporateName { get; set; }
        public string DateInitiated { get; set; }
        public string DateApproved { get; set; }
        public string BusinessSegment { get; set; }
        public string SegmentDesc { get; set; }
        public string RmName { get; set; }
        public string RmCode { get; set; }
        public string ExpiryDate { get; set; }
        public string StreetAddress { get; set; }
    }
}