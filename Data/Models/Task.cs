﻿using Data.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    public class Task : IEntity
    {
        public Task()
        {
            CreatedDate = DateTime.Now;
            CreatedDateForEachTask = DateTime.Now;
        }

        public int ID { get; set; }
        public string JobName { get; set; }
        public string Description { get; set; }
        public string From { get; set; }
        public string Remark { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateForEachTask { get; set; }
        public int ParentID { get; set; }
        public int Level { get; set; }
        public int DepartmentID { get; set; }
        public bool Seen { get; set; }
        public bool FinishedMainTask { get; set; }
        public int ProjectID { get; set; }
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public virtual User User { get; set; }
        public int OCID { get; set; }
        public int JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        [MaxLength(2)]
        public string Priority { get; set; } = "M";
        public string DueDateDaily { get; set; } = "";
        public string DueDateWeekly { get; set; } = "";
        public string DueDateMonthly { get; set; } = "";
        public string DueDateQuarterly { get; set; } = "";
        public string DueDateYearly { get; set; } = "";
        public string SpecificDate { get; set; } = "";
        public string DateOfWeekly { get; set; } = "";
        public string DateOfMonthly { get; set; } = "";
        public Enum.PeriodType periodType { get; set; }
        public string ModifyDateTime { get; set; }

    }
}
