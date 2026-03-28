using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Net;
using System.Windows;
using System.Text;
using System.Runtime.CompilerServices;

namespace P2P_Chat
{
    public class MainWindowViewModel : IDisposable, INotifyPropertyChanged
    {
        private PeerCore peerCore;


        private string textMessage;
        public string TextMessage { get => textMessage; set { textMessage = value; OnPropertyChanged(); } }


        public ObservableCollection<ChatEvent> ChatEvents { get; } = new ObservableCollection<ChatEvent>();
        public ICommand SendMessageCommand { get; }

        public MainWindowViewModel(string myIP, string myName, int TCPPort, int UDPPort)
        {
            try
            {
                peerCore = new PeerCore(myName, IPAddress.Parse(myIP), TCPPort, UDPPort);
                peerCore.nowEvent += OnPeerEvent;

                SendMessageCommand = new RelayCommand(SendMessage, () => !string.IsNullOrWhiteSpace(TextMessage));
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка инициализации чата: {ex.Message}");
            }
        }

        public Task InitializeAsync()
        {
            return peerCore.StartAsync();
        }

        private void OnPeerEvent(ChatEvent obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatEvents.Add(obj);
            }));
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(textMessage)) return;
            await peerCore.BroadCastMessageTCPAsync(Encoding.UTF8.GetBytes(textMessage), 1);
            textMessage = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void Dispose()
        {
            peerCore.Dispose();
        }
    }
}
