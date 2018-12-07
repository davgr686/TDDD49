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
            try
            {
            List<string> tmp = HistoryDB.UpdateUserList();
            string search = searchBox.Text;

            var query_where1 = from a in tmp
                               where a.Contains(search)
                               select a;
            foreach (var a in query_where1)
            {
                    searchResultBox.Items.Add(a);
            }

            }
            catch (Exception ex)
            {
                searchResultBox.Items.Add("There are no users matching your query");
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
                    if (tuple.Item1 == 1)
                    { 
                    chatHistoryBox.Items.Add(tuple.Item2);
                    }
                    else
                    {
                        byte[] img = System.Text.Encoding.Default.GetBytes(tuple.Item2);
                        using (var ms = new System.IO.MemoryStream(img))
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad; // here
                            image.StreamSource = ms;
                            image.DecodePixelHeight = 150;
                            image.DecodePixelWidth = 150;
                            image.EndInit();

                            Image imger = new Image();
                            imger.Source = image;
                            chatHistoryBox.Items.Add(imger);


                        }
                    }
                    //Console.WriteLine(message);
                }
            }
            catch (ArgumentNullException ex)
            {
                p2p.MainWindow.AppWindow.ShowMessage("A problem occured while reading the images from your chat history");
            }

            catch (Exception ex)
            {
                chatHistoryBox.Items.Add("There is no chat history to show");
            }
        }
    }
}
