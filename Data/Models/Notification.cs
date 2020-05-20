using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
   public class Notification
    {
        public Notification()
        {
            CreatedTime = DateTime.Now;
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int? UserID { get; set; }
        public int TaskID { get; set; }
        public string Message { get; set; }
        public string Function { get; set; }
        public string URL { get; set; }
        public DateTime CreatedTime { get; set; }
        public virtual NotificationDetail NotificationDetails { get; set; }
        public virtual User User { get; set; }

    }
}
