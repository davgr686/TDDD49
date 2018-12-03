using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using _DataProtocol;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace p2p
{
    public class SocketCl
    {
        private Socket s;
        private bool connectionAccepted = false;
        private DateTime convoDT;
        private string connectedUsername;
        private string myUsername;

        public void InitSocket()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        }

        public void Connect(string friendIp, string friendPort)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(friendIp), Convert.ToInt32(friendPort)); // kanske ska returnera IPEndPoint
            s.BeginConnect(ipe, new AsyncCallback(ConnectCallback), s);
        }

        public void Listen(string localPort)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(localPort)); // kanske ska returnera IPEndPoint
            s.Bind(localEndPoint);
            s.Listen(10);
            s.BeginAccept(new AsyncCallback(ListenCallback), s);
        }

        public void SendMessage(string message)
        {
            DataProtocol DP = new DataProtocol("Message", myUsername, message, new byte[1]);
            string jsonMessage = JsonConvert.SerializeObject(DP);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            int bytesSent = s.Send(msg);
        }

        public void SendDisconnect()
        {
            DataProtocol disconnect = new DataProtocol("disconnect", myUsername, "Disconnected", new byte[1]);
            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
            int bytesSent = s.Send(disconnectMsg);
            connectionAccepted = false;
        }

        public void SendImage(string path)
        {
            byte[] img = System.IO.File.ReadAllBytes(path);
            DataProtocol imgMessage = new DataProtocol("Image", myUsername, "null", img);
            string jsonMessage = JsonConvert.SerializeObject(imgMessage);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            int bytesSent = s.Send(msg);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket connector = (Socket)ar.AsyncState;
                connector.EndConnect(ar);

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(myUsername); 
                int bytesSent = s.Send(msg);


                /* Outgoing connection request accepted or declined */
                byte[] acceptDecline = new byte[1024 * 5000];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                String data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                DataProtocol response = JsonConvert.DeserializeObject<DataProtocol>(data);
                if (response.Type == "connectionDeclined")
                {
                    //MessageBox.Show("The client declined your request.");
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
                   // Username.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                   // new Action(delegate () { Username.IsEnabled = false; }));
                    //MessageBox.Show(response.Username + " accepted your request.");
                    //disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                   // new Action(delegate () { disconnectButton.IsEnabled = true; }));
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
                            //WriteConvoToDB();
                            //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                           // Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                             //           new Action(delegate () { Listen_button.IsEnabled = true; }));
                            //Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                             //   new Action(delegate () { Connect_button.IsEnabled = true; }));
                        }

                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;

                            /*
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
                                                               new Action(delegate () {
                                                                   listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": Sent you an image");
                                                               }));
                            }
                            */

                        }

                        else
                        {
                           // listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            //                                    new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
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
                //MessageBox.Show("Connection broken.");
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.ToString());
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
                    //disconnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    //new Action(delegate () { disconnectButton.IsEnabled = true; }));
                    //Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    //new Action(delegate () { Connect_button.IsEnabled = false; }));
                    //Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    //new Action(delegate () { Listen_button.IsEnabled = false; }));
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
                        if (responseMessage.Type == "disconnect")
                        {
                            DataProtocol disconnect = new DataProtocol("disconnect", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "Disconnected", new byte[1]);
                            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                            int bytesSent = s.Send(disconnectMsg);
                            connectionAccepted = false;
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            //WriteConvoToDB();
                            //s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            //Listen_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                             //           new Action(delegate () { Listen_button.IsEnabled = true; }));
                            //Connect_button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                             //           new Action(delegate () { Connect_button.IsEnabled = true; }));

                        }
                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;

                            /* using (var ms = new System.IO.MemoryStream(img))
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
                            */
                        }
                        else
                        {

                           // listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            //                                    new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
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
               // MessageBox.Show("Connection broken.");
               // MessageBox.Show(se.ToString());
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

    }
}
