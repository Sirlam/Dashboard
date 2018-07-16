using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class AnalysisUtil
    {
        private readonly string _sqlCs;

        public AnalysisUtil()
        {
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<AnalysisModel> GetMobileAnalysisModels(String periodType, DateTime startDate, DateTime endDate)
        {
            string sqlSelect = "";
            if (periodType.ToUpper() == "WEEKLY")
            {
                sqlSelect = "select trans_code, trans_count, trans_date from mobile_failure_trend where trans_date LIKE ('%2017-03-%') or trans_date LIKE ('%2017-04-%')";
            }
            else if (periodType.ToUpper() == "MONTHLY")
            {
                sqlSelect = "select trans_code, trans_count, trans_date from mobile_failure_trend where ";// where trans_date IN ('2016-10', '2016-11', '2016-12', '2017-01', '2017-02', '2017-03', '2017-04') ";
            }
            List<AnalysisModel> productTrends = new List<AnalysisModel>();

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
                        var transactionSummary = new AnalysisModel()
                        {
                            DateTime = rdr["trans_date"].ToString(),
                            Count = Convert.ToInt32(rdr["trans_count"].ToString()),
                            TransCode = rdr["trans_code"].ToString()//,
                            //CodeDescription = rdr["code_description"].ToString()
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
        public List<AnalysisModel> GetMonthlyAnalysisTrend(String days, String selectedApplication)
        {
            string sqlSelect = "";

            if (selectedApplication.ToUpper() == "RIB")
            {
                if (days.ToUpper() == "WEEKLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from rib_monthly_failure where trans_date LIKE ('%2017-08-%')";
                }
                else if (days.ToUpper() == "MONTHLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from rib_monthly_failure where trans_date IN ('2017-05', '2017-06', '2017-07', '2017-08', '2017-09', '2017-10', '2017-11') ";
                }
            }
            else if (selectedApplication.ToUpper() == "ATM" || selectedApplication.ToUpper() == "POS" || selectedApplication.ToUpper() == "WEB")
            {
                if (days.ToUpper() == "WEEKLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from fep_monthly_failure where channel_type = '" + selectedApplication + "' and trans_date LIKE ('%2017-08-%')";
                }
                else if (days.ToUpper() == "MONTHLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from fep_monthly_failure where channel_type = '" + selectedApplication + "' and trans_date IN ('2017-05', '2017-06', '2017-07', '2017-08', '2017-09', '2017-10', '2017-11') ";
                }
            }
            else if (selectedApplication == "MOBILE_USSD")
            {
                sqlSelect = "select " +
                        "case when substring(trans_code,1,28)='error processing transaction' then 'error processing transaction' else trans_code end trans_code, sum(trans_count) trans_count, convert(varchar(7),trans_date,120) trans_date  " +
                        "from mobile_failure_trend " +
                        "where channel_type='USSD' " +
                        "group by case when substring(trans_code,1,28)='error processing transaction' then 'error processing transaction' else trans_code end,convert(varchar(7),trans_date,120)";

            }
            else if (selectedApplication == "MOBILE_WEB")
            {
                sqlSelect = "select " +
                            "case when substring(trans_code,1,28)='error processing transaction' then 'error processing transaction' else trans_code end trans_code, sum(trans_count) trans_count, convert(varchar(7),trans_date,120) trans_date  " +
                            "from mobile_failure_trend " +
                            "where channel_type='WEB' " +
                            "group by case when substring(trans_code,1,28)='error processing transaction' then 'error processing transaction' else trans_code end,convert(varchar(7),trans_date,120)";

            }
            else if (selectedApplication == "fundTransferAccountToMobile" || selectedApplication == "fundTransferMobileToAccount" || selectedApplication == "fundTransferAccountToAccount" || selectedApplication == "QuickBalance" || selectedApplication == "QuickRecharge")
            {
                if (days.ToUpper() == "WEEKLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from mobile_monthly_failure where category = '" + selectedApplication + "' and trans_date LIKE ('%2017-03-%') or trans_date LIKE ('%2017-04-%')";
                }
                else if (days.ToUpper() == "MONTHLY")
                {
                    sqlSelect = "select trans_code, trans_count, trans_date from mobile_monthly_failure where category = '" + selectedApplication + "' and trans_date IN ('2016-10', '2016-11', '2016-12', '2017-01', '2017-02', '2017-03', '2017-04') ";
                }
            }
            List<AnalysisModel> productTrends = new List<AnalysisModel>();

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
                        var transactionSummary = new AnalysisModel()
                        {
                            DateTime = rdr["trans_date"].ToString(),
                            Count = Convert.ToInt32(rdr["trans_count"].ToString()),
                            TransCode = rdr["trans_code"].ToString()//,
                            //CodeDescription = rdr["code_description"].ToString()
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
public class AnalysisModel
{
    public string DateTime { get; set; }
    public decimal Count { get; set; }
    public string TransCode { get; set; }
    public string CodeDescription { get; set; }
}