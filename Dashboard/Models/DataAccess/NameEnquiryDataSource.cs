using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dashboard.Models.Entities;

namespace Dashboard.Models.DataAccess
{
    public class NameEnquiryDataSource
    {
        private readonly string _nipIncomingCs;
        private readonly string _sqlCs;

        public NameEnquiryDataSource()
        {
            _nipIncomingCs = ConfigurationManager.ConnectionStrings["NipIncomingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        private DateTime? GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM nameenquiry ORDER BY spooled_time DESC ";
            DateTime? lastTime = null;
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
        private List<NameEnquiry> GetNameEnquiryDetails()
        {
            string sqlSelect =
                "select count(*) request_count,case when responsedate is null then 'PENDING' " +
                "when ((responsedate- requestdate)*24*60*60)>10 then 'FAILED' " +
                "when ((responsedate- requestdate)*24*60*60)<=10 then 'SUCCESS' " +
                "end status from compass.fp_nameenquiry " +
                "where trunc(requestdate)=trunc(sysdate) " +
                "group by case when responsedate is null then 'PENDING' " +
                "when ((responsedate- requestdate)*24*60*60)>10 then 'FAILED' " +
                "when ((responsedate- requestdate)*24*60*60)<=10 then 'SUCCESS' " +
                " end";

            List<NameEnquiry> enquiries = new List<NameEnquiry>();
            using (OracleConnection con = new OracleConnection(_nipIncomingCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NameEnquiry enquiry = new NameEnquiry();
                        enquiry.RequestCount = Convert.ToInt32(rdr["request_count"].ToString());
                        enquiry.Category = rdr["status"].ToString();
                        enquiry.SpooledTime = spoolTime;
                        enquiries.Add(enquiry);
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
            return enquiries;
        }
        public Boolean StageNameEnquiryData()
        {

            List<NameEnquiry> enquiries = GetNameEnquiryDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (NameEnquiry enquiry in enquiries)
                    {

                        const string insQuery = "BEGIN Insert Into nameenquiry (category,request_count,spooled_time)" +
                                                "  values (@category,@request_count,@spooled_time); END;";
                    SqlCommand cmd = new SqlCommand(insQuery, con);

                    cmd.Parameters.AddWithValue("@category", SqlDbType.VarChar).Value = enquiry.Category;
                    cmd.Parameters.AddWithValue("@request_count", SqlDbType.Int).Value = enquiry.RequestCount;
                    cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = enquiry.SpooledTime;

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
                return rowAffected > 0;
            }
        }
        public List<NameEnquirySummary> GetTodayNameEnquiryTrend(DateTime deDate)
        {
            List<NameEnquirySummary> productTrends = new List<NameEnquirySummary>();
            string sqlSelect =
                "select sum(case when category='SUCCESS' then request_count else 0 end) success,sum(case when category='PENDING' then request_count else 0 end) pending," +
                "sum(case when category='FAILED' then request_count else 0 end) failed, left(convert(varchar,spooled_time,120),16) spooled_time from nameenquiry where  " +
                "convert(varchar,spooled_time,103)=convert(varchar,cast('" + deDate + "' as datetime),103)" +
                " group by left(convert(varchar,spooled_time,120),16) order by left(convert(varchar,spooled_time,120),16)";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NameEnquirySummary()
                        {
                            Time = rdr["spooled_time"].ToString().Substring(11),

                            Success = Convert.ToInt32(rdr["success"].ToString()),
                            Failed = Convert.ToInt32(rdr["failed"].ToString()),
                            Pending = Convert.ToInt32(rdr["pending"].ToString())
                        };

                        productTrends.Add(transactionSummary);
                    }
                    if (productTrends.Count > 0) productTrends.RemoveAt(0);
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