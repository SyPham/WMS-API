using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.ViewModel.Tutorial
{
   public class TreeViewTutorial
    {
        public TreeViewTutorial()
        {
            this.children = new List<TreeViewTutorial>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public int? ProjectID { get; set; }
        public int? TaskID { get; set; }
        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<TreeViewTutorial> children { get; set; }
    }
}
