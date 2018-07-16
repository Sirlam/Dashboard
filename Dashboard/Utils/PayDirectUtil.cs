using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class PayDirectUtil
    {
        private readonly string _fepCs;
        private readonly string _sqlCs;

        public PayDirectUtil()
        {
            _fepCs = ConfigurationManager.ConnectionStrings["FepCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        private IEnumerable<PayDirectDetail> GetPayDirectDetails()
        {

            string sqlSelect =
                "SELECT b.rsp_code_rsp as rsp_code, count (distinct b.retrieval_reference_nr) as tran_count, sum(settle_amount_rsp)/100 tot_vol " +
                "FROM dbo.post_tran_cust a INNER JOIN " +
                "dbo.post_tran b  ON a.post_tran_cust_id = b.post_tran_cust_id " +
                "WHERE b.tran_type ='50' " +
                "and a.pan like '628051%' " +
                "and a.card_acceptor_id_code ='PAYDIRECT000001' " +
                "and message_type not in ('0420','0421','0220') " +
                "AND ( convert(varchar, b.datetime_tran_local, 112) = convert(varchar, getdate(), 112)) " +
                "and b.rsp_code_rsp in ('00','01','02','05','06','12','14','23','25','30','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','62','63','65','68','75','91','92','96','98') " +
                "group by b.rsp_code_rsp";


            List<PayDirectDetail> payDirectDetails = new List<PayDirectDetail>();
            using (SqlConnection con = new SqlConnection(_fepCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    cmd.CommandTimeout = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        PayDirectDetail payDirectDetail = new PayDirectDetail();
                        payDirectDetail.StateCode = rdr["rsp_code"].ToString();
                        payDirectDetail.TransactionCount = Convert.ToInt32(rdr["tran_count"].ToString());
                        payDirectDetail.TransactionVolume = Convert.ToDecimal(rdr["tot_vol"].ToString());
                        payDirectDetail.SpooledTime = DateTime.Now;
                        payDirectDetails.Add(payDirectDetail);
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
            return payDirectDetails;
        }

        public List<PayDirectStateSummary> GetPayDirectTodayStateSummary()
        {
            List<PayDirectStateSummary> payDirectStateSummaries = new List<PayDirectStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select trans_code,description,case when trans_code='00' then 'Successful' else 'Failed' end state_ind, sum(trans_count) trans_count,sum(trans_volume) trans_volume,convert(varchar,spooled_time,120) transdate " +
                "from paydirect,fep_response_code  " +
                "where trans_code=code  " +
                "and convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)   " +
                "group by trans_code,description,convert(varchar,spooled_time,120) order by state_ind desc ";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new PayDirectStateSummary();

                        transactionSummary.StateCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionStateInd = rdr["state_ind"].ToString();
                        transactionSummary.TransactionStateDesc = rdr["description"].ToString();
                        transactionSummary.TotalTransaction = Convert.ToInt32(rdr["trans_count"].ToString());
                        if (!rdr["trans_volume"].GetType().Name.Equals("DBNull"))
                        {
                            transactionSummary.TotalTransactionVolume = Convert.ToDecimal(rdr["trans_volume"].ToString());
                        }
                        else
                        {
                            transactionSummary.TotalTransactionVolume = 0;
                        }

                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        payDirectStateSummaries.Add(transactionSummary);
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

            return payDirectStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM paydirect ORDER BY ID DESC ";
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

        public Boolean StagePayDirectData()
        {
            IEnumerable<PayDirectDetail> payDirectDetails = GetPayDirectDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var payDirectDetail in payDirectDetails)
                    {
                        const string insQuery = "BEGIN insert into paydirect (trans_code,trans_count,trans_volume,spooled_time)" +
                                                "  values (@trans_code,@trans_count,@trans_volume,@spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.Int).Value = payDirectDetail.StateCode;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = payDirectDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = payDirectDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.NVarChar).Value = payDirectDetail.SpooledTime;

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