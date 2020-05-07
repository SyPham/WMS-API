using Data.Interface;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class OC: IEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

    }
}
