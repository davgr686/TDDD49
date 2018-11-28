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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace p2p
{
    /// <summary>
    /// Interaction logic for ImageSender.xaml
    /// </summary>
    public partial class ImageSender : Window
    {
        public ImageSender()
        {
            InitializeComponent();
        }

        private void Browse_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".jpg";
            ofd.Filter = "JPG-file (.jpg)|*.jpg";
            if (ofd.ShowDialog() == true)
            {
                string filename = ofd.FileName;
                InputPath.Text = filename;
            }

        }
    }
}
