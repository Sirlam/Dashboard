using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Dashboard.Utils
{
    public class MasterPassUtil
    {
        private readonly string _masterPassCs;
        private readonly string _macallaCs;
        private readonly string _sqlCs;
        private readonly string _flexCubeCs;
        private readonly string _masterPass125Cs;
        private readonly string _dateFormat;

        public MasterPassUtil()
        {
            _dateFormat = "dd-MMM-yyyy";

            _masterPassCs = ConfigurationManager.ConnectionStrings["MasterPassCS"].ConnectionString;
            _macallaCs = ConfigurationManager.ConnectionStrings["MacallaLiveCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
            _flexCubeCs = ConfigurationManager.ConnectionStrings["FlexBranchCS"].ConnectionString;
            _masterPass125Cs = ConfigurationManager.ConnectionStrings["MasterPass125CS"].ConnectionString;
        }

        public List<MerchantSummary> GetMerchantTodayPerformance()
        {
            List<MerchantSummary> merchantSummaries = new List<MerchantSummary>();
            
            string sqlSelect =
                "select count(NewMarchantId)MerchantCount, convert(varchar,DateApproved,106)DateApproved, " +
                "sum (case when left(BusinessSegment,2) = 'CM' then 1 else 0 end) CM, " +
                "sum (case when left(BusinessSegment,2) = 'CS' then 1 else 0 end) CS, " +
                "sum (case when left(BusinessSegment,2) = 'DB' then 1 else 0 end) DB, " +
                "sum (case when left(BusinessSegment,2) = '' then 1 else 0 end) Unknown " +
                "from merchant_onboarding a " +
                "where convert(varchar(16),last_spooled_time,120) = convert(varchar(16),cast((SELECT TOP 1 last_spooled_time FROM merchant_onboarding " +
                "WHERE convert(varchar,last_spooled_time,112)=convert(varchar, a.last_spooled_time, 112)  ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,last_spooled_time,112) between convert(varchar,GETDATE()-7,112) and convert(varchar,GETDATE(),112) " +
                "and convert(varchar,DateApproved,112) >=convert(varchar,GETDATE()-7,112) " +
                "group by convert(varchar,DateApproved,106) " +
                "order by DateApproved asc";
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
                        var transactionSummary = new MerchantSummary()
                        {
                            MerchantCount = Convert.ToInt32(rdr["MerchantCount"].ToString()),
                            DateApproved = (rdr["DateApproved"].ToString()),
                            Cm = (rdr["CM"].ToString()),
                            Db = (rdr["DB"].ToString()),
                            Unknown = (rdr["Unknown"].ToString()),
                            Cs = (rdr["CS"].ToString())
                        };
                        merchantSummaries.Add(transactionSummary);
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

            return merchantSummaries;
        }
        public List<SalesSummary> GetSalesTodayPerformance()
        {
            List<SalesSummary> salesSummaries = new List<SalesSummary>();
            DateTime lastSpooled = GetSalesLastFetchDateTime();
            string sqlSelect = "select  " +
                               "sum(case when status = 'Successful' then transaction_count else 0 end) success_status, " +
                               "sum(case when status = 'Failed' then transaction_count else 0 end) failed_status, " +
                               "sum(case when status = 'Successful' then volume else 0 end) success_vol, " +
                               "sum(case when status = 'Failed' then volume else 0 end) failed_vol, " +
                               "sum(transaction_count)transaction_count, " +
                               "sales_date " +
                               "from merchant_sales " +
                               "where convert(varchar,last_spooled_time,120) = convert(varchar,cast('" + lastSpooled + "' as datetime),120)  " +
                               "group by sales_date";

            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new SalesSummary()
                        {
                            TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString()),
                            SuccessCount = Convert.ToInt32(rdr["success_status"].ToString()),
                            FailedCount = Convert.ToInt32(rdr["failed_status"].ToString()),
                            SuccessVolume = Convert.ToDecimal(rdr["success_vol"].ToString()),
                            FailedVolume = Convert.ToDecimal(rdr["failed_vol"].ToString()),
                            SalesDate = (rdr["sales_date"].ToString())
                        };
                        salesSummaries.Add(transactionSummary);
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

            return salesSummaries;
        }
        private DateTime GetSalesLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 last_spooled_time FROM merchant_sales ORDER BY ID DESC ";
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
        private IEnumerable<MerchantOnboarding> GetMerchantOnboardings()
        {
            string sqlSelect = "SELECT distinct(NewMarchantId),FirstName,LastName,DateofBirth,PhoneNo,AccountNo,AccountType,CoporateName, " +
                "convert(varchar,DateApproved,106)DateApproved,ExpiryDate " +
                "FROM MASTERPASSBOARDING_VW where cast(DateApproved as date) = cast(GETDATE() as date)";

            List<MerchantOnboarding> merchantOnboardings = new List<MerchantOnboarding>();
            using (SqlConnection con = new SqlConnection(_masterPassCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MerchantOnboarding merchantOnboarding = new MerchantOnboarding();
                        merchantOnboarding.NewMarchantId = (rdr["NewMarchantId"].ToString());
                        merchantOnboarding.FirstName = (rdr["FirstName"].ToString());
                        merchantOnboarding.LastName = (rdr["LastName"].ToString());
                        merchantOnboarding.DateofBirth = (rdr["DateofBirth"].ToString());
                        merchantOnboarding.PhoneNo = (rdr["PhoneNo"].ToString());
                        merchantOnboarding.AccountNo = (rdr["AccountNo"].ToString());
                        merchantOnboarding.AccountType = (rdr["AccountType"].ToString());
                        merchantOnboarding.CoporateName = (rdr["CoporateName"].ToString());
                        merchantOnboarding.DateApproved = (rdr["DateApproved"].ToString());
                        merchantOnboarding.ExpiryDate = (rdr["ExpiryDate"].ToString());
                        merchantOnboarding.SpooledTime = DateTime.Now;

                        string oraSelect = "select get_account_segment_code(cust_ac_no, branch_code) BusinessSegment " +
                        "from fccngn.sttm_cust_account where cust_ac_no = '" + merchantOnboarding.AccountNo + "'";
                        using (OracleConnection oracon = new OracleConnection(_flexCubeCs))
                        {
                            try
                            {
                                OracleCommand oracleCommand = new OracleCommand(oraSelect, oracon);
                                cmd.CommandTimeout = 0;
                                oracon.Open();
                                OracleDataReader ordr = oracleCommand.ExecuteReader();
                                while (ordr.Read())
                                {
                                    merchantOnboarding.BusinessSegment = (ordr["BusinessSegment"]).ToString();
                                }
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.ToString());
                            }
                        }

                        merchantOnboardings.Add(merchantOnboarding);
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


            return merchantOnboardings;
        }
        private List<Sales> GetSales()
        {

            string sqlSelect = "select count(merchantid)transaction_count, transactionstatus status, " +
"sum(case when transactionstatus = 'Successful' then transactionamount else 0 end) succes_volume, " +
"sum(case when transactionstatus = 'Failed' then transactionamount else 0 end) failed_volume, " +
"convert(varchar,transactiondate,106)creation_date from vw_masterpass_transaction_details " +
"where cast(transactiondate as date) >= cast(getdate()-7 as date) " +
"group by convert(varchar,transactiondate,106), transactionstatus";

            List<Sales> weeklySales = new List<Sales>();
            using (SqlConnection con = new SqlConnection(_masterPass125Cs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Sales weeklySale = new Sales();
                        weeklySale.TotalTransaction = Convert.ToInt32(rdr["transaction_count"].ToString());
                        weeklySale.SalesDate = (rdr["creation_date"].ToString());
                        weeklySale.TotalVolume = Convert.ToDecimal(rdr["succes_volume"].ToString());
                        weeklySale.Status = (rdr["Status"].ToString());
                        weeklySale.SpooledTime = DateTime.Now;

                        weeklySales.Add(weeklySale);
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
            return weeklySales;
        }
        public List<SalesDetails> GetSalesDetails(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, _dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, _dateFormat, CultureInfo.InvariantCulture);

            List<SalesDetails> salesDetails = new List<SalesDetails>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect = "select transactiondate,merchantname,merchantid,terminalid,branch_name,merchantphone,merchant_accountno, " +
                "accountclass,reference Masterpass_Tran_Ref,cba_trn_ref_no,transactionamount,transactionstatus,name_of_issuer_payer,merchantlocation " +
                "from vw_masterpass_transaction_details where cast(transactiondate as date) between '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
                "order by transactiondate asc";

            using (SqlConnection con = new SqlConnection(_masterPass125Cs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        SalesDetails salesDetail = new SalesDetails();
                        salesDetail.Transactiondate = Convert.ToDateTime(rdr["transactiondate"].ToString());
                        salesDetail.Merchantname = (rdr["merchantname"].ToString());
                        salesDetail.Merchantid = (rdr["merchantid"].ToString());
                        salesDetail.Terminalid = (rdr["terminalid"].ToString());
                        salesDetail.BranchName = (rdr["branch_name"].ToString());
                        salesDetail.Merchantphone = (rdr["merchantphone"].ToString());
                        salesDetail.MerchantAccountno = (rdr["merchant_accountno"].ToString());
                        salesDetail.Accountclass = (rdr["accountclass"].ToString());
                        salesDetail.MasterpassTranRef = (rdr["Masterpass_Tran_Ref"].ToString());
                        salesDetail.CbaTrnRefNo = (rdr["cba_trn_ref_no"].ToString());
                        salesDetail.Transactionamount = Convert.ToInt32(rdr["transactionamount"].ToString());
                        salesDetail.Transactionstatus = (rdr["transactionstatus"].ToString());
                        salesDetail.NameOfIssuerPayer = (rdr["name_of_issuer_payer"].ToString());
                        salesDetail.Merchantlocation = (rdr["merchantlocation"].ToString());

                        string oraSelect = "select get_account_segment_code(cust_ac_no, branch_code) BusinessSegment, " +
                        "aekpenyong.get_account_segment_desc(cust_ac_no, branch_code) BusinessSegmentDesc, " +
                        "aekpenyong.get_account_off_code(cust_ac_no,branch_code) RM_CODE, " +
                        "aekpenyong.get_account_offcr_desc(aekpenyong.get_account_off_code(cust_ac_no,branch_code)) RM_NAME " +
                        "from fccngn.sttm_cust_account where cust_ac_no = '" + salesDetail.MerchantAccountno + "'";
                        using (OracleConnection oracon = new OracleConnection(_flexCubeCs))
                        {
                            try
                            {
                                OracleCommand oracleCommand = new OracleCommand(oraSelect, oracon);
                                cmd.CommandTimeout = 0;
                                oracon.Open();
                                OracleDataReader ordr = oracleCommand.ExecuteReader();
                                while (ordr.Read())
                                {
                                    salesDetail.BusinessSegment = (ordr["BusinessSegment"]).ToString();
                                    salesDetail.SegmentDesc = (ordr["BusinessSegmentDesc"]).ToString();
                                    salesDetail.RmCode = (ordr["RM_CODE"]).ToString();
                                    salesDetail.RmName = (ordr["RM_NAME"]).ToString();
                                }
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.ToString());
                            }
                        }

                        salesDetails.Add(salesDetail);
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
            return salesDetails;
        }
        public List<MerchantOnboardingDetails> GetMerchantOnboardingDetails(string startDate, string endDate)
        {
            DateTime startDateTime = DateTime.ParseExact(startDate, _dateFormat, CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, _dateFormat, CultureInfo.InvariantCulture);

            List<MerchantOnboardingDetails> merchantOnboardingDetails = new List<MerchantOnboardingDetails>();

            if (startDateTime > endDateTime)
            {
                return null;
            }

            string sqlSelect = "select NewMarchantId, TerminalID, FirstName, LastName, StreetAddress, DateInitiated, DateApproved, AccountNo, BranchID, " +
                               "PhoneNo, AccountType, " +
                               "CoporateName from MASTERPASSBOARDING_VW where cast(DateInitiated as date) " +
                               "between  '" + startDateTime.ToString("yyyyMMdd") + "' and '" + endDateTime.ToString("yyyyMMdd") + "' " +
                               "order by DateInitiated asc";

            using (SqlConnection con = new SqlConnection(_masterPassCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        MerchantOnboardingDetails merchantOnboardingDetail = new MerchantOnboardingDetails();
                        merchantOnboardingDetail.NewMarchantId = (rdr["NewMarchantId"].ToString());
                        merchantOnboardingDetail.TerminalId = (rdr["TerminalID"].ToString());
                        merchantOnboardingDetail.FirstName = (rdr["FirstName"].ToString());
                        merchantOnboardingDetail.LastName = (rdr["LastName"].ToString());
                        merchantOnboardingDetail.StreetAddress = (rdr["StreetAddress"].ToString());
                        merchantOnboardingDetail.DateInitiated = (rdr["DateInitiated"].ToString());
                        merchantOnboardingDetail.DateApproved = (rdr["DateApproved"].ToString());
                        merchantOnboardingDetail.AccountNo = (rdr["AccountNo"].ToString());
                        merchantOnboardingDetail.BranchCode = (rdr["BranchID"].ToString());
                        merchantOnboardingDetail.PhoneNo = (rdr["PhoneNo"].ToString());
                        merchantOnboardingDetail.AccountType = (rdr["AccountType"].ToString());
                        merchantOnboardingDetail.CoporateName = (rdr["CoporateName"].ToString());

                        string oraSelect = "select account_class, get_account_segment_code(cust_ac_no, branch_code) BusinessSegment, " +
                        "aekpenyong.get_account_segment_desc(cust_ac_no, branch_code) BusinessSegmentDesc, " +
                        "aekpenyong.get_account_off_code(cust_ac_no,branch_code) RM_CODE, " +
                        "aekpenyong.get_account_offcr_desc(aekpenyong.get_account_off_code(cust_ac_no,branch_code)) RM_NAME " +
                        "from fccngn.sttm_cust_account where cust_ac_no = '" + merchantOnboardingDetail.AccountNo + "'";
                        using (OracleConnection oracon = new OracleConnection(_flexCubeCs))
                        {
                            try
                            {
                                OracleCommand oracleCommand = new OracleCommand(oraSelect, oracon);
                                cmd.CommandTimeout = 0;
                                oracon.Open();
                                OracleDataReader ordr = oracleCommand.ExecuteReader();
                                while (ordr.Read())
                                {
                                    merchantOnboardingDetail.AccountClass = (ordr["Account_class"]).ToString();
                                    merchantOnboardingDetail.BusinessSegment = (ordr["BusinessSegment"]).ToString();
                                    merchantOnboardingDetail.SegmentDesc = (ordr["BusinessSegmentDesc"]).ToString();
                                    merchantOnboardingDetail.RmCode = (ordr["RM_CODE"]).ToString();
                                    merchantOnboardingDetail.RmName = (ordr["RM_NAME"]).ToString();
                                }
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.ToString());
                            }
                        }

                        merchantOnboardingDetails.Add(merchantOnboardingDetail);
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
            return merchantOnboardingDetails;
        }
        public Boolean StageMerchantOnboardingData()
        {
            IEnumerable<MerchantOnboarding> merchantOnboardings = GetMerchantOnboardings();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var merchantOnboarding in merchantOnboardings)
                    {
                        const string insQuery = "BEGIN Insert Into merchant_onboarding (NewMarchantId,FirstName,LastName,DateofBirth,PhoneNo,AccountNo,AccountType,CoporateName,DateApproved,ExpiryDate,BusinessSegment,last_spooled_time)" +
                                                "  values (@NewMarchantId,@FirstName,@LastName,@DateofBirth,@PhoneNo,@AccountNo,@AccountType,@CoporateName,@DateApproved,@ExpiryDate,@BusinessSegment,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@NewMarchantId", SqlDbType.Int).Value = merchantOnboarding.NewMarchantId;
                        cmd.Parameters.AddWithValue("@FirstName", SqlDbType.NChar).Value = merchantOnboarding.FirstName;
                        cmd.Parameters.AddWithValue("@LastName", SqlDbType.NChar).Value = merchantOnboarding.LastName;
                        cmd.Parameters.AddWithValue("@DateofBirth", SqlDbType.NChar).Value = merchantOnboarding.DateofBirth;
                        cmd.Parameters.AddWithValue("@PhoneNo", SqlDbType.NVarChar).Value = merchantOnboarding.PhoneNo;
                        cmd.Parameters.AddWithValue("@AccountNo", SqlDbType.NVarChar).Value = merchantOnboarding.AccountNo;
                        cmd.Parameters.AddWithValue("@AccountType", SqlDbType.Char).Value = merchantOnboarding.AccountType;
                        cmd.Parameters.AddWithValue("@CoporateName", SqlDbType.Text).Value = merchantOnboarding.CoporateName;
                        cmd.Parameters.AddWithValue("@DateApproved", SqlDbType.DateTime).Value = merchantOnboarding.DateApproved;
                        cmd.Parameters.AddWithValue("@ExpiryDate", SqlDbType.NChar).Value = merchantOnboarding.ExpiryDate;
                        cmd.Parameters.AddWithValue("@BusinessSegment", SqlDbType.NChar).Value = merchantOnboarding.BusinessSegment;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = merchantOnboarding.SpooledTime;

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
        
        public Boolean StageSalesData()
        {
            IEnumerable<Sales> salesDetails = GetSales();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var salesDetail in salesDetails)
                    {
                        const string insQuery = "BEGIN Insert Into merchant_sales (transaction_count,status,volume,sales_date,last_spooled_time)" +
                                                "  values (@transcount,@status,@volume,@sales_date,@spooledtime); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = salesDetail.TotalTransaction;
                        cmd.Parameters.AddWithValue("@status", SqlDbType.NVarChar).Value = salesDetail.Status;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = salesDetail.TotalVolume;
                        cmd.Parameters.AddWithValue("@sales_date", SqlDbType.NVarChar).Value = salesDetail.SalesDate;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = salesDetail.SpooledTime;

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