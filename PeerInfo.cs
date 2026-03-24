using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace P2P_Chat
{
    public class PeerInfo : IDisposable
    {
        public Socket Socket { get; set; }
        public string Name { get; set; }

        public void Dispose()
        {
            Socket?.Dispose();
        }
    }
}
