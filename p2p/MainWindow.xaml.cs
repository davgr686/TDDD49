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

                String data = null;
                String username = null;

                /* Outgoing connection request accepted or declined */
                bool connectionAccepted = false;
                byte[] acceptDecline = new byte[256];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                if (data == "1")
                {
                    MessageBox.Show("The client declined your request.");
                    connectionAccepted = false;
                    s.Shutdown(SocketShutdown.Both);
                    //s.Close();
                    s.Disconnect(true);
                }
                else
                {
                    /* If the listener accepted your request, collect username of listener */
                    connectionAccepted = true;
                    username = data;
                    MessageBox.Show(username + " accepted your request.");
                    /* If the listener accepted your request, collect username of listener */

                    /* Send username to listener */
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes((string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)));
                    int bytesSent = s.Send(msg);
                    /* Send username to listener */

                }
                /* Outgoing connection request accepted or declined */

                /* Read messages */
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = connector.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { listMessage.Items.Add(username + ": " + data); }));
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
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
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("1");
                    int bytesSent = s.Send(msg);
                    s.Shutdown(SocketShutdown.Both);
                    //s.Close();
                    s.Disconnect(true);
                }
                else
                {
                    connectionAccepted = true;
                    /* Send username to client that connected */
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes((string)Username.Dispatcher.Invoke(new Func<string>(() => Username.Text)));
                    int bytesSent = s.Send(msg);
                    /* Send username to client that connected */
                }
                /* Accept or decline incoming connection request */

                /* Receive username of the connector */
                byte[] connectorUsername = new byte[256];
                int connectorUsernameRec = handler.Receive(connectorUsername);
                String username = Encoding.ASCII.GetString(connectorUsername, 0, connectorUsernameRec);
                /* Receive username of the connector */

                /* Read messages */
                String data = null;
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { listMessage.Items.Add(username + ": " + data); }));
                }
                /* Read messages */
            }
            catch (SocketException se)
            {
                MessageBox.Show("Connection broken.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(textMessage.Text);
            int bytesSent = s.Send(msg);
            listMessage.Items.Add("Me: " + textMessage.Text);
            textMessage.Clear();
        }

        private void chatHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            history histWindow = new history();
            histWindow.Show();
        }
    }
}
