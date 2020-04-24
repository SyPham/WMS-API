using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel.Task
{
   public class CloneTaskViewModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string JobName { get; set; }
        public int ParentID { get; set; }
        public int Level { get; set; }
        public int DepartmentID { get; set; }
        public int ProjectID { get; set; }
        public int CreatedBy { get; set; }
        public int OCID { get; set; }
        public int FromWhoID { get; set; }
        public string Priority { get; set; } = "M";
        public string DueDateDaily { get; set; } = "";
        public string DueDateWeekly { get; set; } = "";
        public string DueDateMonthly { get; set; } = "";
        public string SpecificDate { get; set; } = "";
        public string ModifyDateTime { get; set; }
        public bool FinishedMainTask { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Enum.PeriodType periodType { get; set; }
        public Enum.JobType JobTypeID { get; set; }
        public int ParentTemp { get; set; }
        public int IDTemp { get; set; }
    }
}
