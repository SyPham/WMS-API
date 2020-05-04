using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Data
{
    public class DataContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<OC> OCs { get; set; }
        public DbSet<OCUser> OCUsers { get; set; }
        public DbSet<FromWho> FromWhos { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Deputy> Deputies { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationDetail> NotificationDetails { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentDetail> CommentDetails { get; set; }
        public DbSet<JobType> JobTypes { get; set; }


        public DbSet<Chat> Chats { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Tutorial> Tutorials { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<UserJoinHub> UserJoinHubs { get; set; }
        public DbSet<UploadImage> UploadImages { get; set; }
        public DbSet<CheckTask> CheckTasks { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Manager>().HasKey(ba => new { ba.UserID, ba.ProjectID });
            builder.Entity<TeamMember>().HasKey(ba => new { ba.UserID, ba.ProjectID });
            builder.Entity<Tag>().HasKey(ba => new { ba.TaskID, ba.UserID });
            builder.Entity<OCUser>().HasKey(ba => new { ba.UserID, ba.OCID });

           

        }
    }
}
