using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace eReading
{
    class BookDataBase
    {
        private static BookDataBase instance;
        private static string dbName = "bookdb.db";
        private static string sqlSelectComm = @"SELECT str FROM TestTable WHERE IDs='";
        private static string sqlAddComm = @"INSERT INTO TestTable(IDs,str) VALUES('{0}','{1}')";

        private SQLiteConnection conn = null;
        private SQLiteConnectionStringBuilder connstr = null;
        private SQLiteCommand cmd = null;

        private BookDataBase()
        {
            conn = new SQLiteConnection();
            connstr = new SQLiteConnectionStringBuilder();
            cmd = new SQLiteCommand();
            connstr.DataSource = dbName;
            conn.ConnectionString = connstr.ToString();
            conn.Open();
        }

        public static BookDataBase GetInstance()
        {
            if (instance == null)
            {
                instance = new BookDataBase();
            }
            return instance;
        }

        public string GetBookPID(string dxid)
        {
            cmd.CommandText = sqlSelectComm+dxid+"'";
            cmd.Connection = conn;
            SQLiteDataReader  reader = cmd.ExecuteReader();
            string str = null;
            if (reader.Read())
            {
                str = reader.GetString(0);
            }
            else 
                return null;
            if (str == "")
                return null;
            return GetPIDByStr(str);
            
        }

        public void addNewItem(string ssid, string str)
        {
            cmd.CommandText = String.Format(sqlAddComm, ssid, str);
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
        }

        public string GetPIDByStr(string str)
        {
            string regex = @"(?:pid=(.*?)&)|(?:img\d*/(.*?)/)";
            Regex r = new Regex(regex);
            Match m = r.Match(str);
            if (m.Success)
            {
                if (m.Groups[1].Value != "")
                    return m.Groups[1].Value;
                else
                    return m.Groups[2].Value;
            }
            else
                return null;
        }
    }
}
