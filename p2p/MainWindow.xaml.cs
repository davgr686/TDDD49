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
        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001);
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

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    data = null;
                    NetworkStream stream = client.GetStream();

                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        MessageBox.Show(data);
                        //listMessage.Items.Add(data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);

                        //client.Close();
                    }
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

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(textMessage.Text);
            int bytesSent = s.Send(msg);
        }

        private void Listen_button_Click(object sender, RoutedEventArgs e)
        {
            Listen_for_connection(IPAddress.Parse("127.0.0.1"), 11000);
        }
    }
}
