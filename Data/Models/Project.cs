using Data.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
   public class Project: IEntity
    {
        public Project()
        {
            CreatedDate = DateTime.Now;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public int Room { get; set; }
        public virtual Room RoomTable { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ICollection<Manager> Managers { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<Tutorial> Tutorials { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

    }
}
