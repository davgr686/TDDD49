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
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 11001);
        //ThreadPool.QueueUserWorkItem();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Listen_for_connection(IPAddress ip, int port)
        {
            // Establish the locel endpoint for the socket
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);

            // Create a TCP/IP socket
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //listener.Blocking = false;
                listener.Bind(localEndPoint);
                listener.Listen(100);

                Socket handler = listener.Accept();
                String data = null;

                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    MessageBox.Show("Text received : {0}", data);
                }

                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Additional code to read data goes here.    
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
