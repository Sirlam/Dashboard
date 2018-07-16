using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class NapsUtil
    {
        private readonly string _napsIncoming;
        private readonly string _sqlCs;

        public NapsUtil()
        {
            _napsIncoming = ConfigurationManager.ConnectionStrings["NapsIncomingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<NapsSummary> GetNapsTodayPerformance()
        {
            List<NapsSummary> napsSummaries = new List<NapsSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code<>'00' or trans_code='' then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from naps where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NapsSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        napsSummaries.Add(transactionSummary);
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

            return napsSummaries;
        }
        public List<NapsStateSummary> GetNapsTodayStateSummary()
        {
            List<NapsStateSummary> napsStateSummaries = new List<NapsStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_code, trans_volume, code_description, " +
            "case when trans_code<>'00' then 'Failed' when trans_code is null then 'Pending'  " +
            "else 'Success' end state_ind, convert(varchar,spooled_time,120) transdate from naps  " +
            "where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) " +
            "and convert(varchar,spooled_time,112)=convert(varchar, getdate(), 112) " +
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
                        var transactionSummary = new NapsStateSummary();

                        transactionSummary.TransactionStateCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionStateDesc = rdr["code_description"].ToString();
                        transactionSummary.TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString());
                        if (!rdr["trans_volume"].GetType().Name.Equals("DBNull"))
                        {
                            transactionSummary.TotalTransactionVolume = Convert.ToDecimal(rdr["trans_volume"].ToString());
                        }
                        else
                        {
                            transactionSummary.TotalTransactionVolume = 0;
                        }
                        transactionSummary.TransactionStateInd = rdr["state_ind"].ToString();
                        
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        napsStateSummaries.Add(transactionSummary);
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

            return napsStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM naps ORDER BY ID DESC ";
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

        private IEnumerable<NapsDetail> GetNapsDetails()
        {

            string sqlSelect = "select flexcode, flexmsg, " +
                               "count(*) trans_count, sum(Amount) volume from NAPS_FundTransfer " +
                               "where cast(TransactionDate as date) = cast(GETDATE() as date) " +
                               "group by flexcode, flexmsg";

            List<NapsDetail> napsDetails = new List<NapsDetail>();
            using (SqlConnection con = new SqlConnection(_napsIncoming))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NapsDetail napsDetail = new NapsDetail();
                        if (!rdr["flexcode"].GetType().Name.Equals("DBNull"))
                            napsDetail.StateCode = rdr["flexcode"].ToString();
                        else
                        {
                            napsDetail.StateCode = null;
                        }
                        napsDetail.CodeDescription = rdr["flexmsg"].ToString();
                        napsDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        napsDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        //napsDetail.StateCode = rdr["flexcode"].ToString();
                        napsDetail.SpooledTime = spoolDateTime;
                        napsDetails.Add(napsDetail);
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
            return napsDetails;
        }

        public Boolean StageNapsData()
        {
            IEnumerable<NapsDetail> napsDetails = GetNapsDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var napsDetail in napsDetails)
                    {
                        const string insQuery = "BEGIN Insert Into naps (code_description,trans_code,trans_count,trans_volume,spooled_time)" +
                                                "  values (@codedescription,@transcode,@transcount,@transvolume,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = napsDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = napsDetail.CodeDescription;
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = napsDetail.StateCode;
                        if (napsDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", napsDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@transvolume", SqlDbType.NVarChar).Value = napsDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = napsDetail.SpooledTime;

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