using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace P2P_Chat
{
    /// <summary>
    /// Логика взаимодействия для ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public string IpAddress { get; private set; }
        public string UserName { get; private set; }
        public int TcpPort { get; private set; }
        public int UdpPort { get; private set; }

        public ConnectionDialog()
        {
            InitializeComponent();
            OkButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpTextBox.Text.Trim();
            if (!IPAddress.TryParse(ip, out _))
            {
                MessageBox.Show("Некорректный IP-адрес");
                return;
            }
            if (!int.TryParse(TcpPortTextBox.Text, out int tcp) || tcp <= 0 || tcp > 65535)
            {
                MessageBox.Show("TCP порт должен быть от 1 до 65535");
                return;
            }
            if (!int.TryParse(UdpPortTextBox.Text, out int udp) || udp <= 0 || udp > 65535)
            {
                MessageBox.Show("UDP порт должен быть от 1 до 65535");
                return;
            }
            IpAddress = ip;
            UserName = NameTextBox.Text.Trim();
            TcpPort = tcp;
            UdpPort = udp;
            DialogResult = true;
        }
    }
}
