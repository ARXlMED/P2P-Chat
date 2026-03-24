using System;
using System.Collections.Generic;
using System.Text;

namespace P2P_Chat
{
    public class ChatEvent
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Text { get; set; }
        public string FinalMessage 
        { 
            get
            {
                string time = Timestamp.ToString("g");
                switch (Type)
                {
                    case "MyMessage":
                        return $"[{time}] Вы: {Text}";
                    case "Message":
                        return $"[{time}] {Name} ({Ip}): {Text}";
                    case "PeerJoin":
                        return $"[{time}] {Name} ({Ip}): Присоединился";
                    case "PeerLeft":
                        return $"[{time}] {Name} ({Ip}): Отсоединился";
                    default:
                        return $"[{time}]: {Text}";
                }
            } 
        }

    }
}
