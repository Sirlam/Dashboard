using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class QtInflowUtil
    {
        private readonly string _fepCs;
        private readonly string _sqlCs;

        public QtInflowUtil()
        {
            _fepCs = ConfigurationManager.ConnectionStrings["FepCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<QtInflowStateSummary> GetQtInflowTodayStateSummary()
        {
            List<QtInflowStateSummary> qtInflowStateSummaries = new List<QtInflowStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select rsp_code,description,case when rsp_code='00' then 'Successful' else 'Failed' end state_ind, sum(tran_count) tran_count,sum(tot_vol) tot_vol,convert(varchar,spooled_time,120) transdate " +
                "from qt_inflow,fep_response_code " +
                "where rsp_code=code " +
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
                        var transactionSummary = new QtInflowStateSummary();

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

                        qtInflowStateSummaries.Add(transactionSummary);
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

            return qtInflowStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM qt_inflow ORDER BY ID DESC ";
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

        public IEnumerable<QtInflowDetail> GetQtInflowDetails()
        {
            string sqlSelect =
                "SELECT b.rsp_code_rsp as rsp_code, count (distinct b.retrieval_reference_nr) as tran_count,sum(settle_amount_rsp)/100 tot_vol " +
                "FROM dbo.post_tran_cust a INNER JOIN " +
                "dbo.post_tran b  ON a.post_tran_cust_id = b.post_tran_cust_id " +
                "WHERE b.tran_completed = 1 " +
                "and b.tran_type ='50' " +
                "and b.sink_node_name ='ECOSIMsnk' " +
                "and message_type not in ('0420','0421','0220') " +
                "AND ( convert(varchar, b.datetime_tran_local, 112) = convert(varchar, getdate(), 112)) " +
                "and b.rsp_code_rsp in ('00','01','02','05','06','12','14','23','25','30','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','62','63','65','68','75','91','92','96','98') " +
                "group by b.rsp_code_rsp";

            List<QtInflowDetail> qtInflowDetails = new List<QtInflowDetail>();

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
                        QtInflowDetail qtInflowDetail = new QtInflowDetail();
                        qtInflowDetail.TransactionCount = Convert.ToInt32(rdr["tran_count"].ToString());
                        qtInflowDetail.TransactionVolume = Convert.ToDecimal(rdr["tot_vol"].ToString());
                        object j = rdr["rsp_code"].GetType().Name;
                        if (!rdr["rsp_code"].GetType().Name.Equals("DBNull"))
                            qtInflowDetail.TransactionResponseCode = rdr["rsp_code"].ToString();
                        else
                        {
                            qtInflowDetail.TransactionResponseCode = null;
                        }
                        qtInflowDetail.SpooledTime = DateTime.Now;
                        qtInflowDetails.Add(qtInflowDetail);
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
            return qtInflowDetails;
        }

        public int StageQtInflowData()
        {
            IEnumerable<QtInflowDetail> qtInflowDetails = GetQtInflowDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var qtInflowDetail in qtInflowDetails)
                    {
                        const string insQuery =
                            "BEGIN Insert Into qt_inflow (tran_count,rsp_code,spooled_time,tot_vol)" +
                            "  values (@tran_count,@rsp_code,@spooledtime,@tot_vol); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        if (qtInflowDetail.TransactionResponseCode != null)
                        {
                            cmd.Parameters.AddWithValue("@rsp_code", qtInflowDetail.TransactionResponseCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@rsp_code", DBNull.Value);
                        }
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = nipIncomingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@tran_count", SqlDbType.Int).Value = qtInflowDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@tot_vol", SqlDbType.Decimal).Value = qtInflowDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = qtInflowDetail.SpooledTime;

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