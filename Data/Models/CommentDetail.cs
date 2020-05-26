using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
   public class CommentDetail
    {
        public CommentDetail()
        {
            this.CreatedTime = DateTime.Now;
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public bool Seen { get; set; }
        public DateTime CreatedTime { get; set; }
        public virtual Comment Comment { get; set; }
    }
}
