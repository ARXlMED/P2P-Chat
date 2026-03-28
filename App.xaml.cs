using System.Configuration;
using System.Data;
using System.Windows;

namespace P2P_Chat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var dialog = new ConnectionDialog();
            if (dialog.ShowDialog() == true)
            {
                var mainWindow = new MainWindow(dialog.IpAddress, dialog.UserName, dialog.TcpPort, dialog.UdpPort);
                MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
