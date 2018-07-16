using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class NipOutgoingUtil
    {
        private readonly string _nipOutgoingCs;
        private readonly string _sqlCs;

        public NipOutgoingUtil()
        {
            _nipOutgoingCs = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<NipOutgoingSummary> GetNipOutgoingTodayPerformance()
        {
            List<NipOutgoingSummary> nipOutgoingSummaries = new List<NipOutgoingSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when trans_complete_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when trans_complete_code='x7' then trans_count else 0 end) reversed_status, " +
                "sum(case when trans_complete_code='zz' then trans_count else 0 end) incomplete_status, " +
                "sum(case when trans_complete_code in ('00','x7','zz')  then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from nip_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipOutgoingSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalReversedTransaction = Convert.ToInt32(rdr["reversed_status"].ToString()),
                            TotalIncompleteTransaction = Convert.ToInt32(rdr["incomplete_status"].ToString())
                            //                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        nipOutgoingSummaries.Add(transactionSummary);
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

            return nipOutgoingSummaries;
        }
        public List<NipOutgoingStateSummary> GetNipOutgoingTodayStateSummary()
        {
            List<NipOutgoingStateSummary> nipOutgoingStateSummaries = new List<NipOutgoingStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_complete_code trans_state, code_description, trans_volume, " +
                "case when trans_complete_code='00' then 'Success' when trans_complete_code='x7' then 'Reversal' when trans_complete_code='zz' then 'Incomplete' when trans_complete_code='' then 'Failed' else 'Failed' end state_ind,  " +
                "convert(varchar,spooled_time,120) transdate from nip_outgoing where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) " +
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
                        var transactionSummary = new NipOutgoingStateSummary
                        {
                            TransactionStateCode = rdr["trans_state"].ToString(),
                            TransactionStateDesc = rdr["code_description"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalVolume = Convert.ToDecimal(rdr["trans_volume"].ToString()),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        nipOutgoingStateSummaries.Add(transactionSummary);
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

            return nipOutgoingStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM nip_outgoing ORDER BY ID DESC ";
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

        private IEnumerable<NipOutgoingDetail> GetNipOutgoingDetails()
        {
            //TODO: change shengbele and ghenghen
            string sqlSelect =
                "SELECT COUNT(sessionid) trans_count,A.completecode,A.description,Sum(isnull(cast(trn_amount as decimal),0)) volume " +
                "FROM(select sessionid, completecode,  CASE WHEN completecode = '00' THEN 'SUCCESS' WHEN completecode = 'x7' THEN 'REVERSAL' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '91' THEN 'BENEFICIARY BANK NOT AVAILABLE' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '07' THEN 'INVALID ACCOUNT' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '00' and authoriseflag = 1 THEN 'UNAUTHORISED' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '00' and authoriseflag = 2 THEN '' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '00' and authoriseflag = 3 THEN 'CANCELLED' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '00' and authoriseflag = 0 and (cast(isobalance as float) - cast(trn_amount as float)) < 0  THEN 'INSUFFICIENT BALANCE' " +
                "WHEN completecode = 'zz' and rsp_responsecode = '00' and authoriseflag = 0 and (cast(isobalance as float) - cast(trn_amount as float)) > 0 " +
                "THEN 'TO BE INVESTIGATED' ELSE description END description,trn_amount " +
                "from vw_nameenquiry_out , responsecode b where code=completecode and  cast(requestdate as date) = left(convert(varchar, getdate(), 120), 10) )A " +
                "GROUP BY completecode,description";

            List<NipOutgoingDetail> nipOutgoingDetails = new List<NipOutgoingDetail>();
            using (SqlConnection con = new SqlConnection(_nipOutgoingCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NipOutgoingDetail nipOutgoingDetail = new NipOutgoingDetail();
                        nipOutgoingDetail.CodeDescription = rdr["description"].ToString();
                        nipOutgoingDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        nipOutgoingDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        nipOutgoingDetail.TransactionCompleteCode = rdr["completecode"].ToString();
                        nipOutgoingDetail.SpooledTime = spoolDateTime;
                        nipOutgoingDetails.Add(nipOutgoingDetail);
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
            return nipOutgoingDetails;
        }

        public Boolean StageNipOutgoingData()
        {
            IEnumerable<NipOutgoingDetail> nipOutgoingDetails = GetNipOutgoingDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var nipOutgoingDetail in nipOutgoingDetails)
                    {
                        const string insQuery = "BEGIN Insert Into nip_outgoing (trans_complete_code,code_description,trans_count,spooled_time,trans_volume)" +
                                                "  values (@transcode,@codedescription,@transcount,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = nipOutgoingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = nipOutgoingDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = nipOutgoingDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = nipOutgoingDetail.CodeDescription;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = nipOutgoingDetail.SpooledTime;

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
                if (rowAffected > 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}