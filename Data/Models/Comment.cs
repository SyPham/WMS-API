using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class Comment
    {
        public Comment()
        {
            CreatedTime = DateTime.Now;
        }

        public int ID { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public int ParentID { get; set; }
        public string Content { get; set; }
        public string TaskCode { get; set; }
        public int Level { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
