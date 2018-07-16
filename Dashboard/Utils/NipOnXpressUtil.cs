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
    public class NipOnXpressUtil
    {
        private readonly string _nipIncomingCs;
        private readonly string _sqlCs;

        public NipOnXpressUtil()
        {
            _nipIncomingCs = ConfigurationManager.ConnectionStrings["NipIncomingCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }

        public List<NipOnXpressStateSummary> GetNipOnXpressTodayStateSummary()
        {
            List<NipOnXpressStateSummary> nipOnXpressStateSummaries = new List<NipOnXpressStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect =
                "select trans_code,code_description,case when trans_code='0' then 'Successful' else 'Failed' end state_ind, sum(trans_count) tran_count,sum(trans_volume) tot_vol,convert(varchar,spooled_time,120) transdate " +
                "from nip_xpress " +
                "where "+
                "convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120)  " +
                "group by trans_code,code_description,convert(varchar,spooled_time,120) order by state_ind desc";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new NipOnXpressStateSummary();

                        transactionSummary.TransactionStateCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionStateDesc = rdr["code_description"].ToString();
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

                        nipOnXpressStateSummaries.Add(transactionSummary);
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

            return nipOnXpressStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM nip_xpress ORDER BY ID DESC ";
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

        private IEnumerable<NipOnXpressDetail> GetNipOnXpressDetails()
        {

            string sqlSelect = "select count(*) trans_count, statuscode, statusdescription, " +
                "sum(amount) volume from compass.macalla_fundtransfer where trunc(requestdate) = trunc(sysdate) " +
                "group by statuscode, statusdescription";

            List<NipOnXpressDetail> nipOnXpressDetails = new List<NipOnXpressDetail>();
            using (OracleConnection con = new OracleConnection(_nipIncomingCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        NipOnXpressDetail nipOnXpressDetail = new NipOnXpressDetail();
                        nipOnXpressDetail.CodeDescription = rdr["statusdescription"].ToString();
                        nipOnXpressDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        nipOnXpressDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        object j = rdr["statuscode"].GetType().Name;
                        if (!rdr["statuscode"].GetType().Name.Equals("DBNull"))
                            nipOnXpressDetail.StateCode = rdr["statuscode"].ToString();
                        else
                        {
                            nipOnXpressDetail.StateCode = null;
                        }//nipIncomingDetail.TransactionCompleteCode = rdr["CODE"]!=null ? rdr["CODE"].ToString() : null;
                        nipOnXpressDetail.SpooledTime = spoolDateTime;
                        nipOnXpressDetails.Add(nipOnXpressDetail);
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
            return nipOnXpressDetails;
        }
        public int StageNipOnXpressData()
        {
            IEnumerable<NipOnXpressDetail> nipOnXpressDetails = GetNipOnXpressDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var nipOnXpressDetail in nipOnXpressDetails)
                    {
                        const string insQuery = "BEGIN Insert Into nip_xpress (trans_code,code_description,trans_count,spooled_time,trans_volume)" +
                                                "  values (@transcode,@codedescription,@transcount,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        if (nipOnXpressDetail.StateCode != null)
                        {
                            cmd.Parameters.AddWithValue("@transcode", nipOnXpressDetail.StateCode);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@transcode", DBNull.Value);
                        }
                        //cmd.Parameters.AddWithValue("@transcode", SqlDbType.NVarChar).Value = nipIncomingDetail.TransactionCompleteCode;
                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = nipOnXpressDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = nipOnXpressDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@codedescription", SqlDbType.NVarChar).Value = nipOnXpressDetail.CodeDescription;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = nipOnXpressDetail.SpooledTime;

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