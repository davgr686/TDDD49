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
using _DataProtocol;
using Newtonsoft.Json;


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
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            searchResultBox.Items.Clear();
            List<string> tmp = HistoryDB.UpdateUserList();
            if (tmp.Any())
            { 
            string search = searchBox.Text;

            var query_where1 = from a in tmp //LINQ 
                               where a.Contains(search)
                               select a;
            
            foreach (var a in query_where1)
            {
                    searchResultBox.Items.Add(a);
            }
            }
            else
            {
                searchResultBox.Items.Add("You have no conversations yet");
            }
        }

        private void searchResultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            { 
                chatHistoryBox.Items.Clear();
                string selectedUser = searchResultBox.SelectedItem.ToString();
                List<Tuple<int, string>> chatHistory = HistoryDB.GetHistory(selectedUser);
                foreach (Tuple<int, string> tuple in chatHistory)
                {
                    if (tuple.Item1 == 1) // if item is text
                    { 
                    chatHistoryBox.Items.Add(tuple.Item2);
                    }
                    else // if item is an image
                    {
                        byte[] img = System.Text.Encoding.Default.GetBytes(tuple.Item2);
                        using (var ms = new System.IO.MemoryStream(img))
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad; 
                            image.StreamSource = ms;
                            image.DecodePixelHeight = 150;
                            image.DecodePixelWidth = 150;
                            image.EndInit();

                            Image imger = new Image();
                            imger.Source = image;
                            chatHistoryBox.Items.Add(imger); 
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                p2p.MainWindow.AppWindow.ShowMessage("Search for a username to display your chat history");
            }
        }
    }
}
