using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace P2P_Chat.Models
{
    public class PeerInfo 
    {
        public Socket Socket { get; set; }
        public string Name { get; set; }
    }
}
