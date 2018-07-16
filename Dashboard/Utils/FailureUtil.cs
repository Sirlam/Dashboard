using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class FailureUtil
    {
        private readonly string _sqlCs;

        public FailureUtil()
        {
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<RibFailureTrend> GetMonthlyRibFailureCummTrend()
        {
            List<RibFailureTrend> productTrends = new List<RibFailureTrend>();
            string sqlSelect =
                "select sum( case when trans_code not in ('10','00','13') then trans_count else 0 end) failure_count, " +
                "sum( case when trans_code = '21' then trans_count else 0 end) failure_count_21,  " +
                "sum( case when trans_code = '98' then trans_count else 0 end) failure_count_98,  " +
                "sum( case when trans_code = '99' then trans_count else 0 end) failure_count_99,  " +
                "sum( case when code_description = 'NEW' then trans_count else 0 end) new_failure_count,  " +
                "convert(varchar(7),spooled_time,120) transdate from rib a " +
                "where convert(varchar,spooled_time,120) = convert(varchar,cast((SELECT TOP 1 spooled_time FROM rib " +
                "where convert(varchar,spooled_time,112)=convert(varchar, a.spooled_time, 112)  ORDER BY ID DESC) as datetime),120) " +
                "and convert(varchar,spooled_time,112) >= convert(varchar,getdate()-30,112) " +
                "group by convert(varchar(7),spooled_time,120)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new RibFailureTrend()
                        {
                            DateTime = rdr["transdate"].ToString(),

                            Error21Count = Convert.ToInt32(rdr["failure_count_21"].ToString()),
                            Error98Count = Convert.ToInt32(rdr["failure_count_98"].ToString()),
                            Error99Count = Convert.ToInt32(rdr["failure_count_99"].ToString()),
                            ErrorNewCount = Convert.ToInt32(rdr["new_failure_count"].ToString()),
                            FailureCount = Convert.ToInt32(rdr["failure_count"].ToString())
                        };

                        productTrends.Add(transactionSummary);
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

            return productTrends;
        }
    }
}