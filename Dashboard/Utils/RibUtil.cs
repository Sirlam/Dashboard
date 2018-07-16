using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class RibUtil
    {
        private readonly string _ribCS;
        private readonly string _sqlCs;

        public RibUtil()
        {
            _ribCS = ConfigurationManager.ConnectionStrings["RibCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<RibSummary> GetRibTodayPerformance()
        {
            List<RibSummary> ribSummaries = new List<RibSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "sum(case when code_description='SUCCESS' then trans_count else 0 end) success_status, " +
                "sum(case when code_description='REVERSAL' then trans_count else 0 end) reversed_status, " +
                "sum(case when code_description='Processing' then trans_count else 0 end) pending_status, " +
                "sum(case when code_description in ('REVERSAL','SUCCESS','Processing')  then 0 else trans_count end) failure_status, " +
                "convert(varchar,spooled_time,120) transdate from rib where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new RibSummary
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

                        ribSummaries.Add(transactionSummary);
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

            return ribSummaries;
        }
        public List<RibStateSummary> GetRibTodayStateSummary()
        {
            List<RibStateSummary> ribStateSummaries = new List<RibStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select distinct trans_count transaction_count,trans_code, code_description,description, trans_volume, " +
                "case when code_description!='SUCCESS' and  code_description!='REVERSAL' and code_description!='Processing' then 'Failed'  else code_description end state_ind,  " +
                "convert(varchar,spooled_time,120) transdate from rib full join neft_status on trans_code=code " +
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
                        var transactionSummary = new RibStateSummary
                        {
                            TransactionStateCode = rdr["trans_code"].ToString(),
                            TransactionStateDesc = rdr["code_description"].ToString(),
                            CodeDescription = rdr["description"].ToString(),
                            TotalVolume = Convert.ToDecimal(rdr["trans_volume"].ToString()),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        ribStateSummaries.Add(transactionSummary);
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

            return ribStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM rib ORDER BY ID DESC ";
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

        private IEnumerable<RibDetail> GetRibDetails()
        {

            string sqlSelect = " SELECT  CATEGORY,flexposting,tokenoption, count(*) trans_count,sum(amount) volume FROM ( " +
                            "SELECT TRANSACTIONSTATUS CODE,flexposting,flexpostingmsg,transactionstatusdetails,amount, CASE " +  
                            "WHEN transactionstatusdetails = 'Transaction Completed Successfully' THEN 'SUCCESS' "+
                            "WHEN FLEXREVERSAL is not null THEN 'REVERSAL' "+
                            "WHEN TRANSACTIONSTATUS = 'successful' AND FLEXREVERSAL is null THEN 'Reversal Failed' "+
                            "ELSE TRANSACTIONSTATUS END CATEGORY,tokenoption " +
                            "FROM RIBUSER.CUSTOMERTRANSFERS a,ribuser.customers b  WHERE  to_char(TRANSACTIONDATE,'dd-MON-yyyy') =to_char(sysdate,'dd-MON-yyyy')" +
                               "and a.customernumber=b.customernumber) " +
                            "GROUP BY CATEGORY,flexposting,tokenoption";

            List<RibDetail> ribDetails = new List<RibDetail>();
            using (OracleConnection con = new OracleConnection(_ribCS))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        RibDetail ribDetail = new RibDetail();
                        ribDetail.CodeDescription = rdr["CATEGORY"].ToString();
                        ribDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        ribDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        ribDetail.StateCode = rdr["flexposting"].ToString();
                        ribDetail.TokenOption = rdr["tokenoption"].ToString();
                        ribDetail.SpooledTime = spoolDateTime;
                        ribDetails.Add(ribDetail);
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
            return ribDetails;
        }

        public Boolean StageRibData()
        {
            IEnumerable<RibDetail> ribDetails = GetRibDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var ribDetail in ribDetails)
                    {
                        const string insQuery = "BEGIN Insert Into rib (code_description,trans_code,trans_count,tokentype,spooled_time,trans_volume)" +
                                                "  values (@codedescription,@transcode,@transcount,@tokentype,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = ribDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = ribDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = ribDetail.CodeDescription;
                        cmd.Parameters.AddWithValue("@tokentype", SqlDbType.NVarChar).Value = ribDetail.TokenOption;
                        cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = ribDetail.StateCode;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = ribDetail.SpooledTime;

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