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
                // s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                MessageBox.Show("Connection broken.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
