using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P2P_Chat
{
    public class PeerCore : IDisposable
    {
        private readonly int tcpPort = 14239;
        private readonly int udpPort = 32478;

        public string name;
        public IPAddress myIP;

        private ConcurrentDictionary<IPEndPoint, PeerInfo> peers; // adr + name

        private Socket udpListenSocket;
        private Socket tcpListenSocket;

        //private CancellationToken cancellationToken;

        public bool isAlive = false;

        public PeerCore(string name, IPAddress myIP)
        {
            this.name = name;
            this.myIP = myIP;
        }

        public async Task StartAsync()
        {
            isAlive = true;

            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpListenSocket.Bind(new IPEndPoint(IPAddress.Any, udpPort));
            _ = Task.Run(() => ListenUDPAsync());
            
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpListenSocket.Bind(new IPEndPoint(IPAddress.Any, tcpPort));
            _ = Task.Run(() => ListenTCPAsync());

            await SendBroadcastUDPAsync();
        }

        private async Task SendBroadcastUDPAsync()
        {
            Socket udpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            byte[] dataName = Encoding.UTF8.GetBytes(name);
            IPEndPoint broadcastPoint = new IPEndPoint(IPAddress.Broadcast, udpPort);
            await udpSendSocket.SendToAsync(dataName, broadcastPoint);
        }

        private async Task ListenTCPAsync()
        {

        }

        private async Task ListenUDPAsync()
        {

        }

        public void Dispose()
        {
            isAlive = false;
            udpListenSocket.Dispose();
            tcpListenSocket.Dispose();
            //cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
