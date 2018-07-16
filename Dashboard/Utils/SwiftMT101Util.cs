using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace Dashboard.Utils
{
    public class SwiftMT101Util
    {
        private readonly string _omniflowCs;
        private readonly string _sqlCs;

        public SwiftMT101Util()
        {
            _omniflowCs = ConfigurationManager.ConnectionStrings["OmniFlowCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        private IEnumerable<SwiftMt101Detail> GetSwiftMt101Details()
        {

            string sqlSelect = "select count(*) total, SUM(REGEXP_REPLACE(debit_amount,',','')) volume, curr_ws_name, " +
                "DECODE(curr_ws_name, " +
                "'Work Exit1', 'Successful', " +
                "'Out_Exception', 'Failed', " +
                "'CPC_Checker_Trade', 'Pending', " +
                "'CPC_Maker_Other', 'Pending', " +
                "'Discard1', 'Successful', " +
                "'Hold', 'Pending', " +
                "'Sorter', 'Pending', " +
                "'Exception_Handling', 'Failed', " +
                "'CPC_Maker_Trade', 'Pending', " +
                "'Repair', 'Pending', " +
                "'CPC_Checker_Payment', 'Pending', " +
                "'CPC_Maker_Payment', 'Pending', " +
                "'Book_Transaction', 'Pending', " +
                "'Pending' "+
                ")description " +
                "from ecobankng.eco_stp_exttable " +
                "where message_type ='101'and date_received = to_char(sysdate,'dd-Mon-yyyy') " +
                "group by curr_ws_name";


            List<SwiftMt101Detail> swiftMt101Details = new List<SwiftMt101Detail>();
            using (OracleConnection con = new OracleConnection(_omniflowCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    cmd.CommandTimeout = 0;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        SwiftMt101Detail swiftMt101Detail = new SwiftMt101Detail();
                        swiftMt101Detail.StateCode = rdr["curr_ws_name"].ToString();
                        swiftMt101Detail.Description = rdr["description"].ToString();
                        swiftMt101Detail.TransactionCount = Convert.ToInt32(rdr["total"].ToString());
                        if (!rdr["volume"].GetType().Name.Equals("DBNull"))
                        {
                            swiftMt101Detail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        }
                        else
                        {
                            swiftMt101Detail.TransactionVolume = 0;
                        }
                        swiftMt101Detail.SpooledTime = DateTime.Now;
                        swiftMt101Details.Add(swiftMt101Detail);
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
            return swiftMt101Details;
        }

        public List<SwiftMt101StateSummary> GetSwiftMt101TodayStateSummary()
        {
            List<SwiftMt101StateSummary> swiftMt101StateSummaries = new List<SwiftMt101StateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select trans_code,trans_description state_ind, sum(trans_count) trans_count,sum(trans_volume) trans_volume,convert(varchar,spooled_time,120) transdate " +
                "from swift_mt_101 " +
                "where " +
                "convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)   " +
                "group by trans_code,trans_description,convert(varchar,spooled_time,120) order by state_ind desc ";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new SwiftMt101StateSummary();

                        transactionSummary.StateCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionStateInd = rdr["state_ind"].ToString();
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

                        swiftMt101StateSummaries.Add(transactionSummary);
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

            return swiftMt101StateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM swift_mt_101 ORDER BY ID DESC ";
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

        public Boolean StageSwiftMt101Data()
        {
            IEnumerable<SwiftMt101Detail> swiftMt101Details = GetSwiftMt101Details();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var swiftMt101Detail in swiftMt101Details)
                    {
                        const string insQuery = "BEGIN insert into swift_mt_101 (trans_code,trans_count,trans_description,trans_volume,spooled_time)" +
                                                "  values (@trans_code,@trans_count,@trans_description,@trans_volume,@spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.NVarChar).Value = swiftMt101Detail.StateCode;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = swiftMt101Detail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_description", SqlDbType.NVarChar).Value = swiftMt101Detail.Description;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = swiftMt101Detail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.NVarChar).Value = swiftMt101Detail.SpooledTime;

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