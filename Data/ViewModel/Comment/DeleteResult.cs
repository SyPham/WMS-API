using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Comment
{
   public class DeleteResult
    {
        public bool Status { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
