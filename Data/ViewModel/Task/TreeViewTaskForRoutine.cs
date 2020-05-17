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
        public int ID { get; set; }
        public string Follow { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string TaskCode { get; set; }
        public string ProjectName { get; set; }
        public string JobName { get; set; }
        public string PIC { get; set; }
        public bool VideoStatus { get; set; }
        public TreeViewTutorial Tutorial { get; set; } = new TreeViewTutorial();
        public string VideoLink { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DeputyName { get; set; }
        public Enum.JobType JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        public Enum.PeriodType periodType { get; set; }
        public string From { get; set; }
        public int CreatedBy { get; set; }
        public int ProjectID { get; set; }
        public BeAssigned User { get; set; }
        public List<BeAssigned> BeAssigneds { get; set; }
        public List<BeAssigned> DeputiesList { get; set; }
        public List<int> Deputies { get; set; }
        public List<int> PICs { get; set; }
        public BeAssigned FromWho { get; set; }
        public FromWhere FromWhere { get; set; }
        public ProjectViewModel Project { get; set; } = new ProjectViewModel();
        public List<History.History> Histories { get; set; } = new List<History.History>();
        public string state { get; set; }
        public bool FinishTask { get; set; }
        public string DueDateDaily { get; set; }
        public DateTime DueDate { get; set; }
        public string SpecificDueDate { get; set; }
        public string DueDateWeekly { get; set; }
        public string DueDateMonthly { get; set; }
        public string SpecificDate { get; set; }
        public string ModifyDateTime { get; set; }
        public List<Follow> Follows { get; set; } = new List<Models.Follow>();
        public bool BeAssigned { get; set; }
        public List<TreeViewTask> RelatedTasks { get; set; } = new List<TreeViewTask>();
    }
}
