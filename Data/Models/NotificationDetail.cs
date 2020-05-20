using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
   public class NotificationDetail
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int NotificationID { get; set; }
        public bool Seen { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual User User { get; set; }
    }
    /*
     From system 
     henry
     Update Status
     Finish Status
     Seen
    2/19/2020 3:18
     */
}
