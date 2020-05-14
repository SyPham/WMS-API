using Data.Models;
using Data.ViewModel.Project;
using Data.ViewModel.Tutorial;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Task
{
    public class TreeViewTaskForRoutine
    {
        public TreeViewTask Task { get; set; }
        public List<TreeViewTask> RelatedTasks { get; set; } = new List<TreeViewTask>();
    }
}
