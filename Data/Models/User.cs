using Data.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class User : IEntity
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int OCID { get; set; }
        public int LevelOC { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public string ImageURL { get; set; }
        public byte[] ImageBase64 { get; set; }
        public bool isLeader { get; set; }
        public virtual Role Role { get; set; }
        public virtual Follow Follow { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual Deputy Deputy { get; set; }

    }
}
