using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Dev_Tool
{
    static public class DAL
    {
        public static DataTable GetOracleData(string Query_Oracle, string ConnectStr = "Default")
        {
            DataTable dt = new DataTable();
            string conn;
            
            if (ConnectStr == "Default")
            {
                conn = @"Data Source = ";
            }
            else
            {
                conn = ConnectStr;
            }
            OracleConnection con = new OracleConnection();
            con.ConnectionString = conn;
            con.Open();

            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = Query_Oracle;
            OracleDataReader reader = cmd.ExecuteReader();

            dt.Load(reader);
            return dt;
        }
        public static DataTable GetDBData(string Query, string ConnectStr = "PRD")
        {
            DataTable dt = new DataTable();

            SqlConnection conn = new SqlConnection(GetDBConnectStr(ConnectStr));
            conn.Open();
            SqlCommand cmd = new SqlCommand(Query, conn);
            cmd.CommandTimeout = 600000;
            if (!string.IsNullOrEmpty(Query))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }
        public static DataTable GetDataThroughStoreProcedure(string SP_Name, string ConnectStr = "PRD", int Timeout_Minute = 600)
        {
            SqlDataAdapter MyAdapter = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand(SP_Name)
            {
                CommandType = CommandType.StoredProcedure,
                Connection = new SqlConnection(GetDBConnectStr(ConnectStr)),
                CommandTimeout = Timeout_Minute
            };

            MyAdapter.SelectCommand = cmd;
            DataTable DT = new DataTable();
            MyAdapter.Fill(DT);

            return DT;
        }
        public static DataTable GetDataThroughStoreProcedure(string SP_Name, string ConnectStr = "PRD", List<KeyValuePair<string, string>> Params = null, int Timeout_Minute = 600)
        {
            SqlDataAdapter MyAdapter = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand(SP_Name)
            {
                CommandType = CommandType.StoredProcedure,
                Connection = new SqlConnection(GetDBConnectStr(ConnectStr)),
                CommandTimeout = Timeout_Minute
            };

            if (Params != null)
            {
                foreach (KeyValuePair<string, string> item in Params)
                {
                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            MyAdapter.SelectCommand = cmd;
            DataTable DT = new DataTable();
            MyAdapter.Fill(DT);

            return DT;
        }
        public static DataTable GetDataThroughStoreProcedure(string SP_Name, string ConnectStr = "PRD", List<KeyValuePair<string, object>> Params = null, int Timeout_Minute = 600)
        {
            SqlDataAdapter MyAdapter = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand(SP_Name)
            {
                CommandType = CommandType.StoredProcedure,
                Connection = new SqlConnection(GetDBConnectStr(ConnectStr)),
                CommandTimeout = Timeout_Minute
            };

            if (Params != null && Params.Count > 0)
            {
                foreach (KeyValuePair<string, object> item in Params)
                {
                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            MyAdapter.SelectCommand = cmd;
            DataTable DT = new DataTable();
            MyAdapter.Fill(DT);

            return DT;
        }
        public static void Bulk_Insert(DataTable dt, string _Destination_Table, string ConnectStr = "PRD")
        {
            string DT_Col_Name;

            SqlBulkCopy BulkSQL = new SqlBulkCopy(GetDBConnectStr(ConnectStr), SqlBulkCopyOptions.FireTriggers)
            {
                DestinationTableName = _Destination_Table,
                BulkCopyTimeout = 60000000,
                BatchSize = 50000
            };

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DT_Col_Name = dt.Columns[i].ColumnName;

                BulkSQL.ColumnMappings.Add(DT_Col_Name, DT_Col_Name);
            }
            BulkSQL.WriteToServer(dt);
        }
        public static void RunQuery(string Query, string ConnectStr = "PRD")
        {
            SqlConnection conn = new SqlConnection(GetDBConnectStr(ConnectStr));
            conn.Open();
            SqlCommand cmd = new SqlCommand(Query, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public static string GetDBConnectStr(string ConnectStr)
        {
            string _connectStr;

            if (ConnectStr.ToUpper() == "PRD")
            {
                //below is the production connection string
                _connectStr = @"Data Source = ";
            }
            else if (ConnectStr.ToUpper() == "QAS")
            {
                //below is the qas connection string
                _connectStr = @"Data Source = ";
            }
            else if (!string.IsNullOrEmpty(ConnectStr))
            {
                //below is the customer connection string
                _connectStr = ConnectStr;
            }
            else
            {
                _connectStr = "";
            }

            return _connectStr;
        }
    }
}
