using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
   public class UploadImage
    {
        public int ID { get; set; }
        public int CommentID { get; set; }
        public int ChatID { get; set; }
        public string Image { get; set; }
    }
}
