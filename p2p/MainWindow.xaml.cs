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

namespace p2p
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        public MainWindow()
        {
            InitializeComponent();

            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            textLocalIp.Text = GetLocalIP();
            textFriendsIp.Text = GetLocalIP();
            //textFriendsIp.Text = "192.168.56.1";

        }
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Any, Convert.ToInt32(textLocalPort.Text));

                sck.Bind(epLocal);
                byte[] buffer = new byte[256];
                sck.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(MessageCallBack), buffer);
                //button1.Enabled = false;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[256];
                msg = enc.GetBytes(textMessage.Text);
                sck.Send(msg);
                listMessage.Items.Add(textMessage.Text);
                textMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);
                byte[] buffer = new byte[256];
                textMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceive(aResult);

                if (size > 0)
                {
                    byte[] receivedData = new byte[256];

                    receivedData = (byte[])aResult.AsyncState;

                    int lastIndex = Array.FindLastIndex(receivedData, b => b != 0);

                    Array.Resize(ref receivedData, lastIndex + 1);

                    ASCIIEncoding eEncoding = new ASCIIEncoding();

                    string receivedMessage = eEncoding.GetString(receivedData);
                    //listMessage.Items.Add("hello");
                    listMessage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                        new Action(delegate () { listMessage.Items.Add(receivedMessage); }));
                    

                }

                byte[] buffer = new byte[256];
                sck.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(MessageCallBack), buffer);

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }


        }
    }
}
