using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Task
{
   public class TreeTask
    {
        public int ID { get; set; }
        public string Follow { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string ProjectName { get; set; }
        public string JobName { get; set; }
        public string PIC { get; set; }
        public bool VideoStatus { get; set; }
        public Models.Tutorial Tutorial { get; set; }
        public string VideoLink { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public string CreatedDate { get; set; }
        public string DeputyName { get; set; }
        public Enum.JobType JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        public Enum.PeriodType periodType { get; set; }
        public string From { get; set; }
        public int CreatedBy { get; set; }
        public int ProjectID { get; set; }
        public Models.User User { get; set; }
        public List<BeAssigned> BeAssigneds { get; set; }
        public List<BeAssigned> DeputiesList { get; set; }
        public List<int> Deputies { get; set; }
        public List<int> PICs { get; set; }
        public BeAssigned FromWho { get; set; }
        public FromWhere FromWhere { get; set; }
        public Models.Project Project { get; set; }
        public List<History.History> Histories { get; set; }

        public string state { get; set; }
        public bool FinishTask { get; set; }
        public string DueDateDaily { get; set; }
        public string DueDate { get; set; }
        public string DueDateWeekly { get; set; }
        public string DueDateMonthly { get; set; }
        public string SpecificDate { get; set; }
        public string ModifyDateTime { get; set; }
        public bool BeAssigned { get; set; }
    }
}
