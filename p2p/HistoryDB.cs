using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace p2p
{
    public static class HistoryDB
    {
        public static SQLiteConnection GetConnection()
        {
            string connectionString = "Data Source =database.db; Version = 3;";
            SQLiteConnection conn;
            conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static List<string> UpdateUserList()
        {
            List<string> userList = new List<string>();

            SQLiteConnection conn = GetConnection();
            string sql = "select * from Users";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                //Console.WriteLine("Detta händer: " + reader["Username"]);
                userList.Add(Convert.ToString(reader["Username"]));
            }
            
            return userList;
        }

        public static List<string> GetHistory(string selectedUser)
        {
            List<string> messageHistory = new List<string>();

            SQLiteConnection conn = GetConnection();
            string sql = "select * from " + selectedUser;
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                //Console.WriteLine("Username: " + reader["Message"]);
                string output = reader["DateTime"] + ": " + reader["Message"];
                messageHistory.Add(output);
            }

            return messageHistory;
        }
    }
}
