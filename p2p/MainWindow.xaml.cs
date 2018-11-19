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

        private void Listen_for_connection(IPAddress ip, int port)
        {
            //Endpoint for socket
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //listener.Blocking = false;
                listener.Bind(localEndPoint);
                listener.Listen(100);

                //Accept incoming connection
                Socket handler = listener.Accept();
                String data = null;

                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    MessageBox.Show("From " + handler.RemoteEndPoint.ToString() + ": " + data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textFriendsIp.Text);
            int port = Convert.ToInt32(textFriendsPort.Text);
            IPEndPoint ipe = new IPEndPoint(ip, port);
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
            Listen_for_connection(IPAddress.Any, 11001);
        }
    }
}
