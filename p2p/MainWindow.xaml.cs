using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using _DataProtocol;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;
using System.Drawing;
using System.Windows.Media;



namespace p2p
{
    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;
        
        
        

        Connection session = new Connection { ConnectorIP = "127.0.0.1", ConnectorPort = "11000", ListenerPort = "11000", Username = "DefaultUsername" };

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            DataContext = session;
        }

        public class Connection
        {
            private string UsernameValue;

            public string Username
            {
                get { return UsernameValue; }
                set { UsernameValue = value; }
            }

            private string ConnectorIPValue;

            public string ConnectorIP
            {
                get { return ConnectorIPValue; }
                set { ConnectorIPValue = value; }
            }

            private string ConnectorPortValue;

            public string ConnectorPort
            {
                get { return ConnectorPortValue; }

                set { ConnectorPortValue = value; }
            }
            private string ListenerPortValue;

            public string ListenerPort
            {
                get { return ListenerPortValue; }

                set { ListenerPortValue = value; }
                
            }
        }


            private SocketCl s;


        public bool AcceptRequestBox(string connectingUsername)
        {
            if (MessageBox.Show("Connection request from: " + connectingUsername + ". \nAccept the request?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return false;
            }
            else
            {
                Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                    new Action(delegate () { Send_button.IsEnabled = true; }));
                return true;
            }
                
        }

        public void ShowExcepion(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        public string GetMyUsername()
        {
            return session.Username;
        }

        public void EnableDisconnectButton()
        {
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { disconnectButton.IsEnabled = true; }));
        }

        public void ShowMessageBoxCLientDecline()
        {
                MessageBox.Show("The client declined your request.");
        }

        public void AcceptedRequest(string username)
        {
            Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Username.IsEnabled = false; }));
            MessageBox.Show(username + " accepted your request.");
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { disconnectButton.IsEnabled = true; }));
            Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Send_button.IsEnabled = true; }));
        }

        public void DisconnectCallback(string username, DateTime convoDT)
        {
            //WriteConvoToDB(username, convoDT);
            Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { Listen_button.IsEnabled = true; }));
            Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { Connect_button.IsEnabled = true; }));
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { disconnectButton.IsEnabled = false; }));
            Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Send_button.IsEnabled = false; }));
            Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Username.IsEnabled = true; Username.IsReadOnly = false; }));
        }

        public void DisplayImg(string username, DateTime timestamp, BitmapImage image)
        { 
            listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                               new Action(delegate () {
                                                                   //listMessage.Items.Add(image);
                                                                   listMessage.Items.Add(timestamp + " " + username + ": Sent you an image");
                                                               }));
            System.Windows.Application.Current.Dispatcher.Invoke(
                 () => {
                     Image imger = new Image();
                     imger.Source = image;
                     listMessage.Items.Add(imger);
                 });

        }



        public void AddMessage(string username, string message, DateTime timestamp)
        {
           listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                              new Action(delegate () { listMessage.Items.Add(timestamp + " " + username + ": " + message); }));
        }

        public void ConnectionBroken()
        {
            MessageBox.Show("Connection broken.");
        }

        public void ConnectionAccepted()
        {
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { disconnectButton.IsEnabled = true; }));
            Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Connect_button.IsEnabled = false; }));
            Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Listen_button.IsEnabled = false; }));
        }

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            
            //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            //IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
            try
            {
                s = new SocketCl();
                s.InitSocket();
                s.Connect(session.ConnectorIP, session.ConnectorPort);
                Username.IsReadOnly = true;
                Username.IsEnabled = false;
                Listen_button.IsEnabled = false;
                Connect_button.IsEnabled = false;

            }
            catch (ArgumentNullException ae)
            {
                MessageBox.Show(ae.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show("No user on the specified IP/Port.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Listen_button_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                s = new SocketCl();
                s.InitSocket();
                s.Listen(session.ListenerPort);
                Username.IsReadOnly = true;
                Username.IsEnabled = false;
                Listen_button.IsEnabled = false;
                Connect_button.IsEnabled = false;
            }
            catch (SocketException se)
            {
                MessageBox.Show("Connection broken.");
                Listen_button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            s.SendMessage(textMessage.Text);
            DateTime timestamp = DateTime.Now;
            listMessage.Items.Add(timestamp + " Me: " + textMessage.Text);
            textMessage.Clear();
        }

        private void chatHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            history histWindow = new history();
            histWindow.Show();
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to disconnect?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            else
            {
                s.SendDisconnect();
                Connect_button.IsEnabled = true;
                disconnectButton.IsEnabled = false;
                Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action(delegate () { Listen_button.IsEnabled = true; }));
            }
                
        }

        private void WriteConvoToDB(string username, DateTime convoDT)
        {
           string conversation = "";
           foreach (string s in listMessage.Items)
           {
                conversation += s + "\n";
           }
           //HistoryDB.AddMessage(conversation, convoDT, username);
            
        }

        private void SendImage_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = PathBox.Text;

                s.SendImage(path);
                PathBox.Clear();
                DateTime timestamp = DateTime.Now;
            listMessage.Items.Add(timestamp + " Me: Sent Image");


             byte[] imgi = System.IO.File.ReadAllBytes(path);
               
             byte[] img = imgi;
                using (var ms = new System.IO.MemoryStream(imgi))
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
                    listMessage.Items.Add(imger);
                    /*JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    string photolocation = "tmper.jpg";  //file name 
                    encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                    using (var filestream = new FileStream(photolocation, FileMode.Create))
                        encoder.Save(filestream);*/


                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".jpg";
            ofd.Filter = "JPG-file (.jpg)|*.jpg";
            if (ofd.ShowDialog() == true)
                {
                string filename = ofd.FileName;
                PathBox.Text = filename;
                }
                SendImage_button.IsEnabled = true;
            }
       
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
