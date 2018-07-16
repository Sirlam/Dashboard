using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class NapsOutgoingUtil
    {
        private readonly string _napsOutgoing;
        private readonly string _sqlCs;

        public NapsOutgoingUtil()
        {
            _napsOutgoing = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<NapsOutgoingSummary> GetNapsOutgoingTodayPerformance()
        {
            List<NapsOutgoingSummary> napsOutgoingSummaries = new List<NapsOutgoingSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code<>'00' or trans_code='' then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from naps_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NapsOutgoingSummary
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

                        napsOutgoingSummaries.Add(transactionSummary);
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

            return napsOutgoingSummaries;
        }

        public List<NapsOutgoingStateSummary> GetNapsOutgoingTodayStateSummary()
        {
            List<NapsOutgoingStateSummary> napsOutgoingStateSummaries = new List<NapsOutgoingStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_code, trans_volume, code_description, " +
            "case when trans_code<>'00' then 'Failed' when trans_code is null then 'Pending'  " +
            "else 'Success' end state_ind, convert(varchar,spooled_time,120) transdate from naps_outgoing  " +
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
                        var transactionSummary = new NapsOutgoingStateSummary();

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

                        napsOutgoingStateSummaries.Add(transactionSummary);
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

            return napsOutgoingStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM naps_outgoing ORDER BY ID DESC ";
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

        private IEnumerable<NapsOutgoingDetail> GetNapsOutgoingDetails()
        {

            const string sqlSelect = "select count(*) total, SUM(BEN_AMOUNT) amount, flexresponsecode, " +
                                     "case  " +
                                     "when flexresponsecode = '00' then 'Success' " +
                                     "when flexresponsecode = '10' then 'Insufficient Balance' " +
                                     "when flexresponsecode = '13' then 'Invalid Account' " +
                                     "when flexresponsecode = '98' then 'No Response From Flexcube' " +
                                     "when flexresponsecode = '' then 'Pending' " +
                                     "when flexresponsecode IS NULL then 'Pending' " +
                                     "end description " +
                                     "from NAPSOUTPAYMENTREQUEST  " +
                                     "where " +
                                     "cast(checkerdate as date) = CAST(getdate() as date) " +
                                     "group by flexresponsecode";

            List<NapsOutgoingDetail> napsOutgoingDetails = new List<NapsOutgoingDetail>();
            using (SqlConnection con = new SqlConnection(_napsOutgoing))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NapsOutgoingDetail napsOutgoingDetail = new NapsOutgoingDetail();
                        if (!rdr["flexresponsecode"].GetType().Name.Equals("DBNull"))
                            napsOutgoingDetail.StateCode = rdr["flexresponsecode"].ToString();
                        else
                        {
                            napsOutgoingDetail.StateCode = null;
                        }
                        napsOutgoingDetail.CodeDescription = rdr["description"].ToString();
                        napsOutgoingDetail.TransactionCount = Convert.ToInt32(rdr["total"].ToString());
                        napsOutgoingDetail.TransactionVolume = Convert.ToDecimal(rdr["amount"].ToString());
                        napsOutgoingDetail.SpooledTime = spoolDateTime;
                        napsOutgoingDetails.Add(napsOutgoingDetail);
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
            return napsOutgoingDetails;
        }

        public Boolean StageNapsOutgoingData()
        {
            IEnumerable<NapsOutgoingDetail> napsOutgoingDetails = GetNapsOutgoingDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var napsOutgoingDetail in napsOutgoingDetails)
                    {
                        const string insQuery =
                            "BEGIN Insert Into naps_outgoing (code_description,trans_code,trans_count,trans_volume,spooled_time)" +
                            "  values (@codedescription,@transcode,@transcount,@transvolume,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = napsOutgoingDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value =
                            napsOutgoingDetail.CodeDescription;
                        if (napsOutgoingDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", napsOutgoingDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@transvolume", SqlDbType.NVarChar).Value =
                            napsOutgoingDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = napsOutgoingDetail.SpooledTime;

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