using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.ViewModel.Comment
{
   public class CommentTreeView
    {
        public CommentTreeView()
        {
            this.children = new List<CommentTreeView>();
        }

        public int ID { get; set; }
        public string Username { get; set; }
        public int UserID { get; set; }
        public int TaskID { get; set; }
        public string Content { get; set; }
        public int ParentID { get; set; }
        public int Level { get; set; }
        public byte[] ImageBase64 { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Seen { get; set; }
        public List<string> Images { get; set; }
        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<CommentTreeView> children { get; set; }
    }
}
