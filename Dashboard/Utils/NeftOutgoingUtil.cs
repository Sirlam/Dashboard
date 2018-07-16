using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class NeftOutgoingUtil
    {
        private readonly string _nipOutgoingCs;
        private readonly string _sqlCs;

        public NeftOutgoingUtil()
        {
            _nipOutgoingCs = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<NeftOutgoingSummary> GetNeftOutgoingTodayPerformance()
        {
            List<NeftOutgoingSummary> neftOutgoingSummaries = new List<NeftOutgoingSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, "+
                "sum(case when code_description='SUCCESS' then trans_count else 0 end) success_status,  "+
                "sum(case when code_description ='PENDING' then trans_count else 0 end) pending_status,  "+
                "sum(case when code_description='INCOMPLETE' then trans_count else 0 end) incomplete_status, "+
                "sum(case when code_description ='FAILED'  then  trans_count else 0 end) failure_status, "+
                "convert(varchar,spooled_time,120) transdate from neft_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
           using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NeftOutgoingSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalIncompleteTransaction = Convert.ToInt32(rdr["incomplete_status"].ToString()),
                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        neftOutgoingSummaries.Add(transactionSummary);
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

            return neftOutgoingSummaries;
        }
        public List<NeftOutgoingStateSummary> GetNeftOutgoingTodayStateSummary()
        {
            List<NeftOutgoingStateSummary> neftOutgoingStateSummaries = new List<NeftOutgoingStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select sum(trans_count) transaction_count,trans_complete_code trans_state, code_description, description, trans_volume, " +
                "code_description state_ind,   " +
                "convert(varchar,spooled_time,120) transdate from neft_outgoing full join neft_status on trans_complete_code=code " +
                "where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) " +
                "and convert(varchar,spooled_time,112)=convert(varchar, getdate(), 112)  " +
                "group by trans_complete_code, code_description,trans_volume,description,convert(varchar,spooled_time,120) order by state_ind desc ";
            
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NeftOutgoingStateSummary()
                        {
                            TransactionStateCode = rdr["trans_state"].ToString(),
                            TransactionStateDesc = rdr["code_description"].ToString(),
                            CodeDescription = rdr["description"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalVolume = Convert.ToDecimal(rdr["trans_volume"].ToString()),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());
                        transactionSummary.TransactionDateTime = datetime;

                        neftOutgoingStateSummaries.Add(transactionSummary);
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

            return neftOutgoingStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM neft_outgoing ORDER BY ID DESC ";
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

        private IEnumerable<NeftOutgoingDetail> GetNeftOutgoingDetails()
        {

            string sqlSelect = "select code,TRN_DEBIT_STATUS,category, COUNT(*) trans_count,sum(amount) volume " +
                               "from (SELECT clearingstatus code,TRN_DEBIT_STATUS, amount, CASE WHEN clearingstatus = 'sent' THEN 'SUCCESS' " +
                                "WHEN clearingstatus in ('AwaitingClearing','Generated') THEN 'PENDING' " +
                                "WHEN clearingstatus = ' ' and TRN_CHECKER is null THEN 'INCOMPLETE' " +
                                "WHEN clearingstatus = ' ' and TRN_DEBIT_STATUS <> '00' and TRN_CHECKER is not null THEN 'FAILED' " +
                                "ELSE clearingstatus END category " +
                                "FROM NEFT " +
                                "where SESSION_DATE = convert(varchar, getdate(), 112)) a " +
                                "GROUP BY code,TRN_DEBIT_STATUS,category";

            List<NeftOutgoingDetail> neftOutgoingDetails = new List<NeftOutgoingDetail>();
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
                        NeftOutgoingDetail neftOutgoingDetail = new NeftOutgoingDetail();
                        neftOutgoingDetail.Category = rdr["category"].ToString();
                        neftOutgoingDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        neftOutgoingDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        neftOutgoingDetail.CodeDescription = rdr["code"].ToString();
                        neftOutgoingDetail.TransactionCompleteCode = rdr["TRN_DEBIT_STATUS"].ToString();
                        neftOutgoingDetail.SpooledTime = spoolDateTime;
                        neftOutgoingDetails.Add(neftOutgoingDetail);
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
            return neftOutgoingDetails;
        }

        public Boolean StageNeftOutgoingData()
        {
            IEnumerable<NeftOutgoingDetail> neftOutgoingDetails = GetNeftOutgoingDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var neftOutgoingDetail in neftOutgoingDetails)
                    {
                        const string insQuery = "BEGIN Insert Into neft_outgoing (trans_complete_code,code_description,trans_count,spooled_time,trans_volume)" +
                                                "  values (@transcode,@codedescription,@transcount,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = neftOutgoingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = neftOutgoingDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = neftOutgoingDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = neftOutgoingDetail.Category;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = neftOutgoingDetail.SpooledTime;

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