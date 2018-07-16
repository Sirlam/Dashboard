using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class FepUtil
    {
        private readonly string _fepCs;
        private readonly string _sqlCs;

        public FepUtil()
        {
            _fepCs = ConfigurationManager.ConnectionStrings["FepCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<FepSummary> GetFepTodayPerformance(string fepType)
        {
            List<FepSummary> fepSummaries = new List<FepSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when rsp_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when rsp_code='x7' then trans_count else 0 end) reversed_status, " +
                "sum(case when rsp_code='zz' then trans_count else 0 end) incomplete_status, " +
                "sum(case when rsp_code is null then trans_count else 0 end) pending_status, " +
                "sum(case when (rsp_code in ('00','x7','zz') or rsp_code is null)  then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from fep where channel_type='" + fepType + "' and convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FepSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalReversedTransaction = Convert.ToInt32(rdr["reversed_status"].ToString()),
                            TotalIncompleteTransaction = Convert.ToInt32(rdr["incomplete_status"].ToString()),
                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        fepSummaries.Add(transactionSummary);
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

            return fepSummaries;
        }
        public List<FepStateSummary> GetFepTodayStateSummary(string fepType)
        {
            List<FepStateSummary> fepStateSummaries = new List<FepStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select rsp_code,description,case when rsp_code='00' then 'Successful' else 'Failed' end state_ind, sum(distinct(tran_count)) tran_count,sum(tot_vol) tot_vol,convert(varchar,spooled_time,120) transdate " +
                "from fep,fep_response_code " +
                "where channel_type='" + fepType + "' " +
                "and rsp_code=code " +
                "and convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)  " +
                "group by rsp_code,description,convert(varchar,spooled_time,120) order by state_ind desc";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FepStateSummary();

                        transactionSummary.TransactionStateCode = rdr["rsp_code"].ToString();
                        transactionSummary.TransactionStateDesc = rdr["description"].ToString();
                        transactionSummary.TotalTransaction = Convert.ToInt32(rdr["tran_count"].ToString());
                        if (!rdr["tot_vol"].GetType().Name.Equals("DBNull"))
                        {
                            transactionSummary.TotalTransactionVolume = Convert.ToDecimal(rdr["tot_vol"].ToString());
                        }
                        else
                        {
                            transactionSummary.TotalTransactionVolume = 0;
                        }

                    transactionSummary.TransactionStateInd = rdr["state_ind"].ToString();
                        
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        fepStateSummaries.Add(transactionSummary);
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

            return fepStateSummaries;
        }
        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM fep ORDER BY ID DESC ";
            DateTime lastTime = DateTime.Now.Date;
            using (var con = new SqlConnection(_sqlCs))
            {
                try
                {
                    var cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    var date = cmd.ExecuteScalar().ToString();
                    lastTime = DateTime.Parse(date);

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
            return lastTime;
        }
        public IEnumerable<FepDetail> GetFepDetails()
        {

            string sqlSelect =
                "SELECT b.rsp_code_rsp as rsp_code, count (distinct b.retrieval_reference_nr) as tran_count, " +
                "case when pos_terminal_type = '00' then 'WEB' " +
                "when pos_terminal_type = '01' then 'POS' " +
                "when pos_terminal_type = '02' then 'ATM'  " +
                "when pos_terminal_type = '21' then 'MOBILE'  else 'OTHER_CHANNELS'  " +
                "end CHANNEL_TYPE, " +
                "case when b.tran_type = '00' then 'PURCHASE' " +
                "when b.tran_type = '01' then 'CSHWITHDL' " +
                "when b.tran_type = '40' then 'TRF'  " +
                "when b.tran_type = '50' then 'PAYMNT'  " +
                "when b.tran_type = '21' then 'DEPOSIT' else 'OTHER_tran_type'  " +
                "end tran_type,sum(settle_amount_rsp)/100 tot_vol  " +
                "FROM dbo.post_tran_cust a INNER JOIN " +
                "dbo.post_tran b ON a.post_tran_cust_id = b.post_tran_cust_id " +
                "WHERE b.tran_completed = 1 " +
                "and b.tran_type in ('01', '00', '50', '40','31','92','21') " +
                "and substring(pan,1,6) in ('506118','537010','529751','506122','531992','427872','499908','428602','428603','499910','499909') " +
                "and message_type not in ('0420','0421','0220') " +
                "AND convert(varchar, b.datetime_tran_local, 112) = convert(varchar, getdate(), 112) " +
                "and b.rsp_code_rsp in ('00','01','02','05','06','12','14','23','25','30','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','62','63','65','68','75','91','92','96','98') " +
                "group by b.rsp_code_rsp,pos_terminal_type,b.tran_type";


            List<FepDetail> fepDetails = new List<FepDetail>();
            using (SqlConnection con = new SqlConnection(_fepCs))
            {
                //                try
                //                {
                SqlCommand cmd = new SqlCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    FepDetail fepDetail = new FepDetail();
                    fepDetail.TransactionType = rdr["tran_type"].ToString();
                    fepDetail.ChannelType = rdr["CHANNEL_TYPE"].ToString();
                    fepDetail.TransactionCount = Convert.ToInt32(rdr["tran_count"].ToString());
                    fepDetail.TransactionVolume = Convert.ToDecimal(rdr["tot_vol"].ToString());
                    object j = rdr["rsp_code"].GetType().Name;
                    if (!rdr["rsp_code"].GetType().Name.Equals("DBNull"))
                        fepDetail.TransactionResponseCode = rdr["rsp_code"].ToString();
                    else
                    {
                        fepDetail.TransactionResponseCode = null;
                    }//fepDetail.TransactionCompleteCode = rdr["CODE"]!=null ? rdr["CODE"].ToString() : null;
                    fepDetail.SpooledTime = DateTime.Now;
                    fepDetails.Add(fepDetail);
                }
                //                }
                //                catch (Exception e)
                //                {
                //                    throw e;
                //                    Console.WriteLine(e.ToString());
                //                }
                //                finally
                //                {

                con.Close();
                //                }

            }
            return fepDetails;
        }
        public int StageFepData()
        {
            IEnumerable<FepDetail> fepDetails = GetFepDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var fepDetail in fepDetails)
                    {
                        const string insQuery = "BEGIN Insert Into fep (tran_type,tran_count,rsp_code,channel_type,spooled_time,tot_vol)" +
                                                "  values (@tran_type,@tran_count,@rsp_code,@channel_type,@spooledtime,@tot_vol); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        if (fepDetail.TransactionResponseCode != null)
                        {
                            cmd.Parameters.AddWithValue("@rsp_code", fepDetail.TransactionResponseCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@rsp_code", DBNull.Value);
                        }
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = nipIncomingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@tran_count", SqlDbType.Int).Value = fepDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@tot_vol", SqlDbType.Decimal).Value = fepDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@tran_type", SqlDbType.NVarChar).Value = fepDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.NVarChar).Value = fepDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = fepDetail.SpooledTime;

                        rowAffected += cmd.ExecuteNonQuery();
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

                return rowAffected;
            }
        }
    }
}