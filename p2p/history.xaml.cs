using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;


namespace p2p
{
    /// <summary>
    /// Interaction logic for history.xaml
    /// </summary>
    public partial class history : Window
    {
        public history()
        {
            InitializeComponent();
            UpdateUserList();
        }

        public void UpdateUserList()
        {
            List<string> tmp = HistoryDB.UpdateUserList();
            foreach (var user in tmp)
            {
                UserSelect.Items.Add(user);
                Console.WriteLine(user);
            }

            HistoryDB.AddMessage("YOU: hejsan!", DateTime.Now, "usernametesttesttest");
        }

        private void UserSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chatHistoryBox.Items.Clear();
            string selectedUser = UserSelect.SelectedItem.ToString();
            List<string> chatHistory = HistoryDB.GetHistory(selectedUser);
            foreach (var message in chatHistory)
            {
                chatHistoryBox.Items.Add(message);
                Console.WriteLine(message);
            }

            

        }
    }
}
