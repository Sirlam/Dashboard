using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dashboard.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/
        private string _sqlCs;
        public UploadController()
        {

            _sqlCs = ConfigurationManager.ConnectionStrings["StageCS"].ConnectionString;
        }
        public ActionResult Index()
        {
            List<string[]> reportUploadList=new List<string[]>();
            string sqlSelect = "select top 50 report_date,insert_time,username,count(*) insert_count from dbo.atm_perform group by report_date,insert_time,username order by insert_time desc";
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = sqlSelect;
                SqlDataReader rdr=cmd.ExecuteReader();
                while (rdr.Read())
                {string[] row=new string[4];
                
                    row[0] = rdr["report_date"].ToString();
                    row[1] = rdr["insert_time"].ToString();
                    row[2] = rdr["username"].ToString();
                    row[3] = rdr["insert_count"].ToString();
                    reportUploadList.Add(row);
                }
            }
            return View(reportUploadList);
        }
        public ActionResult Atm()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AtmDisplay(HttpPostedFileBase file)
        {
            HttpPostedFileBase httpPostedFileBase = Request.Files["file"];
            if (httpPostedFileBase != null && httpPostedFileBase.ContentLength > 0)
            {
                DataSet ds = ReadFileToDataset("24 hrs Gasper Reports", httpPostedFileBase);

                List<string[]> list = new List<string[]>();
                string atmType = "";
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString().IndexOf("Gold") > 0)
                    {
                        atmType = "Gold";
                    }
                    else if (ds.Tables[0].Rows[i][0].ToString().IndexOf("Silver") > 0)
                    {
                        atmType = "Silver";
                    }
                    else if (ds.Tables[0].Rows[i][0].ToString().IndexOf("Bronze") > 0)
                    {
                        atmType = "Bronze";
                    }
                    int res;
                    if (ds.Tables[0].Rows[i][0].ToString().Length > 3)
                        if (int.TryParse(ds.Tables[0].Rows[i][0].ToString().Substring(0, 3), out res))
                        {
                            string[] row = new string[12];
                            for (int j = 0; j < 10; j++)
                            {
                                row[j] = ds.Tables[0].Rows[i][j].ToString();
                            }

                            row[10] = atmType;
                            row[11] = ds.Tables[0].Rows[i][0].ToString().Substring(4, 3);
                            list.Add(row);
                        }
                        else if (ds.Tables[0].Rows[i][0].ToString().Equals("ECOBANK NIGERIA"))
                        {
                            string[] row = new string[12];
                            for (int j = 1; j < 9; j++)
                            {
                                row[j+1] = ds.Tables[0].Rows[i][j].ToString();
                            }

                            row[1] = "SUMMARY";
                            row[10] = atmType;
                            row[11] = "AVG";
                            list.Add(row);
                        }


                }
                return View(list);
            }
            return View();
        }
        [HttpPost]
        public ActionResult SaveReport(string reportDate, string[] data0, string[] data1, string[] data2, string[] data3, string[] data4, string[] data5, string[] data6,
            string[] data7, string[] data8, string[] data9, string[] data10, string[] data11)
        {
            using (SqlConnection con = new SqlConnection(_sqlCs))
            {

                con.Open();
                DateTime insertTime = DateTime.Now;
                string username = User.Identity.Name;
                for (int i = 0; i < data1.Length; i++)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText =
                        string.Format(
                            "Insert into atm_perform(terminal_id,atm_class,branch_code,location,in_service,out_of_service,hard_faults, " +
                            "supply_out,cash_out,comms,closed_mode,replenishment,report_date,insert_time,username) Values(@terminal_id{0},@atm_class{0},@branch_code{0},@location{0},@in_service{0},@out_of_service{0}," +
                            "@hard_faults{0},@supply_out{0},@cash_out{0},@comms{0},@closed_mode{0},@replenishment{0},@report_date{0},@insert_time{0},@username{0});",
                            i);
                    cmd.Parameters.AddWithValue("@terminal_id" + i, data0[i]);
                    cmd.Parameters.AddWithValue("@location" + i, data1[i]);
                    cmd.Parameters.AddWithValue("@in_service" + i, data2[i]);
                    cmd.Parameters.AddWithValue("@out_of_service" + i, data3[i]);
                    cmd.Parameters.AddWithValue("@hard_faults" + i, data4[i]);
                    cmd.Parameters.AddWithValue("@supply_out" + i, data5[i]);
                    cmd.Parameters.AddWithValue("@cash_out" + i, data6[i]);
                    cmd.Parameters.AddWithValue("@comms" + i, data7[i]);
                    cmd.Parameters.AddWithValue("@closed_mode" + i, data8[i]);
                    cmd.Parameters.AddWithValue("@replenishment" + i, data9[i]);
                    cmd.Parameters.AddWithValue("@atm_class" + i, data10[i]);
                    cmd.Parameters.AddWithValue("@branch_code" + i, data11[i]);
                    cmd.Parameters.AddWithValue("@report_date" + i, DateTime.Parse(reportDate));
                    cmd.Parameters.AddWithValue("@insert_time" + i, insertTime);
                    cmd.Parameters.AddWithValue("@username" + i, username);

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public DataSet ReadFileToDataset(string sheetName, HttpPostedFileBase httpPostedFileBase)
        {
            DataSet ds = new DataSet();
            string fileExtension =
                 System.IO.Path.GetExtension(httpPostedFileBase.FileName);

            if (fileExtension == ".xls" || fileExtension == ".xlsx")
            {
                //httpPostedFileBase.FileName
                string fileLocation = Server.MapPath("~/Content/")+"temp"+ fileExtension;
                if (System.IO.File.Exists(fileLocation))
                {

                    System.IO.File.Delete(fileLocation);
                }
                httpPostedFileBase.SaveAs(fileLocation);
                string excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                               fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";
                //connection String for xls file format.
                if (fileExtension == ".xls")
                {
                    excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                            fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                }
                //connection String for xlsx file format.
                else if (fileExtension == ".xlsx")
                {
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";
                }
                //Create Connection to Excel work book and add oledb namespace
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                excelConnection.Open();

                DataTable dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int t = 0;
                //excel data saves in temp file here.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[t] = row["TABLE_NAME"].ToString();
                    t++;
                }
                OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                string excelSheetName = excelSheets[0];
                foreach (var excelSheet in excelSheets.Where(excelSheet => excelSheet.IndexOf(sheetName, StringComparison.InvariantCultureIgnoreCase) > 0))
                {
                    excelSheetName = excelSheet;
                    break;
                }

                string query = string.Format("Select * from [{0}]", excelSheetName);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    dataAdapter.Fill(ds);
                }
//                if (System.IO.File.Exists(fileLocation))
//                {
//
//                    System.IO.File.Delete(fileLocation);
//                }
            }

            return ds;
        }

        public string RemoveInvalidChar(string path)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                path = path.Replace(c.ToString(), ""); // or with "."
            }
            return path;
        }
    }
}
