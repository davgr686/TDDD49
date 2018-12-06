﻿using System;
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
            try
            { 
            string connectionString = "Data Source = database.db; Version = 3;";
            SQLiteConnection conn;
            conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
            }
            catch (SQLiteException se)
            {
                //messagebox "no connection"
                p2p.MainWindow.AppWindow.ShowExcepion(se);
            }
        }

        public static void AddMessage(string message, DateTime dt, string username) //ska ändras till klassen message senare
        {
            SQLiteConnection conn = GetConnection();
            string insertSql = "INSERT INTO " + username + " (Type, DateTime, Message) VALUES ('Text', '" + dt.ToString() + "', '" + message + "')";
            //Console.WriteLine(insertSql);
            try
            {
                SQLiteCommand command2 = new SQLiteCommand(insertSql, conn);
                command2.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(se);
            }
        }

        public static void AddImage(byte[] Image, DateTime dt, string username) //ska ändras till klassen message senare
        {
            try
            { 
            SQLiteConnection conn = GetConnection();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = "INSERT INTO " + username + " (ImageData, DateTime, Type) VALUES (@img, '" + dt.ToString() + "', 'Image')";
            cmd.Prepare();
            cmd.Parameters.Add("@img", DbType.Binary, Image.Length);
            cmd.Parameters["@img"].Value = Image;
            cmd.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(se);
            }


        }

        public static void InitConvo(string username)
        {
            string sql = "CREATE TABLE IF NOT EXISTS '" + username + "' ( `ID` INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,`Type` TEXT,`DateTime` TEXT, `Message` TEXT,`ImageData` BLOB)";
            SQLiteConnection conn = GetConnection();
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
            }
            
            AddToUserList(username);

            catch (SQLiteException se)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(se);
            }
        }

        public static void AddToUserList(string username)
        {
            string insertSql = "INSERT OR REPLACE INTO Users (Username) values('" + username + "')";
            SQLiteConnection conn = GetConnection();

            try
            { 
            SQLiteCommand command = new SQLiteCommand(insertSql, conn);
            command.ExecuteNonQuery();
            }

            catch (SQLiteException se)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(se);
            }
        }

        public static List<string> UpdateUserList()
        {
            List<string> userList = new List<string>();
            try
            { 
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
            catch (SQLiteException ex)
            {
                List<string> userList1 = new List<string>();
                userList1.Add("Something went wrong");
                return userList1;
            }
        }

        public static List<Tuple<int, string>> GetHistory(string selectedUser)
        {
            List<Tuple<int, string>> messageHistory = new List<Tuple<int, string>>();
            string sql = "select * from " + selectedUser;
            try
            { 
            SQLiteConnection conn = GetConnection();
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

            }
            return messageHistory;
            }
            catch (SQLiteException se)
            {
                List<Tuple<int, string>> messageHistoryFail = new List<Tuple<int, string>>();
                Tuple<int, string> tpl = new Tuple<int, string>(1, "Something went wrong");
                messageHistoryFail.Add(tpl);
                return messageHistoryFail;
            }
        }
    }
}
