using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace Dashboard.Models.DataAccess
{
    public enum DBConnectionStatus
    {
        NotConnected = 0,
        Connecting = 1,
        Connected = 2,
        ConnectionFailed = 3
    }

    public class OracleAccess
    {

        //public static log4net.ILog logger; 
        private OracleConnection con;// = new OracleConnection();  
        private OracleCommand OraCmd = null;
        private bool handleErrors = false;
        private string strLastError = "";
        private string strConnectionString;
        public Exception Exceptions;
        private DBConnectionStatus _ConnectionStatus;

        public DBConnectionStatus ConnectionStatus
        {
            get
            {
                return _ConnectionStatus;

            }
        }

        public CommandType CommandType
        {
            get
            {
                return OraCmd.CommandType;
            }
            set
            {
                OraCmd.CommandType = value;

            }
        }


        // = "Data Source=10.2.50.140:1521/ngbrn;User ID=helpdesk;Password=Passw0rd"  
        public bool Connect(string ConnStrg)
        {
            bool functionReturnValue = false;
            // Dim cf As New ConfigReg   
            // strConnectionString = cf.DataConnProvider 

            strConnectionString = ConnStrg;

            try
            {
                con = new OracleConnection();
                con.ConnectionString = strConnectionString;
                con.Open();
                OraCmd = new OracleCommand();
                OraCmd.CommandType = CommandType.Text;
                OraCmd.Connection = con;
                OraCmd.CommandTimeout = 0;
                _ConnectionStatus = DBConnectionStatus.Connected;
            }
            catch (Exception ex)
            {
                Exceptions = ex;
                strLastError = ex.Message;
                _ConnectionStatus = DBConnectionStatus.ConnectionFailed;
            }
            if (con.State == ConnectionState.Open)
                functionReturnValue = true;
            return functionReturnValue;
        }

        public bool Connect()
        {
            bool functionReturnValue = false;
            // Dim cf As New ConfigReg   
            // strConnectionString = cf.DataConnProvider 
            con = new OracleConnection();
            strConnectionString = ConfigurationManager.ConnectionStrings["MacallaCS"].ConnectionString;
            con.ConnectionString = strConnectionString;
            try
            {
                con.Open();
                OraCmd = new OracleCommand();
                OraCmd.CommandType = CommandType.Text;
                OraCmd.Connection = con;
                OraCmd.CommandTimeout = 0;
                _ConnectionStatus = DBConnectionStatus.Connected;
            }
            catch (Exception ex)
            {
                strLastError = ex.Message.ToString();
                _ConnectionStatus = DBConnectionStatus.ConnectionFailed;
            }
            if (con.State == ConnectionState.Open)
                functionReturnValue = true;
            return functionReturnValue;
        }
        public bool Disconnect()
        {
            con.Close();
            con = null;
            _ConnectionStatus = DBConnectionStatus.NotConnected;
            return true;
        }
        public OracleAccess()
        {
        }
        public OracleConnection Connection
        {
            get
            {
                return con;
            }
        }
        public OracleDataReader ExecuteReader()
        {
            OracleDataReader reader = null;
            try
            {
                this.Open();
                reader = OraCmd.ExecuteReader();
            }
            catch (OracleException ex)
            {
                if (handleErrors)
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw ex;
                }
            }
            return reader;
        }
        public IDataReader ExecuteReader(string commandtext)
        {
            OracleDataReader reader = null;
            try
            {
                OraCmd.CommandText = commandtext;
                reader = this.ExecuteReader();
            }
            catch (Exception ex)
            {
                if ((handleErrors))
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return reader;
        }
        public object ExecuteScalar()
        {
            object obj = null;
            try
            {
                this.Open(); obj = OraCmd.ExecuteScalar();
                this.Close();
            }
            catch (Exception ex)
            {
                if (handleErrors)
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return obj;
        }

        public object ExecuteScalar(string commandtext)
        {
            object obj = null;
            try
            {
                OraCmd.CommandText = commandtext;
                obj = this.ExecuteScalar();
            }
            catch (Exception ex)
            {
                if ((handleErrors)) { strLastError = ex.Message; }
                else
                {
                    throw;
                }
            }
            return obj;
        }
        public int ExecuteNonQuery()
        {
            int i = -1;
            try
            {
                this.Open(); i = OraCmd.ExecuteNonQuery(); this.Close();
            }
            catch (Exception ex)
            {
                if (handleErrors)
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return i;
        }

        public int ExecuteNonQuery(string commandtext)
        {
            int i = -1;
            try
            {
                OraCmd.CommandText = commandtext; i = this.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (handleErrors)
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return i;
        }

        public DataSet ExecuteDataSet()
        {
            //  Dim db As New Flexcube   
            // db.Connect()  
            OracleDataAdapter da = default(OracleDataAdapter);
            da = new OracleDataAdapter(OraCmd.CommandText, con);
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                da.Fill(ds);
                if (ds == null)
                {
                    return null;
                }
                else
                {
                    return ds;
                }
            }
            catch (Exception ex)
            {
                if ((handleErrors))
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return ds;

        }


        public DataSet ExecuteDataSet(string commandtext)
        {
            DataSet ds = new DataSet();
            try
            {
                OraCmd.CommandText = commandtext;
                ds = this.ExecuteDataSet();
            }
            catch (Exception ex)
            {
                if (handleErrors)
                {
                    strLastError = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            return ds;
        }


        public string CommandText
        {
            get
            {
                return OraCmd.CommandText;
            }
            set
            {
                OraCmd.CommandText = value;
                OraCmd.Parameters.Clear();
            }
        }


        public IDataParameterCollection Parameters
        {
            get
            {
                return OraCmd.Parameters;
            }
        }


        public void AddParameter(string paramname, object paramvalue)
        {
            OracleParameter param = new OracleParameter(paramname, paramvalue);
            OraCmd.Parameters.Add(param);
        }

        public void AddParameter(string paramname, OracleType type, int size, object paramvalue, ParameterDirection direction)
        {
            //OracleParameter param = new OracleParameter(paramname,   type,  size, direction,   direction);
            OracleParameter param = new OracleParameter(paramname, paramvalue);


            OraCmd.Parameters.Add(param);
        }
        public void AddParameter(string paramname, OracleType type, ParameterDirection direction)
        {
            //        OracleParameter param = new OracleParameter(paramname, type,  direction);
            OracleParameter param = new OracleParameter(paramname, type);


            OraCmd.Parameters.Add(param);
        }
        public void AddParameter(IDataParameter param)
        {
            OraCmd.Parameters.Add(param);

        }

        public string ConnectionString
        {
            get
            {
                return strConnectionString;
            }
            set
            {
                strConnectionString = value;
            }
        }

        private void Open()
        {
            if (OraCmd.Connection.State == ConnectionState.Closed)
                OraCmd.Connection.Open();
            _ConnectionStatus = DBConnectionStatus.Connected;
        }
        private void Close()
        {
            if (OraCmd.Connection.State != ConnectionState.Closed)
                OraCmd.Connection.Close();
            _ConnectionStatus = DBConnectionStatus.NotConnected;
        }

        public bool HandleExceptions
        {
            get { return handleErrors; }
            set { handleErrors = value; }
        }

        public string LastError
        {
            get
            {
                return strLastError;
            }
        }

        public void Dispose()
        {
            if (OraCmd != null) OraCmd.Dispose();
        }

    }
}