using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class RemitaUtil
    {
        private readonly string _remita;
        private readonly string _sqlCs;

        public RemitaUtil()
        {
            _remita = ConfigurationManager.ConnectionStrings["RemitaCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        
        public List<RemitaStateSummary> GetRemitaTodayStateSummary()
        {
            List<RemitaStateSummary> remitaStateSummaries = new List<RemitaStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select a.trans_count transaction_count, a.trans_code, a.trans_volume, b.description, " +
                               "case when a.trans_code = '00' then 'Successful' else 'Failed' end state_ind, " +
                               "convert(varchar,a.spooled_time,120) transdate from remita a, fep_response_code b  " +
                               "where a.trans_code = b.code and  " +
                               "convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled +
                               "' as datetime),120)  " +
                               "group by a.trans_count, a.trans_code, a.trans_volume, b.description,convert(varchar,spooled_time,120) order by state_ind desc";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new RemitaStateSummary();

                        transactionSummary.TransactionStateCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionStateDesc = rdr["description"].ToString();
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

                        remitaStateSummaries.Add(transactionSummary);
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

            return remitaStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM remita ORDER BY ID DESC ";
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

        private IEnumerable<RemitaDetail> GetRemitaDetails()
        {
            string sqlSelect = "select responsecode Response, count(responsecode) as CountofTxn, sum(AMOUNT) as volume " +
                               "from STP_REQUEST_DETAILS_UPLOAD " +
                               "where CONVERT(VARCHAR(10),TRANSDATE, 110) = CONVERT(VARCHAR(10),getdate(), 105) " +
                               "group by responsecode";

            List<RemitaDetail> remitaDetails = new List<RemitaDetail>();
            using (SqlConnection con = new SqlConnection(_remita))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        RemitaDetail remitaDetail = new RemitaDetail();
                        if (!rdr["Response"].GetType().Name.Equals("DBNull"))
                            remitaDetail.StateCode = rdr["Response"].ToString();
                        else
                        {
                            remitaDetail.StateCode = null;
                        }
                        remitaDetail.TransactionCount = Convert.ToInt32(rdr["CountofTxn"].ToString());
                        remitaDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        //napsDetail.StateCode = rdr["flexcode"].ToString();
                        remitaDetail.SpooledTime = spoolDateTime;
                        remitaDetails.Add(remitaDetail);
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
            return remitaDetails;
        }

        public Boolean StageRemitaData()
        {
            IEnumerable<RemitaDetail> remitaDetails = GetRemitaDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var remitaDetail in remitaDetails)
                    {
                        const string insQuery = "BEGIN Insert Into remita (trans_code,trans_count,spooled_time,trans_volume)" +
                                                "  values (@transcode,@transcount,@spooledtime,@transvolume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = remitaDetail.TransactionCount;
                        if (remitaDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", remitaDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@transvolume", SqlDbType.NVarChar).Value = remitaDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = remitaDetail.SpooledTime;

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