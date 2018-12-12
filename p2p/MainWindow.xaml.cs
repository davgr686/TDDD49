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
        private SocketCl s;
        private bool disconnectcheck = true;
        Connection session = new Connection { ConnectorIP = "127.0.0.1", ConnectorPort = "11000", ListenerPort = "11000", Username = "DefaultUsername" };

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            DataContext = session;
        }


        public bool AcceptRequestBox(string connectingUsername)
        {
            if (MessageBox.Show("Connection request from: " + connectingUsername + " \nAccept the request?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return false;
            }
            else
            {
                Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                    new Action(delegate () { Send_button.IsEnabled = true; }));
                
                SendImage_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                    new Action(delegate () { SendImage_button.IsEnabled = true; }));
                return true;
            }
        }

        public void ShowExcepion(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        public void ShowMessage(string output)
        {
            MessageBox.Show(output);
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
                MessageBox.Show("The client declined your request");
        }

        public void AcceptedRequest(string username)
        {
            Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Username.IsEnabled = false; }));
            MessageBox.Show(username + " accepted your request");
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { disconnectButton.IsEnabled = true; }));
            Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Send_button.IsEnabled = true; }));
            SendImage_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { SendImage_button.IsEnabled = true; }));
        }

        public void DisconnectCallback()
        {
            if (disconnectcheck)
            {
                MessageBox.Show("The user you were chatting with disconnected.");
            }
            Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { Listen_button.IsEnabled = true; }));
            Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { Connect_button.IsEnabled = true; }));
            disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new Action(delegate () { disconnectButton.IsEnabled = false; }));
            Send_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Send_button.IsEnabled = false; }));
            SendImage_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { SendImage_button.IsEnabled = false; }));
            Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate () { Username.IsEnabled = true; Username.IsReadOnly = false; }));
        }

        public void DisplayImg(string username, DateTime timestamp, BitmapImage image)
        { 
            listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                               new Action(delegate () {
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
            try
            {
                s = new SocketCl();
                s.InitSocket();
                int success = s.Connect(session.ConnectorIP, session.ConnectorPort);
                if (success == 1)
                { 
                    Username.IsReadOnly = true;
                    Username.IsEnabled = false;
                    Listen_button.IsEnabled = false;
                    Connect_button.IsEnabled = false;
                }
            }
                
            catch (ArgumentNullException ae)
            {
                MessageBox.Show(ae.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show("No user on the specified IP/Port.");
            }

        }

        private void Listen_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s = new SocketCl();
                s.InitSocket();
                int success =s.Listen(session.ListenerPort);
                if (success == 1)
                { 
                Username.IsReadOnly = true;
                Username.IsEnabled = false;
                Listen_button.IsEnabled = false;
                Connect_button.IsEnabled = false;
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show("Connection broken.");
                Listen_button.IsEnabled = true;
            }
        }


        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            s.SendMessage(textMessage.Text);
            listMessage.Items.Add(DateTime.Now + " Me: " + textMessage.Text);
            textMessage.Clear();
        }

        private void chatHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            history histWindow = new history();
            histWindow.Show();
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            disconnectcheck = false;
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


        private void SendImage_button_Click(object sender, RoutedEventArgs e)
        {   
            try
            {
                string path = "";
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = ".jpg";
                ofd.Filter = "JPG-file (.jpg)|*.jpg";
                if (ofd.ShowDialog() == true)
                {
                    path = ofd.FileName;
                }
                if (path != "")
                { 
                    s.SendImage(path);
                    byte[] img = System.IO.File.ReadAllBytes(path);
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
                        listMessage.Items.Add(DateTime.Now + " Me: Sent Image");
                        listMessage.Items.Add(imger);
                        }
                }

            }
            catch (ArgumentException aex)
            {
                MessageBox.Show("Something went wrong while trying to send the image");
            }
        }
    }
}
