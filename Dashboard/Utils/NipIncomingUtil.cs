using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class NipIncomingUtil
    {
        private readonly string _nipIncomingCs;
        private readonly string _sqlCs;

        public NipIncomingUtil()
        {
            _nipIncomingCs = ConfigurationManager.ConnectionStrings["NipIncomingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<NipIncomingSummary> GetNipIncomingTodayPerformance()
        {
            List<NipIncomingSummary> nipIncomingSummaries = new List<NipIncomingSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when state='Success' then trans_count else 0 end) success_status, " +
                "sum(case when state='Reversal' then trans_count else 0 end) reversed_status, " +
                "sum(case when state='Incomplete' then trans_count else 0 end) incomplete_status, " +
                "sum(case when state='Pending' then trans_count else 0 end) pending_status, " +
                "sum(case when state='Failed'  then  trans_count else 0  end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from nip_incoming where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipIncomingSummary
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

                        nipIncomingSummaries.Add(transactionSummary);
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

            return nipIncomingSummaries;
        }
        public List<NipIncomingStateSummary> GetNipIncomingTodayStateSummary()
        {
            List<NipIncomingStateSummary> nipIncomingStateSummaries = new List<NipIncomingStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_complete_code trans_state, code_description, trans_volume, " +
                "state state_ind,  " +
                "convert(varchar,spooled_time,120) transdate from nip_incoming where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) " +
                 "order by state_ind desc";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipIncomingStateSummary
                        {
                            TransactionStateCode = rdr["trans_state"].ToString(),
                            TransactionStateDesc = rdr["code_description"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalVolume = Convert.ToDecimal(rdr["trans_volume"].ToString()),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        nipIncomingStateSummaries.Add(transactionSummary);
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

            return nipIncomingStateSummaries;
        }
        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM nip_incoming ORDER BY ID DESC ";
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
        private IEnumerable<NipIncomingDetail> GetNipIncomingDetails()
        {

            string sqlSelect =
                "select response CODE, CASE WHEN response = '00' THEN 'SUCCESS' ELSE description END CATEGORY, " +
                "  case when response='00' then 'Success' when response='x7' then 'Reversal'  " +
                " when response='zz' then 'Incomplete'  when COMPLETEDSTATUS <> 'Processed' and response is null then 'Pending'  " +
                " when COMPLETEDSTATUS = 'Processed' and (response <> '00' or response is null) and chkval = '0' then 'Failed' else 'Failed' end state, " +
                " 'ENG' AFFILIATE_CODE, sum(volume) trans_count, sum(amount) volume, trndate " +
                " from( " +
                "  SELECT  response,description,COMPLETEDSTATUS,chkval, TRUNC(systemdate) trndate, COUNT(*) VOLUME, sum(cast(field4 as float)/100) amount " +
                " FROM compass.Tanking_isolog8583 f full join compass.responsecode b on  response=code WHERE  " +
                " to_char(systemdate,'dd-MON-yyyy') =to_char(sysdate,'dd-MON-yyyy') " +
                " GROUP BY response, description,COMPLETEDSTATUS,chkval, TRUNC(systemdate)) t  " +
                " GROUP BY response,description,COMPLETEDSTATUS,chkval, trndate";

            List<NipIncomingDetail> nipIncomingDetails = new List<NipIncomingDetail>();
            using (OracleConnection con = new OracleConnection(_nipIncomingCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NipIncomingDetail nipIncomingDetail = new NipIncomingDetail();
                        nipIncomingDetail.CodeDescription = rdr["CATEGORY"].ToString();
                        nipIncomingDetail.TransactionStateCode = rdr["state"].ToString();
                        nipIncomingDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        nipIncomingDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        object j = rdr["CODE"].GetType().Name;
                        if (!rdr["CODE"].GetType().Name.Equals("DBNull"))
                            nipIncomingDetail.TransactionCompleteCode = rdr["CODE"].ToString();
                        else
                        {
                            nipIncomingDetail.TransactionCompleteCode = null;
                        }//nipIncomingDetail.TransactionCompleteCode = rdr["CODE"]!=null ? rdr["CODE"].ToString() : null;
                        nipIncomingDetail.SpooledTime = spoolDateTime;
                        nipIncomingDetails.Add(nipIncomingDetail);
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
            return nipIncomingDetails;
        }
        public int StageNipIncomingData()
        {
            IEnumerable<NipIncomingDetail> nipIncomingDetails = GetNipIncomingDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var nipIncomingDetail in nipIncomingDetails)
                    {
                        const string insQuery = "BEGIN Insert Into nip_incoming (trans_complete_code,code_description,state,trans_count,spooled_time,trans_volume)" +
                                                "  values (@transcode,@codedescription,@state,@transcount,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        if (nipIncomingDetail.TransactionCompleteCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", nipIncomingDetail.TransactionCompleteCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = nipIncomingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = nipIncomingDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = nipIncomingDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = nipIncomingDetail.CodeDescription;
                        cmd.Parameters.AddWithValue("@state", SqlDbType.NVarChar).Value = nipIncomingDetail.TransactionStateCode;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = nipIncomingDetail.SpooledTime;

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