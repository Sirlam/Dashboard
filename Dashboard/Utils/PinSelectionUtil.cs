using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class PinSelectionUtil
    {
        private readonly string _fepCs;
        private readonly string _sqlCs;

        public PinSelectionUtil()
        {
            _fepCs = ConfigurationManager.ConnectionStrings["FepCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<PinSelectionStateSummary> GetPinSelectionTodayStateSummary()
        {
            List<PinSelectionStateSummary> pinSelectionStateSummaries = new List<PinSelectionStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select 'Successful' state_ind, sum(trans_count) tran_count,convert(varchar,spooled_time,120) transdate " +
                "from pin_selection " +
                "where convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)  " +
                "group by state_ind, convert(varchar,spooled_time,120) order by state_ind desc";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new PinSelectionStateSummary();

                        transactionSummary.TotalTransaction = Convert.ToInt32(rdr["tran_count"].ToString());
                        transactionSummary.TransactionStateInd = rdr["state_ind"].ToString();

                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        pinSelectionStateSummaries.Add(transactionSummary);
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
            return pinSelectionStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM pin_selection ORDER BY ID DESC ";
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

        public IEnumerable<PinSelectionDetail> GetPinSelectionDetails()
        {
            string sqlSelect = "select COUNT(distinct (a.retrieval_reference_nr)) tran_count " +
                "from post_tran a, post_tran_cust b " +
                "where a.POST_TRAN_CUST_ID = b.POST_TRAN_CUST_ID " +
                "and a.tran_type='92' " +
                "and b.terminal_id like'2ENG%' " +
                "and a.message_type='0600' " +
                "and (convert(DATE, a.DATETIME_REQ) = CONVERT(DATE, GETDATE()))";

            List<PinSelectionDetail> pinSelectionDetails = new List<PinSelectionDetail>();

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
                        PinSelectionDetail pinSelectionDetail = new PinSelectionDetail();
                        pinSelectionDetail.TransactionCount = Convert.ToInt32(rdr["tran_count"].ToString());
                        pinSelectionDetail.SpooledTime = DateTime.Now;
                        pinSelectionDetails.Add(pinSelectionDetail);
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
            return pinSelectionDetails;
        }

        public int StagePinSelectionData()
        {
            IEnumerable<PinSelectionDetail> pinSelectionDetails = GetPinSelectionDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var pinSelectionDetail in pinSelectionDetails)
                    {
                        const string insQuery =
                            "BEGIN Insert Into pin_selection (trans_count,spooled_time)" +
                            "  values (@tran_count,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@tran_count", SqlDbType.Int).Value = pinSelectionDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = pinSelectionDetail.SpooledTime;

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