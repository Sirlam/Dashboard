using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class FlexBranchUtil
    {
        private readonly string _flexBranchCS;
        private readonly string _sqlCs;

        public FlexBranchUtil()
        {
            _flexBranchCS = ConfigurationManager.ConnectionStrings["FlexBranchCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public List<FlexBranchSummary> GetFlexBranchTodayPerformance()
        {
            List<FlexBranchSummary> flexBranchSummaries = new List<FlexBranchSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count, " +
                "convert(varchar,spooled_time,120) transdate from flexbranch where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FlexBranchSummary
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32("0"),
                            TotalReversedTransaction = Convert.ToInt32("0"),
                            TotalPendingTransaction = Convert.ToInt32("0")
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        flexBranchSummaries.Add(transactionSummary);
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

            return flexBranchSummaries;
        }
        public List<FlexBranchStateSummary> GetFlexBranchTodayStateSummary()
        {
            List<FlexBranchStateSummary> flexBranchStateSummaries = new List<FlexBranchStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select branch_code, branch_name, trans_count, trans_module, convert(varchar,spooled_time,120) transdate from flexbranch " +
                "where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) " +
                "and convert(varchar,spooled_time,112)=convert(varchar, getdate(), 112) ";
            
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new FlexBranchStateSummary
                        {
                            BranchCode = rdr["branch_code"].ToString(),
                            BranchName = rdr["branch_name"].ToString(),
                            TransactionModule = rdr["trans_module"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["trans_count"].ToString()),
                        };
                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());

                        transactionSummary.TransactionDateTime = datetime;

                        flexBranchStateSummaries.Add(transactionSummary);
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

            return flexBranchStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM flexbranch ORDER BY ID DESC ";
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

        private IEnumerable<FLexBranchDetail> GetFLexBranchDetails()
        {

            string sqlSelect = "select substr(trn_ref_no,1,3)txn_branch, bouser.get_branch_name (substr(trn_ref_no,1,3)) branch_name, " +
                "module,count(distinct trn_ref_no) txn_count " +
                "from fccngn.actb_daily_log where user_id not in ('S','SYSTEM') and module='RT' " +
                "group by substr(trn_ref_no,1,3),module " +
                "order by txn_branch";

            List<FLexBranchDetail> fLexBranchDetails = new List<FLexBranchDetail>();
            using (OracleConnection con = new OracleConnection(_flexBranchCS))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        FLexBranchDetail fLexBranchDetail = new FLexBranchDetail();
                        fLexBranchDetail.BranchCode = rdr["txn_branch"].ToString();
                        fLexBranchDetail.TransactionCount = Convert.ToInt32(rdr["txn_count"].ToString());
                        fLexBranchDetail.BranchName = rdr["branch_name"].ToString();
                        fLexBranchDetail.TransactionModule = rdr["module"].ToString();
                        fLexBranchDetail.SpooledTime = DateTime.Now;
                        fLexBranchDetails.Add(fLexBranchDetail);
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
            return fLexBranchDetails;
        }

        public List<AtmTransactionsLog> GetAtmTransactionsLogs()
        {
            string sqlSelect = "select count(*)trans_count from fccngn.iftb_atm_txn_log";

            List<AtmTransactionsLog> atmTransactionsLogs = new List<AtmTransactionsLog>();
            using (OracleConnection con = new OracleConnection(_flexBranchCS))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        AtmTransactionsLog atmTransactionsLog = new AtmTransactionsLog();
                        atmTransactionsLog.TransactionCount = Convert.ToInt64(rdr["trans_count"].ToString());
                        atmTransactionsLog.SpooledTime = spoolDateTime;
                        atmTransactionsLogs.Add(atmTransactionsLog);
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
            return atmTransactionsLogs;
        } 

        public Boolean StageFLexBranchData()
        {
            if (!SummaryUtil.IsWorkingTime()) return false;
            IEnumerable<FLexBranchDetail> fLexBranchDetails = GetFLexBranchDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var fLexBranchDetail in fLexBranchDetails)
                    {
                        const string insQuery = "BEGIN Insert Into flexbranch (branch_code,branch_name,trans_count,spooled_time,trans_module)" +
                                                "  values (@branch_code,@branch_name,@trans_count,@spooled_time,@trans_module); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@branch_code", SqlDbType.NVarChar).Value = fLexBranchDetail.BranchCode;
                        cmd.Parameters.AddWithValue("@branch_name", SqlDbType.NVarChar).Value = fLexBranchDetail.BranchName;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = fLexBranchDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_module", SqlDbType.NVarChar).Value = fLexBranchDetail.TransactionModule;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = fLexBranchDetail.SpooledTime;

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

        public Boolean StageAtmTransactionsData()
        {
            IEnumerable<AtmTransactionsLog> atmTransactionsLogs = GetAtmTransactionsLogs();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var atmTransactionsLog in atmTransactionsLogs)
                    {
                        const string insQuery = "BEGIN Insert Into atm_trans_log (trans_count,spooled_time)" +
                                                "  values (@trans_count,@spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.NVarChar).Value = atmTransactionsLog.TransactionCount;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = atmTransactionsLog.SpooledTime;

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