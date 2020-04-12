﻿using Data.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    public class History : IEntity
    {
        public History()
        {
            CreatedDate = DateTime.Now;
            this.ModifyDateTime = DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt");
        }

        public int ID { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public bool Status { get; set; }
        public string Deadline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifyDateTime { get; set; }
    }
}
