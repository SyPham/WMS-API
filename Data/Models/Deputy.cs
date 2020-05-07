using Data.ViewModel.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
   public class Deputy
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int TaskID { get; set; }
        public virtual User User { get; set; }
        public virtual Task Task { get; set; }
    }
}
