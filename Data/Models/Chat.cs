using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    public class Chat
    {
        public Chat()
        {
            CreatedTime = DateTime.Now;
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int RoomID { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
