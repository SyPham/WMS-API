using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Chat
{
   public class Chat
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public byte[] ImageBase64 { get; set; }
        public List<string> Images { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
