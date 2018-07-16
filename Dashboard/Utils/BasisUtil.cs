using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace Dashboard.Utils
{
    public class BasisUtil
    {
        private readonly string _basisCS;
        private readonly string _sqlCs;

        public BasisUtil()
        {
            _basisCS = ConfigurationManager.ConnectionStrings["BasisCS"].ConnectionString;
            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        
        private IEnumerable<BasisDetail> GetBasisDetails()
        {

            string sqlSelect =
                " SELECT COLLECTION_NAME COLLECTION, CREDIT_ACCOUNT ACCOUNT, COUNT(TOTAL_AMT) VOLUME, SUM(TOTAL_AMT) VALUE FROM ( " +
                "SELECT B.CREDIT_ACCOUNT, B.TOTAL_AMT, COLLECTION_NAME FROM basis.BAS_ENG_ONLINE_TRANS A, " +
                "(SELECT BASIS_TRAN_REF, CREDIT_ACCOUNT, SUM(AMOUNT) TOTAL_AMT, MIN(VALUE_DATE) VALUE_DATE, COLLECTION_NAME FROM basis.BAS_RR_RECEIPTS " +
                "WHERE STATUS = 'POSTED' AND CREDIT_ACCOUNT_DOMAIN = 'ECOBANK' AND RECEIPT_TYPE IN ('CASH', 'TRANSFER', 'CHEQUE') " +
                "GROUP BY BASIS_TRAN_REF, CREDIT_ACCOUNT, COLLECTION_NAME) B " +
                "WHERE A.TRAN_TYPE = 'COLLECTION' " +
                "AND A.RESP_CODE = '000' " +
                "AND A.ISO_TRAN_ID IS NOT NULL " +
                "AND A.PRC_STATUS = 'Y' " +
                "AND A.BAS_TRAN_REF = B.BASIS_TRAN_REF " +
                "AND A.CREDIT_ACCT = B.CREDIT_ACCOUNT " +
                "AND A.TRAN_AMOUNT = B.TOTAL_AMT " +
                "AND TRUNC(A.VALUE_DATE) = TRUNC(B.VALUE_DATE) " +
                "AND TRUNC(A.VALUE_DATE) = TRUNC(sysdate) " +
                "AND RVSL_FLG = 'N' " +
                "UNION ALL " +
                "SELECT B.CREDIT_ACCOUNT, B.TOTAL_AMT, COLLECTION_NAME FROM basis.BAS_ENG_QUEUE_TRANS A, " +
                "(SELECT BASIS_TRAN_REF, CREDIT_ACCOUNT, SUM(AMOUNT) TOTAL_AMT, MIN(VALUE_DATE) VALUE_DATE, COLLECTION_NAME FROM basis.BAS_RR_RECEIPTS " +
                "WHERE STATUS = 'POSTED' AND CREDIT_ACCOUNT_DOMAIN = 'ECOBANK' AND RECEIPT_TYPE IN ('CASH', 'TRANSFER', 'CHEQUE') " +
                "GROUP BY BASIS_TRAN_REF, CREDIT_ACCOUNT, COLLECTION_NAME) B " +
                "WHERE A.TRAN_TYPE = 'COLLECTION' " +
                "AND A.RESP_CODE = '000' " +
                "AND A.ISO_TRAN_ID IS NOT NULL " +
                "AND A.PRC_STATUS = 'Y' " +
                "AND A.BAS_TRAN_REF = B.BASIS_TRAN_REF " +
                "AND A.CREDIT_ACCT = B.CREDIT_ACCOUNT " +
                "AND A.TRAN_AMOUNT = B.TOTAL_AMT " +
                "AND TRUNC(A.VALUE_DATE) = TRUNC(B.VALUE_DATE) " +
                "AND TRUNC(A.VALUE_DATE) = TRUNC(sysdate) " +
                "AND RVSL_FLG = 'N' " +
                "UNION ALL " +
                "SELECT B.CREDIT_ACCOUNT, B.TOTAL_AMT, COLLECTION_NAME FROM basis.BAS_ENG_QUEUE_TRANS A, " +
                "(SELECT INSTRUMENT_REFERENCE, CREDIT_ACCOUNT, SUM(AMOUNT) TOTAL_AMT, MIN(VALUE_DATE) VALUE_DATE, COLLECTION_NAME FROM basis.BAS_RR_RECEIPTS A,basis.BAS_RR_OUTWARD_CHEQUES B " +
                "WHERE A.CREDIT_ACCOUNT_DOMAIN = 'ECOBANK' AND A.RECEIPT_TYPE = 'OTHER_CHEQ' AND A.OUTWARD_CHQ_ID = B.CHEQUE_ID AND B.POSTED = 'Y' AND B.RVSL_FLG = 'N' " +
                "GROUP BY INSTRUMENT_REFERENCE, CREDIT_ACCOUNT, COLLECTION_NAME) B " +
                "WHERE A.TRAN_TYPE = 'COLLECTION' " +
                "AND A.RESP_CODE = '000' " +
                "AND A.ISO_TRAN_ID IS NOT NULL " +
                "AND A.PRC_STATUS = 'Y' " +
                "AND A.CHEQ_NO = B.INSTRUMENT_REFERENCE " +
                "AND A.CREDIT_ACCT = B.CREDIT_ACCOUNT " +
                "AND A.TRAN_AMOUNT = B.TOTAL_AMT " +
                "AND TRUNC(A.VALUE_DATE) = TRUNC(sysdate) " +
                "AND RVSL_FLG = 'N') " +
                "GROUP BY COLLECTION_NAME, CREDIT_ACCOUNT " +
                "ORDER BY COLLECTION_NAME, CREDIT_ACCOUNT";


            List<BasisDetail> basisDetails = new List<BasisDetail>();
            using (OracleConnection con = new OracleConnection(_basisCS))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(sqlSelect, con);
                    con.Open();
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        BasisDetail basisDetail = new BasisDetail();
                        basisDetail.CodeDescription = rdr["Collection"].ToString();
                        basisDetail.TransactionCount = Convert.ToInt32(rdr["Volume"].ToString());
                        basisDetail.TransactionVolume = Convert.ToDecimal(rdr["Value"].ToString());
                        basisDetail.StateCode = rdr["Account"].ToString();
                        basisDetail.SpooledTime = DateTime.Now;
                        basisDetails.Add(basisDetail);
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
            return basisDetails;
        }

        public List<BasisStateSummary> GetBasisTodayStateSummary()
        {
            List<BasisStateSummary> basisStateSummaries = new List<BasisStateSummary>();
            DateTime lastSpooled = GetLastFetchDateTime();
            string sqlSelect = "select trans_count, description, trans_volume, " +
                               "convert(varchar,spooled_time,120) transdate from basis " +
                               "where convert(varchar,spooled_time,120)=convert(varchar,cast((SELECT TOP 1 spooled_time FROM basis ORDER BY ID DESC) as datetime),120) and " +
                               "convert(varchar,spooled_time,103)=convert(varchar,getdate(),103) group by convert(varchar,spooled_time,120),trans_count,description,trans_volume";


            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlSelect, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var transactionSummary = new BasisStateSummary();

                        transactionSummary.TransactionStateInd = rdr["description"].ToString();
                        transactionSummary.TotalTransaction = Convert.ToInt32(rdr["trans_count"].ToString());
                        if (!rdr["trans_volume"].GetType().Name.Equals("DBNull"))
                        {
                            transactionSummary.TotalTransactionVolume = Convert.ToDecimal(rdr["trans_volume"].ToString());
                        }
                        else
                        {
                            transactionSummary.TotalTransactionVolume = 0;
                        }

                        DateTime datetime = DateTime.Parse(rdr["transdate"].ToString());


                        transactionSummary.TransactionDateTime = datetime;

                        basisStateSummaries.Add(transactionSummary);
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

            return basisStateSummaries;
        }

        private DateTime GetLastFetchDateTime()
        {
            const string sqlSelect = "SELECT TOP 1 spooled_time FROM basis ORDER BY ID DESC ";
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

        public Boolean StageBasisData()
        {
            IEnumerable<BasisDetail> basisDetails = GetBasisDetails();
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                int rowAffected = 0;

                try
                {
                    con.Open();
                    foreach (var basisDetail in basisDetails)
                    {
                        const string insQuery = "BEGIN Insert Into basis (description,account,trans_count,spooled_time,trans_volume)" +
                                                "  values (@description,@account,@transcount,@spooledtime,@trans_volume); END;";
                        SqlCommand cmd = new SqlCommand(insQuery, con);


                        cmd.Parameters.AddWithValue("@transcount", SqlDbType.Int).Value = basisDetail.TransactionCount;
                        cmd.Parameters.AddWithValue("@trans_volume", SqlDbType.Decimal).Value = basisDetail.TransactionVolume;
                        cmd.Parameters.AddWithValue("@description", SqlDbType.NVarChar).Value = basisDetail.CodeDescription;
                        cmd.Parameters.AddWithValue("@account", SqlDbType.NVarChar).Value = basisDetail.StateCode;
                        cmd.Parameters.AddWithValue("@spooledtime", SqlDbType.DateTime).Value = basisDetail.SpooledTime;

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