using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace P2P_Chat.Models
{
    public class ChatEvent
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Text { get; set; }
        [JsonIgnore]
        public bool MyMessage { get; set; } = false;
        public string FinalMessage
        {
            get
            {
                string time = Timestamp.ToString("g");
                switch (Type)
                {
                    case "Message": //1
                        if (!MyMessage) return $"[{time}] {Name} ({Ip}): {Text}";
                        else return $"[{time}] Вы: {Text}";
                    //case "MyMessage": //1
                    //    return $"[{time}] Вы: {Text}";
                    case "Name": //2
                        return $"[{time}] {Name} ({Ip}): Присоединился";
                    case "CloseConnection": //3
                        return $"[{time}] {Name} ({Ip}): Отсоединился";
                    case "History": //4
                        return $"[{time}] {Name} ({Ip}): {Text}";
                    default: //?
                        return $"[{time}]: {Text}";
                }
            }
        }
        public override string ToString()
        {
            return FinalMessage;
        }
    }


}
