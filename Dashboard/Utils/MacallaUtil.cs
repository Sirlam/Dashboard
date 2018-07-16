using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace Dashboard.Utils
{

    public class MacallaUtil
    {
        private readonly string _macallaOraCs;
        private readonly string _macallaSqlCs;
        private readonly string _macallaQuickTopUpCs;
        private readonly string _macallaReportCs;
        public MacallaUtil()
        {
            //MacallaOraCS = ConfigurationManager.ConnectionStrings["MacallaCS"].ConnectionString;
            _macallaOraCs = ConfigurationManager.ConnectionStrings["MacallaLiveCS"].ConnectionString;
            _macallaSqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
            _macallaQuickTopUpCs = ConfigurationManager.ConnectionStrings["MacallaQuickTopUpCS"].ConnectionString;
            _macallaReportCs = ConfigurationManager.ConnectionStrings["MobileReportCS"].ConnectionString;
        }

        public List<MacallaTransactionSummary> GetMacallaLastPerformance()
        {
            List<MacallaTransactionSummary> macallaTransactionSummaries = new List<MacallaTransactionSummary>();
            string today = DateTime.Now.ToString("dd-MMM-yyyy");
            string sqlSelect = "select count(id) transaction_count,typename, sum(Decode(statetype, 'Confirmation', 1, 'Initiated', 0, 'Transaction Cleanup', 0, " +
                    "'Cancellation', 0, 'Fulfilment', 0, 'Credit', 0, 0)) success_status, " +
                    "sum(Decode(statetype, 'Confirmation',  " +
                    "0, 'Initiated', 1, 'Transaction Cleanup', 1,  " +
                    "'Cancellation', 1, 'Fulfilment', 1, 'Credit', 1, 1)) failure_status,   " +
                    " to_char(creationdate, 'dd-MM-YYYY') transdate  from  " +
                    "(select a.id, a.requestor, a.requestor_username, a.recipient, a.creationdate, g.channelname, d.statetype,  " +
                    "a.failurereason, " +
                    "a.topup_value, a.payment_amount, a.external_reference, " +
                    "b.productname,f.nickname as DR_ACCOUNT, e.nickname as CR_ACCOUNT, " +
                    "  c.servicename,   h.typename   " +
                    "from " +
                    "daffy_ecobank.VW_MCS_TRANSACTIONS a, daffy_ecobank.mcs_products b, daffy_ecobank.mcs_service c, daffy_ecobank.mcs_trans_state_change d,  " +
                    "daffy_ecobank.MCS_INSTRUMENT e,  daffy_ecobank.MCS_INSTRUMENT f, daffy_ecobank.mcs_channel g, daffy_ecobank.MCS_TYPES h  " +
                    "where " +
                    "a.productid = b.id(+) " +
                    "and a.serviceid = c.id(+) " +
                    "and a.last_state_change_id = d.id(+) " +
                    "and a.cr_instrumentid = e.id(+) " +
                    "and a.db_instrumentid = f.id(+) " +
                    "and g.id = a.channelid " +
                    "and a.typeid = h.id " +
                    "and trunc(a.creationdate) between '" + today + "' and '" + today + "' " +
                    "union " +
                    "select a.id, a.requestor, a.requestor_username, a.recipient, a.creationdate, g.channelname, d.statetype,  " +
                    "a.failurereason, " +
                    "a.topup_value, a.payment_amount, a.external_reference, " +
                    "b.productname,f.nickname as DR_ACCOUNT, e.nickname as CR_ACCOUNT, " +
                    "   c.servicename,   h.typename   " +
                    "from " +
                    "daffy_ecobank.VW_MCS_TRANSACTIONS a, daffy_ecobank.mcs_products b, daffy_ecobank.mcs_service c, daffy_ecobank.mcs_trans_state_change d,  " +
                    "daffy_ecobank.MCS_INSTRUMENT e,  daffy_ecobank.MCS_INSTRUMENT f, daffy_ecobank.mcs_channel g, daffy_ecobank.MCS_TYPES h  " +
                    "where " +
                    "a.productid = b.id(+) " +
                    "and a.serviceid = c.id(+) " +
                    "and a.last_state_change_id = d.id(+) " +
                    "and a.cr_instrumentid = e.id(+) " +
                    "and a.db_instrumentid = f.id(+) " +
                    "and g.id = a.channelid " +
                    "and a.typeid = h.id " +
                    "and trunc(a.creationdate) between '" + today + "' and '" + today + "' " +
                    ") group by to_char(creationdate, 'dd-MM-YYYY'), typename";
            using (OracleConnection con = new OracleConnection(_macallaOraCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaTransactionSummary
                        {
                            TransactionType = rdr["typename"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        transactionSummary.TransactionDateTime = datetime;//DateTime.Parse(datetime);

                        macallaTransactionSummaries.Add(transactionSummary);
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

            return macallaTransactionSummaries;
        }
        public List<MacallaTransactionSummary> GetMacallaTodayPerformance()
        {
            List<MacallaTransactionSummary> macallaTransactionSummaries = new List<MacallaTransactionSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count,trans_type typename, " +
"sum(case when trans_state_code='Confirmation' OR trans_state_code='Sucessful' then trans_count else 0 end) success_status,  " +
"sum(case when (trans_state_code='Confirmation' OR trans_state_code='Fulfilment' OR trans_state_code='Sucessful' OR trans_state_code='Pending') then 0 else trans_count end) failure_status,  " +
"sum(case when trans_state_code='Fulfilment' OR trans_state_code='Pending' then trans_count else 0 end) pending_status,  " +
"convert(varchar,spooled_time,120) transdate from macalla where convert(varchar,spooled_time,120)=convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by trans_type, convert(varchar,spooled_time,120)";
            using (SqlConnection con = new SqlConnection(_macallaSqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaTransactionSummary
                        {
                            TransactionType = rdr["typename"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalSuccessfulTransaction = Convert.ToInt32(rdr["success_status"].ToString()),
                            TotalFailedTransaction = Convert.ToInt32(rdr["failure_status"].ToString()),
                            TotalPendingTransaction = Convert.ToInt32(rdr["pending_status"].ToString())
                        };
                        string datetime = rdr["transdate"].ToString();
                        string dt =
                            DateTimeUtil.GetDisplayDateFromDateTime(DateTime.Parse(datetime));

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        macallaTransactionSummaries.Add(transactionSummary);
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

            return macallaTransactionSummaries;
        }
        public List<MacallaStateSummary> GetMacallaTodayStateSummary(string statecode)
        {
            List<MacallaStateSummary> macallaTransactionSummaries = new List<MacallaStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select sum(trans_count) transaction_count,trans_state_code trans_state, volume, " +
                        "case when trans_state_code in ('Confirmation','Sucessful', '00') then 'Success' when trans_state_code in ('Fulfilment' ,'Pending') then 'Pending' else 'Failed' end state_ind,  " +
                        "convert(varchar,spooled_time,120) transdate from macalla where trans_type='" + statecode + "' and convert(varchar,spooled_time,120)= convert(varchar,cast('" + lastSpooled + "' as datetime),120) group by trans_state_code, volume,convert(varchar,spooled_time,120) " +
                               " order by state_ind desc";
            using (SqlConnection con = new SqlConnection(_macallaSqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new MacallaStateSummary
                        {
                            TransactionState = rdr["trans_state"].ToString(),
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            TotalVolume = Convert.ToDecimal(rdr["volume"].ToString()),
                            TransactionStateInd = rdr["state_ind"].ToString()
                        };
                        string datetime = rdr["transdate"].ToString();
                        DateTime dt = DateTime.Parse(datetime);

                        transactionSummary.TransactionDateTime = dt;//DateTime.Parse(datetime);

                        macallaTransactionSummaries.Add(transactionSummary);
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

            return macallaTransactionSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM macalla ORDER BY ID DESC ";
            DateTime lastTime = DateTime.Now.Date;
            using (var con = new SqlConnection(_macallaSqlCs))
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

        private IEnumerable<MacallaDetail> GetMacallaDetail()
        {
            string sqlSelect = "select count(id) transaction_count,typename,statetype,sum(payment_amount) volume  from  (select a.id, a.requestor,a.payment_amount, a.requestor_username, a.recipient,a.creationdate, g.channelname, d.statetype, " +
                    "a.failurereason,a.topup_value,  a.external_reference, b.productname,f.nickname as DR_ACCOUNT, e.nickname as CR_ACCOUNT,c.servicename,h.typename  " +
                    "from daffy_ecobank.VW_MCS_TRANSACTIONS a, daffy_ecobank.mcs_products b, daffy_ecobank.mcs_service c, daffy_ecobank.mcs_trans_state_change d,  daffy_ecobank.MCS_INSTRUMENT e, " +
                    "daffy_ecobank.MCS_INSTRUMENT f, daffy_ecobank.mcs_channel g, daffy_ecobank.MCS_TYPES h, daffy_ecobank.mcs_transactions k  where a.productid = b.id(+) and k.id = a.id and a.serviceid = c.id(+)  " +
                    "and a.last_state_change_id = d.id(+) and a.cr_instrumentid = e.id(+) and a.db_instrumentid = f.id(+) and h.typename<>'QuickTopup'and h.typename<>'UtilityBillPayment' and g.id = a.channelid and a.typeid = h.id  " +
                    "and to_char(a.creationdate,'yyy-MM-dd') =to_char(sysdate,'yyy-MM-dd') and (k.reversal_status != 'reversalSuccess' or k.reversal_status is null)) group by statetype, typename";

            string sqlSelectQuickTop = "select code,description,trans_count,volume, " +
                    "case when code='00' and description='0' then 'Sucessful'  " +
                    "when code='00' and description='1' then 'Vending Failed'  " +
                    "when code='00' and description='8' then 'Debit Failed'   " +
                    "when code='00' and description is null then 'Pending' else description end detail  " +
                    "from ( " +
                    "select mobilerecharge.statusdescription code,dbo.mobilerecharge.flashmessage description, count(*) trans_count,sum(mobilerecharge.amount) volume " +
                    "from mobilerecharge " +
                    "where cast(mobilerecharge.datereceived as date) = convert(varchar,getdate(),112) " +
                    "and mobilerecharge.status <> '0' " +
                    "group by mobilerecharge.statusdescription, dbo.mobilerecharge.flashmessage " +
                    "union " +
                    "select mobilerecharge.statusdescription code,coalesce(dbo.mobileflexcubeprocessed.flexcuberesponse,mobilerecharge.flashmessage) description,count(*) trans_count,sum(mobilerecharge.amount) volume " +
                    "from mobilerecharge  " +
                    "left join dbo.Mobileflexcubeprocessed  " +
                    "on mobilerecharge.macrefno = Mobileflexcubeprocessed.macrefno  " +
                    "where cast(mobilerecharge.datereceived as date) = convert(varchar,getdate(),112) " +
                    "and (dbo.mobileflexcubeprocessed.flexcuberesponse <> '00') " +
                    "group by mobilerecharge.statusdescription, dbo.mobilerecharge.flashmessage, dbo.mobileflexcubeprocessed.flexcuberesponse " +
                    "union  " +
                    "select Mobileflexcubeprocessed.flexcuberesponse xx, mobilePhoneVendingStatus.statusreceived yy, count(*) trans_count,sum(Mobileflexcubeprocessed.amount) volume " +
                    "from Mobileflexcubeprocessed " +
                    "left join dbo.mobilePhoneVendingStatus " +
                    "on Mobileflexcubeprocessed.macrefno = mobilePhoneVendingStatus.macrefno " +
                    "where cast(Mobileflexcubeprocessed.dateadded as date) = convert(varchar,getdate(),112) " +
                    "and dbo.mobileflexcubeprocessed.flexcuberesponse = '00' " +
                    "group by Mobileflexcubeprocessed.flexcuberesponse, mobilePhoneVendingStatus.statusreceived) a";

            string sqlSelectQuickBal = "select flexcubecode as StatusCode,transactionmethod as Category, " +
                                       "Case  " +
                                       " when flexcubecode = '00' then 'Success' " +
                                       "when flexcubecode = '03' then 'InsufficientBalance'  " +
                                       "when flexcubecode = '10' then 'InsufficientBalance' " +
                                       "when flexcubecode = '13' then 'Invalid Account' " +
                                       "  when flexcubecode = '15' then 'Invalid Account' " +
                                       "when flexcubecode = '25' then 'Error Processing Transaction' " +
                                       "when flexcubecode = '29' then 'EOD'  " +
                                       "when flexcubecode = '97' then 'Failed Transaction' " +
                                       "    when flexcubecode = '99' then 'Error Processing Transaction'  else 'system malfunction' end " +
                                       "as statusdescription, " +
                                       "count(responsedate) TotalCount, sum(txtAmount) TotalVolume  from compassmdw.dbo.flextransactions " +
                                       " where cast(responsedate as date) = cast(getdate() as date) " +
                                       "and transactionmethod = 'QuickBalance' " +
                                       "group by   " +
                                       "transactionmethod,flexcubecode";

            List<MacallaDetail> macallaDetails = new List<MacallaDetail>();
            using (OracleConnection con = new OracleConnection(_macallaOraCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaDetail macallaDetail = new MacallaDetail();
                        macallaDetail.TransactionType = rdr["typename"].ToString();
                        macallaDetail.TransactionCount = Convert.ToInt32(rdr["transaction_count"].ToString());
                        macallaDetail.TransactionStateCode = rdr["statetype"].ToString();
                        macallaDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        macallaDetail.SpooledTime = DateTime.Now;
                        macallaDetails.Add(macallaDetail);
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
            using (SqlConnection con = new SqlConnection(_macallaQuickTopUpCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelectQuickTop, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaDetail macallaDetail = new MacallaDetail();
                        macallaDetail.TransactionType = "QuickTopup";
                        macallaDetail.TransactionCount = Convert.ToInt32(rdr["trans_count"].ToString());
                        macallaDetail.TransactionVolume = Convert.ToDecimal(rdr["volume"].ToString());
                        macallaDetail.TransactionStateCode = rdr["detail"].ToString();
                        macallaDetail.SpooledTime = DateTime.Now;
                        macallaDetails.Add(macallaDetail);
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

            using (SqlConnection con = new SqlConnection(_macallaQuickTopUpCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelectQuickBal, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaDetail macallaDetail = new MacallaDetail();
                        macallaDetail.TransactionType = "QuickBalance";
                        macallaDetail.TransactionCount = Convert.ToInt32(rdr["TotalCount"].ToString());
                        macallaDetail.TransactionVolume = Convert.ToDecimal(rdr["TotalVolume"].ToString());
                        macallaDetail.TransactionStateCode = rdr["StatusCode"].ToString();
                        macallaDetail.SpooledTime = DateTime.Now;
                        macallaDetails.Add(macallaDetail);
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
            return macallaDetails;
        }

        public Boolean StageMacallaData()
        {
            IEnumerable<MacallaDetail> macallaDetails = GetMacallaDetail();
            using (SqlConnection con = new SqlConnection(_macallaSqlCs))
            {
                int rowAffected = 0;

                try
                {
                    DateTime insertDate = DateTime.Now;
                    con.Open();
                    foreach (var macallaDetail in macallaDetails)
                    {
                        const string insQuery = "BEGIN Insert Into macalla (trans_type,trans_count,trans_state_code,spooled_time,volume)" +
                                                "  values (@transtype,@transcount,@transstatecode,@spooledtime,@volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@transtype", SqlDbType.NVarChar).Value = macallaDetail.TransactionType;
                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = macallaDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = macallaDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@transstatecode", SqlDbType.NVarChar).Value = macallaDetail.TransactionStateCode;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = insertDate;

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

        public List<MacallaReport> GetMacallaReports()
        {
            List<MacallaReport> macallaReports = new List<MacallaReport>();
            String trandate = DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy");

            String sqlSelect = "SELECT count(id) total_trans, typename, " +
                               "SUM(case when STATETYPE = 'Confirmation' or flexcuberesponse = '00' or vendingstatus ='0' or flexcubecode = '00' or rtnstatus = '0' THEN 1 ELSE 0 end) success, " +
                               "SUM(case when FAILUREREASON = 'Fulfilment Failure'  AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) fulfilment_failure, " +
                               "SUM(case when flashmessage = 'Top-Up Limit Exceeded'  AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) topup_exceeded, " +
                               "SUM(case when flashmessage = 'Unable to Vend, call Ecobank Contact Centre.'  AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) unable_to_vend, " +
                               "SUM(case when FAILUREREASON IN ('compass return status null','compass return unknown error','compass returned status has null','compass returns unknown error','Compass returns UnknownError', 'compassTimeout') AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) compass_error, " +
                               "SUM(case when flashmessage = 'Error processing request. Please try again.' OR " +
                               "FAILUREREASON IN ('Error processing request. Please try again','failed') OR flexcubemessage like ('Error Processing Transaction%') OR flexcubemessage = 'Failed Transaction' or rtnstatus IN('04','25','94','97','99','98') AND STATETYPE <> 'Confirmation' AND rtnstatus <> '0' AND vendingstatus <> '0' THEN 1 ELSE 0 END) error_processing, " +
                               "SUM(case when flashmessage = 'Unable to process request. Please try again.' or FAILUREREASON = 'InsufficientBalance' OR FAILUREREASON LIKE 'Insufficient balance%' or flexcubemessage = 'InsufficientBalance' or flexcubemessage like 'Insufficient Balance%' or rtnstatus IN ('03','10') AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) insufficient_Balance, " +
                               "SUM(case when FAILUREREASON = 'Invalid Acccount Number' OR FAILUREREASON LIKE 'Invalid Account%' or flexcubemessage like 'Invalid Account%' or rtnstatus = '13' AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) invalid_account, " +
                               "SUM(case when FAILUREREASON IN ('Unknown Error from Nibss','Unknown Error from Nibssnull') AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) nibss_error, " +
                               "SUM(case when FAILUREREASON = 'Transaction Cleanup' AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) transaction_cleanup, " +
                               "SUM(case when flexcuberesponse is null and vendingstatus is null and flexcubecode is null and rtnstatus is null and " +
                               "FAILUREREASON is null and flashmessage is null and flexcubemessage is null AND STATETYPE <> 'Confirmation' THEN 1 ELSE 0 END) null_response " +
                               "from  " +
                               "(select e.ID, e.STATETYPE, e.CHANNELNAME, e.typename, e.FAILUREREASON, a.macrefno, a.datereceived, a.dateresponse, a.flashmessage, b.flexcuberesponse,  " +
                               "c.processingstatus, " +
                               " d.statusreceived as vendingstatus, " +
                               "d.flexstatus, f.flexcubecode, f.flexcubemessage, g.rtncode, g.rtnmsg, g.rtnstatus, g.rtnstatusDescription " +
                               "from CompassMobile.dbo.mobileflexcubeprocessed b " +
                               "FULL OUTER JOIN [ENG-MACALAMW-03].Compassmobile.[dbo].[mobilerecharge] a " +
                               "ON b.macrefno = a.macrefno " +
                               "FULL OUTER JOIN [ENG-MACALAMW-03].Compassmobile.[dbo].mobiletransactions c " +
                               "ON a.macrefno = c.macrefno " +
                               "FULL OUTER JOIN [ENG-MACALAMW-03].Compassmobile.[dbo].mobilePhoneVendingStatus d " +
                               "ON a.macrefno = d.macrefno " +
                               "FULL OUTER JOIN Daffy_Ecobank.dbo.mcs_transactions_reporter e " +
                               "ON e.ID = a.macrefno " +
                               "FULL OUTER JOIN[ENG-MACALAMW-03].Compassmdw.[dbo].[flextransactions] f " +
                               "ON e.ID = f.txtRefID " +
                               "FULL OUTER JOIN[ENG-MACALAMW-03].Compassmdw.[dbo].[fundtransfer] g " +
                               "ON e.ID = g.txtRefID " +
                               "where e.typename <> 'QuickTopup' " +
                               "and cast(e.TRANSDATE as date) ='"+trandate+"') t " +
                               "group by typename";

            string sqlSelectQuickTop = "select sum(trans_count) total_trans, " +
            "sum(case when code = '00' and description is null THEN trans_count ELSE 0 END) fullfilment_failure, " +
            "sum(case when code = '00' and description = '0' THEN trans_count ELSE 0 END) successful, " +
            "sum(case when code = 'failed' and description = 'E-Top-Up Limit Exceeded' THEN trans_count ELSE 0 END) topup_exceeded, " +
            "sum(case when code = 'failed' and description = 'Unable to Vend, call Ecobank Contact Centre.' THEN trans_count ELSE 0 END) unable_to_vend, " +
            "sum(case when code = 'success' and description = 'InsufficientBalance' THEN trans_count ELSE 0 END) insufficient_balance, " +
            "sum(case when code = 'success' and description = 'NO DEBIT' THEN trans_count ELSE 0 END) no_debit, " +
            "sum(case when code = 'success' and description = 'Unauthorized Account' THEN trans_count ELSE 0 END) invalid_account " +
            "from (  " +
            "select mobilerecharge.statusdescription code,dbo.mobilerecharge.flashmessage description, count(*) trans_count,sum(mobilerecharge.amount) volume  " +
            "from mobilerecharge  " +
            "where cast(mobilerecharge.datereceived as date) = convert(varchar,getdate()-1,112)  " +
            "and mobilerecharge.status <> '0'  " +
            "group by mobilerecharge.statusdescription, dbo.mobilerecharge.flashmessage  " +
            "union  " +
            "select mobilerecharge.statusdescription code,coalesce(dbo.mobileflexcubeprocessed.flexcuberesponse,mobilerecharge.flashmessage) description,count(*) trans_count,sum(mobilerecharge.amount) volume  " +
            "from mobilerecharge   " +
            "left join dbo.Mobileflexcubeprocessed   " +
            "on mobilerecharge.macrefno = Mobileflexcubeprocessed.macrefno  " +
            "where cast(mobilerecharge.datereceived as date) = convert(varchar,getdate()-1,112)  " +
            "and (dbo.mobileflexcubeprocessed.flexcuberesponse <> '00')  " +
            "group by mobilerecharge.statusdescription, dbo.mobilerecharge.flashmessage, dbo.mobileflexcubeprocessed.flexcuberesponse  " +
            "union   " +
            "select Mobileflexcubeprocessed.flexcuberesponse xx, mobilePhoneVendingStatus.statusreceived yy, count(*) trans_count,sum(Mobileflexcubeprocessed.amount) volume  " +
            "from Mobileflexcubeprocessed  " +
            "left join dbo.mobilePhoneVendingStatus  " +
            "on Mobileflexcubeprocessed.macrefno = mobilePhoneVendingStatus.macrefno  " +
            "where cast(Mobileflexcubeprocessed.dateadded as date) = convert(varchar,getdate()-1,112)  " +
            "and dbo.mobileflexcubeprocessed.flexcuberesponse = '00'  " +
            "group by Mobileflexcubeprocessed.flexcuberesponse, mobilePhoneVendingStatus.statusreceived " +
            ")a";

            using (SqlConnection con = new SqlConnection(_macallaReportCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaReport macallaReport = new MacallaReport();
                        macallaReport.TransactionType = rdr["typename"].ToString();
                        macallaReport.TotalTransactions = Convert.ToInt32(rdr["total_trans"].ToString());
                        macallaReport.SuccessCount = Convert.ToInt32(rdr["success"].ToString());
                        macallaReport.FulfilmentFailure = Convert.ToInt32(rdr["fulfilment_failure"].ToString());
                        macallaReport.TopUpExceeded = Convert.ToInt32(rdr["topup_exceeded"].ToString());
                        macallaReport.UnableToVend = Convert.ToInt32(rdr["unable_to_vend"].ToString());
                        macallaReport.CompassError = Convert.ToInt32(rdr["compass_error"].ToString());
                        macallaReport.ErrorProcessing = Convert.ToInt32(rdr["error_processing"].ToString());
                        macallaReport.InsufficientBalance = Convert.ToInt32(rdr["insufficient_balance"].ToString());
                        macallaReport.InvalidAccount = Convert.ToInt32(rdr["invalid_account"].ToString());
                        macallaReport.NibssError = Convert.ToInt32(rdr["nibss_error"].ToString());
                        macallaReport.NullResponse = Convert.ToInt32(rdr["null_response"].ToString());

                        macallaReports.Add(macallaReport);
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

            using (SqlConnection con = new SqlConnection(_macallaQuickTopUpCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelectQuickTop, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaReport macallaReport = new MacallaReport();
                        macallaReport.TransactionType = "QuickTopup";
                        macallaReport.TotalTransactions = Convert.ToInt32(rdr["total_trans"].ToString());
                        macallaReport.FulfilmentFailure = Convert.ToInt32(rdr["fullfilment_failure"].ToString());
                        macallaReport.SuccessCount = Convert.ToInt32(rdr["successful"].ToString());
                        macallaReport.TopUpExceeded = Convert.ToInt32(rdr["topup_exceeded"].ToString());
                        macallaReport.UnableToVend = Convert.ToInt32(rdr["unable_to_vend"].ToString());
                        macallaReport.InsufficientBalance = Convert.ToInt32(rdr["insufficient_balance"].ToString());
                        macallaReport.NullResponse = Convert.ToInt32(rdr["no_debit"].ToString());
                        macallaReport.InvalidAccount = Convert.ToInt32(rdr["invalid_account"].ToString());

                        macallaReports.Add(macallaReport);
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
            return macallaReports;
        }

        public List<MacallaReport> GetMobileReports(String tranDate)
        {
            List<MacallaReport> mobileReports = new List<MacallaReport>();

            String sqlSelect = "select * from mobile_report where replace(convert(NVARCHAR, tran_date, 106), ' ', '-') = '"+tranDate+"'";

            using (SqlConnection con = new SqlConnection(_macallaSqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MacallaReport mobileReport = new MacallaReport();
                        mobileReport.TransactionType = rdr["typename"].ToString();
                        mobileReport.TotalTransactions = Convert.ToInt32(rdr["total_trans"].ToString());
                        mobileReport.SuccessCount = Convert.ToInt32(rdr["success"].ToString());
                        mobileReport.FulfilmentFailure = Convert.ToInt32(rdr["fulfilment_failure"].ToString());
                        mobileReport.TopUpExceeded = Convert.ToInt32(rdr["topup_exceeded"].ToString());
                        mobileReport.UnableToVend = Convert.ToInt32(rdr["unable_to_vend"].ToString());
                        mobileReport.CompassError = Convert.ToInt32(rdr["compass_error"].ToString());
                        mobileReport.ErrorProcessing = Convert.ToInt32(rdr["error_processing"].ToString());
                        mobileReport.InsufficientBalance = Convert.ToInt32(rdr["insufficient_balance"].ToString());
                        mobileReport.InvalidAccount = Convert.ToInt32(rdr["invalid_account"].ToString());
                        mobileReport.NibssError = Convert.ToInt32(rdr["nibss_error"].ToString());
                        mobileReport.NullResponse = Convert.ToInt32(rdr["null_response"].ToString());

                        mobileReports.Add(mobileReport);
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
            return mobileReports;
        }

        public Boolean StageMacallaReport()
        {
            List<MacallaReport> macallaReports = GetMacallaReports();
            using (SqlConnection con = new SqlConnection(_macallaSqlCs))
            {
                int rowAffected = 0;

                try
                {
                    DateTime reportDate = DateTime.Now.AddDays(-1);
                    DateTime insertDate = DateTime.Now;
                    con.Open();
                    foreach (var macallaReport in macallaReports)
                    {
                        const string insQuery = "BEGIN Insert Into mobile_report (typename,total_trans,success,fulfilment_failure,topup_exceeded,unable_to_vend,compass_error,error_processing,insufficient_balance,invalid_account,nibss_error,null_response,tran_date,spooled_time)" +
                                                "  values (@typename,@total_trans,@success,@fulfilment_failure,@topup_exceeded,@unable_to_vend,@compass_error,@error_processing,@insufficient_balance,@invalid_account,@nibss_error,@null_response,@tran_date,@spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@typename", SqlDbType.NVarChar).Value = macallaReport.TransactionType;
                        cmd.Parameters.AddWithValue("@total_trans", SqlDbType.Int).Value = macallaReport.TotalTransactions;
                        cmd.Parameters.AddWithValue("@success", SqlDbType.Int).Value = macallaReport.SuccessCount;
                        cmd.Parameters.AddWithValue("@fulfilment_failure", SqlDbType.Int).Value = macallaReport.FulfilmentFailure;
                        cmd.Parameters.AddWithValue("@topup_exceeded", SqlDbType.Int).Value = macallaReport.TopUpExceeded;
                        cmd.Parameters.AddWithValue("@unable_to_vend", SqlDbType.Int).Value = macallaReport.UnableToVend;
                        cmd.Parameters.AddWithValue("@compass_error", SqlDbType.Int).Value = macallaReport.CompassError;
                        cmd.Parameters.AddWithValue("@error_processing", SqlDbType.Int).Value = macallaReport.ErrorProcessing;
                        cmd.Parameters.AddWithValue("@insufficient_balance", SqlDbType.Int).Value = macallaReport.InsufficientBalance;
                        cmd.Parameters.AddWithValue("@invalid_account", SqlDbType.Int).Value = macallaReport.InvalidAccount;
                        cmd.Parameters.AddWithValue("@nibss_error", SqlDbType.Int).Value = macallaReport.NibssError;
                        cmd.Parameters.AddWithValue("@null_response", SqlDbType.Int).Value = macallaReport.NullResponse;
                        cmd.Parameters.AddWithValue("@tran_date", SqlDbType.DateTime).Value = reportDate;
                        cmd.Parameters.AddWithValue("@spooled_time", SqlDbType.DateTime).Value = insertDate;

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