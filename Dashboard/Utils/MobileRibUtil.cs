using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class MobileRibUtil
    {
        private readonly string _ribCS;
        private readonly string _sqlCs;

        public MobileRibUtil()
        {
            _ribCS = ConfigurationManager.ConnectionStrings["RibCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<MobileRibStateSummary> GetMobileRibTodayStateSummary()
        {
            List<MobileRibStateSummary> mobileRibStateSummaries = new List<MobileRibStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select token,username,device,date_generated,initial_payload, " +
                               "case when right(initial_payload, 2) = '00' then 'Success' " +
                               "when right(initial_payload, 2) = '01' then 'Failed' " +
                               "when right(initial_payload, 2) = '02' then 'Failed' "+
                               "when initial_payload = '' then 'Failed' " +
                               "else 'Success' end state_ind from dbo.mobile_rib_onboarding " +
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
                        var transactionSummary = new MobileRibStateSummary
                        {
                            Token = rdr["token"].ToString(),
                            Username = rdr["username"].ToString(),
                            Device = rdr["device"].ToString(),
                            DateGenerated = Convert.ToDateTime(rdr["date_generated"].ToString()),
                            InitialPayload = rdr["initial_payload"].ToString(),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };

                        mobileRibStateSummaries.Add(transactionSummary);
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

            return mobileRibStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM mobile_rib_onboarding ORDER BY ID DESC ";
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

        private IEnumerable<MobileRibDetail> GetMobileRibDetails()
        {

            string sqlSelect = "select token, username, device, dategenerated, dateexpiring, initialpayload from " +
                               "ribuser.mobileapi where to_char(dategenerated, 'dd-MON-yyyy') = " +
                               "to_char(sysdate-5, 'dd-MON-yyyy')";

            List<MobileRibDetail> mobileRibDetails = new List<MobileRibDetail>();
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
                        MobileRibDetail mobileRibDetail = new MobileRibDetail();
                        mobileRibDetail.Token = rdr["token"].ToString();
                        mobileRibDetail.Username = rdr["username"].ToString();
                        mobileRibDetail.Device = rdr["device"].ToString();
                        mobileRibDetail.DateGenerated = Convert.ToDateTime(rdr["dategenerated"].ToString());
                        mobileRibDetail.DateExpiring = Convert.ToDateTime(rdr["dateexpiring"].ToString());
                        mobileRibDetail.InitialPayload = rdr["initialpayload"].ToString();
                        mobileRibDetail.SpooledTime = spoolDateTime;
                        mobileRibDetails.Add(mobileRibDetail);
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
            return mobileRibDetails;
        }

        public Boolean StageMobileRibData()
        {
            IEnumerable<MobileRibDetail> mobileRibDetails = GetMobileRibDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var mobileRibDetail in mobileRibDetails)
                    {
                        const string insQuery = "BEGIN Insert Into mobile_rib_onboarding (token,username,device,date_generated,date_expiring,initial_payload,alert_registered,spooled_time)" +
                                                "  values (@token,@username,@device,@date_generated,@date_expiring,@initial_payload,@alert_registered,@spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@token", SqlDbType.NVarChar).Value = mobileRibDetail.Token;
                        cmd.Parameters.AddWithValue("@username", SqlDbType.NVarChar).Value = mobileRibDetail.Username;
                        cmd.Parameters.AddWithValue("@device", SqlDbType.NVarChar).Value = mobileRibDetail.Device;
                        cmd.Parameters.AddWithValue("@date_generated", SqlDbType.DateTime).Value = mobileRibDetail.DateGenerated;
                        cmd.Parameters.AddWithValue("@date_expiring", SqlDbType.DateTime).Value = mobileRibDetail.DateExpiring;
                        cmd.Parameters.AddWithValue("@initial_payload", SqlDbType.NVarChar).Value = mobileRibDetail.InitialPayload;
                        cmd.Parameters.AddWithValue("@alert_registered", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileRibDetail.SpooledTime;

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