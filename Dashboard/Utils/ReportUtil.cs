using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Globalization;

namespace Dashboard.Utils
{
    public class ReportUtil
    {
        private readonly string _ribCs;
        private readonly string _nipOutgoingCs;
        private readonly string _nipIncomingCs;
        private readonly string _napsIncoming;
        private readonly string _macallaCs;
        private readonly string _fepCs;

        public class Report
        {
            public string StateCode { get; set; }
            public string StateDesc { get; set; }
            public string Category { get; set; }
            public string AffiliateCode { get; set; }
            [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
            public int TransCount { get; set; }
            [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
            public decimal Volume { get; set; }
        }

        private string dateFormat;
        Dictionary<string, string> responseCode;
            
        public ReportUtil()
        {
            dateFormat = "dd-MMM-yyyy";

            _ribCs = ConfigurationManager.ConnectionStrings["RibCS"].ConnectionString;
            _nipOutgoingCs = ConfigurationManager.ConnectionStrings["NipOutgoingCS"].ConnectionString;
            _nipIncomingCs = ConfigurationManager.ConnectionStrings["NipIncomingCS"].ConnectionString;
            _napsIncoming = ConfigurationManager.ConnectionStrings["NapsIncomingCS"].ConnectionString;
            _macallaCs = ConfigurationManager.ConnectionStrings["MacallaLiveCS"].ConnectionString;
            _fepCs = ConfigurationManager.ConnectionStrings["FepCS"].ConnectionString;

           responseCode = new Dictionary<string, string>();

            responseCode.Add("x11", "Transaction has been accepted (However, the state of the transaction is not known) Please check RE-QUERY Queue to Retry.");
            responseCode.Add("x12", "Transaction status cannot be confirmed from Nibss. Manual Reversal Required.");
            responseCode.Add("x13", "Financle Unreachable");
            responseCode.Add("x14", "Financle Timeout");
            responseCode.Add("10", "Insufficient Balance");
            responseCode.Add("15", "Invalid Branch Code");
            responseCode.Add("98", "No Response from Flexcube");
            responseCode.Add("00", "Approved or completed successfully");
            responseCode.Add("03", "Invalid Sender");
            responseCode.Add("05", "Do not honor");
            responseCode.Add("06", "Dormant Account");
            responseCode.Add("07", "Invalid Account");
            responseCode.Add("08", "Account Name Mismatch");
            responseCode.Add("09", "Request processing in progress");
            responseCode.Add("12", "Invalid transaction");
            responseCode.Add("13", "Invalid Amount");
            responseCode.Add("21", "No action taken");
            responseCode.Add("25", "Unable to locate record");
            responseCode.Add("26", "Duplicate record");
            responseCode.Add("30", "Format error");
            responseCode.Add("34", "Suspected fraud");
            responseCode.Add("35", "Contact sending bank");
            responseCode.Add("51", "No sufficient funds");
            responseCode.Add("57", "Transaction not permitted to sender");
            responseCode.Add("58", "Transaction not permitted on channel");
            responseCode.Add("61", "Transfer limit Exceeded");
            responseCode.Add("63", "Security violation");
            responseCode.Add("65", "Exceeds withdrawal frequency");
            responseCode.Add("68", "Response received too late");
            responseCode.Add("91", "Beneficiary Bank not available");
            responseCode.Add("92", "Routing error");
            responseCode.Add("94", "Duplicate transaction");
            responseCode.Add("96", "System malfunction");
            responseCode.Add("x1", "Requery Transaction");
            responseCode.Add("x2", "Unable to connect to the ISO Server");
            responseCode.Add("x3", "ISO request has timed out");
            responseCode.Add("99", "Error Occured During Processing");
            responseCode.Add("zz", "Not Processed");
            responseCode.Add("x9", "ISO System Error");
            responseCode.Add("x4", "Error Occured while getting Flexcube Account Balance");
            responseCode.Add("x5", "Error Occured During Account Holder Debit");
            responseCode.Add("x6", "Transaction Failed at Destination Bank.");
            responseCode.Add("x7", "Fund Transfer Failed, Transaction Reversed");
            responseCode.Add("x8", "Transaction Did not Pass NameEnquiry");
            responseCode.Add("17", "EOD");

        }

        public List<Report> RibReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            
            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect =
                "SELECT   flexposting,CODE,description,CATEGORY,'ENG' AFFILIATE_CODE, 'INQUIRY' TRAN_TYPE, count(*) Transaction_count, sum(amount) volume FROM ( " +
                "SELECT TRANSACTIONSTATUS CODE,description,flexposting,flexpostingmsg,transactionstatusdetails,amount, CASE  " +
                "WHEN transactionstatusdetails = 'Transaction Completed Successfully' THEN 'SUCCESS'  " +
                "WHEN FLEXREVERSAL is not null THEN 'REVERSAL'  " +
                "WHEN TRANSACTIONSTATUS = 'successful' AND FLEXREVERSAL is null THEN 'Reversal Failed'  " +
                "ELSE TRANSACTIONSTATUS END CATEGORY  " +
                "FROM RIBUSER.CUSTOMERTRANSFERS,RIBUSER.responsecode  " +
                "WHERE flexposting=code(+) and to_char(TRANSACTIONDATE,'yyyyMMdd') between '" + startDateTime.ToString("yyyyMMdd") + "'  AND '" + endDateTime.ToString("yyyyMMdd") + "')  " +
                "GROUP BY CATEGORY,flexposting,CODE,description order by flexposting,CATEGORY desc";

            using (OracleConnection con = new OracleConnection(_ribCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["CATEGORY"].ToString(),
                            StateCode = rdr["flexposting"].ToString(),
                            StateDesc = rdr["description"].ToString() == "" ? responseCode[rdr["flexposting"].ToString()] : rdr["description"].ToString(),
                            TransCount = Convert.ToInt32(rdr["Transaction_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        } 
        public List<Report> MacallaReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            
            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect ="select count(id) transaction_count,typename,statetype,sum(payment_amount) volume  from  (select a.id, a.requestor,a.payment_amount, a.requestor_username, a.recipient,a.creationdate, g.channelname, " +
            "d.statetype, a.failurereason,a.topup_value,  a.external_reference, b.productname,f.nickname as DR_ACCOUNT, e.nickname as CR_ACCOUNT,c.servicename,h.typename  " +
            "from daffy_ecobank.VW_MCS_TRANSACTIONS a, daffy_ecobank.mcs_products b, daffy_ecobank.mcs_service c, daffy_ecobank.mcs_trans_state_change d,  daffy_ecobank.MCS_INSTRUMENT e, " +
            "daffy_ecobank.MCS_INSTRUMENT f, daffy_ecobank.mcs_channel g, daffy_ecobank.MCS_TYPES h  where a.productid = b.id(+) and a.serviceid = c.id(+) " +
            "and a.last_state_change_id = d.id(+) and a.cr_instrumentid = e.id(+) and a.db_instrumentid = f.id(+) and g.id = a.channelid and a.typeid = h.id " +
            "and to_char(a.creationdate,'yyyyMMdd') between '" + startDateTime.ToString("yyyyMMdd") + "'  AND '" + endDateTime.ToString("yyyyMMdd") + "') group by statetype, typename order by typename";

            using (OracleConnection con = new OracleConnection(_macallaCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = "ENG",
                            Category = rdr["typename"].ToString(),
                            StateCode = "",
                            StateDesc = rdr["statetype"].ToString(),
                            TransCount = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
        public List<Report> NipOutgoingReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect =
                "select completecode,'ENG' AFFILIATE_CODE,CASE WHEN completecode = '00' THEN 'SUCCESS' " +
                "WHEN completecode = 'x7' THEN 'REVERSAL' WHEN completecode = 'zz' THEN 'INCOMPLETE' " +
                "ELSE description END description,sum(trans) trans_count,sum(cast(trn_amount as decimal)) volume  " +
                "from  " +
                "(select completecode,description,1 trans,trn_amount " +
                "from archive_fp_nameenquiry_out , responsecode b where code=completecode and  " +
                "cast(requestdate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
                "union all  " +
                "select completecode,description,1 trans, trn_amount " +
                "from fp_nameenquiry_out a, responsecode b where code=completecode and    " +
                "cast(requestdate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "')t   " +
                "group by completecode,description ";


            using (SqlConnection con = new SqlConnection(_nipOutgoingCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = " ",
                            StateCode = rdr["completecode"].ToString(),
                            StateDesc = rdr["description"].ToString(),
                            TransCount = Convert.ToInt32(rdr["trans_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
        public List<Report> EbillsReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect = "select responsecode, 'ENG' AFFILIATE_CODE, txnstatus, " +
            "count(*) trans_count, sum(Amount) volume, " +
            "case when responsecode='00' then 'SUCCESS' when txnstatus='PROCESSING' then 'PENDING' " +
            "when txnstatus='Transaction Failed and Reversed - 00' then 'REVERSED' " +
            "when responsecode<>'00' and txnstatus != 'Transaction Failed and Reversed - 00' or txnstatus in ('UNAUTHORISED', 'CANCELLED') " +
            "THEN 'FAILED' end description " +
            "from eBillspayTransactions where  " +
            "cast(dateadded as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "'  " +
            "group by responsecode, txnstatus";

            using (SqlConnection con = new SqlConnection(_nipOutgoingCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["description"].ToString(),
                            StateCode = rdr["responsecode"].ToString(),
                            StateDesc = rdr["txnstatus"].ToString(),
                            TransCount = Convert.ToInt32(rdr["trans_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        } 
        public List<Report> NapsReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect = "select flexcode, 'ENG' AFFILIATE_CODE, flexmsg, " +
            "count(*) trans_count, sum(Amount) volume,  " +
            "case when flexcode='00' then 'SUCCESS' when flexcode is null then 'PENDING' when flexcode<>'00' or flexcode='' THEN 'FAILED' end description " +
            "from NAPS_FundTransfer " +
            "where cast(TransactionDate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
            "group by flexcode, flexmsg";


            using (SqlConnection con = new SqlConnection(_napsIncoming))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["description"].ToString(),
                            StateCode = rdr["flexcode"].ToString(),
                            StateDesc = rdr["flexmsg"].ToString(),
                            TransCount = Convert.ToInt32(rdr["trans_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
        public List<Report> NeftOutgoingReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect =
                "select code,TRN_DEBIT_STATUS,description,'ENG' AFFILIATE_CODE, category, COUNT(*) trans_count, SUM(amount) VOLUME " +
                "from  " +
                "(SELECT clearingstatus code,description,TRN_DEBIT_STATUS, amount, CASE WHEN clearingstatus = 'sent' THEN 'SUCCESS'  " +
                "WHEN clearingstatus in ('AwaitingClearing','Generated') THEN 'PENDING'  " +
                "WHEN clearingstatus = ' ' and TRN_CHECKER is null THEN 'INCOMPLETE'  " +
                "WHEN clearingstatus = ' ' and TRN_DEBIT_STATUS <> '00' and TRN_CHECKER is not null THEN 'FAILED'  " +
                "ELSE clearingstatus END category  " +
                "FROM NEFT full join responsecode on TRN_DEBIT_STATUS=code " +
                "where SESSION_DATE between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "') a  " +
                "GROUP BY code,TRN_DEBIT_STATUS,category,description ";


            using (SqlConnection con = new SqlConnection(_nipOutgoingCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["category"].ToString(),
                            StateCode = rdr["TRN_DEBIT_STATUS"].ToString(),
                            StateDesc = rdr["description"].ToString(),
                            TransCount = Convert.ToInt32(rdr["trans_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["volume"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
        public List<Report> NipIncomingReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect =
                "select response CODE, CASE WHEN response = '00' THEN 'SUCCESS' ELSE description END CATEGORY, " +
                "case when response='00' then 'Success' when response='x7' then 'Reversal' " +
                "when response='zz' then 'Incomplete'  when response is null then 'Pending' " +
                "when response='' then 'Failed' else 'Failed' end state, 'ENG' AFFILIATE_CODE, sum(volume) trans_count, amount " +
                "from( SELECT  response,description,  COUNT(*) VOLUME, sum(cast(field4 as float)) amount " +
                "FROM Tanking_isolog8583 f full join responsecode b on  response=code WHERE " +
                "cast (systemdate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
                "GROUP BY response, description " +
                "union " +
                "SELECT  response,description, COUNT(*) VOLUME, sum(cast(field4 as float)) " +
                "FROM Tanking_isolog8583_archive_all f full join responsecode b on  response=code WHERE " +
                "cast (systemdate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
                "GROUP BY response, description ) t " +
                "GROUP BY response,description, amount";


            using (SqlConnection con = new SqlConnection(_nipIncomingCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["state"].ToString(),
                            StateCode = rdr["CODE"].ToString(),
                            StateDesc = rdr["CATEGORY"].ToString(),
                            TransCount = Convert.ToInt32(rdr["trans_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["amount"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
        public List<Report> FepReport(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture);
            List<Report> reports = new List<Report>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect = "SELECT b.rsp_code_rsp as rsp_code, count (distinct b.retrieval_reference_nr) as tran_count, " +
                "case when pos_terminal_type = '00' then 'WEB' " +
                "when pos_terminal_type = '01' then 'POS' " +
                "when pos_terminal_type = '02' then 'ATM'  " +
                "when pos_terminal_type = '21' then 'MOBILE'  else 'OTHER_CHANNELS'  " +
                "end CHANNEL_TYPE, " +
                "case when b.tran_type = '00' then 'PURCHASE' " +
                "when b.tran_type = '01' then 'CSHWITHDL' " +
                "when b.tran_type = '40' then 'TRF'  " +
                "when b.tran_type = '50' then 'PAYMNT'  " +
                "when b.tran_type = '21' then 'DEPOSIT' else 'OTHER_tran_type'  " +
                "end tran_type,sum(settle_amount_rsp)/100 tot_vol  " +
                "FROM dbo.post_tran_cust a INNER JOIN " +
                "dbo.post_tran b ON a.post_tran_cust_id = b.post_tran_cust_id " +
                "WHERE b.tran_completed = 1 " +
                "and b.tran_type in ('01', '00', '50', '40','31','92','21') " +
                "and substring(pan,1,6) in ('506118','537010','529751','506122','531992','427872','499908','428602','428603','499910','499909') " +
                "and message_type not in ('0420','0421','0220') " +
                "AND ( convert(varchar, b.datetime_tran_local, 112) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "') " +
                "and b.rsp_code_rsp in ('00','01','02','05','06','12','14','23','25','30','39','40','41','43','48','51','52','53','54','55','56','57','58','59','61','62','63','65','68','75','91','92','96','98') " +
                "group by b.rsp_code_rsp,pos_terminal_type,b.tran_type";


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
                        var report = new Report
                        {
                            AffiliateCode = rdr["AFFILIATE_CODE"].ToString(),
                            Category = rdr["CHANNEL_TYPE"].ToString(),
                            StateCode = rdr["rsp_code"].ToString(),
                            StateDesc = rdr["tran_type"].ToString(),
                            TransCount = Convert.ToInt32(rdr["tran_count"].ToString()),
                            Volume = Convert.ToDecimal(rdr["tot_vol"].ToString())
                        };

                        reports.Add(report);
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
            return reports;
        }
    }
}