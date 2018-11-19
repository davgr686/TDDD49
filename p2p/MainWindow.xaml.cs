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
        //ThreadPool.QueueUserWorkItem();

        public MainWindow()
        {
            InitializeComponent();
        }

        private Socket s;
        private Socket Handler;

        private void Listen_for_connection(IPAddress ip, int port)
        {
            //Endpoint for socket
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                /* Asynchronous */
                //listener.Blocking = false;
                listener.Bind(localEndPoint);
                listener.Listen(100);

                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                /* Asynchronous */

                /* Synchronous 
                //Accept incoming connection
                Socket handler = listener.Accept();
                String data = null;
                while (true)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    MessageBox.Show("From " + handler.RemoteEndPoint.ToString() + ": " + data);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                        new Action(delegate () { listMessage.Items.Add(data); }));
                }
                Synchronous */
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

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                /* Accept or decline incoming connection request */
                if (handler.Connected == true)
                {
                    MessageBox.Show("Connection request from: " + handler.RemoteEndPoint.ToString());
                }
                else
                {
                    int x;
                }
                Handler = handler;
                String data = null;
                while (true)
                {
                    byte[] bytes = new byte[256];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    MessageBox.Show("From " + handler.RemoteEndPoint.ToString() + ": " + data);
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                        new Action(delegate () { listMessage.Items.Add(data); }));
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("Message received!");
                    int bytesSent = handler.Send(msg);
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show("Connection to " + Handler.RemoteEndPoint.ToString() + " broken.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

            private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
            try
            {
                s.Connect(ipe);
                MessageBox.Show("Connected to: " + s.RemoteEndPoint.ToString());
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

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(textMessage.Text);
            int bytesSent = s.Send(msg);
        }

        private void Listen_button_Click(object sender, RoutedEventArgs e)
        {
            Listen_for_connection(IPAddress.Any, Convert.ToInt32(textLocalPort.Text));
        }
    }
}
