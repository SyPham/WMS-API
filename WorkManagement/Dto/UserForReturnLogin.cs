﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkManagement.Dto
{
    public class UserForReturnLogin
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Alias { get; set; }
        public int Role { get; set; }
        public int OCLevel { get; set; }
        public object ListOCs { get; set; }
        public byte[] image { get; set; }
        public bool IsLeader { get; set; }
    }
}
