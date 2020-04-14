using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.ViewModel.Task
{
    public class TreeViewTask
    {
        public TreeViewTask()
        {
            this.children = new List<TreeViewTask>();
        }
        
        public bool VideoStatus { get; set; }
        public Data.Models.Tutorial Tutorial { get; set; }
        public string VideoLink { get; set; }
        public int ID { get; set; }
        public string JobName { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public string CreatedDate { get; set; }
        public string Remark { get; set; }
        public string PIC { get; set; }
        public string From { get; set; }
        public string DeputyName { get; set; }
        public string ProjectName { get; set; }
        public string Follow { get; set; }
        public int JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        public Data.Enum.PeriodType periodType { get; set; }

        public int CreatedBy { get; set; }
        public int ProjectID { get; set; }
        public Data.Models.User User { get; set; }
        public List<BeAssigned> BeAssigneds { get; set; }
        public List<BeAssigned> DeputiesList { get; set; }
        public List<int> Deputies { get; set; }
        public List<int> PICs { get; set; }
        public BeAssigned FromWho { get; set; }
        public FromWhere FromWhere { get; set; }
        public string DateOfWeekly { get; set; }
        public string DateOfMonthly { get; set; }
        public Models.Project Project { get; set; }
        public List<History.History> Histories { get; set; }

        public string state { get; set; }
        public bool FinishTask { get; set; }
        public string DueDateDaily { get; set; }
        public string DueDateWeekly { get; set; }
        public string DueDateMonthly { get; set; }
        public string DueDateQuarterly { get; set; }
        public string DueDateYearly { get; set; }
        public string SpecificDate { get; set; }
        public string CreatedDateForEachTask { get; set; }
        public string ModifyDateTime { get; set; }
        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<TreeViewTask> children { get; set; }
    }

    public class BeAssigned
    {
        public int ID { get; set; }
        public string Username { get; set; }
    }
    public class FromWhere
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
