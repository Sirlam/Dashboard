using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class OnBoardingUtil
    {
        private readonly string _flexCubeCs;
        private readonly string _sqlCs;
        private readonly string _ribCs;

        public OnBoardingUtil()
        {
            _ribCs = ConfigurationManager.ConnectionStrings["RibCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
            _flexCubeCs = ConfigurationManager.ConnectionStrings["FlexBranchCS"].ConnectionString;

        }

        private IEnumerable<OnBoarding> GetCardIssued()
        {
            string sqlSelect =
                "SELECT get_account_segment_code(foracid,substr(foracid,1,3)) segment,substr(pan2,1,6) card_number,count(pan2) card_count,'VISA' card_type FROM indevapp.DBCARD_CARDS where trunc(cre_date)=trunc(sysdate-1) " +
                "group by substr(pan2,1,6),get_account_segment_code(foracid,substr(foracid,1,3)) " +
                "union all " +
                "SELECT get_account_segment_code(account_no,substr(account_no,1,3)) segment,substr(card_number2,1,6) card_number,count(card_number2) card_count,'MASTERCARD' card_type FROM indevapp.ECO_MC_PROCESSED_CARDS q,indevapp.ECO_MC_CUST_DETAILS b  where  q.application_no=b.application_no and trunc(processed_date) = trunc(sysdate-1) " +
                "group by substr(card_number2,1,6),get_account_segment_code(account_no,substr(account_no,1,3)) " +
                "union all " +
                "SELECT get_account_segment_code(account_no,substr(account_no,1,3)) segment,substr(card_number2,1,6) card_number,count(card_number2) card_count,'VERVE' FROM indevapp.VPC_PROCESSED_CARDS q,indevapp.VPC_CUST_DETAILS b where q.application_no=b.application_no and trunc(processed_date) = trunc(sysdate-1) " +
                "group by substr(card_number2,1,6),get_account_segment_code(account_no,substr(account_no,1,3))";

            List<OnBoarding> onBoardings = new List<OnBoarding>();
            using (OracleConnection con = new OracleConnection(_flexCubeCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        OnBoarding onBoarding = new OnBoarding();
                        onBoarding.SegmentCode = rdr["segment"].ToString();
                        onBoarding.CardType = rdr["card_type"].ToString();
                        onBoarding.CardNumber = rdr["card_number"].ToString();
                        onBoarding.CardCount = Convert.ToInt32((rdr["card_count"].ToString()));
                        onBoarding.ReportDate = DateTime.Now.AddDays(-1);
                        onBoarding.SpoonDateTime = spoolDateTime;

                        onBoardings.Add(onBoarding);
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


            return onBoardings;
        }

        public int StageCardIssuedData()
        {
            IEnumerable<OnBoarding> onBoardings = GetCardIssued();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var onBoarding in onBoardings)
                    {
                        const string insQuery =
                            "BEGIN Insert Into card_issued (card_type,card_number,segment_code,card_count,report_date,spool_time)" +
                            "  values (@cardtype,@carddnumber,@segmentcode,@cardcount,@reportdate,@spoon_date); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@cardtype", onBoarding.CardType);
                        cmd.Parameters.AddWithValue("@carddnumber", SqlDbType.NVarChar).Value = onBoarding.CardNumber;
                        cmd.Parameters.AddWithValue("@cardcount", SqlDbType.Int).Value = onBoarding.CardCount;
                        cmd.Parameters.AddWithValue("@segmentcode", SqlDbType.NVarChar).Value = onBoarding.SegmentCode;
                        cmd.Parameters.AddWithValue("@reportdate", SqlDbType.DateTime).Value = onBoarding.ReportDate;
                        cmd.Parameters.AddWithValue("@spoon_date", SqlDbType.DateTime).Value = onBoarding.SpoonDateTime;

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

        public List<MerchantSummary> GetCardIssuedFor7Days()
        {
            List<MerchantSummary> merchantSummaries = new List<MerchantSummary>();

            string sqlSelect =
                "select sum(card_count) CardCount, convert(varchar,report_date,106) ReportDate, " +
                "sum (case when left(segment_code,2) = 'CM' then card_count else 0 end) CM,  " +
                "sum (case when left(segment_code,2) = 'CS' then card_count else 0 end) CS,  " +
                "sum (case when left(segment_code,2) = 'DB' then card_count else 0 end) DB, " +
                "sum (case when left(segment_code,2)in ('CM','CS','DB') and segment_code is not null then 0 else 1 end) Unknown   " +
                "from card_issued a  " +
                "where convert(varchar(16),spool_time,120) = convert(varchar(16),cast((SELECT TOP 1 spool_time FROM card_issued  " +
                "WHERE convert(varchar,spool_time,112)=convert(varchar, a.spool_time, 112)  ORDER BY spool_time DESC) as datetime),120)  " +
                "and convert(varchar,report_date,112) >=convert(varchar,GETDATE()-7,112) " +
                "group by convert(varchar,report_date,106)  " +
                "order by convert(varchar,report_date,106) asc";
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
                            MerchantCount = Convert.ToInt32(rdr["CardCount"].ToString()),
                            DateApproved = (rdr["ReportDate"].ToString()),
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
 private IEnumerable<OnBoarding> GetRibOnboarding()
        {
            string sqlSelect =
                " select count(customernumber) rib_count from ribuser.CUSTOMERS where loginstatus='active' and trunc(datecreated) =trunc(sysdate)-1";

            List<OnBoarding> onBoardings = new List<OnBoarding>();
            using (OracleConnection con = new OracleConnection(_ribCs))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    DateTime spoolDateTime = DateTime.Now;
                    while (rdr.Read())
                    {
                        OnBoarding onBoarding = new OnBoarding();
                        onBoarding.CardCount = Convert.ToInt32((rdr["rib_count"].ToString()));
                        onBoarding.ReportDate = DateTime.Now.AddDays(-1);
                        onBoarding.SpoonDateTime = spoolDateTime;

                        onBoardings.Add(onBoarding);
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


            return onBoardings;
        }

        public int StageRibData()
        {
            IEnumerable<OnBoarding> onBoardings = GetRibOnboarding();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var onBoarding in onBoardings)
                    {
                        const string insQuery =
                            "BEGIN Insert Into rib_onboard (rib_count,report_date,spool_time)" +
                            "  values (@ribcount,@reportdate,@spooldate); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@ribcount", SqlDbType.Int).Value = onBoarding.CardCount;
                        cmd.Parameters.AddWithValue("@reportdate", SqlDbType.DateTime).Value = onBoarding.ReportDate;
                        cmd.Parameters.AddWithValue("@spooldate", SqlDbType.DateTime).Value = onBoarding.SpoonDateTime;

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

        public List<MerchantSummary> GetRibFor7Days()
        {
            List<MerchantSummary> merchantSummaries = new List<MerchantSummary>();

            string sqlSelect =
                "select sum(rib_count) RibCount, convert(varchar,report_date,106) ReportDate " +
                "from rib_onboard a " +
                "where convert(varchar(16),spool_time,120) = convert(varchar(16),cast((SELECT TOP 1 spool_time FROM rib_onboard " +
                "WHERE convert(varchar,spool_time,112)=convert(varchar, a.spool_time, 112)  ORDER BY spool_time DESC) as datetime),120)  " +
                "and convert(varchar,report_date,112) >=convert(varchar,GETDATE()-7,112) " +
                "group by convert(varchar,report_date,106) " +
                "order by convert(varchar,report_date,106) asc";
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
                            MerchantCount = Convert.ToInt32(rdr["RibCount"].ToString()),
                            DateApproved = (rdr["ReportDate"].ToString())
                            
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


        private IEnumerable<CasaDeposits> GetCasaDeposits()
        {
            string sqlSelect = "SELECT count(1) total, b.trn_dt transaction_date, " +
                               "decode(a.ACCOUNT_TYPE, 'U', 'CURRENT', 'S', 'SAVINGS', 'OTHERS') ACCOUNT_TYPE, " +
                               "SUM(DECODE(DRCR_IND,'C',DECODE(b.ac_ccy,'NGN',lcy_amount,fcy_amount))) CREDIT_TURNOVER " +
                               "FROM fccngn.sttm_cust_account a,fccngn.acvw_all_ac_entries b,fccngn.sttm_trn_code c " +
                               "WHERE a.cust_ac_no=b.ac_no " +
                               "and b.trn_code=c.trn_code " +
                               "and b.trn_dt = to_char(sysdate -1, 'dd-Mon-yyyy') " +
                               "and a.record_stat = 'O' " +
                               "AND C.CONSIDER_FOR_ACTIVITY='Y' " +
                               "group by a.ACCOUNT_TYPE, b.trn_dt ";

            List<CasaDeposits> casaDeposits = new List<CasaDeposits>();

            using (OracleConnection con = new OracleConnection(_flexCubeCs))
            {
                try
                {
                    con.Open();
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    OracleDataReader ordr = cmd.ExecuteReader();
                    while (ordr.Read())
                    {
                        CasaDeposits casaDeposit = new CasaDeposits();
                        casaDeposit.Count = Convert.ToInt32(ordr["total"]);
                        casaDeposit.Volume = Convert.ToDecimal(ordr["CREDIT_TURNOVER"]);
                        casaDeposit.AccountType = (ordr["ACCOUNT_TYPE"]).ToString();
                        casaDeposit.SegementCode = "";
                        casaDeposit.BusinessSegment = "";
                        casaDeposit.TransactionDateTime = Convert.ToDateTime(ordr["transaction_date"]);
                        casaDeposit.SpooledTime = DateTime.Now;
                        casaDeposits.Add(casaDeposit);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return casaDeposits;
        }

        public List<CasaSummary> GetCasaSummary()
        {
            List<CasaSummary> casaSummaries = new List<CasaSummary>();

            string sqlSelect =
                "select sum(total) total, sum(volume) volume, convert(varchar,transaction_date,106)transaction_date, " +
                "sum(case when account_type='SAVINGS' then total else 0 end ) SAVINGS_count, " +
                "sum(case when account_type='CURRENT' then total else 0 end ) CURRENT_count, " +
                "sum(case when account_type='OTHERS' then total else 0 end ) OTHERS_count, " +
                "sum(case when account_type='SAVINGS' then volume else 0 end ) SAVINGS_volume, " +
                "sum(case when account_type='CURRENT' then volume else 0 end ) CURRENT_volume, " +
                "sum(case when account_type='OTHERS' then volume else 0 end ) OTHERS_volume " +
                "from casa a  " +
                "where convert(varchar(16),last_spooled_time,120) = convert(varchar(16),cast((SELECT TOP 1 last_spooled_time FROM casa  " +
                "WHERE convert(varchar,last_spooled_time,112)=convert(varchar, a.last_spooled_time, 112) and convert(varchar,transaction_date,112)=convert(varchar, a.transaction_date, 112) ORDER BY ID DESC) as datetime),120)  " +
                "and convert(varchar,transaction_date,112) >=convert(varchar,GETDATE()-7,112)  " +
                "and convert(varchar,last_spooled_time,112) between convert(varchar,GETDATE()-7,112) and convert(varchar,GETDATE(),112)  " +
                "group by convert(varchar,transaction_date,106)  " +
                "order by transaction_date asc ";


            using (SqlConnection con = new SqlConnection(_sqlCs) )
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    cmd.CommandTimeout = 0;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        CasaSummary casaSummary = new CasaSummary();
                        casaSummary.Count = Convert.ToInt32(rdr["total"]);
                        casaSummary.Volume = Convert.ToDecimal(rdr["volume"]);
                        casaSummary.SavingsCount = Convert.ToInt32(rdr["SAVINGS_count"]);
                        casaSummary.CurrentCount = Convert.ToInt32(rdr["CURRENT_count"]);
                        casaSummary.OthersCount = Convert.ToInt32(rdr["OTHERS_count"]);
                        casaSummary.SavingsVolume = Convert.ToDecimal(rdr["SAVINGS_volume"]);
                        casaSummary.CurrentVolume = Convert.ToDecimal(rdr["CURRENT_volume"]);
                        casaSummary.OthersVolume = Convert.ToDecimal(rdr["OTHERS_volume"]);
                        casaSummary.TransactionDate = (rdr["transaction_date"]).ToString();
                        casaSummaries.Add(casaSummary);
;                    }
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
            return casaSummaries;
        } 
        public Boolean StageCasaDeposits()
        {
            IEnumerable<CasaDeposits> casaDeposits = GetCasaDeposits();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;
                try
                {
                    con.Open();
                    foreach (var casaDeposit in casaDeposits)
                    {
                        const string insQuery =
                            "BEGIN INSERT into casa(total, volume, account_type, segment_code, business_segment, transaction_date, last_spooled_time) " +
                            "VALUES (@total, @volume, @account_type, @segment_code, @business_segment, @transaction_date, @last_spooled_time); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);

                        cmd.Parameters.AddWithValue("@total", SqlDbType.Int).Value = casaDeposit.Count;
                        cmd.Parameters.AddWithValue("@volume", SqlDbType.Decimal).Value = casaDeposit.Volume;
                        cmd.Parameters.AddWithValue("@account_type", SqlDbType.VarChar).Value = casaDeposit.AccountType;
                        cmd.Parameters.AddWithValue("@segment_code", SqlDbType.VarChar).Value = casaDeposit.SegementCode;
                        cmd.Parameters.AddWithValue("@business_segment", SqlDbType.VarChar).Value =
                            casaDeposit.BusinessSegment;
                        cmd.Parameters.AddWithValue("@transaction_date", SqlDbType.DateTime).Value =
                            casaDeposit.TransactionDateTime;
                        cmd.Parameters.AddWithValue("@last_spooled_time", SqlDbType.DateTime).Value =
                            casaDeposit.SpooledTime;

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