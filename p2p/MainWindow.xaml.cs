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


namespace p2p
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Socket s;

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
            try
            {
                s.BeginConnect(ipe, new AsyncCallback(ConnectCallback), s);
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

                /* Outgoing connection request accepted or declined */
                bool connectionAccepted = false;
                byte[] acceptDecline = new byte[256];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                String data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                DataProtocol response = JsonConvert.DeserializeObject<DataProtocol>(data);
                if (response.Type == "connectionDeclined")
                {
                    MessageBox.Show("The client declined your request.");
                    connectionAccepted = false;
                    s.Shutdown(SocketShutdown.Both);
                    s.Disconnect(true);
                }
                else
                {
                    connectionAccepted = true;
                    MessageBox.Show(response.Username + " accepted your request.");
                }
                /* Outgoing connection request accepted or declined */

                /* Read messages */
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = connector.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    DataProtocol responseMessage = JsonConvert.DeserializeObject<DataProtocol>(data);
                    DateTime timestamp = DateTime.Now;
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, 
                                                        new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Disconnect(true);
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
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(textLocalPort.Text));
            try
            {
                s.Bind(localEndPoint);
                s.Listen(100);
                s.BeginAccept(new AsyncCallback(ListenCallback), s);
            }
            catch (SocketException se)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Disconnect(true);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                MessageBox.Show("Connection broken.");
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

                /* Accept or decline incoming connection request */
                bool connectionAccepted = false;
                if (MessageBox.Show("Connection request from: " + handler.RemoteEndPoint.ToString() + ". \nAccept the request?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    connectionAccepted = false;
                    DataProtocol declineRequest = new DataProtocol("connectionDeclined", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "null");
                    string jsonDeclineRequest = JsonConvert.SerializeObject(declineRequest);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonDeclineRequest);
                    int bytesSent = s.Send(msg);

                    s.Shutdown(SocketShutdown.Both);
                    s.Disconnect(true);
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    connectionAccepted = true;
                    DataProtocol acceptRequest = new DataProtocol("connectionAccepted", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), "null");
                    string jsonAcceptRequest = JsonConvert.SerializeObject(acceptRequest);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonAcceptRequest);
                    int bytesSent = s.Send(msg);
                }
                /* Accept or decline incoming connection request */

                /* Read messages */
                String data = null;
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    DataProtocol responseMessage = JsonConvert.DeserializeObject<DataProtocol>(data);
                    DateTime timestamp = DateTime.Now;
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                        new Action(delegate () { listMessage.Items.Add(timestamp + " " + responseMessage.Username + ": " + responseMessage.Message); }));
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Disconnect(true);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                MessageBox.Show("Connection broken.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            DataProtocol message = new DataProtocol("Message", (string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)), textMessage.Text);
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonMessage);
            int bytesSent = s.Send(msg);
            DateTime timestamp = DateTime.Now;
            listMessage.Items.Add(timestamp + " Me: " + textMessage.Text);
            textMessage.Clear();
        }
    }
}
