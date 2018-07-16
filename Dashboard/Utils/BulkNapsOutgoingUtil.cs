using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class BulkNapsOutgoingUtil
    {
        private readonly string _napsBkOutgoing;
        private readonly string _sqlCs;

        public BulkNapsOutgoingUtil()
        {
            _napsBkOutgoing = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<BulkNapsOutgoingSummary> GetBulkNapsOutgoingTodayPerformance()
        {
            List<BulkNapsOutgoingSummary> bulkNapsOutgoingSummaries = new List<BulkNapsOutgoingSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when trans_code='00' then trans_count else 0 end) success_status, " +
                "sum(case when trans_code is null then trans_count else 0 end) pending_status, " +
                "sum(case when trans_code<>'00' or trans_code='' then trans_count else 0 end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from bk_naps_outgoing where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new BulkNapsOutgoingSummary()
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

                        bulkNapsOutgoingSummaries.Add(transactionSummary);
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

            return bulkNapsOutgoingSummaries;
        }



        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM bk_naps_outgoing ORDER BY ID DESC ";
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

        public List<BulkNapsOutgoingStateSummary> GetBulkNapsOutgoingTodayStateSummary()
        {
            List<BulkNapsOutgoingStateSummary> bulkNapsOutgoingStateSummaries = new List<BulkNapsOutgoingStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_code, trans_volume, code_description, " +
            "case when trans_code<>'00' then 'Failed' when trans_code is null then 'Pending'  " +
            "else 'Success' end state_ind, convert(varchar,spooled_time,120) transdate from bk_naps_outgoing  " +
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
                        var transactionSummary = new BulkNapsOutgoingStateSummary();

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

                        bulkNapsOutgoingStateSummaries.Add(transactionSummary);
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

            return bulkNapsOutgoingStateSummaries;
        }

        private IEnumerable<BulkNapsOutgoingDetail> GetBulkNapsOutgoingDetails()
        {

            const string sqlSelect = "select count(*) total, SUM(enteredamount) amount, trn_debit_status, " +
                                     "case  " +
                                     "when trn_debit_status = '00' then 'Success' " +
                                     "when trn_debit_status = '10' then 'Insufficient Balance' " +
                                     "when trn_debit_status = '13' then 'Invalid Account' " +
                                     "when trn_debit_status = '98' then 'No Response From Flexcube' " +
                                     "when trn_debit_status = '' then 'Pending' " +
                                     "when trn_debit_status IS NULL then 'Pending' " +
                                     "end description " +
                                     "from bulk_napsheader  " +
                                     "where " +
                                     "cast(date_authorised as date) = CAST(getdate() as date) " +
                                     "group by trn_debit_status";

            List<BulkNapsOutgoingDetail> bulkNapsOutgoingDetails = new List<BulkNapsOutgoingDetail>();
            using (SqlConnection con = new SqlConnection(_napsBkOutgoing))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        BulkNapsOutgoingDetail bulkNapsOutgoingDetail = new BulkNapsOutgoingDetail();
                        if (!rdr["trn_debit_status"].GetType().Name.Equals("DBNull"))
                            bulkNapsOutgoingDetail.StateCode = rdr["trn_debit_status"].ToString();
                        else
                        {
                            bulkNapsOutgoingDetail.StateCode = null;
                        }
                        bulkNapsOutgoingDetail.CodeDescription = rdr["description"].ToString();
                        bulkNapsOutgoingDetail.TransactionCount = Convert.ToInt32(rdr["total"].ToString());
                        bulkNapsOutgoingDetail.TransactionVolume = Convert.ToDecimal(rdr["amount"].ToString());
                        bulkNapsOutgoingDetail.SpooledTime = spoolDateTime;
                        bulkNapsOutgoingDetails.Add(bulkNapsOutgoingDetail);
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
            return bulkNapsOutgoingDetails;
        }

        public Boolean StageBulkNapsOutgoingData()
        {
            IEnumerable<BulkNapsOutgoingDetail> bulkNapsOutgoingDetails = GetBulkNapsOutgoingDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var bulkNapsOutgoingDetail in bulkNapsOutgoingDetails)
                    {
                        const string insQuery =
                            "BEGIN Insert Into bk_naps_outgoing (code_description,trans_code,trans_count,trans_volume,spooled_time)" +
                            "  values (@codedescription,@transcode,@transcount,@transvolume,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = bulkNapsOutgoingDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value =
                            bulkNapsOutgoingDetail.CodeDescription;
                        if (bulkNapsOutgoingDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", bulkNapsOutgoingDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@transvolume", SqlDbType.NVarChar).Value =
                            bulkNapsOutgoingDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = bulkNapsOutgoingDetail.SpooledTime;

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