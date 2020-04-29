using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Chat
{
   public class CreateChatParameters
    {
        public string RoomID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
        public List<string> Images { get; set; }
    }
}
