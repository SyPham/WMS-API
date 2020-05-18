using Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
namespace Data.ViewModel.Task
{
   public class CreateTaskViewModel
    {
        public CreateTaskViewModel()
        {
        }

        public int ID { get; set; }
        public string JobName { get; set; }
        public int CreatedBy { get; set; }
        public int? ProjectID { get; set; }
        public int ParentID { get; set; }
        public int? OCID { get; set; }
        public string Priority { get; set; }
        public bool Status { get; set; }
        public int[] PIC { get; set; }

        public int CurrentUser { get; set; }
        public int UserID { get; set; }
        public JobType JobTypeID { get; set; }
        public PeriodType periodType { get; set; }
        public int FromWhoID { get; set; }
        public DateTime DueDate { get; set; }
        public int[] Deputies { get; set; }
        public int DepartmentID { get; set; }

    }
}
