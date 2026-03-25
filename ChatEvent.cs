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
                    case "Message": //1
                        return $"[{time}] {Name} ({Ip}): {Text}";
                    case "MyMessage": //1
                        return $"[{time}] Вы: {Text}";
                    case "Name": //2
                        return $"[{time}] {Name} ({Ip}): Присоединился";
                    case "PeerLeft": //3
                        return $"[{time}] {Name} ({Ip}): Отсоединился";
                    default: //?
                        return $"[{time}]: {Text}";
                }
            } 
        }

    }
}
