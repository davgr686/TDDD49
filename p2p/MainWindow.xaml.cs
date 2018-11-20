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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                /* Outgoing connection request accepted or declined */
                bool connectionAccepted = false;
                byte[] acceptDecline = new byte[256];
                int acceptDeclineRec = connector.Receive(acceptDecline);
                data = Encoding.ASCII.GetString(acceptDecline, 0, acceptDeclineRec);
                if (data == "1")
                {
                    connectionAccepted = true;
                    MessageBox.Show("The client accepted your request.");
                }
                else if (data == "2")
                {
                    MessageBox.Show("The client declined your request.");
                    connectionAccepted = false;
                    s.Shutdown(SocketShutdown.Both);
                    //s.Close();
                    s.Disconnect(true);
                }
                /* Outgoing connection request accepted or declined */
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = connector.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { listMessage.Items.Add(data); }));
                }
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
                bool connectionAccepted = false;
                /* Accept or decline incoming connection request */
                if (MessageBox.Show("Connection request from: " + handler.RemoteEndPoint.ToString() + ". \nAccept the request?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    connectionAccepted = true;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("1");
                    int bytesSent = s.Send(msg);
                }
                else
                {
                    connectionAccepted = false;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("2");
                    int bytesSent = s.Send(msg);
                    s.Shutdown(SocketShutdown.Both);
                    //s.Close();
                    s.Disconnect(true);
                }
                /* Accept or decline incoming connection request */
                String data = null;
                while (connectionAccepted)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { listMessage.Items.Add(data); }));
                }
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
            listMessage.Items.Add(textMessage.Text);
            textMessage.Clear();
        }
    }
}
