using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Dashboard.Utils
{
    public class SummaryUtil
    {
        private readonly string _sqlCs;
        private const int NipIncomingPendingAlert = 480;
        private const int RibPendingAlert = 100;
        private const double SuccessMinimumRate = 0.2;
        private const double FailureMaximumRate = 0.2;

        public class Summary
        {
            public string Product { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int TransactionCount { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int SuccessStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int ReversedStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int PendingStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int IncompleteStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int SystemFailureStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int FailureStatus { get; set; }
            public DateTime TransDate { get; set; }
        }
        public class Volume
        {
            public string Product { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal TransactionVolume { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal SuccessStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal ReversedStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal PendingStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal IncompleteStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal SystemFailureStatus { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal FailureStatus { get; set; }
            public DateTime? TransDate { get; set; }
        }
        public class Products
        {
            public string DateTime { get; set; }
            public int Rib { get; set; }
            public int NipOutgoing { get; set; }
            public int NipIncoming { get; set; }
            public int Neft { get; set; }
            public int Macalla { get; set; }
            public int Fep { get; set; }
            public int Naps { get; set; }
            public int NapsOutgoing { get; set; }
            public int Ebills { get; set; }
            public int FlexBranch { get; set; }
        }
        public class ProductTrend
        {
            public string DateTime { get; set; }
            public int SuccessCount { get; set; }
        }
        public SummaryUtil()
        {
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<Summary> GetTodayPerformanceSummary()
        {
            List<Summary> summaries = new List<Summary>();
            string sqlSelect =
                "select a.*,case when(a.success_status+a.system_failure)=0 then 0 else ((a.success_status*1000)/(a.success_status+a.system_failure)) end percet  from ( " +
                "select 'NEFT' product,sum(trans_count) transaction_count, " +
                "sum(case when code_description='SUCCESS' then trans_count else 0 end) success_status,'' reversed_status, " +
                "sum(case when code_description ='PENDING' then trans_count else 0 end) pending_status, " +
                "sum(case when code_description='INCOMPLETE' then trans_count else 0 end) incomplete_status, " +
                "sum(case when trans_complete_code not in ('13', '10') and code_description not in('success','pending','incomplete') then  trans_count else 0 end) system_failure, " +
                "sum(case when code_description ='FAILED'  then  trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from neft_outgoing " +
                "where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM neft_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by convert(varchar,spooled_time,120)  " +
                "union all  " +
                "select 'RIB' product,sum(trans_count) transaction_count,  " +
                "sum(case when code_description='SUCCESS' then trans_count else 0 end) success_status, " +
                "sum(case when code_description='REVERSAL' then trans_count else 0 end) reversed_status, " +
                "sum(case when code_description='Processing' then trans_count else 0 end) pending_status, '' incomplete_status, " +
                "sum( case when trans_code not in ('10','00','13') then trans_count else 0 end) system_failure, " +
                "sum(case when code_description in ('REVERSAL','SUCCESS','Processing')  then 0 else trans_count end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from rib where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM rib ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'Mobile_Onboarding_With_RIB' product, count(*) transaction_count, " +
                "sum(case when right(initial_payload, 2) = '00' then 1 else 0 end) success_status, '' reversed_status, '' system_failure, '' incomplete_status, " +
                "sum(case when right(initial_payload, 2) = '' then 1 else 0 end) pending_status, " +
                "sum(case when right(initial_payload, 2) IN ('01','','02','lu') then 1 else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile_rib_onboarding where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM mobile_rib_onboarding ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) " +
                "union all  " +
                "select 'EBILLS' product, sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status,  " +
                "sum(case when code_description in ('processing', 'UNAUTHORISED') then trans_count else 0 end) pending_status, " +
                "sum(case when code_description='Transaction Failed and Reversed - 00' or code_description='Transaction Failed and Reversed - null' then trans_count else 0 end) reversed_status,'' incomplete_status, " +
                "sum(case when trans_code<>'00' and trans_code<>'10' and code_description != 'Transaction Failed and Reversed - 00' and code_description!='Transaction Failed and Reversed - null' or code_description = 'CANCELLED' then trans_count else 0 end) system_failure,  " +
                "sum(case when trans_code<>'00' and code_description != 'Transaction Failed and Reversed - 00' and code_description!='Transaction Failed and Reversed - null' or code_description = 'CANCELLED' then trans_count else 0 end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from ebills where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM ebills ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'NAPS INCOMING' product, sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, '' reversed_status," +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, '' incomplete_status, " +
                "sum(case when trans_code not in ('10', '00') then trans_count else 0 end) system_failure, " +
                "sum(case when trans_code='' or trans_code = '10' then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from naps where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM naps ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'NAPS OUTGOING' product, sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, '' reversed_status," +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, '' incomplete_status, " +
                "sum(case when trans_code = '98' then trans_count else 0 end) system_failure, " +
                "sum(case when trans_code in ('','10','13','98') then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from naps_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM naps_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'BULK NAPS OUTGOING' product, sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, '' reversed_status," +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, '' incomplete_status, " +
                "sum(case when trans_code = '98' then trans_count else 0 end) system_failure, " +
                "sum(case when trans_code in ('','10','13','98') then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from bk_naps_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM bk_naps_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'FLEXBRANCH- Teller Transactions' product, sum(trans_count) transaction_count, " +
                "sum(trans_count) success_status, '' reversed_status," +
                "'' pending_status, '' incomplete_status, " +
                "'' system_failure, " +
                "'' failure_status, " +
                "convert(varchar,spooled_time,120) transdate from flexbranch where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM flexbranch ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all " +
                "select 'ATM Transactions Log' product, trans_count transaction_count, " +
                "trans_count success_status_count, '' reversed_status, '' pending_status, '' incomplete_status, '' system_failure, '' failure_status, " +
                "convert(varchar,spooled_time,120) transdate from atm_trans_log where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM atm_trans_log ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120), trans_count  " +
                "union all " +
                "select 'NIP OUTGOING' product,sum(trans_count) transaction_count,  " +
                "sum(case when trans_complete_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_status, ''pending_status, " +
                "sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_status, " +
                "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz') then trans_count else 0 end) system_failure, " +
                "sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from nip_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) " +
                "union all  " +
                "select 'NIP INCOMING' product, sum(trans_count) transaction_count,  " +
                "sum(case when trans_complete_code='00' then trans_count else 0 end) success_status,  " +
                "sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_status,sum(case when trans_complete_code is null then trans_count else 0 end) pending_status, " +
                "sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_status, " +
                "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz','13') then trans_count else 0 end) system_failure, " +
                "sum(case when (trans_complete_code in ('00','x7','zz') or trans_complete_code is null)  then 0 else trans_count end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from nip_incoming where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_incoming ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) " +
                "union all  " +
                "select (channel_type + '-' + trans_type) as product, sum(trans_count) transaction_count, " +
                "sum(case when trans_code IN ('000','00') then trans_count else 0 end) success_status, " +
                "sum(case when trans_code IN ('E92','x7') then trans_count else 0 end) reversed_status, " +
                "sum(case when trans_code IN ('','zz', null, 'null') then trans_count else 0 end) pending_status,  '' incomplete_status, " +
                "sum(case when trans_code IN ('00','x12','x11','x1','x7','zz','13', '000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) system_failure, " +
                "sum(case when trans_code IN ('00','x7','zz','000',null,'null','','E92') then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile where convert(varchar(16),spooled_time,120)=convert(varchar(16),cast((SELECT TOP 1 spooled_time FROM mobile ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by channel_type, trans_type, convert(varchar,spooled_time,120)" +
                "union all " +
                "select 'Name enquiry' product,sum(request_count) transaction_count,sum(case when category='SUCCESS' then request_count else 0 end) success_status, " +
                "0 reversed_status, sum(case when category='PENDING' then request_count else 0 end) pending_status,0incomplete_status, " +
                "sum(case when category='FAILED' then request_count else 0 end) system_failure,  " +
                "sum(case when category='FAILED' then request_count else 0 end) failure_status, convert(varchar,spooled_time,120) transdate from nameenquiry where  " +
                "convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nameenquiry ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) " +
                "union all " +
                "select 'CARDS ON-'+channel_type product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                "'' reversed_status,'' pending_status,'' incomplete_status, " +
                "sum(case when rsp_code IN ('06','91','96') then tran_count else 0 end) system_failure, " +
                "sum(case when rsp_code <>'00'  then tran_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from fep " +
                "where channel_type in ('POS','WEB','ATM') " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM fep ORDER BY ID DESC) as datetime),120) " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  "+
                "group by channel_type,convert(varchar,spooled_time,120)) a order by percet ";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new Summary();

                        transactionSummary.Product = rdr["product"].ToString();
                        transactionSummary.SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString());
                        transactionSummary.SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString());
                        transactionSummary.FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString());
                        transactionSummary.IncompleteStatus = Convert.ToInt32(rdr["incomplete_status"].ToString());
                        transactionSummary.ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString());
                        transactionSummary.PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString());
                        transactionSummary.TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString());

                        //                        string datetime = rdr["transdate"].ToString();
                        //                        string dt =
                        //                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));
                        //
                        //                        transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                        summaries.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return summaries;
        }
        public List<Volume> GetTodayVolumeSummary()
        {
            List<Volume> summaries = new List<Volume>();
            string sqlSelect =
                "select a.*,case when(a.success_status+a.system_failure)=0 then 0 else ((a.success_status*1000)/(a.success_status+a.system_failure)) end percet  from ( " +
                "select 'NEFT' product,sum(trans_volume) transaction_volume,  " +
                "sum(case when code_description='SUCCESS' then trans_volume else 0 end) success_status,0 reversed_status,  " +
                "sum(case when code_description ='PENDING' then trans_volume else 0 end) pending_status,  " +
                "sum(case when code_description='INCOMPLETE' then trans_volume else 0 end) incomplete_status,  " +
                "sum(case when trans_complete_code not in ('13', '10') and code_description not in('success','pending','incomplete')  then  trans_volume else 0 end) system_failure,  " +
                "sum(case when code_description ='FAILED'  then  trans_volume else 0 end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from neft_outgoing  " +
                "where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM neft_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by convert(varchar,spooled_time,120)   " +
                "union all   " +
                "select 'RIB' product,sum(trans_volume) transaction_volume,   " +
                "sum(case when code_description='SUCCESS' then trans_volume else 0 end) success_status,  " +
                "sum(case when code_description='REVERSAL' then trans_volume else 0 end) reversed_status,  " +
                "sum(case when code_description='Processing' then trans_volume else 0 end) pending_status, 0 incomplete_status,  " +
                "sum( case when trans_code not in ('10','00','13') then trans_volume else 0 end) system_failure,  " +
                "sum(case when code_description in ('REVERSAL','SUCCESS','Processing')  then 0 else trans_volume end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from rib where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM rib ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all   " +
                "select 'EBILLS' product, sum(trans_volume) transaction_volume,  " +
                "sum(case when trans_code='00' then trans_volume else 0 end) success_status,  " +
                "sum(case when code_description in ('processing', 'UNAUTHORISED') then trans_volume else 0 end) pending_status,  " +
                "sum(case when code_description='Transaction Failed and Reversed - 00' or code_description='Transaction Failed and Reversed - null' then trans_volume else 0 end) reversed_status,0 incomplete_status,  " +
                "sum(case when trans_code<>'00' and trans_code<>'10' and code_description != 'Transaction Failed and Reversed - 00' and code_description!='Transaction Failed and Reversed - null' or code_description = 'CANCELLED' then trans_volume else 0 end) system_failure,   " +
                "sum(case when trans_code<>'00' and code_description != 'Transaction Failed and Reversed - 00' and code_description!='Transaction Failed and Reversed - null' or code_description = 'CANCELLED' then trans_volume else 0 end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from ebills where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM ebills ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all  " +
                "select 'NAPS' product, sum(trans_volume) transaction_volume,  " +
                "sum(case when trans_code='00' then trans_volume else 0 end) success_status, 0 reversed_status, " +
                "sum(case when trans_code is null then trans_volume else 0 end) pending_status, 0 incomplete_status,  " +
                "sum(case when trans_code not in ('10', '00') then trans_volume else 0 end) system_failure,  " +
                "sum(case when trans_code='' or trans_code = '10' then trans_volume else 0 end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from naps where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM naps ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all  " +
                "select 'NIP OUTGOING' product,sum(trans_volume) transaction_volume,  " +
                "sum(case when trans_complete_code='00' then trans_volume else 0 end) success_status,  " +
                "sum(case when trans_complete_code='x7' then trans_volume else 0 end) reversed_status, 0 pending_status,  " +
                "sum(case when trans_complete_code='zz' then trans_volume else 0 end) incomplete_status,  " +
                "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz') then trans_volume else 0 end) system_failure,  " +
                "sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_volume end) failure_status,   " +
                "convert(varchar,spooled_time,120) transdate from nip_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_outgoing ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) " +
                "union all   " +
                "select 'NIP INCOMING' product, sum(trans_volume) transaction_incoming,   " +
                "sum(case when state='Success' then trans_volume else 0 end) success_status,   " +
                "sum(case when state='Reversal' then trans_volume else 0 end) reversed_status, " +
                "sum(case when state='Pending' then trans_volume else 0 end) pending_status,  " +
                "sum(case when state='Incomplete' then trans_volume else 0 end) incomplete_status,  " +
                "sum(case when state='Failed' and trans_complete_code !='13' then trans_volume else 0 end) system_failure,  " +
                "sum(case when state='Failed'  then trans_volume else 0 end) failure_status,  " +
                "convert(varchar,spooled_time,120) transdate from nip_incoming where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_incoming ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)  " +
                "union all   " +
                "select 'MOBILE-'+trans_type product, sum(volume) transaction_volume, " +
                "sum(case when trans_code = '000' then volume else 0 end) success_status, " +
                "sum(case when trans_code = 'E92' then volume else 0 end) reversed_status, " +
                "sum(case when trans_code IN ('', null, 'null') then volume else 0 end) pending_status,  0 incomplete_status, " +
                "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else volume end) system_failure, " +
                "sum(case when trans_code IN ('000',null,'null','','E92') then 0 else volume end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile where trans_type != 'XPRESS ACCOUNT ONBOARDING' and convert(varchar(16),spooled_time,120)=convert(varchar(16),cast((SELECT TOP 1 spooled_time FROM mobile ORDER BY ID DESC) as datetime),120) and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by trans_type, convert(varchar,spooled_time,120)" +
                "union all  " +
                "select 'CARDS ON-'+channel_type product,sum(tot_vol) transaction_volume, sum(case when rsp_code='00' then tot_vol else 0 end) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status,  " +
                "sum(case when rsp_code IN ('06','91','96') then tot_vol else 0 end) system_failure,  " +
                "sum(case when rsp_code <>'00'  then tot_vol  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate   " +
                "from fep " +
                "where channel_type in ('POS','WEB','ATM')  " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM fep ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) "+
                "group by channel_type,convert(varchar,spooled_time,120)) a order by percet  ";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var volumeSummary = new Volume();

                        volumeSummary.Product = rdr["product"].ToString();
                        volumeSummary.SuccessStatus = Convert.ToDecimal(rdr["success_status"].ToString());
                        volumeSummary.SystemFailureStatus = Convert.ToDecimal(rdr["system_failure"].ToString());
                        volumeSummary.FailureStatus = Convert.ToDecimal(rdr["failure_status"].ToString());
                        volumeSummary.IncompleteStatus = Convert.ToDecimal(rdr["incomplete_status"].ToString());
                        volumeSummary.ReversedStatus = Convert.ToDecimal(rdr["reversed_status"].ToString());
                        volumeSummary.PendingStatus = Convert.ToDecimal(rdr["pending_status"].ToString());
                        volumeSummary.TransactionVolume = Convert.ToDecimal(rdr["transaction_volume"].ToString());



                        summaries.Add(volumeSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return summaries;
        }
        public List<Products> GetTodaySuccessTrending()
        {
            List<Products> products = new List<Products>();
            string sqlSelect = "select a.transdate,NEFT,RIB,NIP_OUTGOING,NIP_INCOMING,MACALLA from (select " +
                               "case when(sum(trans_count)-sum(case when code_description ='PENDING' then trans_count else 0 end)-sum(case when code_description='INCOMPLETE' then trans_count else 0 end))=0 then 0 else " +
                               "(sum(case when code_description='SUCCESS' then trans_count else 0 end)*100)/(sum(trans_count)-sum(case when code_description ='PENDING' then trans_count else 0 end)-sum(case when code_description='INCOMPLETE' then trans_count else 0 end))end NEFT, " +
                               "left(convert(varchar,spooled_time,120),16) transdate from neft_outgoing " +
                               "where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16) ) a, " +
                               "(select case when(sum(trans_count)-sum(case when code_description ='REVERSAL' then trans_count else 0 end)-sum(case when code_description='Processing' then trans_count else 0 end))=0 then 0 else " +
                               "(sum(case when code_description='SUCCESS' then trans_count else 0 end)*100)/(sum(trans_count)-sum(case when code_description ='REVERSAL' then trans_count else 0 end)-sum(case when code_description='Processing' then trans_count else 0 end))end RIB, " +
                               "left(convert(varchar,spooled_time,120),16) transdate from rib where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) b, " +
                               "(select case when (sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)=0) then 0 " +
                               "else (sum(case when trans_complete_code='00' then trans_count else 0 end)*100)/(sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)) end NIP_OUTGOING, " +
                               "left(convert(varchar,spooled_time,120),16) transdate from nip_outgoing where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) c, " +
                               "(select case when (sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)=0) then 0 " +
                               "else (sum(case when trans_complete_code='00' then trans_count else 0 end)*100)/(sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)) end NIP_INCOMING, " +
                               "left(convert(varchar,spooled_time,120),16) transdate from nip_incoming where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) d, " +
                               "(select case when(sum(trans_count)-sum(case when trans_state_code='Fulfilment' then trans_count else 0 end))=0 then 0 else  " +
                               "(sum(case when trans_state_code='Confirmation' then trans_count else 0 end)*100)/(sum(trans_count)-sum(case when trans_state_code='Fulfilment' then trans_count else 0 end))end MACALLA, " +
                               "left(convert(varchar,spooled_time,120),16) transdate from macalla where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) e " +
                               "where a.transdate=b.transdate and a.transdate = c.transdate and a.transdate = d.transdate and a.transdate = e.transdate";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new Products
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),
                            Rib = Convert.ToInt32(rdr["RIB"].ToString()),
                            NipOutgoing = Convert.ToInt32(rdr["NIP_OUTGOING"].ToString()),
                            Macalla = Convert.ToInt32(rdr["MACALLA"].ToString()),
                            Neft = Convert.ToInt32(rdr["NEFT"].ToString()),
                            NipIncoming = Convert.ToInt32(rdr["NIP_INCOMING"].ToString())
                        };

                        products.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return products;
        }
        public List<Products> GetTodaySuccessCummTrend()
        {
            List<Products> products = new List<Products>();
            string sqlSelect = "select a.transdate,NEFT,RIB,NIP_OUTGOING,NIP_INCOMING,MACALLA from (select " +
"sum(case when code_description='SUCCESS' then trans_count else 0 end) NEFT,  " +
"left(convert(varchar,spooled_time,120),16) transdate from neft_outgoing " +
"where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16) ) a, " +
"(select sum(case when code_description='SUCCESS' then trans_count else 0 end) RIB, " +
"left(convert(varchar,spooled_time,120),16) transdate from rib where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) b, " +
"(select sum(case when trans_complete_code='00' then trans_count else 0 end) NIP_OUTGOING,  " +
"left(convert(varchar,spooled_time,120),16) transdate from nip_outgoing where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) c, " +
"(select sum(case when trans_complete_code='00' then trans_count else 0 end) NIP_INCOMING,  " +
"left(convert(varchar,spooled_time,120),16) transdate from nip_incoming where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) d, " +
"(select sum(case when trans_state_code in ('Confirmation','Sucessful') then trans_count else 0 end) MACALLA,  " +
"left(convert(varchar,spooled_time,120),16) transdate from macalla where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) e " +
"where a.transdate=b.transdate and a.transdate = c.transdate and a.transdate = d.transdate and a.transdate = e.transdate";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new Products
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),
                            Rib = Convert.ToInt32(rdr["RIB"].ToString()),
                            NipOutgoing = Convert.ToInt32(rdr["NIP_OUTGOING"].ToString()),
                            Macalla = Convert.ToInt32(rdr["MACALLA"].ToString()),
                            Neft = Convert.ToInt32(rdr["NEFT"].ToString()),
                            NipIncoming = Convert.ToInt32(rdr["NIP_INCOMING"].ToString())
                        };

                        products.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return products;
        }
        public List<MacallaTrend> GetTodayMacallaSuccessCummTrend(DateTime deDate)
        {
            List<MacallaTrend> productTrends = new List<MacallaTrend>();
            string sqlSelect = "select sum(case when trans_state_code IN ('Confirmation', 'Sucessful', '00') then trans_count else 0 end) success_count, " +
                "sum(case when trans_state_code in ('Confirmation','Fulfilment','Sucessful','00', '03', '10', '13', 'Pending','InsufficientBalance','Blocked Account','Unable to Vend, call Ecobank Contact Centre.','E-Top-Up Limit Exceeded','NOT NORMAL ACCOUNT','Unauthorized Account','Unauthorized Customer') then 0 else trans_count end) failure_count, " +
                "sum(case when trans_state_code='Fulfilment' or trans_state_code='Pending' then trans_count else 0 end) pending_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate  " +
                "from macalla where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MacallaTrend> GetTodayMacallaInterAccDCSuccessCummTrend(DateTime deDate)
        {
            List<MacallaTrend> productTrends = new List<MacallaTrend>();
            string sqlSelect = "select sum(case when trans_state_code='Confirmation' or trans_state_code='Sucessful' then trans_count else 0 end) success_count, " +
                "sum(case when trans_state_code in ('Confirmation','Fulfilment','Sucessful','Pending','InsufficientBalance','Blocked Account','Unable to Vend, call Ecobank Contact Centre.','E-Top-Up Limit Exceeded','NOT NORMAL ACCOUNT','Unauthorized Account','Unauthorized Customer') then 0 else trans_count end) failure_count, " +
                "sum(case when trans_state_code='Fulfilment' or trans_state_code='Pending' then trans_count else 0 end) pending_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate  " +
                "from macalla where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) and trans_type= 'InterAccountDebitCredit' group by left(convert(varchar,spooled_time,120),16)";
           
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MobileTrend> GetTodayMobileSuccessCummTrend(DateTime deDate)
        {
            List<MobileTrend> productTrends = new List<MobileTrend>();
            string sqlSelect = "select sum(case when trans_code = '000' then trans_count else 0 end) success_count, " +
                    "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_count,  " +
                    "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_count,  " +
                    "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from mobile where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MobileTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MobileTrend> GetTodayTopupSuccessCummTrend(DateTime deDate)
        {
            List<MobileTrend> productTrends = new List<MobileTrend>();
            string sqlSelect = "select sum(case when trans_code = '000' then trans_count else 0 end) success_count, " +
                    "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_count,  " +
                    "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_count,  " +
                    "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from mobile where trans_type = 'TOPUP' and convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MobileTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MobileTrend> GetTodayInterBankSuccessCummTrend(DateTime deDate)
        {
            List<MobileTrend> productTrends = new List<MobileTrend>();
            string sqlSelect = "select sum(case when trans_code = '000' then trans_count else 0 end) success_count, " +
                    "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_count,  " +
                    "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_count,  " +
                    "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from mobile where trans_type = 'INTER-BANK TRANSFERS' and convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MobileTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MobileTrend> GetTodayXpressBoardingSuccessCummTrend(DateTime deDate)
        {
            List<MobileTrend> productTrends = new List<MobileTrend>();
            string sqlSelect = "select sum(case when trans_code = '000' then trans_count else 0 end) success_count, " +
                    "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_count,  " +
                    "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_count,  " +
                    "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from mobile where trans_type = 'XPRESS ACCOUNT ONBOARDING' and convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MobileTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<RibTrend> GetTodayRibSuccessCummTrend(DateTime deDate)
        {
            List<RibTrend> productTrends = new List<RibTrend>();
            string sqlSelect = "select sum(case when code_description='SUCCESS' then trans_count else 0 end) success_count, " +
                    "sum(case when code_description='REVERSAL' then trans_count else 0 end) reversed_count,  " +
                    "sum(case when code_description='Processing' then trans_count else 0 end) pending_count,  " +
                    "sum( case when trans_code not in ('10','00','13') then trans_count else 0 end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from rib where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new RibTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<FlexBranchTrend> GetTodayFlexBranchSuccessCummTrend(DateTime deDate)
        {
            List<FlexBranchTrend> productTrends = new List<FlexBranchTrend>();
            string sqlSelect = "select sum(trans_count) success_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from flexbranch where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FlexBranchTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NapsTrend> GetTodayNapsSuccessCummTrend(DateTime deDate)
        {
            List<NapsTrend> productTrends = new List<NapsTrend>();
            string sqlSelect = "select sum(case when trans_code='00' then trans_count else 0 end) success_count, " +
                "sum(case when trans_code is null then trans_count else 0 end) pending_count, " +
                "sum(case when trans_code not in ('10', '00') then trans_count else 0 end) failure_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate from naps where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NapsTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<EbillsTrend> GetTodayEbillsSuccessCummTrend(DateTime deDate)
        {
            List<EbillsTrend> productTrends = new List<EbillsTrend>();
            string sqlSelect = "select sum(case when trans_code='00' then trans_count else 0 end) success_count,  " +
            "sum(case when code_description in ('processing', 'UNAUTHORISED') then trans_count else 0 end) pending_count, " +
            "sum(case when code_description='Transaction Failed and Reversed - 00' then trans_count else 0 end) reversed_count, " +
            "sum(case when trans_code<>'00'  and trans_code<>'10' and code_description != 'Transaction Failed and Reversed - 00' or code_description = 'CANCELLED' then trans_count else 0 end) failure_count,  " +
            "left(convert(varchar,spooled_time,120),16) transdate from ebills where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new EbillsTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NipOutgoingTrend> GetTodayNipOutgoingSuccessCummTrend(DateTime deDate)
        {
            List<NipOutgoingTrend> productTrends = new List<NipOutgoingTrend>();
            string sqlSelect = "select sum(case when trans_complete_code='00' then trans_count else 0 end) success_count, " +
                    "sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_count, " +
                    "sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_count, " +
                    "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz') then trans_count else 0 end) failure_count, " +
                    "left(convert(varchar,spooled_time,120),16) transdate from nip_outgoing where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipOutgoingTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            IncompleteCount = Convert.ToInt32(rdr["incomplete_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NipIncomingTrend> GetTodayNipIncomingSuccessCummTrend(DateTime deDate)
        {
            List<NipIncomingTrend> productTrends = new List<NipIncomingTrend>();
            string sqlSelect =
                "select sum(case when state='Success' then trans_count else 0 end) success_count, " +
                "sum(case when state='Reversal' then trans_count else 0 end) reversed_count,  " +
                "sum(case when state='Incomplete' then trans_count else 0 end) incomplete_count, " +
                "sum(case when state='Pending' then trans_count else 0 end) pending_count, " +
                "sum(case when state='Failed' then trans_count else 0 end) failure_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate from nip_incoming where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipIncomingTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToInt32(rdr["reversed_count"].ToString()),
                            IncompleteCount = Convert.ToInt32(rdr["incomplete_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<FepTrend> GetTodayFepSuccessCummTrend(DateTime deDate, String channelType)
        {
            List<FepTrend> productTrends = new List<FepTrend>();
            string sqlSelect =
                "select sum(case when rsp_code='00' then tran_count else 0 end) success_count, " +
"sum(case when rsp_code in ('06','91','96')  then tran_count else 0 end) failure_count,  " +
"left(convert(varchar,spooled_time,120),16) transdate from fep where channel_type ='" + channelType + "' and  " +
"convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FepTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NeftTrend> GetTodayNeftSuccessCummTrend(DateTime deDate)
        {
            List<NeftTrend> productTrends = new List<NeftTrend>();
            string sqlSelect =
                "select sum(case when code_description='SUCCESS' then trans_count else 0 end) success_count, " +
                "sum(case when code_description ='PENDING' then trans_count else 0 end) pending_count,  " +
                "sum(case when code_description='INCOMPLETE' then trans_count else 0 end) incomplete_count,  " +
                "sum(case when code_description ='FAILED' and trans_complete_code not in ('13', '10')  then  trans_count else 0 end) failure_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate from neft_outgoing where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16) ";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NeftTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToInt32(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToInt32(rdr["pending_count"].ToString()),
                            IncompleteCount = Convert.ToInt32(rdr["incomplete_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<MacallaVolumeTrend> GetTodayMacallaSuccessVolTrend(DateTime deDate)
        {
            List<MacallaVolumeTrend> productTrends = new List<MacallaVolumeTrend>();
            string sqlSelect =
                "select sum(case when trans_state_code in ('Confirmation', 'Sucessful', '00') then volume else 0 end) success_count, " +
                "sum(case when trans_state_code in ('Confirmation','Fulfilment','Sucessful', '00', '03', '10', '13', 'Pending','InsufficientBalance','Blocked Account','Unable to Vend, call Ecobank Contact Centre.','E-Top-Up Limit Exceeded','NOT NORMAL ACCOUNT','Unauthorized Account','Unauthorized Customer') then 0 else volume end) failure_count, " +
                "sum(case when trans_state_code='Fulfilment' or trans_state_code='Pending' then volume else 0 end) pending_count,  " +
                "left(convert(varchar,spooled_time,120),16) transdate   " +
                "from macalla where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";


            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<RibVolumeTrend> GetTodayRibSuccessVolTrend(DateTime deDate)
        {
            List<RibVolumeTrend> productTrends = new List<RibVolumeTrend>();
            string sqlSelect =
                "select sum(case when code_description='SUCCESS' then trans_volume else 0 end) success_count, " +
                "sum(case when code_description='REVERSAL' then trans_volume else 0 end) reversed_count,   " +
                "sum(case when code_description='Processing' then trans_volume else 0 end) pending_count,  " +
                "sum( case when trans_code not in ('10','00','13') then trans_volume else 0 end) failure_count,  " +
                "left(convert(varchar,spooled_time,120),16) transdate from rib where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new RibVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToDecimal(rdr["reversed_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NapsVolumeTrend> GetTodayNapsSuccessVolTrend(DateTime deDate)
        {
            List<NapsVolumeTrend> productTrends = new List<NapsVolumeTrend>();
            string sqlSelect = "select sum(case when trans_code='00' then trans_volume else 0 end) success_count, " +
                               "sum(case when trans_code is null then trans_volume else 0 end) pending_count,  " +
                               "sum(case when trans_code not in ('10', '00') then trans_volume else 0 end) failure_count,  " +
                               "left(convert(varchar,spooled_time,120),16) transdate from naps where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NapsVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<EbillsVolumeTrend> GetTodayEbillsSuccessVolTrend(DateTime deDate)
        {
            List<EbillsVolumeTrend> productTrends = new List<EbillsVolumeTrend>();
            string sqlSelect = "select sum(case when trans_code='00' then trans_volume else 0 end) success_count,  " +
                               "sum(case when code_description in ('UNAUTHORISED', 'processing') then trans_volume else 0 end) pending_count,  " +
                               "sum(case when code_description='Transaction Failed and Reversed - 00' then trans_volume else 0 end) reversed_count,  " +
                               "sum(case when trans_code<>'00' and trans_code<>'10' and code_description != 'Transaction Failed and Reversed - 00' or code_description = 'CANCELLED' then trans_volume else 0 end) failure_count,    " +
                               "left(convert(varchar,spooled_time,120),16) transdate from ebills where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new EbillsVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString()),
                            ReversedCount = Convert.ToDecimal(rdr["reversed_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NipOutgoingVolumeTrend> GetTodayNipOutgoingSuccessVolTrend(DateTime deDate)
        {
            List<NipOutgoingVolumeTrend> productTrends = new List<NipOutgoingVolumeTrend>();
            string sqlSelect =
                "select sum(case when trans_complete_code='00' then trans_volume else 0 end) success_count, " +
                "sum(case when trans_complete_code='x7' then trans_volume else 0 end) reversed_count,  " +
                "sum(case when trans_complete_code='zz' then trans_volume else 0 end) incomplete_count,  " +
                "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz') then trans_volume else 0 end) failure_count,  " +
                "left(convert(varchar,spooled_time,120),16) transdate from nip_outgoing where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipOutgoingVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToDecimal(rdr["reversed_count"].ToString()),
                            IncompleteCount = Convert.ToDecimal(rdr["incomplete_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NipIncomingVolumeTrend> GetTodayNipIncomingSuccessVolTrend(DateTime deDate)
        {
            List<NipIncomingVolumeTrend> productTrends = new List<NipIncomingVolumeTrend>();
            string sqlSelect =
                "select sum(case when trans_complete_code='00' then trans_volume else 0 end) success_count, " +
                "sum(case when trans_complete_code='x7' then trans_volume else 0 end) reversed_count, " +
                "sum(case when trans_complete_code='zz' then trans_volume else 0 end) incomplete_count, " +
                "sum(case when trans_complete_code is null then trans_volume else 0 end) pending_count, " +
                "sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz','13') then trans_volume else 0 end) failure_count, " +
                "left(convert(varchar,spooled_time,120),16) transdate from nip_incoming where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipIncomingVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            ReversedCount = Convert.ToDecimal(rdr["reversed_count"].ToString()),
                            IncompleteCount = Convert.ToDecimal(rdr["incomplete_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<FepVolumeTrend> GetTodayFepSuccessVolTrend(DateTime deDate, String channelType)
        {
            List<FepVolumeTrend> productTrends = new List<FepVolumeTrend>();
            string sqlSelect = "select sum(case when rsp_code='00' then tot_vol else 0 end) success_count, " +
                               "sum(case when rsp_code in ('06','91','96')  then tot_vol else 0 end) failure_count,  " +
                               "left(convert(varchar,spooled_time,120),16) transdate from fep where channel_type ='" + channelType + "' and  " +
                               "convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FepVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString()),
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<NeftVolumeTrend> GetTodayNeftSuccessVolTrend(DateTime deDate)
        {
            List<NeftVolumeTrend> productTrends = new List<NeftVolumeTrend>();
            string sqlSelect =
                "select sum(case when code_description='SUCCESS' then trans_volume else 0 end) success_count, " +
                "sum(case when code_description ='PENDING' then trans_volume else 0 end) pending_count,   " +
                "sum(case when code_description='INCOMPLETE' then trans_volume else 0 end) incomplete_count, " +
                "sum(case when code_description ='FAILED' and trans_complete_code not in ('13', '10') then  trans_volume else 0 end) failure_count,  " +
                "left(convert(varchar,spooled_time,120),16) transdate from neft_outgoing where convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103) group by left(convert(varchar,spooled_time,120),16)";


            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NeftVolumeTrend()
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),

                            SuccessCount = Convert.ToDecimal(rdr["success_count"].ToString()),
                            PendingCount = Convert.ToDecimal(rdr["pending_count"].ToString()),
                            IncompleteCount = Convert.ToDecimal(rdr["incomplete_count"].ToString()),
                            FailureCount = Convert.ToDecimal(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return productTrends;
        }
        public List<Products> GetTodayFailureTrending()
        {
            List<Products> products = new List<Products>();
            string sqlSelect = "select a.transdate,NEFT,RIB,NIP_OUTGOING,NIP_INCOMING,MACALLA from (select " +
"case when(sum(trans_count)-sum(case when code_description ='PENDING' then trans_count else 0 end)-sum(case when code_description='INCOMPLETE' then trans_count else 0 end))=0 then 0 else  " +
"(sum(case when code_description='FAILED' then trans_count else 0 end)*100)/(sum(trans_count)-sum(case when code_description ='PENDING' then trans_count else 0 end)-sum(case when code_description='INCOMPLETE' then trans_count else 0 end))end NEFT,  " +
"left(convert(varchar,spooled_time,120),16) transdate from neft_outgoing  " +
"where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16) ) a,  " +
"(select case when(sum(trans_count)-sum(case when code_description ='REVERSAL' then trans_count else 0 end)-sum(case when code_description='Processing' then trans_count else 0 end))=0 then 0 else  " +
"(sum(case when code_description in ('REVERSAL','SUCCESS','Processing')  then 0 else trans_count end)*100)/(sum(trans_count)-sum(case when code_description ='REVERSAL' then trans_count else 0 end)-sum(case when code_description='Processing' then trans_count else 0 end))end RIB,  " +
"left(convert(varchar,spooled_time,120),16) transdate from rib where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) b,  " +
"(select case when (sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)=0) then 0  " +
"else (sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)*100)/(sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)) end NIP_OUTGOING,  " +
"left(convert(varchar,spooled_time,120),16) transdate from nip_outgoing where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) c,  " +
"(select case when (sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)=0) then 0  " +
"else (sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)*100)/(sum(case when trans_complete_code='00' then trans_count else 0 end)+sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end)) end NIP_INCOMING,  " +
"left(convert(varchar,spooled_time,120),16) transdate from nip_incoming where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) d,  " +
"(select case when(sum(trans_count)-sum(case when trans_state_code='Fulfilment' then trans_count else 0 end))=0 then 0 else   " +
"(sum(case when (trans_state_code='Confirmation' OR trans_state_code='Fulfilment') then 0 else trans_count end)*100)/(sum(trans_count)-sum(case when trans_state_code='Fulfilment' then trans_count else 0 end))end MACALLA,  " +
"left(convert(varchar,spooled_time,120),16) transdate from macalla where convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by left(convert(varchar,spooled_time,120),16)) e  " +
"where a.transdate=b.transdate and a.transdate = c.transdate and a.transdate = d.transdate and a.transdate = e.transdate";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new Products
                        {
                            DateTime = rdr["transdate"].ToString().Substring(11),
                            Rib = Convert.ToInt32(rdr["RIB"].ToString()),
                            NipOutgoing = Convert.ToInt32(rdr["NIP_OUTGOING"].ToString()),
                            Macalla = Convert.ToInt32(rdr["MACALLA"].ToString()),
                            Neft = Convert.ToInt32(rdr["NEFT"].ToString()),
                            NipIncoming = Convert.ToInt32(rdr["NIP_INCOMING"].ToString())
                        };

                        products.Add(transactionSummary);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }

            return products;
        }
        public void SendMail(string[] addresses, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();

                //Setting From , To and CC
                mail.Subject = subject;
                mail.From = new MailAddress("boservice@ecobank.com", "BoService");
                foreach (var address in addresses)
                {
                    mail.To.Add(new MailAddress(address));
                }
                mail.Body = body;
                mail.IsBodyHtml = true;
                //mail.CC.Add(new MailAddress("aoyedere@ecobank.com"));
                //mail.CC.Add(new MailAddress("cokafor@ecobank.com"));
                var smtp = new SmtpClient();

                smtp.Host = "10.12.14.29";
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                var networkCred = new NetworkCredential("boservice", "2Hard4U123$", "ecobankgroup");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = networkCred;
                smtp.Port = 25;

                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (s, cert, chain, sslPolicyErrors) => true;
                smtp.Send(mail);
            }
            catch (SmtpException ste)
            {
                Console.WriteLine(ste.ToString());
            }

        }
        public string CheckProductInProblem()
        {
            string products = "";
            if (!IsRibOk())
            {
                if (products != "")
                    products += ", ";
                products += "Rib ";
                SendMail(new string[] { "oiloghalu@ecobank.com", "aoladejo@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON RIB SERVICE", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Application server or posting middleware might be down");
            }
            if (!IsNipIncomingOk())
            {
                if (products != "")
                    products += ", ";
                products += "NIP Incoming ";
                SendMail(new string[] { "oiloghalu@ecobank.com", "aoladejo@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON INCOMING NIP SERVICE", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: TS QUERY might not be responding or posting service might be down");
            }
            if (!IsNipOutgoingOk() && IsWorkingTime())
            {
                if (products != "")
                    products += ", ";
                products += "NIP Outgoing ";
                SendMail(new string[] { "oiloghalu@ecobank.com", "aoladejo@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON OUTGOING NIP SERVICE", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Application server or posting middleware might be down. Ignore if it is COB");
            }
            if (!IsAtmOk())
            {
                if (products != "")
                    products += ", ";
                products += "ATM ";
                SendMail(new string[] { "oige@ecobank.com", "eafolabi@ecobank.com", "ologunsanya@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON CARDS SERVICES", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Transactions not processing or postilion not normalizing");
            }
            if (!IsPosOk())
            {
                if (products != "")
                    products += ", ";
                products += "POS ";
                SendMail(new string[] { "oige@ecobank.com", "eafolabi@ecobank.com", "ologunsanya@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON CARDS SERVICES", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Transactions not processing or postilion not normalizing");
            }
            if (!IsWebOk())
            {
                if (products != "")
                    products += ", ";
                products += "WEB ";
                SendMail(new string[] { "oige@ecobank.com", "eafolabi@ecobank.com", "ologunsanya@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON CARDS SERVICES", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Transactions not processing or postilion not normalizing");
            }
            if (!IsNameEnquiryOk())
            {
                if (products != "")
                    products += ", ";
                products += "NIP Name Enquiry ";
                SendMail(new string[] { "oiloghalu@ecobank.com", "aoladejo@ecobank.com", "ALLENG-ITSERVICEMONITORINGTEAM@ecobank.com", "votuekong@ecobank.com", "risaac@ecobank.com" },
                    "Possible Service Outage - POSSIBLE DOWNTIME ON OUTGOING NAME ENQUIRY", "Please be informed of the Service Outage below and escalate to the Appropriate Support Team. POSSIBLE CAUSE: Major failure on name enquiry");
            }
            if (!IsInterBankOk())
            {
                if (products != "")
                    products += ", ";
                products += "Inter-Bank Transfers ";
            }
            if (!IsTopUpOk())
            {
                if (products != "")
                    products += ", ";
                products += "TopUp ";
            }
            if (!IsXpressAccountBoardingOk())
            {
                if (products != "")
                    products += ", ";
                products += "Xpress-Account Onboarding ";
            }
            return products;
        }
        private Boolean IsTopUpOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("mobile");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'TopUp' product,sum(trans_count) transaction_count, " +
                "sum(case when trans_code = '000' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_status, " +
                "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) system_failure, " +
                "sum(case when trans_code IN ('000',null,'null','','E92') then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile" +
                "where trans_type = 'TOPUP' and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) ";

                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            }
            double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalMinutes;
                rate = (successDiff / timeDiff) * 60;
            }
            return (rate < SuccessMinimumRate);
        }
        private Boolean IsInterBankOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("mobile");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'TopUp' product,sum(trans_count) transaction_count, " +
                "sum(case when trans_code = '000' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_status, " +
                "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) system_failure, " +
                "sum(case when trans_code IN ('000',null,'null','','E92') then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile" +
                "where trans_type = 'INTER-BANK TRANSFERS' and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) ";

                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            }
            double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return (rate < SuccessMinimumRate);
        }
        private Boolean IsXpressAccountBoardingOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("mobile");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'TopUp' product,sum(trans_count) transaction_count, " +
                "sum(case when trans_code = '000' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code = 'E92' then trans_count else 0 end) reversed_status, " +
                "sum(case when trans_code IN ('', null, 'null') then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code IN ('000',null, 'null','', 'E19','E45','E42','E62','E92','E11','E43','E81','E15','E05','E03','K03') then 0 else trans_count end) system_failure, " +
                "sum(case when trans_code IN ('000',null,'null','','E92') then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from mobile" +
                "where trans_type = 'XPRESS ACCOUNT ONBOARDING' and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) ";

                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            }
            double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return (rate < SuccessMinimumRate);
        }
        private Boolean IsRibOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("rib");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'RIB' product,sum(trans_count) transaction_count, " +
                                   "sum(case when code_description='SUCCESS' then trans_count else 0 end) success_status, " +
                                   "sum(case when code_description='REVERSAL' then trans_count else 0 end) reversed_status, " +
                                   "sum(case when code_description='Processing' then trans_count else 0 end) pending_status,  " +
                                   "sum( case when trans_code not in ('10','00','13') then trans_count else 0 end) system_failure, " +
                                   "sum(case when code_description in ('REVERSAL','SUCCESS','Processing')  then 0 else trans_count end) failure_status,  " +
                                   "convert(varchar,spooled_time,120) transdate from rib  " +
                                   "where convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)  " +
                                   "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120) ";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            if (rate < SuccessMinimumRate)
            {
                return false;
            }
            if (summaries[0].PendingStatus > RibPendingAlert)
            {
                return false;
            }
            return true;
        }
        private Boolean IsNipIncomingOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("nip_incoming");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'NIP INCOMING' product, sum(trans_count) transaction_count, " +
"sum(case when trans_complete_code='00' then trans_count else 0 end) success_status, " +
"sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_status,sum(case when trans_complete_code is null then trans_count else 0 end) pending_status, " +
"sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_status, " +
"sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz','13') then trans_count else 0 end) system_failure, " +
"sum(case when (trans_complete_code in ('00','x7','zz') or trans_complete_code is null)  then 0 else trans_count end) failure_status,  " +
"convert(varchar,spooled_time,120) transdate from nip_incoming where convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)   " +
"and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            if (rate < SuccessMinimumRate)
            {
                return false;
            }
            if (summaries[0].PendingStatus > NipIncomingPendingAlert)
            {
                return false;
            }
            return true;
        }
        private Boolean IsNipOutgoingOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("nip_outgoing");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect = "select 'NIP OUTGOING' product,sum(trans_count) transaction_count,  " +
"sum(case when trans_complete_code='00' then trans_count else 0 end) success_status, " +
"sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_status, 0 pending_status, " +
"sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_status, " +
"sum(case when trans_complete_code not in ('00','x12','x11','x1','x7','zz') then trans_count else 0 end) system_failure, " +
"sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end) failure_status,  " +
"convert(varchar,spooled_time,120) transdate from nip_outgoing " +
" where convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime + "' as datetime),120)   " +
"and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  group by convert(varchar,spooled_time,120)";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return !(rate < SuccessMinimumRate);
            //            if (summaries[0].PendingStatus > NipIncomingPendingAlert)
            //            {
            //                return false;
            //            }
        }
        private Boolean IsAtmOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("fep");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect =
                    "select 'CARDS ON-'+channel_type product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                    "0 reversed_status,0 pending_status,0 incomplete_status, " +
                    "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure, " +
                    "sum(case when rsp_code <>'00'  then tran_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                    "from fep,fep_response_code " +
                    "where channel_type ='ATM' " +
                    "and rsp_code=code and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime +
                    "' as datetime),120)   " +
                    "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) " +
                    "group by channel_type,convert(varchar,spooled_time,120)";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return !(rate < SuccessMinimumRate);
            //            if (summaries[0].PendingStatus > NipIncomingPendingAlert)
            //            {
            //                return false;
            //            }
        }
        private Boolean IsPosOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("fep");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect =
                    "select 'CARDS ON-'+channel_type product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                    "0 reversed_status,0 pending_status,0 incomplete_status, " +
                    "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure, " +
                    "sum(case when rsp_code <>'00'  then tran_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                    "from fep,fep_response_code " +
                    "where channel_type ='POS' " +
                    "and rsp_code=code and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime +
                    "' as datetime),120)   " +
                    "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) " +
                    "group by channel_type,convert(varchar,spooled_time,120)";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return !(rate < SuccessMinimumRate);
            //            if (summaries[0].PendingStatus > NipIncomingPendingAlert)
            //            {
            //                return false;
            //            }
        }
        private Boolean IsWebOk()
        {
            List<DateTime> dateTimes = GetLastTwoFetchDateTime("fep");
            List<Summary> summaries = new List<Summary>();
            foreach (DateTime dateTime in dateTimes)
            {
                string sqlSelect =
                    "select 'CARDS ON-'+channel_type product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                    "0 reversed_status,0 pending_status,0 incomplete_status, " +
                    "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure, " +
                    "sum(case when rsp_code <>'00'  then tran_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                    "from fep,fep_response_code " +
                    "where channel_type ='WEB' " +
                    "and rsp_code=code and convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime +
                    "' as datetime),120)   " +
                    "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) " +
                    "group by channel_type,convert(varchar,spooled_time,120)";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            var transactionSummary = new Summary
                            {
                                Product = rdr["product"].ToString(),
                                SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
                                SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
                                FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
                                ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
                                PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
                                TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
                            };
                            string datetime = rdr["transdate"].ToString();
                            DateTime dt =
                                DateTime.Parse(datetime);

                            transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

                            summaries.Add(transactionSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                }
            } double rate = 0;
            if (summaries.Count == 2)
            {
                float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
                double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
                rate = (successDiff / timeDiff) * 60;
            }
            return !(rate < SuccessMinimumRate);
            //            if (summaries[0].PendingStatus > NipIncomingPendingAlert)
            //            {
            //                return false;
            //            }
        }
        //private Boolean IsMacallaInterAccDcOk()
        //{
        //    List<DateTime> dateTimes = GetLastTwoFetchDateTime("macalla");
        //    List<Summary> summaries = new List<Summary>();
        //    foreach (DateTime dateTime in dateTimes)
        //    {
        //        string sqlSelect =
        //            "select 'Macalla InterAccountDC' product,sum(trans_count) transaction_count,sum(case when trans_state_code='Confirmation' or trans_state_code='Sucessful' then trans_count else 0 end) success_status, " +
        //            "sum(case when trans_state_code in ('Confirmation','Fulfilment','Sucessful','Pending','InsufficientBalance','Blocked Account','Unable to Vend, call Ecobank Contact Centre.','E-Top-Up Limit Exceeded','NOT NORMAL ACCOUNT','Unauthorized Account','Unauthorized Customer') then 0 else trans_count end) failure_status,  " +
        //            "sum(case when trans_state_code='Fulfilment' or trans_state_code='Pending' then trans_count else 0 end) pending_status,  " +
        //            "left(convert(varchar,spooled_time,120),16) transdate   " +
        //            "from macalla where convert(varchar,spooled_time,120)=convert(varchar,cast('" + dateTime +
        //            "' as datetime),120)  " +
        //            "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) and " +
        //            "trans_type= 'InterAccountDebitCredit' group by left(convert(varchar,spooled_time,120),16) ";
        //        using (SqlConnection con = new SqlConnection(_sqlCs))
        //        {
        //            try
        //            {
        //                SqlCommand cmd = new SqlCommand(sqlSelect, con);
        //                con.Open();
        //                SqlDataReader rdr = cmd.ExecuteReader();
        //                while (rdr.Read())
        //                {
        //                    var transactionSummary = new Summary
        //                    {
        //                        Product = rdr["product"].ToString(),
        //                        SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString()),
        //                        //SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString()),
        //                        FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString()),
        //                        //ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString()),
        //                        PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString()),
        //                        TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString())
        //                    };
        //                    string datetime = rdr["transdate"].ToString();
        //                    DateTime dt =
        //                        DateTime.Parse(datetime);

        //                    transactionSummary.TransDate = dt;//DateTime.Parse(datetime);

        //                    summaries.Add(transactionSummary);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(e.ToString());
        //            }
        //            finally
        //            {

        //                con.Close();
        //            }


        //        }
        //    } double rate = 0;
        //    if (summaries.Count == 2)
        //    {
        //        float successDiff = summaries[0].SuccessStatus - summaries[1].SuccessStatus;
        //        double timeDiff = summaries[0].TransDate.Subtract(summaries[1].TransDate).TotalSeconds;
        //        rate = (successDiff / timeDiff) * 60;
        //    }
        //    return !(rate < SuccessMinimumRate);
            
        //}
        private Boolean IsNameEnquiryOk()
        {
            int time = -1;
               string sqlSelect =
                    "select response_time,request_count FROM nameenquiry where convert(varchar,spooled_time,120)= " +
                    "(select top 1 convert(varchar,spooled_time,120) sa from nameenquiry group by convert(varchar,spooled_time,120) ORDER BY convert(varchar,spooled_time,120) DESC) ";
                using (SqlConnection con = new SqlConnection(_sqlCs))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sqlSelect, con);
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            time = Convert.ToInt32(rdr["suresponse_timeccess_status"].ToString());
                           
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {

                        con.Close();
                    }


                
            }
                return (time < 10);
            
        }
        private List<DateTime> GetLastTwoFetchDateTime(string tableName)
        {
            string sqlSelect = "select top 2 convert(varchar,spooled_time,120) sa from " + tableName + " group by convert(varchar,spooled_time,120) ORDER BY convert(varchar,spooled_time,120) DESC ";
            List<DateTime> dateTimes = new List<DateTime>();
            using (var con = new SqlConnection(_sqlCs))
            {
                try
                {
                    var cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        dateTimes.Add(DateTime.Parse(rdr["sa"].ToString()));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {

                    con.Close();
                }
            }
            return dateTimes;
        }
        public static bool IsWorkingTime()
        {
            DateTime dateTime = DateTime.Now;
            List<string> workingDays = new List<string> {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday"};
            if (workingDays.Contains(dateTime.DayOfWeek.ToString()))
            {
                if (dateTime.Hour >= 7 && dateTime.Hour <= 20)
                {
                    return true;
                }
            }
            return false;
        }
        public static ThresholdState GetThresholdState(float successPercent)
        {
            if (successPercent >= 95f)
            {
                return ThresholdState.Good;

            }
            else if (successPercent >= 90f)
            {
                return ThresholdState.Fair;
            }
            else
            {
                return ThresholdState.Bad;
            }
            return 0;
        }
        public enum ThresholdState
        {
            Good = 0,
            Fair = 1,
            Bad = 2
        }

    }
}