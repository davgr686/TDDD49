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
            string connectionString = "Data Source = database.db; Version = 3;";
            SQLiteConnection conn;
            conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static void AddMessage(string message, DateTime dt, string username) //ska ändras till klassen message senare
        {
            SQLiteConnection conn = GetConnection();
            // if table for username not exist -> create table, else insert into existing.

            string sql = "CREATE TABLE IF NOT EXISTS '" + username + "' ( `ID` INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, `DateTime` TEXT, `Message` TEXT)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            //Console.WriteLine(sql);
            command.ExecuteNonQuery();

            AddToUserList(username);

            string insertSql = "INSERT INTO " + username + " (DateTime, Message) VALUES ('" + dt.ToString() + "', '" + message + "')";
            SQLiteCommand command2 = new SQLiteCommand(insertSql, conn);
            //Console.WriteLine(insertSql);
            command2.ExecuteNonQuery();
        }

        public static void AddToUserList(string username)
        {
            SQLiteConnection conn = GetConnection();
            string insertSql = "INSERT OR REPLACE INTO Users (Username) values('" + username + "')";
            SQLiteCommand command = new SQLiteCommand(insertSql, conn);
            //Console.WriteLine(insertSql);
            command.ExecuteNonQuery();
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
                messageHistory.Add("New conversation started " + reader["DateTime"] + "\n" + reader["Message"].ToString());
                //string output = reader["Message"].ToString();
                //messageHistory.Add(output);
            }

            return messageHistory;
        }
    }
}
