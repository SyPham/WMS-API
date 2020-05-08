using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Notification
{
   public class NotificationViewModel
    {
        public int ID { get; set; }
        public string Function { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Seen { get; set; }
        public int BeAssigned { get; set; }
        public string URL { get; set; }
        public byte[] ImageBase64 { get; set; }
        public string Sender { get; set; }
        public int? CreatedBy { get; set; }
      
        // public IEnumerable<NotificationDetail> NotificationDetails { get; set; }
    }
}
