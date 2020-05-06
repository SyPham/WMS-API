using Data.Extensions;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.ViewModel.Task
{
    public class TreeViewTask
    {
        public TreeViewTask()
        {
            this.children = new HashSet<TreeViewTask>();
        }
        public TreeViewTask(
            int ID,
            int ParentID,
            int Level,
            int ProjectID,
            Enum.JobType JobTypeID,
            Enum.PeriodType periodType,
            int CreatedBy,
            List<int> Deputies,
            List<int> PICs,
            string PIC,
            string JobName,
            string DeputyName,
            string ProjectName,
            string Follow,
            string VideoLink,
           string DueDateDaily,
           string DueDateWeekly,
           string DueDateMonthly,
           string SpecificDate,
           string ModifyDateTime,
            string state,
             List<BeAssigned> BeAssigneds,
             List<BeAssigned> DeputiesList,
            BeAssigned FromWho,
            FromWhere FromWhere,
            Models.Project Project,
            List<History.History> Histories,
            Models.User User,
            Models.Tutorial Tutorial
            )
        {
            this.ID = ID;
            this.ParentID = ParentID;
            this.Level = Level;
            this.ProjectID = ProjectID;
            this.JobTypeID = JobTypeID;
            this.periodType = periodType;
            this.CreatedBy = CreatedBy;
            this.Deputies = Deputies;
            this.PICs = PICs;
            this.PIC = PIC;
            this.JobName = JobName;
            this.DeputyName = DeputyName;
            this.DeputyName = ProjectName;
            this.Follow = Follow;
            this.VideoLink = VideoLink;
            this.DueDateDaily = DueDateDaily;
            this.DueDateWeekly = DueDateWeekly;
            this.DueDateMonthly = DueDateMonthly;
            this.SpecificDate = SpecificDate;
            this.ModifyDateTime = ModifyDateTime;
            this.state = state;
            this.BeAssigneds = BeAssigneds;
            this.DeputiesList = DeputiesList;
            this.FromWho = FromWho;
            this.FromWhere = FromWhere;
            this.Project = Project;
            this.Histories = Histories;
            this.User = User;
            this.Tutorial = Tutorial;
            this.children = new HashSet<TreeViewTask>();
        }
        public int ID { get; set; }
        public string Follow { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string TaskCode { get; set; }
        public string ProjectName { get; set; }
        public string JobName { get; set; }
        public string PIC { get; set; }
        public bool VideoStatus { get; set; }
        public Models.Tutorial Tutorial { get; set; }
        public string VideoLink { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public string CreatedDate { get; set; }
        public string DeputyName { get; set; }
        public Enum.JobType JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        public Enum.PeriodType periodType { get; set; }
        public string From { get; set; }
        public int CreatedBy { get; set; }
        public int ProjectID { get; set; }
        public Models.User User { get; set; }
        public List<BeAssigned> BeAssigneds { get; set; }
        public List<BeAssigned> DeputiesList { get; set; }
        public List<int> Deputies { get; set; }
        public List<int> PICs { get; set; }
        public BeAssigned FromWho { get; set; }
        public FromWhere FromWhere { get; set; }
        public Models.Project Project { get; set; }
        public List<History.History> Histories { get; set; }

        public string state { get; set; }
        public bool FinishTask { get; set; }
        public string DueDateDaily { get; set; }
        public string DueDate { get; set; }
        public string SpecificDueDate { get; set; }
        public string DueDateWeekly { get; set; }
        public string DueDateMonthly { get; set; }
        public string SpecificDate { get; set; }
        public string ModifyDateTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public List<Follow> Follows { get; set; }
        public bool BeAssigned { get; set; }
        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public HashSet<TreeViewTask> children { get; set; }
        public TreeViewTask AddChildOrg(
            int ID,
            int ParentID,
            int Level,
            int ProjectID,
            Enum.JobType JobTypeID,
            Enum.PeriodType periodType,
            int CreatedBy,
            List<int> Deputies,
            List<int> PICs,
            string PIC,
            string JobName,
            string DeputyName,
            string ProjectName,
            string Follow,
            string VideoLink,
           string DueDateDaily,
           string DueDateWeekly,
           string DueDateMonthly,
           string SpecificDate,
           string ModifyDateTime,
            string state,
             List<BeAssigned> BeAssigneds,
             List<BeAssigned> DeputiesList,
            BeAssigned FromWho,
            FromWhere FromWhere,
            Models.Project Project,
            List<History.History> Histories,
            Models.User User,
            Models.Tutorial Tutorial,
            string CreatedDate
            )
        {
            var newTree = new TreeViewTask(ID, ParentID, Level, ProjectID, JobTypeID, periodType, CreatedBy, Deputies,
            PICs, PIC, JobName, DeputyName, ProjectName, Follow, VideoLink, DueDateDaily, DueDateWeekly, DueDateMonthly,
           SpecificDate, ModifyDateTime, state, BeAssigneds, DeputiesList, FromWho, FromWhere, Project, Histories, User, Tutorial);

            children.Add(newTree);
            return newTree;
        }
        public static TreeViewTask GetNode(TreeViewTask father, int id)
        {
            if (father != null)
            {
                if (father.ID.Equals(id))
                    return father;


                if (father.children != null)
                    foreach (var child in father.children)
                    {
                        if (child.ID.Equals(id))
                            return child;

                        var tree = GetNode(child, id);

                        if (tree != null)
                            return tree;
                    }
            }
            return null;
        }
        public static HashSet<TreeViewTask> FillRecursive(HashSet<TreeViewTask> flatTasks, int parentId)
        {
            HashSet<TreeViewTask> recursiveObjects = new HashSet<TreeViewTask>();
            foreach (var c in flatTasks.Where(x => x.ParentID.Equals(parentId)))
            {
                recursiveObjects.Add(new TreeViewTask
                {
                    ID = c.ID,
                    JobName = c.JobName,
                    Level = c.Level,
                    ProjectID = c.ProjectID,
                    CreatedBy = c.CreatedBy,
                    CreatedDate = c.CreatedDate,
                    ProjectName = c.ProjectName,
                    state = c.state,
                    PriorityID = c.PriorityID,
                    Priority = c.Priority,
                    Follow = c.Follow,
                    PIC = c.PIC,
                    Histories = c.Histories,
                    PICs = c.PICs,
                    JobTypeID = c.JobTypeID,
                    FromWho = c.FromWho,
                    FromWhere = c.FromWhere,
                    BeAssigneds = c.BeAssigneds,
                    Deputies = c.Deputies,
                    VideoLink = c.VideoLink,
                    VideoStatus = c.VideoStatus,
                    DeputiesList = c.DeputiesList,
                    DueDateDaily = c.DueDateDaily,
                    DueDateWeekly = c.DueDateWeekly,
                    DueDateMonthly = c.DueDateMonthly,
                    SpecificDate = c.SpecificDate,
                    DeputyName = c.DeputyName,
                    Tutorial = c.Tutorial,
                    ModifyDateTime = c.ModifyDateTime,
                    periodType = c.periodType,
                    children = FillRecursive(flatTasks, c.ID)
                });
            }
            return recursiveObjects;
        }
        public static List<TreeViewTask> Flatten(TreeViewTask root)
        {

            var flattened = new List<TreeViewTask> { root };

            var children = root.children;

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(Flatten(child));
                }
            }

            return flattened;
        }
    }

    public class BeAssigned
    {
        public int ID { get; set; }
        public string Username { get; set; }
    }
    public class FromWhere
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
