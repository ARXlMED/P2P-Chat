using System;
using System.Collections.Generic;
using System.Text;

namespace P2P_Chat
{
    public class DiscoveryPacket
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TcpPort { get; set; }
    }
}
