using P2P_Chat.Core;
using P2P_Chat.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace P2P_Chat.ViewModels
{
    public class MainWindowViewModel : IDisposable, INotifyPropertyChanged
    {
        private PeerCore peerCore;
        private string textMessage;

        public string TextMessage
        {
            get => textMessage;
            set
            {
                textMessage = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

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
                AddEventSorted(obj);
            }));
        }

        private void AddEventSorted(ChatEvent item)
        {
            if (ChatEvents.Count == 0 || ChatEvents[^1].Timestamp <= item.Timestamp)
            {
                ChatEvents.Add(item);
                return;
            }

            int index = 0;
            while (index < ChatEvents.Count && ChatEvents[index].Timestamp <= item.Timestamp)
                index++;

            ChatEvents.Insert(index, item);
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(textMessage)) return;
            await peerCore.BroadCastMessageTCPAsync(Encoding.UTF8.GetBytes(textMessage), 1);
            textMessage = "";
            OnPropertyChanged(nameof(TextMessage));
            CommandManager.InvalidateRequerySuggested();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            peerCore?.Dispose();
        }
    }
}