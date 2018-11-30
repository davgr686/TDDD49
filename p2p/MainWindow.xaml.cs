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



namespace p2p
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Socket s;
        private string connectedUsername;
        private DateTime convoDT;
        private bool connectionAccepted = false;

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
            try
            {
                s.BeginConnect(ipe, new AsyncCallback(ConnectCallback), s);
                Username.IsEnabled = false;
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

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket connector = (Socket)ar.AsyncState;
                connector.EndConnect(ar);
                string userName = (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text));
                
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(userName);
                int bytesSent = s.Send(msg);


                /* Outgoing connection request accepted or declined */
                byte[] acceptDecline = new byte[1024 * 5000];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                String data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                DataProtocol response = JsonConvert.DeserializeObject<DataProtocol>(data);
                if (response.Type == "connectionDeclined")
                {
                    MessageBox.Show("The client declined your request.");
                    connectionAccepted = false;
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                    //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    connectionAccepted = true;
                    connectedUsername = response.Username;
                    convoDT = DateTime.Now;
                    Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { Username.IsEnabled = false; }));
                    MessageBox.Show(response.Username + " accepted your request.");
                    disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { disconnectButton.IsEnabled = true; }));
                }
                /* Outgoing connection request accepted or declined */

                /* Read messages */
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[1024 * 5000];
                    int bytesRec = connector.Receive(bytes);
                    if (connector.Connected && bytesRec != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        DataProtocol responseMessage = JsonConvert.DeserializeObject<DataProtocol>(data);
                        DateTime timestamp = DateTime.Now;

                        if (responseMessage.Type == "disconnect")
                        {
                            DataProtocol disconnect = new DataProtocol("disconnect", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "Disconnected", new byte[1]);
                            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                            int byteSent = s.Send(disconnectMsg);
                            connectionAccepted = false;
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            WriteConvoToDB();
                            //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action(delegate () { Listen_button.IsEnabled = true; }));
                            Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(delegate () { Connect_button.IsEnabled = true; }));
                        }

                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;

                            using (var ms = new System.IO.MemoryStream(img))
                            {
                                var image = new BitmapImage();
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad; // here
                                image.StreamSource = ms;
                                image.EndInit();

                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                string photolocation = "tmper.jpg";  //file name 
                                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                                using (var filestream = new FileStream(photolocation, FileMode.Create))
                                    encoder.Save(filestream);
                                listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                               new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": Sent you an image");
                                                                                        
                                                               }));
                            }
                            
                        }

                        else
                        { 

                        listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
                        }
                    }
                    else
                        break;
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
                
               // s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                MessageBox.Show("Connection broken.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Listen_button_Click(object sender, RoutedEventArgs e)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(textLocalPort.Text));
            Listen_button.IsEnabled = false;
            try
            {
                s.Bind(localEndPoint);
                s.Listen(10);
                s.BeginAccept(new AsyncCallback(ListenCallback), s);
            }
            catch (SocketException se)
            {
                connectionAccepted = false;
                MessageBox.Show("Connection broken.");
                Listen_button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ListenCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                s = handler;
                byte[] bytes = new byte[1024 * 5000];
                int bytesRec = handler.Receive(bytes);
                string currUser = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                connectedUsername = currUser;
                convoDT = DateTime.Now;
                //MessageBox.Show(currUser);


                /* Accept or decline incoming connection request */
                
                if (MessageBox.Show("Connection request from: " + currUser + ". \nAccept the request?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    connectionAccepted = false;
                    DataProtocol declineRequest = new DataProtocol("connectionDeclined", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "null", new byte[1]);
                    string jsonDeclineRequest = JsonConvert.SerializeObject(declineRequest);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonDeclineRequest);
                    int bytesSent = s.Send(msg);

                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                    //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    connectionAccepted = true;
                    disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { disconnectButton.IsEnabled = true; }));
                    Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { Connect_button.IsEnabled = false; }));
                    Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () { Listen_button.IsEnabled = false; }));
                    DataProtocol acceptRequest = new DataProtocol("connectionAccepted", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "null", new byte[1]);
                    string jsonAcceptRequest = JsonConvert.SerializeObject(acceptRequest);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonAcceptRequest);
                    int bytesSent = s.Send(msg);
                }
                /* Accept or decline incoming connection request */

                /* Read messages */
                String data = null;
                while (connectionAccepted)
                {
                    byte[] rbytes = new byte[1024 * 5000];
                    int rbytesRec = handler.Receive(rbytes);
                    if (handler.Connected && rbytesRec != 0)
                    {
                        data = Encoding.ASCII.GetString(rbytes, 0, rbytesRec);
                        DataProtocol responseMessage = JsonConvert.DeserializeObject<DataProtocol>(data);
                        DateTime timestamp = DateTime.Now;
                        //MessageBox.Show(responseMessage.ToString());
                        //MessageBox.Show(responseMessage.Type);
                        if (responseMessage.Type == "disconnect")
                        {
                            DataProtocol disconnect = new DataProtocol("disconnect", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "Disconnected", new byte[1]);
                            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                            int bytesSent = s.Send(disconnectMsg);
                            connectionAccepted = false;
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            WriteConvoToDB();
                            //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action(delegate () { Listen_button.IsEnabled = true; }));
                            Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action(delegate () { Connect_button.IsEnabled = true; }));

                        }
                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;

                             using (var ms = new System.IO.MemoryStream(img))
                             {
                                 var image = new BitmapImage();
                                 image.BeginInit();
                                 image.CacheOption = BitmapCacheOption.OnLoad; // here
                                 image.StreamSource = ms;
                                 image.DecodePixelHeight = 50;
                                 image.DecodePixelWidth = 50;
                                 image.EndInit();

                               

                                 JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                 string photolocation = "tmper.jpg";  //file name 
                                 encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                                 using (var filestream = new FileStream(photolocation, FileMode.Create))
                                     encoder.Save(filestream);
                                listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                              new Action(delegate () {
                                                                  listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": Sent you an image");
                                                                  
                                                              }));
                            }
                            
                        }
                        else
                        { 

                        listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
                        }
                    }
                    else
                        break;
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
                
                //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectionAccepted = false;
                MessageBox.Show("Connection broken.");
                MessageBox.Show(se.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            DataProtocol message = new DataProtocol("Message", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), textMessage.Text, new byte[1]);
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            int bytesSent = s.Send(msg);
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
                //List<string> conversation = new List<string>();

                
                DataProtocol disconnect = new DataProtocol("disconnect", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "Disconnected", new byte[1]);
                string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                int bytesSent = s.Send(disconnectMsg);
                connectionAccepted = false;
                Connect_button.IsEnabled = true;
                disconnectButton.IsEnabled = false;
                Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action(delegate () { Listen_button.IsEnabled = true; }));
            }
                
        }

        private void WriteConvoToDB()
        {
           string conversation = "";
           foreach (string s in listMessage.Items)
           {
                conversation += s + "\n";
           }
           HistoryDB.AddConvo(conversation, convoDT, connectedUsername);
            
        }

        private void SendImage_button_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            string path = PathBox.Text;
            //MemoryStream ms = new MemoryStream();
            byte[] img = System.IO.File.ReadAllBytes(path);

                /* using (var ms = new System.IO.MemoryStream(img))
                 {
                     var image = new BitmapImage();
                     image.BeginInit();
                     image.CacheOption = BitmapCacheOption.OnLoad; // here
                     image.StreamSource = ms;
                     image.EndInit();

                     JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                     string photolocation = "test.jpg";  //file name 
                     encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                     using (var filestream = new FileStream(photolocation, FileMode.Create))
                         encoder.Save(filestream);
                 } */

            MessageBox.Show("1");
            DataProtocol imgMessage = new DataProtocol("Image", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "null",img);
            MessageBox.Show("2");
            string jsonMessage = JsonConvert.SerializeObject(imgMessage);
                //MessageBox.Show(jsonMessage);
                Console.WriteLine(jsonMessage);
                MessageBox.Show("3");
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            int bytesSent = s.Send(msg);
            DateTime timestamp = DateTime.Now;
            listMessage.Items.Add(timestamp + " Me: Sent Image");

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
