using Data.Interface;
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
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string JobName { get; set; }
        public int ParentID { get; set; }
        public int Level { get; set; }
        public int DepartmentID { get; set; }
        public int? ProjectID { get; set; }
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public int? OCID { get; set; }
        public int FromWhoID { get; set; }
        [MaxLength(2)]
        public string Priority { get; set; } = "M";
        public string ModifyDateTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public bool FinishedMainTask { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Enum.PeriodType periodType { get; set; }
        public Enum.JobType JobTypeID { get; set; }
        public virtual User User { get; set; } 
        public virtual ICollection<Tag> Tags { get; set; } 
        public virtual ICollection<Deputy> Deputies { get; set; } 
        public virtual ICollection<Follow> Follows { get; set; } 
        public virtual Comment Comment { get; set; }
        public virtual Project Project { get; set; } 
        public virtual Tutorial Tutorial { get; set; } 
        public virtual OC OC { get; set; }
        public string GroupCode { get; set; }
    }
}
