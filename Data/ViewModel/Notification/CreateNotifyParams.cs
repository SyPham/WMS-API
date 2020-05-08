using Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Notification
{
   public class CreateNotifyParams
    {
        public List<int> Users { get; set; }
        public AlertType AlertType { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
        public int? UserID { get; set; }
        public int TaskID { get; set; }
    }
}
