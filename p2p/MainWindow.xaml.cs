using System;
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
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 11001);
        //ThreadPool.QueueUserWorkItem();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Listen_for_connection(IPAddress ip, int port)
        {
            try
            {
                TcpListener server = new TcpListener(ip, port);
                server.Start();

                Byte[] bytes = new Byte[256];
                String data = null;

                Socket client = server.AcceptSocket();
                /*IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;

                MessageBox.Show(newclient.Address.ToString());*/

                while (true)
                {
                    data = null;
                    int k = client.Receive(bytes);
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, k);
                    MessageBox.Show(data);

                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s.Connect(ipe);
            }
            catch (ArgumentNullException ae)
            {
                MessageBox.Show(ae.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static IPAddress GetIPAddress()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Environment.MachineName);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address;
            }

            return null;
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
