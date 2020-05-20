using Data.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [JsonObject(IsReference = true)]
    public class User : IEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
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
        public virtual ICollection<NotificationDetail> NotificationDetails { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Follow> Follows { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Deputy> Deputies { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<Manager> Managers { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
