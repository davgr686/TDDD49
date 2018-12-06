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
    public class SocketCl
    {
        private Socket s;
        private bool connectionAccepted = false;
        private DateTime convoDT;
        private string connectedUsername;
        private string myUsername;

        public void InitSocket()
        {
            try
            { 
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }

            catch (SocketException sex)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(sex);
            }
        }

        public void Connect(string friendIp, string friendPort)
        {
            myUsername = p2p.MainWindow.AppWindow.GetMyUsername();
            try
            { 
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(friendIp), Convert.ToInt32(friendPort)); // kanske ska returnera IPEndPoint
            s.BeginConnect(ipe, new AsyncCallback(ConnectCallback), s);
            }
            catch (SocketException sex)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(sex);
            }
        }

        public void Listen(string localPort)
        {
            
            myUsername = p2p.MainWindow.AppWindow.GetMyUsername();
            try
            {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(localPort)); // kanske ska returnera IPEndPoint
            s.Bind(localEndPoint);
            s.Listen(10);
            s.BeginAccept(new AsyncCallback(ListenCallback), s);
            }
            catch (SocketException sex)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(sex);
            }
        }

        public void SendMessage(string message)
        {
            DataProtocol DP = new DataProtocol("Message", myUsername, message, new byte[1]);
            string jsonMessage = JsonConvert.SerializeObject(DP);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            HistoryDB.AddMessage(message, DateTime.Now, connectedUsername);
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
            HistoryDB.AddImage(img, DateTime.Now, connectedUsername);
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

                byte[] acceptDecline = new byte[1024 * 5000];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                String data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                DataProtocol response = JsonConvert.DeserializeObject<DataProtocol>(data);
                if (response.Type == "connectionDeclined")
                {
                    p2p.MainWindow.AppWindow.ShowMessageBoxCLientDecline();
                    connectionAccepted = false;
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }
                else
                {
                    connectionAccepted = true;
                    HistoryDB.InitConvo(response.Username);
                    connectedUsername = response.Username;
                    convoDT = DateTime.Now;
                    HistoryDB.AddMessage("New conversation started", convoDT, response.Username);
                    p2p.MainWindow.AppWindow.AcceptedRequest(response.Username);
                }
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
                            DataProtocol disconnect = new DataProtocol("disconnect", myUsername, "Disconnected", new byte[1]);
                            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                            HistoryDB.AddMessage(connectedUsername + "Disconnected", timestamp, connectedUsername);
                            int byteSent = s.Send(disconnectMsg);
                            connectionAccepted = false;
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            p2p.MainWindow.AppWindow.DisconnectCallback(connectedUsername, convoDT);
                        }

                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;

                            HistoryDB.AddImage(img, DateTime.Now, connectedUsername);

                            using (var ms = new System.IO.MemoryStream(img))
                            {
                                 var image = new BitmapImage();
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad; // here
                                image.StreamSource = ms;
                                image.DecodePixelHeight = 150;
                                image.DecodePixelWidth = 150;
                                image.EndInit();
                                image.Freeze();


                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                string photolocation = "tmper.jpg";  //file name 
                                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                                using (var filestream = new FileStream(photolocation, FileMode.Create))
                                    encoder.Save(filestream);
                                p2p.MainWindow.AppWindow.DisplayImg(responseMessage.Username, timestamp, image);
                                
                            }
                            
                        }

                        else
                        {
                            HistoryDB.AddMessage(responseMessage.Message, timestamp, responseMessage.Username);
                            p2p.MainWindow.AppWindow.AddMessage(responseMessage.Username, responseMessage.Message, timestamp);
                          
                        }
                    }
                    else
                        break;
                }
            }
            catch (SocketException se)
            {
                p2p.MainWindow.AppWindow.ConnectionBroken();
            }
            catch (Exception ex)
            {
                p2p.MainWindow.AppWindow.ShowExcepion(ex);
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


                /* Accept or decline incoming connection request */
               
                 if (!p2p.MainWindow.AppWindow.AcceptRequestBox(currUser)) // if not accepted connection
                 {
                     connectionAccepted = false;
                     DataProtocol declineRequest = new DataProtocol("connectionDeclined", myUsername, "null", new byte[1]);
                     string jsonDeclineRequest = JsonConvert.SerializeObject(declineRequest);
                     byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonDeclineRequest);
                     int bytesSent = s.Send(msg);

                     s.Shutdown(SocketShutdown.Both);
                     s.Close();
                 }
                else
                {
                    connectionAccepted = true;
                    HistoryDB.InitConvo(currUser);
                    HistoryDB.AddMessage("New conversation started", convoDT, currUser);
                    //p2p.MainWindow.AppWindow.ConnectionAccepted();
                    p2p.MainWindow.AppWindow.EnableDisconnectButton();
                    DataProtocol acceptRequest = new DataProtocol("connectionAccepted", myUsername, "null", new byte[1]);
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
                            DataProtocol disconnect = new DataProtocol("disconnect", myUsername, "Disconnected", new byte[1]);
                            string jsonDisconnect = JsonConvert.SerializeObject(disconnect);
                            byte[] disconnectMsg = System.Text.Encoding.ASCII.GetBytes(jsonDisconnect);
                            int bytesSen = s.Send(disconnectMsg);
                            HistoryDB.AddMessage(connectedUsername + "Disconnected", timestamp, connectedUsername);
                            connectionAccepted = false;
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            p2p.MainWindow.AppWindow.DisconnectCallback(connectedUsername, convoDT);

                        }
                        else if (responseMessage.Type == "Image")
                        {
                            byte[] img = responseMessage.imgByte;
                            HistoryDB.AddImage(img, DateTime.Now, connectedUsername);

                            using (var ms = new System.IO.MemoryStream(img))
                            {
                                var image = new BitmapImage();
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad; // here
                                image.StreamSource = ms;
                                image.DecodePixelHeight = 150;
                                image.DecodePixelWidth = 150;
                                image.EndInit();
                                image.Freeze();



                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                string photolocation = "tmpeeeer.jpg";  //file name 
                                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                                using (var filestream = new FileStream(photolocation, FileMode.Create))
                                    encoder.Save(filestream);
                                p2p.MainWindow.AppWindow.DisplayImg(responseMessage.Username, timestamp, image);

                            }

                        }
                        else
                        {
                            HistoryDB.AddMessage(responseMessage.Message, timestamp, responseMessage.Username);
                            p2p.MainWindow.AppWindow.AddMessage(responseMessage.Username, responseMessage.Message, timestamp);

                        }
                    }
                    else
                        break;
                }
            }
            catch (SocketException se)
            {
                connectionAccepted = false;
                p2p.MainWindow.AppWindow.ConnectionBroken();
            }
        }

    }
}
