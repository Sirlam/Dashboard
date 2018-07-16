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
    public class MobileUtil
    {
        private readonly string _sqlCs;
        private readonly string _esbCs;
        private readonly string _nipIncomingCs;

        public MobileUtil()
        {
            _esbCs = ConfigurationManager.ConnectionStrings["EsbCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
            _nipIncomingCs = ConfigurationManager.ConnectionStrings["NipIncomingCS"].ConnectionString;
        }

        public List<MobileDetail> GetXpressAccountDetails()
        {
            string sqlSelect = "select count(*) as total, response_code,response_msg, source_channel  from mesb_sadc.mul_xpress_account_log where affiliate_code = 'ENG' " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by response_code,response_msg, source_channel ";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "XPRESS ACCOUNT ONBOARDING";
                    mobileDetail.TransactionCode = ordr["response_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["response_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_channel"].ToString();
                    //mobileDetail.TransactionVolume = new decimal(null);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetTopupDetails()
        {
            string sqlSelect = "select count(*) as total, sum(amount) as volume, response_code, response_msg, source_channel from mesb_sadc.bil_tran_log where affiliate_code='ENG' " +
                "and biller_code LIKE ('%TOPUP%') " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by response_code, response_msg, source_channel " +
                "order by source_channel";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "TOPUP";
                    mobileDetail.TransactionCode = ordr["response_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["response_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_channel"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetBillPaymentDetails()
        {
            string sqlSelect = "select count(*) as total, sum(amount) as volume, response_code, response_msg, source_channel from mesb_sadc.bil_tran_log where affiliate_code='ENG' " +
                "and biller_code NOT LIKE ('%TOPUP%') " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by response_code, response_msg, source_channel " +
                "order by source_channel";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "BILL PAYMENT";
                    mobileDetail.TransactionCode = ordr["response_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["response_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_channel"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetInterBankTransferDetails()
        {
            string sqlSelect = "select count(*) as total, sum(amount) as volume, cba_rsp_code, cba_rsp_msg, source_code from mesb_sadc.MUL_IBT_LOG where affiliate_code='ENG' " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by cba_rsp_code, cba_rsp_msg, source_code " +
                "order by source_code";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "INTER-BANK TRANSFERS";
                    mobileDetail.TransactionCode = ordr["cba_rsp_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["cba_rsp_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_code"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetInterAffiliateTransferDetails()
        {
            string sqlSelect = "select count(*) as total, sum(send_amount) as volume, receive_rsp_code, receive_rsp_msg, source_code from mesb_sadc.EDI_TRANSFER_MASTER where affiliate_code='ENG' " +
                "and trunc(tran_date) = trunc(sysdate) " +
                "group by receive_rsp_code, receive_rsp_msg, source_code " +
                "order by source_code";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "INTER-AFFILIATE TRANSFERS";
                    mobileDetail.TransactionCode = ordr["receive_rsp_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["receive_rsp_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_code"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetMasterPassMvisaDetails()
        {
            string sqlSelect = "select count(*) as total, sum(amount) as volume, cba_rsp_code, cba_rsp_msg, source_code from mesb_sadc.MV_MERCHANT_PAYMENT_LOG where affiliate_code='ENG' " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by cba_rsp_code, cba_rsp_msg, source_code " +
                "order by source_code";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "MASTERPASS-MVISA";
                    mobileDetail.TransactionCode = ordr["cba_rsp_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["cba_rsp_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_code"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetMerchantP2PDetails()
        {
            string sqlSelect = "select count(*) as total, sum(AMT) as volume, cba_rsp_code, cba_rsp_msg from mesb_sadc.MV_MERCHANT_P2P_PAYMENT_LOG where affiliate_code='ENG' " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by cba_rsp_code, cba_rsp_msg";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "MERCHANT-P2P";
                    mobileDetail.TransactionCode = ordr["cba_rsp_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["cba_rsp_msg"].ToString();
                    mobileDetail.ChannelType = "";
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetAgencyBankingDetails()
        {
            string sqlSelect = "select count(*) as total, sum(amount) as volume, response_code, response_msg, source_channel from mesb_sadc.BIL_AGENT_LOG where affiliate_code='ENG' " +
                "and trunc(request_date) = trunc(sysdate) " +
                "group by response_code, response_msg, source_channel " +
                "order by source_channel";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_esbCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "AGENCY-BANKING";
                    mobileDetail.TransactionCode = ordr["response_code"].ToString();
                    mobileDetail.TransactionDescription = ordr["response_msg"].ToString();
                    mobileDetail.ChannelType = ordr["source_channel"].ToString();
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileDetail> GetMobileIncomingDetails()
        {
            string sqlSelect =
                "SELECT COUNT(*) as total, sum(amount) as volume, responsecode, description  FROM compass.FP_FUNDTRANSFERREQUEST " +
                "full join compass.responsecode b on responsecode = code " +
                "where destinationbankcode = '100008' and trunc(requestdate) = trunc(sysdate) " +
                "group by   responsecode, description";

            var mobileDetails = new List<MobileDetail>();

            using (OracleConnection con = new OracleConnection(_nipIncomingCs))
            {
                OracleCommand cmd = new OracleCommand(sqlSelect, con);
                con.Open();
                cmd.CommandTimeout = 0;
                OracleDataReader ordr = cmd.ExecuteReader();

                while (ordr.Read())
                {
                    var mobileDetail = new MobileDetail();
                    mobileDetail.TransactionCount = Convert.ToInt32(ordr["total"]);
                    mobileDetail.TransactionType = "INCOMING-TO-XPRESS-ACCOUNT";
                    mobileDetail.TransactionCode = ordr["responsecode"].ToString();
                    mobileDetail.TransactionDescription = ordr["description"].ToString();
                    mobileDetail.ChannelType = "";
                    mobileDetail.TransactionVolume = Convert.ToDecimal(ordr["volume"]);
                    mobileDetail.SpooledTime = Convert.ToDateTime(DateTime.Now.ToString("g"));

                    mobileDetails.Add(mobileDetail);
                }
            }
            return mobileDetails;
        }
        public List<MobileStateSummary> GetMobileStateSummaries(String tranType)
        {
            List<MobileStateSummary> mobileStateSummaries = new List<MobileStateSummary>();

            DateTime lastSpooled = GetLastFetchDateTime();
            String sqlSelect = "select sum(trans_count) transaction_count,trans_code,trans_state_code, volume, channel_type, " +
                               "case when trans_code = '000' then 'Success' when trans_state_code in ('', null, 'null') then 'Pending' else 'Failed' end state_ind,  " +
                               "convert(varchar,spooled_time,120) transdate from mobile where trans_type='" + tranType + "' and convert(varchar(16),spooled_time,120)= convert(varchar(16),cast('" + lastSpooled + "' as datetime),120)  " +
                               "group by trans_code, trans_state_code, volume,convert(varchar,spooled_time,120),channel_type " +
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
                        var transactionSummary = new MobileStateSummary();

                        transactionSummary.TransactionCount = Convert.ToInt32(rdr["transaction_count"]);
                        transactionSummary.TransactionCode = rdr["trans_code"].ToString();
                        transactionSummary.TransactionDescription = rdr["trans_state_code"].ToString();
                        transactionSummary.TransactionState = rdr["state_ind"].ToString();
                        transactionSummary.ChannelType = rdr["channel_type"].ToString();
                        if (!rdr["volume"].GetType().Name.Equals("DBNull"))
                        {
                            transactionSummary.TransactionVolume = Convert.ToDecimal(rdr["volume"]);
                        }
                        else
                        {
                            transactionSummary.TransactionVolume = 0;
                        }

                        mobileStateSummaries.Add(transactionSummary);
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
                return mobileStateSummaries;
            }
        } 
        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM mobile ORDER BY ID DESC ";
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
        public int StageXpressAccountData()
        {
            var mobileDetails = GetXpressAccountDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;

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
        public int StageTopUpData()
        {
            var mobileDetails = GetTopupDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageBillPaymentData()
        {
            var mobileDetails = GetBillPaymentDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageInterBankTransferData()
        {
            var mobileDetails = GetInterBankTransferDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageInterAffiliateTransferData()
        {
            var mobileDetails = GetInterAffiliateTransferDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageMasterPassMvisaData()
        {
            var mobileDetails = GetMasterPassMvisaDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageMerchantP2PData()
        {
            var mobileDetails = GetMerchantP2PDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageAgencyBankingData()
        {
            var mobileDetails = GetAgencyBankingDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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
        public int StageMobileIncomingData()
        {
            var mobileDetails = GetMobileIncomingDetails();

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();

                    foreach (var mobileDetail in mobileDetails)
                    {
                        const string insQuery =
                            "BEGIN insert into mobile (trans_type,trans_count,trans_code,trans_state_code,channel_type,spooled_time,volume)" +
                            "  values (@trans_type,@trans_count,@trans_code,@trans_state_code,@channel_type,@spooled_time,@volume); END;";

                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@trans_type", SqlDbType.VarChar).Value = mobileDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@trans_count", SqlDbType.Int).Value = mobileDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_code", SqlDbType.VarChar).Value = mobileDetail.TransactionCode;
                        cmd.Parameters.AddWithValue("@trans_state_code", SqlDbType.VarChar).Value = mobileDetail.TransactionDescription;
                        cmd.Parameters.AddWithValue("@channel_type", SqlDbType.VarChar).Value = mobileDetail.ChannelType;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = mobileDetail.SpooledTime;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = mobileDetail.TransactionVolume;

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