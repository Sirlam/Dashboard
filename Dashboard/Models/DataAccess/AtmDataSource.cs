using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Dashboard.Models.Entities;
using Microsoft.Ajax.Utilities;

namespace Dashboard.Models.DataAccess
{
    public class AtmDataSource
    {
        private readonly string _sqlCs;

        public AtmDataSource()
        {
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<AtmPerformanceDetail> GetAtmClassSummayList()
        {
            List<AtmPerformanceDetail> performanceDetails = new List<AtmPerformanceDetail>();
            string sqlSelect = "select atm_class,in_service,out_of_service,hard_faults,supply_out,cash_out,comms, " +
                               "closed_mode,replenishment,report_date from atm_perform where location='SUMMARY' and  " +
                               "convert(varchar,report_date,103)=convert(varchar, " +
                               "cast((SELECT TOP 1 report_date FROM atm_perform ORDER BY report_date DESC) as datetime),103)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new AtmPerformanceDetail
                        {
                            AtmClass = rdr["atm_class"].ToString(),
                            CashOut = Convert.ToDouble(rdr["cash_out"].ToString()),
                            ClosedMode = Convert.ToDouble(rdr["closed_mode"].ToString()),
                            Comms = Convert.ToDouble(rdr["comms"].ToString()),
                            HardFaults = Convert.ToDouble(rdr["hard_faults"].ToString()),
                            InService = Convert.ToDouble(rdr["in_service"].ToString()),
                            OutOfService = Convert.ToDouble(rdr["out_of_service"].ToString()),
                            Replenishment = Convert.ToDouble(rdr["replenishment"].ToString()),
                            SupplyOut = Convert.ToDouble(rdr["supply_out"].ToString()),
                            ReportDate = Convert.ToDateTime(rdr["report_date"].ToString()),
                        };

                        performanceDetails.Add(transactionSummary);
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

            return performanceDetails;
        }
        public List<AtmPerformanceDetail> GetDetail(string atmclass)
        {
            List<AtmPerformanceDetail> performanceDetails = new List<AtmPerformanceDetail>();
            string cond = "";
            if (!atmclass.IsNullOrWhiteSpace())
            {
                cond = " atm_class='" + atmclass + "' and ";
            }
            string sqlSelect =
                "select branch_code,terminal_id,atm_class,location,in_service,out_of_service,hard_faults,supply_out,cash_out,comms, " +
                "closed_mode,replenishment,report_date from atm_perform where " + cond + " " +
                "convert(varchar,report_date,103)=convert(varchar, " +
                "cast((SELECT TOP 1 report_date FROM atm_perform ORDER BY report_date DESC) as datetime),103) ";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new AtmPerformanceDetail
                        {
                            AtmClass = rdr["atm_class"].ToString(),
                            BranchCode = rdr["branch_code"].ToString(),
                            TerminalId = rdr["terminal_id"].ToString(),
                            Location = rdr["location"].ToString(),
                            CashOut = Convert.ToDouble(rdr["cash_out"].ToString()),
                            ClosedMode = Convert.ToDouble(rdr["closed_mode"].ToString()),
                            Comms = Convert.ToDouble(rdr["comms"].ToString()),
                            HardFaults = Convert.ToDouble(rdr["hard_faults"].ToString()),
                            InService = Convert.ToDouble(rdr["in_service"].ToString()),
                            OutOfService = Convert.ToDouble(rdr["out_of_service"].ToString()),
                            Replenishment = Convert.ToDouble(rdr["replenishment"].ToString()),
                            SupplyOut = Convert.ToDouble(rdr["supply_out"].ToString()),
                            ReportDate = Convert.ToDateTime(rdr["report_date"].ToString()),
                        };

                        performanceDetails.Add(transactionSummary);
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

            return performanceDetails;
        }
        public List<SummaryTrend> GetAtmClassSummaryTrend()
        {
            List<SummaryTrend> summaryTrends = new List<SummaryTrend>();
            const string sqlSelect = "select max(case when atm_class='Gold' then in_service else 0 end) Gold_count, " +
                                     "max(case when atm_class='Silver' then in_service else 0 end) silver_count, " +
                                     "max(case when atm_class='Bronze' then in_service else 0 end) bronze_count, " +
                                     "left(convert(varchar,report_date,120),16) transdate from atm_perform " +
                                     "where location='SUMMARY' and convert(varchar,report_date,112)>=convert(varchar,getdate()-30,112) " +
                                     "group by left(convert(varchar,report_date,120),16) " +
                                     "order by transdate ";
            using (var con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new SummaryTrend
                        {
                            GoldCount = Convert.ToDouble(rdr["Gold_count"].ToString()),
                            SilverCount = Convert.ToDouble(rdr["silver_count"].ToString()),
                            BronzeCount = Convert.ToDouble(rdr["bronze_count"].ToString()),
                            DateTime = rdr["transdate"].ToString()
                        };

                        summaryTrends.Add(transactionSummary);
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

            return summaryTrends;
        }
    }
}