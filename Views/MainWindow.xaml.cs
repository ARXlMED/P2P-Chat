using P2P_Chat.ViewModels;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2P_Chat
{
    public partial class MainWindow : Window
    {
        public MainWindow(string ip, string name, int tcpport, int udpport)
        {
            InitializeComponent();

            var vm = new MainWindowViewModel(ip, name, tcpport, udpport);
            DataContext = vm;

            Loaded += async (_, __) =>
            {
                try
                {
                    await vm.InitializeAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка запуска сети: {ex.Message}");
                }
            };

            Closing += (_, __) => vm.Dispose();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is MainWindowViewModel vm)
            {
                vm.SendMessageCommand.Execute(null);
            }
        }
    }
    
}