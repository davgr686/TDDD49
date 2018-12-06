using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

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
            string insertSql = "INSERT INTO " + username + " (Type, DateTime, Message) VALUES ('Text', '" + dt.ToString() + "', '" + message + "')";
            SQLiteCommand command2 = new SQLiteCommand(insertSql, conn);
            //Console.WriteLine(insertSql);
            command2.ExecuteNonQuery();
        }

        public static void AddImage(byte[] Image, DateTime dt, string username) //ska ändras till klassen message senare
        {
            SQLiteConnection conn = GetConnection();
            //string insertSql = "INSERT INTO " + username + " (Type, DateTime, ImageData) VALUES ('Image', '" + dt.ToString() + "', '" + Image + "')";
            SQLiteCommand cmd = new SQLiteCommand(conn);
            //Console.WriteLine(insertSql);
            cmd.CommandText = "INSERT INTO " + username + " (ImageData, DateTime, Type) VALUES (@img, '" + dt.ToString() + "', 'Image')";
            cmd.Prepare();

            cmd.Parameters.Add("@img", DbType.Binary, Image.Length);
            cmd.Parameters["@img"].Value = Image;
            cmd.ExecuteNonQuery();

        
        }

        public static void InitConvo(string username)
        {
            SQLiteConnection conn = GetConnection();
            // if table for username not exist -> create table, else insert into existing.

            string sql = "CREATE TABLE IF NOT EXISTS '" + username + "' ( `ID` INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,`Type` TEXT,`DateTime` TEXT, `Message` TEXT,`ImageData` BLOB)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            //Console.WriteLine(sql);
            command.ExecuteNonQuery();

            AddToUserList(username);
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

        public static List<Tuple<int, string>> GetHistory(string selectedUser)
        {
            List<Tuple<int, string>> messageHistory = new List<Tuple<int, string>>();

            SQLiteConnection conn = GetConnection();
            string sql = "select * from " + selectedUser;
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                if (reader["Type"].ToString() == "Text")
                {
                    Tuple<int, string> tpl = new Tuple<int, string>(1, reader["DateTime"] + "\n" + reader["Message"].ToString());
                    messageHistory.Add(tpl);
                }

                else
                {
                    string byteToString = System.Text.Encoding.Default.GetString((byte [])reader["ImageData"]);
                    Tuple<int, string> tpl = new Tuple<int, string>(0, byteToString);
                    messageHistory.Add(tpl);
                }
                //string output = reader["Message"].ToString();
                //messageHistory.Add(output);
            }

            return messageHistory;
        }
    }
}
