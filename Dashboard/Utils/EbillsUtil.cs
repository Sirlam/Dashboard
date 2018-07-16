using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class EbillsUtil
    {
        private readonly string _nipOutgoing;
        private readonly string _sqlCs;

        public EbillsUtil()
        {
            _nipOutgoing = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<EbillsSummary> GetEbiilsTodayPerformance()
        {
            List<EbillsSummary> ebillsSummaries = new List<EbillsSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
            "sum(case when trans_code='00' then trans_count else 0 end) success_status,  " +
            "sum(case when code_description in ('PROCESSING', ''UNAUTHORISED) then trans_count else 0 end) pending_status, " +
            "sum(case when code_description='Transaction Failed and Reversed - 00' or code_description='Transaction Failed and Reversed - null' then trans_count else 0 end) reversed_status, " +
            "sum(case when trans_code<>'00' and code_description != 'Transaction Failed and Reversed - 00' and code_description!='Transaction Failed and Reversed - null' or code_description = 'CANCELLED' then trans_count else 0 end) failure_status,  " +
            "convert(varchar,spooled_time,120) transdate from ebills  " +
            "where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new EbillsSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalReversedTransaction = Convert.ToInt32(rdr["reversed_status"].ToString()),
                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        ebillsSummaries.Add(transactionSummary);
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

            return ebillsSummaries;
        }
        public List<EbillsStateSummary> GetEbillsTodayStateSummary()
        {
            List<EbillsStateSummary> ebillsStateSummaries = new List<EbillsStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_code, trans_volume, code_description, " +
            "case when trans_code<>'00' and code_description != 'Transaction Failed and Reversed - 00' and code_description != 'Transaction Failed and Reversed - null' or code_description =  'CANCELLED' then 'Failed' when code_description in ('processing','UNAUTHORISED') then 'Pending'   " +
            "when code_description = 'Transaction Failed and Reversed - 00' or code_description = 'Transaction Failed and Reversed - null' then 'Reversal' " +
            "else 'Success' end state_ind, convert(varchar,spooled_time,120) transdate from ebills   " +
            "where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)  " +
            "and convert(varchar,spooled_time,112)=convert(varchar, getdate(), 112)  " +
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
                        var transactionSummary = new EbillsStateSummary();

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

                        ebillsStateSummaries.Add(transactionSummary);
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

            return ebillsStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM ebills ORDER BY ID DESC ";
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

        private IEnumerable<EbillsDetail> GetEbillsDetails()
        {

            string sqlSelect = "select responsecode, txnstatus, count(*) trans_count, sum(Amount) volume " +
            "from eBillspayTransactions where " +
            "cast(dateadded as date) = cast(GETDATE() as date) " +
            "group by responsecode, txnstatus";

            List<EbillsDetail> ebillsDetails = new List<EbillsDetail>();
            using (SqlConnection con = new SqlConnection(_nipOutgoing))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        EbillsDetail ebillsDetail = new EbillsDetail();
                        if (!rdr["responsecode"].GetType().Name.Equals("DBNull"))
                            ebillsDetail.StateCode = rdr["responsecode"].ToString();
                        else
                        {
                            ebillsDetail.StateCode = null;
                        }
                        ebillsDetail.CodeDescription = rdr["txnstatus"].ToString();
                        ebillsDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        ebillsDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        //napsDetail.StateCode = rdr["flexcode"].ToString();
                        ebillsDetail.SpooledTime = spoolDateTime;
                        ebillsDetails.Add(ebillsDetail);
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
            return ebillsDetails;
        }

        public Boolean StageEbillsData()
        {
            IEnumerable<EbillsDetail> ebillsDetails = GetEbillsDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var ebillsDetail in ebillsDetails)
                    {
                        const string insQuery = "BEGIN Insert Into ebills (code_description,trans_code,trans_count,trans_volume,spooled_time)" +
                                                "  values (@codedescription,@transcode,@transcount,@transvolume,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = ebillsDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = ebillsDetail.CodeDescription;
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = napsDetail.StateCode;
                        if (ebillsDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", ebillsDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@transvolume", SqlDbType.NVarChar).Value = ebillsDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = ebillsDetail.SpooledTime;

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