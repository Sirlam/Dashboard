using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class CollectionsSummaryUtil
    {
        private readonly string _sqlCs;

        public class CollectionsSummary
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
       
        public CollectionsSummaryUtil()
        {
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<CollectionsSummary> GetTodayPerformanceSummary()
        {
            List<CollectionsSummary> collectionsSummaries = new List<CollectionsSummary>();
            string sqlSelect =
                "select a.*,case when(a.success_status+a.system_failure)=0 then 0 else ((a.success_status*1000)/(a.success_status+a.system_failure)) end percet from ( " +
                "select 'BASIS' product,sum(trans_count)transaction_count,sum(trans_count)success_status, 0 reversed_status, " +
                "0 pending_status,0 incomplete_status,0 system_failure,0 failure_status, " +
                "convert(varchar,spooled_time,120) transdate from basis  " +
                "where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM basis ORDER BY ID DESC) as datetime),120) and  " +
                "convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by convert(varchar,spooled_time,120) " +
                "union all " +
                "select 'SWIFTMT101',sum(trans_count) transaction_count, sum(case when trans_code IN ('Work Exit1', 'Discard1') then trans_count else 0 end) success_status, " +
                "0 reversed_status,0 incomplete_status, " +
                "sum(case when trans_code not in ('Work Exit1', 'Discard1', 'Out_Exception','Exception_Handling') then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code in ('Out_Exception','Exception_Handling') then trans_count else 0 end) system_failure, " +
                "sum(case when trans_code in ('Out_Exception','Exception_Handling')  then trans_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from swift_mt_101 " +
                "where " +
                "convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM swift_mt_101 ORDER BY ID DESC) as datetime),120) " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  "+
                "group by convert(varchar,spooled_time,120) " +
                "union all " +
                "select 'FCY CARDS ON-'+channel_type product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status, " +
                "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure, " +
                "sum(case when rsp_code <>'00'  then tran_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from fcy_cards,fep_response_code " +
                "where channel_type in ('POS','WEB','ATM', 'OTHER_CHANNELS') " +
                "and rsp_code=code " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM fcy_cards ORDER BY ID DESC) as datetime),120) " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by channel_type,convert(varchar,spooled_time,120) "+
                "union all "+
                "select 'QT_Inflow' product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status, "+
                "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure,  "+
                "sum(case when rsp_code <> '00' then tran_count else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  "+
                "from qt_inflow,fep_response_code  "+
                "where rsp_code=code  "+
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM qt_inflow ORDER BY ID DESC) as datetime),120)  "+
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) "+
                "union all "+
                "select 'pin_selection' product,sum(trans_count) transaction_count, sum(trans_count) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status, " +
                "0 system_failure,  " +
                "0 failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from pin_selection  " +
                "where " +
                "convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM pin_selection ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) " +
                "union all "+
                "select 'Xcash_On_Atm' product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status, " +
                "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure,  " +
                "sum(case when rsp_code <> '00' then tran_count else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from xcash_atm,fep_response_code  " +
                "where rsp_code=code  " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM xcash_atm ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) " +
                "union all "+
                "select 'Nip_On_Atm' product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  "+
                "0 reversed_status,0 pending_status,0 incomplete_status, "+
                "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure,  "+
                "sum(case when rsp_code <> '00' then tran_count else 0 end) failure_status,convert(varchar,spooled_time,120) transdate   "+
                "from nip_atm,fep_response_code  "+
                "where rsp_code=code  "+
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_atm ORDER BY ID DESC) as datetime),120)  "+
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  "+
                "group by convert(varchar,spooled_time,120) " +
                "union all "+
                "select 'PayArena_On_Atm' product,sum(tran_count) transaction_count, sum(case when rsp_code='00' then tran_count else 0 end) success_status,  " +
                "0 reversed_status,0 pending_status,0 incomplete_status, " +
                "sum(case when rsp_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then tran_count else 0 end) system_failure,  " +
                "sum(case when rsp_code <> '00' then tran_count else 0 end) failure_status,convert(varchar,spooled_time,120) transdate   " +
                "from payarena_atm,fep_response_code  " +
                "where rsp_code=code  " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM payarena_atm ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) " +
                "union all "+
                "select 'Nip_Xpress_Account' product,sum(trans_count) transaction_count, sum(case when trans_code='0' then trans_count else 0 end) success_status,  "+
                "0 reversed_status,0 incomplete_status,  "+
                "sum(case when trans_code='' or trans_code is null then trans_count else 0 end) pending_status,  "+
                "sum(case when code_description in ('Due to technical difficulties the service encountered on unexpected error.','Service Error 158, please contact customer care.') then trans_count else 0 end) system_failure,  "+
                "sum(case when trans_code <> '0' then trans_count else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  "+
                "from nip_xpress "+
                "where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM nip_xpress ORDER BY ID DESC) as datetime),120)  "+
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) " +
                "union all " +
                "select 'REMITA',sum(trans_count) transaction_count, "+
                "sum(case when trans_code = '00' then trans_count else 0 end) success_status,0 reversed_status,0 incomplete_status,0 pending_status, "+
                "sum(case when trans_code in ('06','91','96','X906','X11') then trans_count else 0 end) system_failure, "+
                "sum(case when trans_code <> '00'  then trans_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate "+
                "from remita "+
                "where "+
                "convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM remita ORDER BY ID DESC) as datetime),120) "+
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120) " +
                "union all "+
                "select 'PAYDIRECT',sum(trans_count) transaction_count, sum(case when trans_code='00' then trans_count else 0 end) success_status, " +
                "0 reversed_status,0 pending_status,0 incomplete_status, " +
                "sum(case when trans_code not in ('00','01','02','05','12','14','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','63','65','75') then trans_count else 0 end) system_failure, " +
                "sum(case when trans_code <>'00'  then trans_count  else 0 end) failure_status,convert(varchar,spooled_time,120) transdate  " +
                "from paydirect,fep_response_code " +
                "where trans_code=code " +
                "and convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM paydirect ORDER BY ID DESC) as datetime),120) " +
                "and convert(varchar,spooled_time,103)=convert(varchar,getdate(),103)  " +
                "group by convert(varchar,spooled_time,120)" +
                ")a order by percet ";


            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new CollectionsSummary();

                        transactionSummary.Product = rdr["product"].ToString();
                        transactionSummary.SuccessStatus = Convert.ToInt32(rdr["success_status"].ToString());
                        transactionSummary.SystemFailureStatus = Convert.ToInt32(rdr["system_failure"].ToString());
                        transactionSummary.FailureStatus = Convert.ToInt32(rdr["failure_status"].ToString());
                        transactionSummary.IncompleteStatus = Convert.ToInt32(rdr["incomplete_status"].ToString());
                        transactionSummary.ReversedStatus = Convert.ToInt32(rdr["reversed_status"].ToString());
                        transactionSummary.PendingStatus = Convert.ToInt32(rdr["pending_status"].ToString());
                        transactionSummary.TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString());

                        collectionsSummaries.Add(transactionSummary);
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

            return collectionsSummaries;
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