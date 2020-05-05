using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Comment
{
   public class AddSubViewModel
    {
        public int ParentID { get; set; }
        public int UserID { get; set; }
        public int CurrentUser { get; set; }
        public int TaskID { get; set; }
        public string Content { get; set; }
        public string TaskCode { get; set; }
        public Enum.ClientRouter ClientRouter { get; set; }
    }
}
