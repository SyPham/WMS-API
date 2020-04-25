﻿using AutoMapper;
using Data;
using Data.Extensions;
using Data.Models;
using Data.ViewModel.Notification;
using Data.ViewModel.OC;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class TaskService : ITaskService
    {
        #region Properties
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IOCService _ocService;
        private readonly INotificationService _notificationService;
        private readonly string formatDaily = "{0:ddd, d MMM, yyyy}";
        private readonly string formatSpecificDate = "{0:d MMM, yyyy hh:mm:ss tt}";
        private readonly string formatCreatedDate = "{0:d MMM, yyyy hh:mm:ss tt}";
        private readonly string[] _monthArray = {
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
      };
        private readonly string[] _everydayArray = {
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday"
        };
        #endregion

        #region Constructor

        public TaskService(DataContext context, INotificationService notificationService, IMapper mapper, IUserService userService, IProjectService projectService, IOCService ocService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _projectService = projectService;
            _ocService = ocService;
            _notificationService = notificationService;
        }

        #endregion

        #region Helpers
        private bool TimeComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate, comparedate);

            return res < 0 ? true : false;
        }
        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns></returns>
        private bool DateTimeComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate, comparedate);
            return res <= 0 ? true : false;
        }
        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns></returns>
        private bool DateComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate.Date, comparedate.Date);
            return res <= 0 ? true : false;
        }
        private string ToFullTextMonthly(string month)
        {
            if (month.IsNullOrEmpty())
                return "";
            var result = string.Empty;
            foreach (var item in this._monthArray)
            {
                if (item.ToLower().Contains(month.ToLower()))
                {
                    result = item;
                    break;
                }
            };
            return result;
        }

        private string ToFullTextWeekly(string day)
        {
            if (day.IsNullOrEmpty())
                return "";
            var result = string.Empty;
            foreach (var item in this._everydayArray)
            {
                if (item.ToLower().Contains(day.ToLower()))
                {
                    result = item;
                    break;
                }
            };
            return result;
        }
        public string CastPriority(string value)
        {
            value = value.ToSafetyString().ToUpper() ?? "";
            if (value == "H")
                return "High";
            if (value == "M")
                return "Medium";
            if (value == "L")
                return "Low";
            return value;
        }
        public void HieararchyWalk(List<TreeViewTask> hierarchy)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    //Console.WriteLine(string.Format("{0} {1}", item.Id, item.Text));
                    HieararchyWalk(item.children);
                }
            }
        }

        public List<TreeViewTask> GetChildren(List<TreeViewTask> tasks, int parentid)
        {
            return tasks
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new TreeViewTask()
                    {
                        ID = c.ID,
                        DueDate = c.DueDate,
                        JobName = c.JobName,
                        Level = c.Level,
                        ProjectID = c.ProjectID,
                        CreatedBy = c.CreatedBy,
                        CreatedDate = c.CreatedDate,
                        From = c.From,
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
                        children = GetChildren(tasks, c.ID)
                    })
                    .OrderByDescending(x => x.ID)
                    .ToList();
        }
        public List<TreeViewTask> GetChildrenForMap(List<TreeViewTask> tasks, int parentid)
        {
            return tasks
                    .Where(c => c.ParentID == parentid && c.BeAssigned)
                    .Select(c => new TreeViewTask()
                    {
                        ID = c.ID,
                        DueDate = c.DueDate,
                        JobName = c.JobName,
                        Level = c.Level,
                        ProjectID = c.ProjectID,
                        CreatedBy = c.CreatedBy,
                        CreatedDate = c.CreatedDate,
                        From = c.From,
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
                        periodType = c.periodType,
                        ModifyDateTime = c.ModifyDateTime,
                        children = GetChildren(tasks, c.ID)
                    })
                    .OrderByDescending(x => x.ID)
                    .ToList();
        }
        public IEnumerable<TreeViewOC> GetAllDescendants(IEnumerable<TreeViewOC> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
            return rootNodes.Concat(descendants);
        }
        //public IEnumerable<TreeView> GetAllDescendants(IEnumerable<TreeView> rootNodes)
        //{
        //    var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
        //    return rootNodes.Concat(descendants);
        //}
        public IEnumerable<TreeViewTask> GetAllTaskDescendants(IEnumerable<TreeViewTask> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllTaskDescendants(x.children));
            return rootNodes.Concat(descendants);
        }

        private int FindParentByChild(IEnumerable<Data.Models.Task> rootNodes, int taskID)
        {
            //Kiem tra xem co trong list chua
            var parentid = 0;
            if (rootNodes.Any(x => x.ID.Equals(taskID)))
            {
                var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID));
                parentid = parent.ParentID;
                if (parent.ParentID == 0)
                    return parent.ID;
                else
                    return parent.ParentID;
            }
            else
                return FindParentByChild(rootNodes, parentid);

        }
        private Data.Models.Task ToFindParentByChild(IEnumerable<Data.Models.Task> rootNodes, int taskID)
        {
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID));
            else
                return ToFindParentByChild(rootNodes, parent);
        }
        private int FindParentByChild(IEnumerable<Data.ViewModel.Task.Task> rootNodes, int taskID)
        {
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ID;
            else
                return FindParentByChild(rootNodes, parent);
        }
        private async Task<List<int>> GetListUsersAlert(Data.Enum.JobType jobType, int userid, int projectid, int taskid)
        {
            var task = await _context.Tasks.FindAsync(taskid);
            var listUsers = new List<int> { task.FromWhoID, task.CreatedBy };
            switch (jobType)
            {
                case Data.Enum.JobType.Project:
                    var managers = await _context.Managers.Where(_ => _.ProjectID.Equals(projectid)).Select(_ => _.UserID).ToListAsync();
                    var members = await _context.TeamMembers.Where(_ => _.ProjectID.Equals(projectid)).Select(_ => _.UserID).ToListAsync();
                    listUsers.AddRange(managers.Union(members));
                    break;
                case Data.Enum.JobType.Routine:
                case Data.Enum.JobType.Abnormal:
                    var deputies = await _context.Deputies.Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
                    var pic = await _context.Tags.Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
                    var follow = await _context.Follows.Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
                    listUsers.AddRange(pic.Union(deputies).Union(follow));
                    break;
                default:
                    break;
            }
            //khong gui thong bao den chinh nguoi hoan thanh task
            return listUsers.Where(x => x != userid).Distinct().ToList();
        }

        private async Task<Tuple<List<int>>> AlertDeadlineChanging(Data.Enum.AlertDeadline alert, Data.Models.Task task, int userid, List<int> users)
        {
            var projectName = string.Empty;
            if (task.ProjectID > 0)
            {
                var project = await _context.Projects.FindAsync(task.ProjectID);
                projectName = project.Name;
            }
            var user = await _context.Users.FindAsync(userid);
            string urlResult = $"/todolist/{task.JobName.ToUrlEncode()}";
            var listUsers = new List<int>();
            switch (alert)
            {
                case Data.Enum.AlertDeadline.Weekly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.ChangeWeekly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeWeekly, task.DueDateWeekly),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case Data.Enum.AlertDeadline.Monthly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.ChangeMonthly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeMonthly, task.DueDateMonthly),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case Data.Enum.AlertDeadline.Quarterly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.ChangeQuarterly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeQuarterly),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case Data.Enum.AlertDeadline.Deadline:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.ChangeDeadline,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeDeputy),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                default:
                    break;
            }
            return Tuple.Create(listUsers);
        }
        private string AlertMessage(string username, string jobName, string project, bool isProject, Data.Enum.AlertType alertType, string deadline = "")
        {
            var message = string.Empty;
            switch (alertType)
            {
                case Data.Enum.AlertType.Done:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has already finished the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has already finished the task name ' {jobName} '";
                    break;
                case Data.Enum.AlertType.Remark:
                    break;
                case Data.Enum.AlertType.Undone:
                    break;
                case Data.Enum.AlertType.UpdateRemark:
                    break;
                case Data.Enum.AlertType.Assigned:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} assigned you the task name ' {jobName} ' ";
                    break;
                case Data.Enum.AlertType.ChangeDeputy:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as deputy of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has assigned you as deputy of the task name ' {jobName} '";
                    break;
                case Data.Enum.AlertType.Manager:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as manager of {project} project";
                    break;
                case Data.Enum.AlertType.Member:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as member of {project} project";
                    break;
                case Data.Enum.AlertType.ChangeDeadline:
                    break;
                case Data.Enum.AlertType.ChangeWeekly:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {ToFullTextWeekly(deadline)} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {ToFullTextWeekly(deadline)} of the task name ' {jobName} '";
                    break;
                case Data.Enum.AlertType.ChangeMonthly:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {ToFullTextMonthly(deadline)} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {ToFullTextMonthly(deadline)} of the task name ' {jobName} '";
                    break;
                default:
                    break;
            }
            return message;
        }
        private string CheckMessage(Data.Enum.JobType jobtype, string project, string username, string jobName, Data.Enum.AlertType alertType, string deadline = "")
        {
            var message = string.Empty;
            switch (jobtype)
            {
                case Data.Enum.JobType.Project:
                    message = AlertMessage(username, jobName, project, true, alertType, deadline);
                    break;
                case Data.Enum.JobType.Routine:
                case Data.Enum.JobType.Abnormal:
                    message = AlertMessage(username, jobName, project, false, alertType, deadline);
                    break;
            }
            return message;
        }
        private bool BeAlert(int taskId, string alertType)
        {
            var currentDate = DateTime.Now.Date;
            return _context.Notifications.Any(x => x.TaskID == taskId && x.CreatedTime.Date == currentDate && x.Function.Equals(alertType));
            //return false;
        }
        #endregion

        #region Mapper
        private string MapDueDateWithPeriod(Data.Models.Task task)
        {
            string result = string.Empty;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    result = task.DueDateDaily;
                    break;
                case Data.Enum.PeriodType.Weekly:
                    result = task.DueDateWeekly;
                    break;
                case Data.Enum.PeriodType.Monthly:
                    result = task.DueDateMonthly;
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    result = task.SpecificDate;
                    break;
                default:
                    break;
            }
            return result != string.Empty ? result.ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt") : string.Empty;
        }
        private List<TreeViewTask> MapperTreeViewTask(List<TreeViewTask> tasks)
        {
            var hierarchyTemp = new List<TreeViewTask>();
            var roots = tasks.Where(x => x.ParentID == 0).ToList();
            var childs = tasks.Where(x => x.ParentID != 0).ToList();

            var BeAdded = new List<TreeViewTask>();
            foreach (var c in roots)
            {
                var item = new TreeViewTask
                {
                    ID = c.ID,
                    DueDate = c.DueDate,
                    JobName = c.JobName,
                    Level = c.Level,
                    ProjectID = c.ProjectID,
                    CreatedBy = c.CreatedBy,
                    CreatedDate = c.CreatedDate,
                    From = c.From,
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
                    children = GetChildren(tasks, c.ID)
                };
                hierarchyTemp.Add(item);
                BeAdded.Add(item);
            }

            var flatBedAdded = BeAdded.Flatten().ToList();
            var except = childs.Where(x => !flatBedAdded.Select(c => c.ID).Contains(x.ID)).Select(x => x.ID).ToList();
            var lastChilds = childs.Where(x => except.Contains(x.ID)).ToList();
            var result = hierarchyTemp.Union(lastChilds).ToList();
            return result;
        }
        private List<TreeViewTask> GetListTreeViewTask(List<Data.Models.Task> listTasks, int userid)
        {
            var tasks = new List<TreeViewTask>();
            var ocModel = _context.OCs;
            var userModel = _context.Users;
            var subModel = _context.Follows;
            var tagModel = _context.Tags;
            var deputyModel = _context.Deputies;

            foreach (var item in listTasks)
            {
                var beAssigneds = tagModel.Where(x => x.TaskID == item.ID)
                                   .Include(x => x.User)
                                   .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                var deputiesList = deputyModel.Where(x => x.TaskID == item.ID)
                   .Join(_context.Users,
                   de => de.UserID,
                   user => user.ID,
                   (de, user) => new
                   {
                       user.ID,
                       user.Username
                   })
                   .Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).ToList();
                var statusTutorial = _context.Tutorials.Any(x => x.TaskID == item.ID);
                var tutorialModel = _context.Tutorials.FirstOrDefault(x => x.TaskID == item.ID);
                //var tasksTree = await GetListTree(item.ParentID, item.ID);
                var arrTasks = FindParentByChild(listTasks, item.ID);

                var levelItem = new TreeViewTask();
                //Primarykey
                levelItem.Follow = subModel.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

                levelItem.ID = item.ID;
                levelItem.PriorityID = item.Priority;
                levelItem.ProjectName = item.ProjectID == 0 ? "" : _context.Projects.Find(item.ProjectID).Name;
                levelItem.JobName = item.JobName.IsNotAvailable();
                levelItem.From = item.DepartmentID > 0 ? levelItem.From = ocModel.Find(item.DepartmentID).Name : levelItem.From = userModel.Find(item.FromWhoID).Username;
                levelItem.PIC = tagModel.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray().ToJoin().IsNotAvailable();
                levelItem.DeputiesList = deputiesList;
                levelItem.DeputyName = deputiesList.Select(x => x.Username).ToArray().ToJoin(" , ").IsNotAvailable();

                //DateTime
                levelItem.CreatedDate = item.CreatedDate.ToStringFormat(formatCreatedDate).IsNotAvailable();
                levelItem.DueDateDaily = item.DueDateDaily.ToStringFormatISO(formatDaily).IsNotAvailable();
                levelItem.DueDateWeekly = item.DueDateWeekly.IsNotAvailable();
                levelItem.DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth().IsNotAvailable();
                levelItem.SpecificDate = item.SpecificDate.ToStringFormatISO(formatSpecificDate).IsNotAvailable();
                levelItem.periodType = item.periodType;

                levelItem.ModifyDateTime = item.ModifyDateTime;
                levelItem.User = item.User;
                levelItem.BeAssigned = beAssigneds.Select(x => x.ID).Contains(userid);
                levelItem.Level = item.Level;
                levelItem.ProjectID = item.ProjectID;
                levelItem.ParentID = item.ParentID;
                levelItem.state = item.Status == false ? "Undone" : "Done";

                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PICs = beAssigneds.Select(x => x.ID).ToList();

                levelItem.BeAssigneds = beAssigneds;
                levelItem.Deputies = deputiesList.Select(_ => _.ID).ToList();
                levelItem.FromWhere = ocModel.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault() ?? new FromWhere();
                levelItem.FromWho = userModel.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault() ?? new BeAssigned();
                levelItem.JobTypeID = item.JobTypeID;
                levelItem.periodType = item.periodType;

                levelItem.FromWhoID = item.FromWhoID;
                levelItem.CreatedBy = item.CreatedBy;
                levelItem.VideoStatus = statusTutorial;
                levelItem.Tutorial = tutorialModel;
                levelItem.VideoLink = statusTutorial ? tutorialModel.URL : "";
                levelItem.DueDate = MapDueDateWithPeriod(item);
                tasks.Add(levelItem);
            }
            return tasks.OrderByDescending(x => x.ID).ToList();
        }
        public async Task<List<ProjectViewModel>> GetListProject()
        {
            return await _projectService.GetListProject();
        }
        public async Task<IEnumerable<TreeViewTask>> GetListTree(int parentID, int id)
        {
            var listTasks = await _context.Tasks
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();
            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                var levelItem = new TreeViewTask();
                levelItem.ID = item.ID;
                levelItem.Level = item.Level;
                levelItem.ParentID = item.ParentID;
                tasks.Add(levelItem);
            }

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        public IEnumerable<TreeViewTask> GetListTreeByParent(List<TreeViewTask> tasks, int parentID, int id)
        {
            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        public async Task<IEnumerable<TreeViewTask>> GetListTreeByParent(int parentID, int id, int userid)
        {
            var listTasks = await _context.Tasks
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();
            var tasks = GetListTreeViewTask(listTasks, userid);

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        private async Task<IEnumerable<TreeViewTask>> GetListTreeForUndo(int parentID, int id)
        {
            var listTasks = await _context.Tasks
               .Where(x => x.Status == true && x.FinishedMainTask == true)
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();

            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();

                var levelItem = new TreeViewTask();
                levelItem.ID = item.ID;
                levelItem.PIC = string.Join(" , ", _context.Tags.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray());
                levelItem.ProjectName = item.ProjectID == 0 ? "" : _context.Projects.Find(item.ProjectID).Name;
                levelItem.ProjectID = item.ProjectID;
                levelItem.BeAssigneds = beAssigneds;
                levelItem.Level = item.Level;

                levelItem.ParentID = item.ParentID;
                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PriorityID = item.Priority;
                levelItem.CreatedDate = String.Format("{0:d MMM, yyyy}", item.CreatedDate);
                levelItem.User = item.User;
                levelItem.FromWhere = _context.OCs.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault();

                levelItem.FromWho = _context.Users.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault();
                levelItem.JobName = item.JobName.IsNotAvailable();
                levelItem.state = item.Status == false ? "Undone" : "Done";

                levelItem.From = item.OCID > 0 ? _context.OCs.Find(item.OCID).Name : _context.Users.Find(item.FromWhoID).Username.IsNotAvailable();
                levelItem.DueDateDaily = item.DueDateDaily.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:D}", item.DueDateDaily.ToParseIso8601());
                levelItem.DueDateWeekly = item.DueDateWeekly;
                levelItem.DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth();
                levelItem.SpecificDate = item.SpecificDate.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:s}", item.SpecificDate.ToParseIso8601());

                tasks.Add(levelItem);
            }

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        #endregion

        #region LoadData helper
        private List<int> GetListUserRelateToTask(int taskId, bool isProject)
        {
            var task = _context.Tasks.Find(taskId);
            //var listManager = _context.Managers.Where(_ => _.ProjectID.Equals(task.ProjectID)).Select(_ => _.UserID).ToList();
            //var listMember = _context.TeamMembers.Where(_ => _.ProjectID.Equals(task.ProjectID)).Select(_ => _.UserID).ToList();
            var listPIC = _context.Tags.Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            var listFollow = _context.Follows.Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            var listDeputie = _context.Deputies.Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            if (isProject)
                return listPIC.Union(listFollow).ToList();
            else
                return listPIC.Union(listFollow).Union(listDeputie).ToList();
        }
        private async System.Threading.Tasks.Task AlertTasksIsLate(TreeViewTask item, string message, bool isProject)
        {
            var notifyParams = new CreateNotifyParams
            {
                TaskID = item.ID,
                Users = GetListUserRelateToTask(item.ID, isProject),
                Message = message,
                URL = "",
                AlertType = Data.Enum.AlertType.BeLate
            };
            var update = await _context.Tasks.FindAsync(item.ID);
            if (update.periodType != Data.Enum.PeriodType.SpecificDate)
            {
                update.Status = false;
                update.FinishedMainTask = false;
                await _context.SaveChangesAsync();

                var history = new History
                {
                    TaskID = item.ID,
                    Status = false

                };
                switch (item.periodType)
                {
                    case Data.Enum.PeriodType.Daily:
                        history.Deadline = update.DueDateDaily;
                        update.DueDateDaily = PeriodDaily(update);
                        break;
                    case Data.Enum.PeriodType.Weekly:
                        history.Deadline = update.DueDateWeekly;
                        update.DueDateWeekly = PeriodWeekly(update);
                        break;
                    case Data.Enum.PeriodType.Monthly:
                        history.Deadline = update.DueDateMonthly;
                        update.DueDateMonthly = PeriodMonthly(update);
                        break;
                    default:
                        break;
                }
                if (notifyParams.Users.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    await PushTaskToHistory(history);
                    await _notificationService.Create(notifyParams);
                }
            }

        }
        private string Message(Data.Enum.PeriodType periodType, TreeViewTask item)
        {
            var mes = string.Empty;
            switch (periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateDaily.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var dateofWeely = item.DueDateWeekly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    mes = $"You are late for the task name: '{item.JobName}' on {dateofWeely}";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var dateofmonthly = item.DueDateMonthly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    mes = $"You are late for the task name: '{item.JobName}' on {dateofmonthly}";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.SpecificDate.ToStringFormatISO(formatDaily)}";
                    break;
                default:
                    break;
            }
            return mes;
        }
        private async System.Threading.Tasks.Task PeriodType(TreeViewTask item, bool isProject)
        {
            string mes = Message(item.periodType, item);
            string belate = Data.Enum.AlertType.BeLate.ToSafetyString();
            var task = await _context.Tasks.FindAsync(item.ID);
            var currentDate = DateTime.Now.Date;
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var dateDaily = item.DueDateDaily.ToParseIso8601();
                    if ((currentDate > dateDaily.Date || DateTime.Now.Date < dateDaily.Date) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var dateWeekly = PeriodWeekly(task).ToParseStringDateTime();
                    if (currentDate > dateWeekly.Date && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var dateofmonthly = PeriodMonthly(task).ToParseStringDateTime();
                    if (currentDate > dateofmonthly.Date && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var specific = item.SpecificDate.ToParseIso8601();
                    if (!TimeComparator(specific) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                default:
                    break;
            }
        }
        private async System.Threading.Tasks.Task ProjectTaskIsLate(List<TreeViewTask> tasks)
        {

            foreach (var item in tasks)
            {
                await PeriodType(item, true);
            }
        }
        private async System.Threading.Tasks.Task RoutineTaskIsLate(List<TreeViewTask> tasks)
        {
            foreach (var item in tasks)
            {
                await PeriodType(item, false);
            }
        }
        private async System.Threading.Tasks.Task AbnormalTaskIsLate(List<TreeViewTask> tasks)
        {

            foreach (var item in tasks)
            {
                await PeriodType(item, false);
            }
        }
        public async System.Threading.Tasks.Task ProjectTaskIsLate(int userid)
        {
            //Lay tat ca list task chua hoan thanh va task co con chua hoan thnah
            var listTasks = await _context.Tasks
                            .Where(x => x.Status == false)
                            .Include(x => x.User)
                            .OrderBy(x => x.Level).ToListAsync();
            var tasks = GetListTreeViewTask(listTasks, userid);
            try
            {
                await ProjectTaskIsLate(tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Project).ToList());
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async System.Threading.Tasks.Task TaskListIsLate(int userid)
        {
            //Lay tat ca list task chua hoan thanh va task co con chua hoan thnah
            var listTasks = await _context.Tasks
                            .Where(x => x.Status == false)
                            .Include(x => x.User)
                            .OrderBy(x => x.Level).ToListAsync();
            var tasks = GetListTreeViewTask(listTasks, userid).Where(x => x.PICs.Count > 0).ToList();
            try
            {

                await ProjectTaskIsLate(tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Project).ToList());
                await RoutineTaskIsLate(tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Routine).ToList());
                await AbnormalTaskIsLate(tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Abnormal).ToList());
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private List<Data.Models.Task> Fillter(List<Data.Models.Task> listTasks, string sort, string priority, int userid, string startDate, string endDate, string weekdays, string monthly, string quarterly)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).ToList();
            }

            //Loc theo weekdays
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateWeekly.ToSafetyString().ToLower().Equals(weekdays.ToLower())).ToList();
            }
            //loc theo thang
            if (!monthly.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateMonthly.ToSafetyString().ToLower().Equals(monthly.ToLower())).ToList();
            }

            if (!sort.IsNullOrEmpty())
            {
                sort = sort.ToLower();
                if (sort == Data.Enum.JobType.Project.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Project)).OrderByDescending(x => x.ProjectID).ToList();
                if (sort == Data.Enum.JobType.Routine.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Routine)).OrderByDescending(x => x.CreatedDate).ToList();
                if (sort == Data.Enum.JobType.Abnormal.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Abnormal)).OrderByDescending(x => x.CreatedDate).ToList();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).ToList();
            }
            return listTasks;
        }
        private async Task<List<int>> GetTasksByUser(int userid)
        {
            // Lay tat ca task chua hoan thanh tren todolist
            var listTasks = await _context.Tasks
              .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
              .Include(x => x.User)
              .OrderByDescending(x => x.ID).ToListAsync();
            // Tim task ma user duoc chi dinh lam
            var assignedTasks = _context.Tags.Where(x => x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();
            var taskdeputy = _context.Deputies.Where(x => x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();

            // Tim projectid lien quan den task
            var taskManager = _context.Managers.Where(x => x.UserID.Equals(userid)).Select(x => x.ProjectID).ToArray();
            var taskMember = _context.TeamMembers.Where(x => x.UserID.Equals(userid)).Select(x => x.ProjectID).ToArray();
            var listProjectID = taskManager.Union(taskMember);

            // Co duoc listProjectID tim list taskID
            var listTaskIDProject = listTasks.Where(x => listProjectID.Contains(x.ProjectID)).Select(x => x.ID).ToList();
            // Tim Asinged va beAsigned trong list nay
            var listTaskIDBeAssignedProject = _context.Tags.Where(x => listTaskIDProject.Contains(x.TaskID) && x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();

            var listCreateBy = listTasks.Where(x => x.CreatedBy == userid).Select(x => x.ID).ToList();
            var listFromWho = listTasks.Where(x => x.FromWhoID == userid).Select(x => x.ID).ToList();
            var listAll = assignedTasks.Union(taskdeputy).Union(listTaskIDBeAssignedProject).Union(listCreateBy).Union(listFromWho).ToList();
            return listAll;
        }
        private object GetAlertDueDate()
        {
            var date = DateTime.Now.Date;
            var list = _context.Tasks.Where(x => x.periodType == Data.Enum.PeriodType.SpecificDate && x.CreatedDate.Date == date).Select(x => new
            {
                x.CreatedDate,
                x.SpecificDate
            }).ToList();
            return list;
        }
        public async Task<object> LoadTask(string name, int userid, int ocID, int page, int pageSize)
        {
            var source = _context.Tasks.Where(x => x.CreatedBy == userid && x.Status == false).AsQueryable();

            var userBeAssignedTask = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToArray();
            if (userBeAssignedTask.Count() > 0)
            {
                source = source.Where(x => userBeAssignedTask.Contains(x.ID));
            }

            if (!name.IsNullOrEmpty())
            {
                source = source.Where(x => x.JobName.Contains(name));
            }
            return await PagedList<Data.Models.Task>.CreateAsync(source, page, pageSize);
        }
        public async Task<object> LoadTaskHistory(string name, int userid, int ocID, int page, int pageSize)
        {
            var source = _context.Tasks.Where(x => x.CreatedBy == userid && x.Status == true).AsQueryable();

            var userBeAssignedTask = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToArray();
            if (userBeAssignedTask.Count() > 0)
            {
                source = source.Where(x => userBeAssignedTask.Contains(x.ID));
            }

            if (!name.IsNullOrEmpty())
            {
                source = source.Where(x => x.JobName.Contains(name));
            }
            return await PagedList<Data.Models.Task>.CreateAsync(source, page, pageSize);
        }
        public async Task<object> GetListUser(int userid, int projectid)
        {
            if (projectid > 0)
            {
                var userModel = _context.Users;
                // var manager = await _context.Managers.FindAsync(projectid);
                var member = await _context.TeamMembers.Where(x => x.ProjectID == projectid).Select(x => x.UserID).ToListAsync();
                return await _context.Users.Where(x => member.Contains(x.ID)).Select(x => new { x.ID, x.Username }).ToListAsync();
            }
            else
            {
                var user = await _userService.GetByID(userid);
                var ocID = user.OCID;
                var oc = await _context.OCs.FindAsync(ocID);
                var OCS = await _ocService.GetListTreeOC(oc.ParentID, oc.ID);
                var arrOCs = GetAllDescendants(OCS).Select(x => x.ID).ToArray();
                return await _context.Users.Where(x => arrOCs.Contains(x.OCID)).Select(x => new { x.ID, x.Username }).ToListAsync();
            }
        }
        public async Task<object> From(int userid)
        {
            var user = await _userService.GetByID(userid);
            var ocID = user.OCID;
            var arrOCs = new List<int>();
            var ocs = new object();
            if (ocID > 0)
            {
                var oc = await _context.OCs.FindAsync(ocID);
                var OCS = await _ocService.GetListTreeOC(oc.ParentID, oc.ID);
                arrOCs = GetAllDescendants(OCS).Select(x => x.ID).ToList();
                ocs = await _context.OCs.Where(x => arrOCs.Contains(x.ID)).Select(x => new { x.ID, x.Name }).ToListAsync();
            }
            var users = await _context.Users.Where(x => arrOCs.Contains(x.OCID)).Select(x => new { x.ID, x.Username }).ToListAsync();
            return new
            {
                users,
                ocs
            };
        }
        public async Task<List<TreeViewTask>> GetListTree()
        {
            var listTasks = await _context.Tasks
                .Where(x => x.Status == false)
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();

            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();

                var levelItem = new TreeViewTask();
                levelItem.ID = item.ID;
                levelItem.PIC = string.Join(" , ", _context.Tags.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray());
                levelItem.ProjectName = item.ProjectID == 0 ? "" : _context.Projects.Find(item.ProjectID).Name;
                levelItem.ProjectID = item.ProjectID;
                levelItem.BeAssigneds = beAssigneds;
                levelItem.Level = item.Level;

                levelItem.ParentID = item.ParentID;
                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PriorityID = item.Priority;
                levelItem.CreatedDate = String.Format("{0:d MMM, yyyy}", item.CreatedDate);
                levelItem.User = item.User;
                levelItem.FromWhere = _context.OCs.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault();

                levelItem.FromWho = _context.Users.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault();
                levelItem.JobName = item.JobName.IsNotAvailable();
                levelItem.state = item.Status == false ? "Undone" : "Done";
                levelItem.From = item.OCID > 0 ? _context.OCs.Find(item.OCID).Name : _context.Users.Find(item.FromWhoID).Username.IsNotAvailable();
                levelItem.DueDateDaily = item.DueDateDaily.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:D}", item.DueDateDaily.ToParseIso8601());
                levelItem.DueDateWeekly = item.DueDateWeekly;
                levelItem.DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth();
                levelItem.SpecificDate = item.SpecificDate.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:s}", item.SpecificDate.ToParseIso8601());

                tasks.Add(levelItem);
            }

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ParentID == 0)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        private IEnumerable<TreeViewTask> FindListTreeByParentID(List<TreeViewTask> tasks, int parentID, int id)
        {

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
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
                                periodType = c.periodType,
                                ModifyDateTime = c.ModifyDateTime,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();
            return hierarchy;
        }

        public async Task<object> GetDeputies()
        {
            return await _context.Users.Where(x => x.Username != "admin").Select(x => new { x.Username, x.ID }).ToListAsync();
        }
        #endregion

        #region Main LoadData
        //todolist()
        public async Task<List<TreeViewTask>> GetListTree(string beAssigned, string assigned, int userid)
        {

            try
            {
                //Lay tat ca list task chua hoan thanh va task co con chua hoan thnah
                var tags = await _context.Tags.ToListAsync();

                var listTasks = await _context.Tasks
                .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();

                //Tim tat ca task dc chi dinh lam pic
                var taskpic = tags.Where(x => x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();
                var taskdeputy = _context.Deputies.Where(x => x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();
                var taskPICDeputies = taskpic.Union(taskdeputy);

                //tim tat ca task dc chi dinh lam manager, member
                var taskManager = _context.Managers.Where(x => x.UserID.Equals(userid)).Select(x => x.ProjectID).ToArray();
                var taskMember = _context.TeamMembers.Where(x => x.UserID.Equals(userid)).Select(x => x.ProjectID).ToArray();
                var taskManagerMember = taskManager.Union(taskMember);

                //vao bang pic tim tat ca nhung task va thanh vien giao cho nhau hoac manager giao cho member
                //Tim dc tat ca cac task ma manager hoac thanh vien tao ra
                var taskpicProject = listTasks.Where(x => taskManagerMember.Contains(x.CreatedBy)).Select(x => x.ID).ToArray();
                //Tim tiep nhung tag nao duoc giao cho user hien tai
                var beAssignProject = tags.Where(x => taskpicProject.Contains(x.TaskID) && x.UserID.Equals(userid)).Select(x => x.TaskID).ToArray();
                var listbeAssignProject = new List<int>();
                var taskModel = listTasks.Where(x => beAssignProject.Contains(x.ID)).ToList();

                foreach (var task in taskModel)
                {
                    var tasksTree = await GetListTree(task.ParentID, task.ID);
                    var arrTasks = GetAllTaskDescendants(tasksTree).Select(x => x.ID).ToList();
                    listbeAssignProject.AddRange(arrTasks);
                }

                listTasks = listTasks.Where(x => x.CreatedBy.Equals(userid) || taskPICDeputies.Contains(x.ID) || listbeAssignProject.Contains(x.ID)).Distinct().ToList();


                if (!beAssigned.IsNullOrEmpty() && beAssigned == "BeAssigned")
                {
                    listTasks = listTasks.Where(x => taskpic.Contains(x.ID)).ToList();
                }
                if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                {
                    listTasks = listTasks.Where(x => x.CreatedBy.Equals(userid)).ToList();
                }
                var tasks = GetListTreeViewTask(listTasks, userid);

                if (tasks.Count == 1)
                {
                    if (!tasks.FirstOrDefault().HasChildren)
                        return tasks;
                }
                return MapperTreeViewTask(tasks).OrderByDescending(x => x.JobTypeID)
                    .OrderByDescending(x => x.ID)
                    .ToList();
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }

        }

        public async Task<List<TreeViewTask>> GetListTree(string sort, string priority, int userid, string startDate, string endDate, string weekdays, string monthly, string quarterly)
        {

            try
            {
                var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
                var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

                var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
                var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

                var UserProjectList = managers.Union(members).ToList();
                var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

                var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();
                var listTasks = await _context.Tasks
                .Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid)
                .Where(x => ((x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false)) && x.JobTypeID != Data.Enum.JobType.Project || (x.JobTypeID == Data.Enum.JobType.Project && x.Status == false && x.FinishedMainTask == false || x.JobTypeID == Data.Enum.JobType.Project && x.Status == true && x.FinishedMainTask == false))
                .Include(x => x.User)
                .OrderByDescending(x => x.ID).ToListAsync();
                var listTasksFillter = Fillter(listTasks, sort, priority, userid, startDate, endDate, weekdays, monthly, quarterly);
                //Flatten task
                var tasks = GetListTreeViewTask(listTasksFillter, userid).Where(x => x.PICs.Count > 0).ToList();
                //hierarchy task
                var hierarchy = MapperTreeViewTask(tasks)
                  .OrderByDescending(x => x.JobTypeID)
                  .OrderByDescending(x => x.ID)
                  .ToList();
                return hierarchy;
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }

        }
        private List<Data.Models.Task> FilterAbnormal(List<Data.Models.Task> listTasks, int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;

                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).ToList();
            }
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateWeekly.Equals(weekdays)).ToList();
            }

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).ToList();
            }
            return listTasks;
        }
        public async Task<List<TreeViewTask>> GetListTreeAbnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            try
            {
                //Khai bao enum Jobtype la Abnormal
                var jobtype = Data.Enum.JobType.Abnormal;

                //Lay tat ca task theo jobtype la abnormal va oc
                var listTasks = await _context.Tasks
                .Where(x => x.OCID.Equals(ocid) && x.JobTypeID.Equals(jobtype))
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();
                listTasks = FilterAbnormal(listTasks, ocid, priority, userid, startDate, endDate, weekdays);

                var tasks = GetListTreeViewTask(listTasks, userid).Where(
                   x =>
                   x.PICs.Contains(userid)
                || x.FromWhoID == userid
                || x.CreatedBy == userid
                || x.Deputies.Contains(userid)).ToList();

                var hierarchy = MapperTreeViewTask(tasks)

                .OrderByDescending(x => x.ID)
                .ToList();

                return hierarchy;
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }

        }
        private List<Data.Models.Task> FilterTaskDetail(List<Data.Models.Task> listTasks, string priority)
        {
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).ToList();
            }
            return listTasks;
        }
        //private List<Data.Models.Task> SortTaskDetail(List<Data.Models.Task> listTasks, string sort)
        //{
        //    if (!sort.IsNullOrEmpty())
        //    {
        //        listTasks = listTasks.Where(x => x.Priority.Equals(Data.Enum.)).ToList();
        //    }
        //    return listTasks;
        //}
        public async Task<List<TreeViewTask>> GetListTreeProjectDetail(string sort, string priority, int userid, int projectid)
        {
            try
            {
                var jobtype = Data.Enum.JobType.Project;

                var listTasks = await _context.Tasks
                .Where(x => x.JobTypeID.Equals(jobtype) && x.ProjectID.Equals(projectid))
                .Include(x => x.User)
                .ToListAsync();
                listTasks = FilterTaskDetail(listTasks, priority);
                var tasks = GetListTreeViewTask(listTasks, userid).ToList();

                return MapperTreeViewTask(tasks).OrderByDescending(x => x.JobTypeID)
                   .OrderByDescending(x => x.ID)
                   .ToList();
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }

        }

        private List<Data.Models.Task> SortFollow(List<Data.Models.Task> listTasks, string sort, string priority)
        {

            if (!sort.IsNullOrEmpty())
            {
                if (sort == "project")
                    listTasks = listTasks.Where(x => x.ProjectID > 0).ToList();
                if (sort == "routine")
                    listTasks = listTasks.Where(x => x.ProjectID == 0).ToList();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).ToList();
            }
            return listTasks;
        }
        public async Task<List<TreeViewTask>> GetListTreeFollow(string sort, string priority, int userid)
        {
            try
            {
                var listTasks = await _context.Tasks
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();

                listTasks = SortFollow(listTasks, sort, priority);
                var tasks = GetListTreeViewTask(listTasks, userid);

                List<TreeViewTask> hierarchy = new List<TreeViewTask>();
                if (tasks.Count == 1)
                {
                    if (!tasks.FirstOrDefault().HasChildren)
                        return tasks;
                }
                hierarchy = tasks.Where(c => c.ParentID == 0)
                                .Select(c => new TreeViewTask
                                {
                                    ID = c.ID,
                                    DueDate = c.DueDate,
                                    JobName = c.JobName,
                                    Level = c.Level,
                                    ProjectID = c.ProjectID,
                                    CreatedBy = c.CreatedBy,
                                    CreatedDate = c.CreatedDate,
                                    From = c.From,
                                    ProjectName = c.ProjectName,
                                    state = c.state,
                                    FromWho = c.FromWho,
                                    FromWhere = c.FromWhere,
                                    PIC = c.PIC,
                                    PriorityID = c.PriorityID,
                                    Priority = c.Priority,
                                    BeAssigneds = c.BeAssigneds,
                                    Follow = c.Follow,
                                    DueDateDaily = c.DueDateDaily,
                                    DueDateWeekly = c.DueDateWeekly,
                                    DueDateMonthly = c.DueDateMonthly,
                                    SpecificDate = c.SpecificDate,
                                    JobTypeID = c.JobTypeID,
                                    periodType = c.periodType,
                                    children = GetChildren(tasks, c.ID)
                                })
                                .ToList();
                HieararchyWalk(hierarchy);

                return hierarchy.OrderByDescending(x => x.JobTypeID)
                    .OrderByDescending(x => x.ProjectID)
                    .ToList(); ;
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }

        }
        private List<Data.Models.Task> SortRoutine(List<Data.Models.Task> listTasks, string sort, string priority)
        {

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).ToList();
            }
            return listTasks;
        }
        public async Task<List<TreeViewTask>> GetListTreeRoutine(string sort, string priority, int userid, int ocid)
        {
            try
            {
                var jobtype = Data.Enum.JobType.Routine;
                var user = await _context.Users.FindAsync(userid);
                if (ocid == 0)
                    return new List<TreeViewTask>();

                var listTasks = await _context.Tasks
                    .Where(x => x.OCID.Equals(ocid) && x.JobTypeID.Equals(jobtype))
                    .Include(x => x.User)
                    .OrderBy(x => x.Level).ToListAsync();

                //sort
                listTasks = SortRoutine(listTasks, sort, priority);

                //Duoi nay la load theo tree view con nao thi cha do
                var tasks = GetListTreeViewTask(listTasks, userid).Where(
                   x =>
                   x.PICs.Contains(userid)
                || x.FromWhoID == userid
                || x.CreatedBy == userid
                || x.Deputies.Contains(userid)).ToList();

                var hierarchy = MapperTreeViewTask(tasks)
                .OrderByDescending(x => x.ID)
                .ToList();

                return hierarchy;
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }
        }
        public async Task<List<TreeViewTask>> GetListTreeHistory(int userid, string start, string end)
        {
            try
            {
                var tasks = new List<TreeViewTask>();
                var tasks2 = new List<TreeViewTask>();
                var ocModel = _context.OCs;
                var userModel = _context.Users;
                var subModel = _context.Follows;
                var deputies = _context.Deputies;
                var managers = _context.Managers;
                var teamMembers = _context.TeamMembers;
                var tags = _context.Tags;
                var listTasks = new List<Data.Models.Task>();
                var listTasks2 = await _context.Histories.Join(
               _context.Tasks.Include(x => x.User),
                his => his.TaskID,
                task => task.ID,
               (his, task) => new
               {
                   his,
                   task
               }).ToListAsync();

                foreach (var x in listTasks2)
                {
                    listTasks.Add(new Data.Models.Task
                    {
                        ID = x.his.TaskID,
                        CreatedBy = x.task.CreatedBy,
                        Status = x.his.Status,
                        CreatedDate = x.task.CreatedDate,
                        ParentID = x.task.ParentID,
                        Level = x.task.Level,
                        ProjectID = x.task.ProjectID,
                        JobName = x.task.JobName,
                        OCID = x.task.OCID,
                        FromWhoID = x.task.FromWhoID,
                        Priority = x.task.Priority,
                        FinishedMainTask = x.task.FinishedMainTask,
                        JobTypeID = x.task.JobTypeID,
                        periodType = x.task.periodType,
                        User = x.task.User,
                        DepartmentID = x.task.DepartmentID,
                        DueDateDaily = x.task.DueDateDaily,
                        DueDateMonthly = x.task.DueDateMonthly,
                        DueDateWeekly = x.task.DueDateWeekly,
                        SpecificDate = x.task.SpecificDate,
                        ModifyDateTime = x.his.ModifyDateTime,
                    });
                }

                if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
                {
                    var timespan = new TimeSpan(0, 0, 0);
                    var startDate = start.Replace(" GMT+0700 (Indochina Time)", "").ToParseStringDateTime().Date;
                    var endDate = end.Replace(" GMT+0700 (Indochina Time)", "").ToParseStringDateTime().Date;
                    listTasks = listTasks.Where(x => x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date).ToList();
                }

                foreach (var item in listTasks)
                {
                    var beAssigneds = tags.Where(x => x.TaskID == item.ID)
                         .Include(x => x.User)
                         .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                    var deputiesList = deputies.Where(x => x.TaskID == item.ID)
                       .Join(_context.Users,
                       de => de.UserID,
                       user => user.ID,
                       (de, user) => new
                       {
                           user.ID,
                           user.Username
                       })
                       .Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).ToList();
                    var levelItem = new TreeViewTask();
                    levelItem.ID = item.ID;
                    levelItem.Deputies = deputies.Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToList();
                    levelItem.FromWhoID = item.FromWhoID;
                    levelItem.PICs = beAssigneds.Select(x => x.ID).ToList();
                    levelItem.PIC = string.Join(" , ", _context.Tags.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray()).IsNotAvailable();
                    levelItem.ProjectName = item.ProjectID == 0 ? "" : _context.Projects.Find(item.ProjectID).Name;
                    levelItem.ProjectID = item.ProjectID;
                    levelItem.BeAssigneds = beAssigneds;
                    levelItem.DeputiesList = deputiesList;
                    levelItem.DeputyName = string.Join(" , ", deputiesList.Select(x => x.Username).ToArray()).IsNotAvailable();
                    levelItem.Level = item.Level;
                    levelItem.Follow = subModel.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No"; ;
                    levelItem.ParentID = item.ParentID;
                    levelItem.Priority = CastPriority(item.Priority);
                    levelItem.PriorityID = item.Priority;
                    levelItem.CreatedBy = item.CreatedBy;
                    levelItem.JobTypeID = item.JobTypeID;

                    levelItem.User = item.User;
                    levelItem.FromWhere = ocModel.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault() ?? new FromWhere();
                    levelItem.FromWho = userModel.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault() ?? new BeAssigned();
                    levelItem.JobName = item.JobName.IsNotAvailable();
                    levelItem.state = !item.Status ? "Late" : "Ontime";
                    if (item.DepartmentID > 0)
                    {
                        levelItem.From = ocModel.Find(item.DepartmentID).Name;
                    }
                    else
                    {
                        levelItem.From = userModel.Find(item.FromWhoID).Username;
                    }
                    levelItem.periodType = item.periodType;
                    levelItem.CreatedDate = item.CreatedDate.ToString("d MMM, yyyy hh:mm:ss tt");
                    levelItem.DueDateDaily = item.DueDateDaily;
                    levelItem.DueDateWeekly = item.DueDateWeekly;
                    levelItem.DueDateMonthly = item.DueDateMonthly;
                    levelItem.SpecificDate = item.SpecificDate;
                    levelItem.ModifyDateTime = item.ModifyDateTime;
                    levelItem.DueDate = MapDueDateWithPeriod(item);
                    tasks.Add(levelItem);
                }
                tasks = tasks.Where(
                     x =>
                     x.PICs.Contains(userid)
                  || x.FromWhoID == userid
                  || x.CreatedBy == userid
                  || x.Deputies.Contains(userid))
                    .ToList();
                return tasks.OrderByDescending(x => x.ID).ToList();
            }
            catch (Exception ex)
            {

                return new List<TreeViewTask>();
            }
        }

        #endregion

        #region Event( Create Task, Sub-Task, Follow, Undo, Delete, Done, Remark, ...)

        public async Task<object> Unsubscribe(int id, int userid)
        {
            try
            {
                if (_context.Follows.Any(x => x.TaskID == id && x.UserID == userid))
                {
                    var sub = await _context.Follows.FirstOrDefaultAsync(x => x.TaskID == id && x.UserID == userid);
                    var taskModel = await _context.Tasks.FindAsync(sub.TaskID);

                    var tasks = await GetListTree(taskModel.ParentID, taskModel.ID);
                    var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();

                    var listTasks = await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).Select(x => x.ID).ToListAsync();


                    var listSub = await _context.Follows.Where(x => listTasks.Contains(x.TaskID) && x.UserID == userid).ToListAsync();
                    _context.Follows.RemoveRange(listSub);

                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<object> CreateSubTask(CreateTaskViewModel task)
        {
            try
            {
                task.DueDate = task.DueDate.ToStringFormatDateTime();
                if (!await _context.Tasks.AnyAsync(x => x.ID == task.ID))
                {
                    var item = _mapper.Map<Data.Models.Task>(task);

                    //Level cha tang len 1 va gan parentid cho subtask
                    var taskParent = _context.Tasks.Find(item.ParentID);
                    item.Level = taskParent.Level + 1;
                    item.ParentID = task.ParentID;
                    item.ProjectID = task.ProjectID;
                    item.JobTypeID = taskParent.JobTypeID;
                    item = CheckDuedate(item, task);
                    await _context.Tasks.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await CloneCode(item);
                    if (task.PIC != null)
                    {
                        var tags = new List<Tag>();
                        foreach (var pic in task.PIC)
                        {
                            tags.Add(new Tag
                            {
                                UserID = pic,
                                TaskID = item.ID
                            });
                        }
                        await _context.Tags.AddRangeAsync(tags);
                    }
                    if (task.Deputies != null)
                    {
                        var deputies = new List<Deputy>();
                        foreach (var deputy in task.Deputies)
                        {
                            deputies.Add(new Deputy
                            {
                                UserID = deputy,
                                TaskID = item.ID
                            });
                        }
                        await _context.Deputies.AddRangeAsync(deputies);
                    }
                    await _context.SaveChangesAsync();

                    return true;

                }
                else
                {
                    var edit = _context.Tasks.Find(task.ID);
                    edit.Priority = task.Priority.ToUpper();
                    edit.JobName = task.JobName;
                    edit.Priority = task.Priority;
                    edit.OCID = task.OCID;
                    edit.FromWhoID = task.FromWhoID;
                    edit = CheckDuedate(edit, task);
                    if (task.PIC != null)
                    {
                        var tags = new List<Tag>();
                        var listDelete = await _context.Tags.Where(x => task.PIC.Contains(x.UserID) && x.TaskID == edit.ID).ToListAsync();
                        if (listDelete.Count > 0)
                        {
                            _context.Tags.RemoveRange(listDelete);
                        }

                        foreach (var pic in task.PIC)
                        {
                            tags.Add(new Tag
                            {
                                UserID = pic,
                                TaskID = edit.ID
                            });
                            await _context.Tags.AddRangeAsync(tags);
                        }
                    }
                    if (task.Deputies != null)
                    {
                        var deputies = new List<Deputy>();
                        var listDelete = await _context.Deputies.Where(x => task.Deputies.Contains(x.UserID) && x.TaskID == edit.ID).ToListAsync();
                        if (listDelete.Count > 0)
                        {
                            _context.Deputies.RemoveRange(listDelete);
                        }

                        foreach (var deputy in task.Deputies)
                        {
                            deputies.Add(new Deputy
                            {
                                UserID = deputy,
                                TaskID = edit.ID
                            });
                            await _context.Deputies.AddRangeAsync(deputies);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Data.Models.Task CheckDuedate(Data.Models.Task task, CreateTaskViewModel createTaskView)
        {

            switch (createTaskView.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    task.DueDateDaily = createTaskView.DueDate;
                    break;
                case Data.Enum.PeriodType.Weekly:
                    task.DueDateWeekly = createTaskView.DueDate;
                    break;
                case Data.Enum.PeriodType.Monthly:
                    task.DueDateMonthly = createTaskView.DueDate;
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    task.SpecificDate = createTaskView.DueDate;
                    break;
                default:
                    break;
            }
            return task;
        }
        private Data.Enum.JobType checkJobType(CreateTaskViewModel task)
        {
            switch (task.JobTypeID)
            {
                case Data.Enum.JobType.Project:
                    return Data.Enum.JobType.Project;
                case Data.Enum.JobType.Routine:
                    return Data.Enum.JobType.Routine;
                case Data.Enum.JobType.Abnormal:
                    return Data.Enum.JobType.Abnormal;
                default:
                    return Data.Enum.JobType.Unknown;
            }
        }

        private async Task<List<int>> AddDeputy(CreateTaskViewModel task, Data.Models.Task item)
        {
            var listUsers = new List<int>();
            var deputies = new List<Deputy>();
            foreach (var deputy in task.Deputies)
            {
                deputies.Add(new Deputy
                {
                    UserID = deputy,
                    TaskID = item.ID
                });
            }
            await _context.Deputies.AddRangeAsync(deputies);
            await _context.SaveChangesAsync();
            var projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _context.Projects.FindAsync(item.ProjectID);
                projectName = project.Name;
            }
            var user = await _context.Users.FindAsync(task.UserID);
            string urlResult = $"/todolist/{item.JobName.ToUrlEncode()}";
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = Data.Enum.AlertType.ChangeDeputy,
                Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.ChangeDeputy),
                Users = task.Deputies.ToList(),
                TaskID = item.ID,
                URL = urlResult,
                UserID = task.UserID
            });
            listUsers.AddRange(task.Deputies);
            return listUsers;
        }
        private async Task<List<int>> AddPIC(CreateTaskViewModel task, Data.Models.Task item)
        {
            var listUsers = new List<int>();
            var tags = new List<Tag>();
            foreach (var pic in task.PIC)
            {
                tags.Add(new Tag
                {
                    UserID = pic,
                    TaskID = item.ID
                });
            }
            await _context.Tags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
            var user = await _context.Users.FindAsync(task.UserID);
            var projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _context.Projects.FindAsync(item.ProjectID);
                projectName = project.Name;
            }
            string urlResult = $"/todolist/{item.JobName.ToUrlEncode()}";
            string message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.Assigned);
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = Data.Enum.AlertType.Assigned,
                Message = message,
                Users = task.PIC.ToList(),
                TaskID = item.ID,
                URL = urlResult,
                UserID = task.UserID
            });
            listUsers.AddRange(task.PIC);
            return listUsers;
        }
        private async Task<List<int>> EditPIC(CreateTaskViewModel task, Data.Models.Task edit)
        {
            var listUsers = new List<int>();
            //Lay la danh sach assigned
            var oldPIC = await _context.Tags.Where(x => x.TaskID == edit.ID).Select(x => x.UserID).ToArrayAsync();
            var oldPICTemp = oldPIC;
            var newPIC = task.PIC;
            //loc ra danh sach cac ID co trong newPIC ma khong co trong oldPIC
            var withOutInOldPIC = newPIC.Except(oldPIC).ToArray();
            // var withOutInNewPIC = oldPIC.Except(newPIC).ToArray();
            if (newPIC.Count() == 0 && oldPIC.Count() > 0)
            {

                var listDeletePIC = await _context.Tags.Where(x => oldPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Tags.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
            }
            if (oldPIC.Count() == 1 && newPIC.Count() == 1 && !oldPIC.SequenceEqual(newPIC))
            {
                var listDeletePIC = await _context.Tags.Where(x => oldPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Tags.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
            }
            //xoa het thang cu them lai tu dau
            if (withOutInOldPIC.Length > 0)
            {
                var tags = new List<Tag>();
                foreach (var pic in withOutInOldPIC)
                {
                    tags.Add(new Tag
                    {
                        UserID = pic,
                        TaskID = edit.ID
                    });
                }
                if (tags.Count > 0)
                {
                    await _context.Tags.AddRangeAsync(tags);
                }
                var projectName = string.Empty;
                if (edit.ProjectID > 0)
                {
                    var project = await _context.Projects.FindAsync(edit.ProjectID);
                    projectName = project.Name;
                }
                var user = await _context.Users.FindAsync(task.UserID);
                string urlResult = $"/todolist/{edit.JobName.ToUrlEncode()}";
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = Data.Enum.AlertType.Assigned,
                    Message = CheckMessage(edit.JobTypeID, projectName, user.Username, edit.JobName, Data.Enum.AlertType.Assigned),
                    Users = withOutInOldPIC.ToList(),
                    TaskID = edit.ID,
                    URL = urlResult,
                    UserID = task.UserID
                });
                listUsers.AddRange(withOutInOldPIC);
                //Day la userID se bi xoa
                var withOutInNewPIC = oldPIC.Where(x => !newPIC.Contains(x)).ToArray();
                var listDeletePIC = await _context.Tags.Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Tags.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Day la userID se bi xoa
                var withOutInNewPIC = oldPIC.Where(x => !newPIC.Contains(x)).ToArray();
                var listDeletePIC = await _context.Tags.Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Tags.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
            }
            return listUsers;
        }
        private async Task<List<int>> EditDeputy(CreateTaskViewModel task, Data.Models.Task edit)
        {
            var listUsers = new List<int>();
            //Lay la danh sach assigned
            var oldDeputies = await _context.Deputies.Where(x => x.TaskID == edit.ID).Select(x => x.UserID).ToArrayAsync();
            var newDeputies = task.Deputies;
            //loc ra danh sach cac ID co trong newPIC ma khong co trong oldPIC
            var withOutInOldDeputy = newDeputies.Except(oldDeputies).ToArray();
            if (newDeputies.Count() == 0 && oldDeputies.Count() > 0)
            {
                var listDeleteDeputy = await _context.Deputies.Where(x => oldDeputies.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Deputies.RemoveRange(listDeleteDeputy);
                await _context.SaveChangesAsync();
            }
            if (oldDeputies.Count() == 1 && newDeputies.Count() == 1 && !oldDeputies.SequenceEqual(newDeputies))
            {
                var listDeleteDeputy = await _context.Deputies.Where(x => oldDeputies.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Deputies.RemoveRange(listDeleteDeputy);
                await _context.SaveChangesAsync();
            }
            if (withOutInOldDeputy.Length > 0)
            {
                var deputies = new List<Deputy>();
                foreach (var deputy in withOutInOldDeputy)
                {
                    deputies.Add(new Deputy
                    {
                        UserID = deputy,
                        TaskID = edit.ID
                    });
                }
                if (deputies.Count > 0)
                {
                    await _context.Deputies.AddRangeAsync(deputies);
                }
                var projectName = string.Empty;
                if (edit.ProjectID > 0)
                {
                    var project = await _context.Projects.FindAsync(edit.ProjectID);
                    projectName = project.Name;
                }
                var user = await _context.Users.FindAsync(task.UserID);
                string urlResult = $"/todolist/{edit.JobName.ToUrlEncode()}";
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = Data.Enum.AlertType.ChangeDeputy,
                    Message = CheckMessage(edit.JobTypeID, projectName, user.Username, edit.JobName, Data.Enum.AlertType.ChangeDeputy),
                    Users = withOutInOldDeputy.ToList(),
                    TaskID = edit.ID,
                    URL = urlResult,
                    UserID = task.UserID
                });
                //Day la userID se bi xoa
                var withOutInNewPIC = oldDeputies.Where(x => !newDeputies.Contains(x)).ToArray();
                var listDeletePIC = await _context.Deputies.Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Deputies.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
                listUsers.AddRange(withOutInOldDeputy);
            }
            else
            {
                //Day la userID se bi xoa
                var withOutInNewPIC = oldDeputies.Where(x => !newDeputies.Contains(x)).ToArray();
                var listDeletePIC = await _context.Deputies.Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _context.Deputies.RemoveRange(listDeletePIC);
                await _context.SaveChangesAsync();
            }
            return listUsers;
        }
        private async System.Threading.Tasks.Task CloneCode(Data.Models.Task task)
        {
            var createCode = await _context.Tasks.FindAsync(task.ID);
            createCode.Code = $"{task.ID}-{task.periodType}-{task.JobTypeID}";
            await _context.SaveChangesAsync();
        }
        public async Task<Tuple<bool, string, object>> CreateTask(CreateTaskViewModel task)
        {
            try
            {
                task.DueDate = task.DueDate.ToStringFormatDateTime();
                var listUsers = new List<int>();
                var jobTypeID = checkJobType(task);
                if (!await _context.Tasks.AnyAsync(x => x.ID == task.ID))
                {
                    if (jobTypeID == Data.Enum.JobType.Unknown)
                        return Tuple.Create(false, "", new object());
                    task.Priority = task.Priority.ToUpper();

                    var item = _mapper.Map<Data.Models.Task>(task);
                    item = CheckDuedate(item, task);
                    item.Level = 1;
                    item.JobTypeID = jobTypeID;
                    await _context.Tasks.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await CloneCode(item);
                    if (task.PIC.Count() > 0)
                    {
                        listUsers.AddRange(await AddPIC(task, item));
                    }
                    if (task.Deputies.Count() > 0)
                    {
                        listUsers.AddRange(await AddDeputy(task, item));

                    }
                    return Tuple.Create(true, string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                }
                else
                {
                    var edit = _context.Tasks.Find(task.ID);
                    edit.Priority = task.Priority.ToUpper();
                    edit.JobName = task.JobName;
                    edit.Priority = task.Priority;
                    edit.DepartmentID = task.DepartmentID;
                    edit.FromWhoID = task.FromWhoID;

                    if (task.PIC.Count() >= 0)
                    {
                        listUsers.AddRange(await EditPIC(task, edit));
                    }
                    if (task.Deputies.Count() >= 0)
                    {
                        listUsers.AddRange(await EditDeputy(task, edit));
                    }
                    var pics = await _context.Tags.Where(x => x.TaskID.Equals(edit.ID)).Select(x => x.UserID).ToListAsync();
                    switch (task.periodType)
                    {
                        case Data.Enum.PeriodType.Daily:
                            var daily = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Daily, edit, task.UserID, pics);
                            edit.DueDateDaily = task.DueDate;
                            listUsers.AddRange(daily.Item1);
                            break;
                        case Data.Enum.PeriodType.Weekly:
                            var weekly = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Weekly, edit, task.UserID, pics);
                            edit.DueDateWeekly = task.DueDate;
                            listUsers.AddRange(weekly.Item1);
                            break;
                        case Data.Enum.PeriodType.Monthly:
                            var mon = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Monthly, edit, task.UserID, pics);
                            edit.DueDateMonthly = task.DueDate;
                            listUsers.AddRange(mon.Item1);
                            break;
                        case Data.Enum.PeriodType.SpecificDate:
                            var due = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Deadline, edit, task.UserID, pics);
                            listUsers.AddRange(due.Item1);
                            edit.SpecificDate = task.DueDate;
                            break;
                        default:
                            break;
                    }
                }
                await _context.SaveChangesAsync();

                return Tuple.Create(true, string.Join(",", listUsers.Distinct()), GetAlertDueDate());
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, "", new object());
            }
        }
        public async Task<object> Delete(int id, int userid)
        {
            try
            {

                var item = await _context.Tasks.FindAsync(id);
                if (!item.CreatedBy.Equals(userid))
                    return false;
                var tasks = await GetListTree(item.ParentID, item.ID);
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();

                _context.Tags.RemoveRange(await _context.Tags.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Tasks.RemoveRange(await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToListAsync());
                _context.Follows.RemoveRange(await _context.Follows.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Refactor Done Method
        private async Task<bool> PushTaskToHistory(History history)
        {
            try
            {
                await _context.AddAsync(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;

                throw;
            }
        }
        private string PeriodDaily(Data.Models.Task task)
        {
            return task.DueDateDaily.ToParseStringDateTime().AddDays(1).ToString("dd MMM, yyyy hh:mm:ss tt");
        }
        private string PeriodWeekly(Data.Models.Task task)
        {
            //Hoan thanh task thi tang len 1 ngay
            return task.DueDateWeekly.ToParseStringDateTime().AddDays(7).ToString("dd MMM, yyyy hh:mm:ss tt");
        }
        #endregion


        #region Helpers Done()
        private string PeriodMonthly(Data.Models.Task task)
        {
            //var monthsHas31Days = new List<int> { 1, 3, 5, 7, 8, 10, 12 };
            //var monthsHas30Days = new List<int> { 4, 6, 9, 11 };
            //var monthsHas28or29Days = new List<int> { 2 };
            var lastdayofFeb = new List<int> { 28, 29 };
            var date = task.DueDateMonthly.ToParseStringDateTime();
            if (date.Month == 1 && date.Day == 30 || date.Month == 1 && date.Day == 29)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, 2);
                return new DateTime(date.Year, 2, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else if (date.Month == 3 && date.Day == 31)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, 4);
                return new DateTime(date.Year, 4, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else if (date.Month == 5 && date.Day == 31)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, 6);
                return new DateTime(date.Year, 6, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else if (date.Month == 8 && date.Day == 31)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, 9);
                return new DateTime(date.Year, 9, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else if (date.Month == 10 && date.Day == 31)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, 10);
                return new DateTime(date.Year, 10, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else if (date.Month == 12 && date.Day == 30)
            {
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year + 1, 1);
                return new DateTime(date.Year + 1, 1, lastDayOfMonth, date.Hour, date.Minute, date.Second).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
            else
            {
                return task.DueDateMonthly.ToParseStringDateTime().AddMonths(1).ToString("dd MMM, yyyy hh:mm:ss tt");
            }
        }

        private bool CheckDailyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateDaily.ToParseStringDateTime().Date) >= 0 ? true : false;
        }
        private bool CheckWeeklyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateWeekly.ToParseStringDateTime().Date) <= 0 ? true : false;
        }
        private bool CheckMonthlyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateMonthly.ToParseStringDateTime().Date) <= 0 ? true : false;
        }

        // CheckSpecificDateOntime
        private bool CheckSpecificDateOntime(Data.Models.Task update)
        {
            return TimeComparator(update.SpecificDate.ToParseStringDateTime());
        }
        private async Task<Data.Models.Task> CloneEachPeriod(Data.Models.Task task, int parentID)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            var history = new History
            {
                TaskID = update.ID,
            };

            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    history.Deadline = update.DueDateDaily.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckDailyOntime(update);
                    update.DueDateDaily = PeriodDaily(task);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    history.Deadline = update.DueDateWeekly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckWeeklyOntime(update);
                    update.DueDateWeekly = PeriodWeekly(task).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy");
                    break;
                case Data.Enum.PeriodType.Monthly:
                    history.Deadline = update.DueDateMonthly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckMonthlyOntime(update);
                    update.DueDateMonthly = PeriodMonthly(task);
                    break;
                default:
                    break;
            }
            if (update.periodType == Data.Enum.PeriodType.SpecificDate)
            {
                history.Deadline = update.SpecificDate.ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                history.Status = CheckSpecificDateOntime(task);
            }
            await PushTaskToHistory(history);
            await _context.SaveChangesAsync();
            return update;
        }
        private async Task<List<int>> CloneTask(List<Data.Models.Task> tasks)
        {
            int parentID = 0;
            var listTaskUpdateStatus = new List<int>();
            foreach (var item in tasks)
            {
                if (item.Level == 1)
                {
                    parentID = 0;
                }
                var itemAdd = await CloneEachPeriod(item, parentID);
                listTaskUpdateStatus.Add(itemAdd.ID);
                parentID = itemAdd.ID;
            }
            return listTaskUpdateStatus;
        }
        private async System.Threading.Tasks.Task CloneItemTask(Data.Models.Task tasks)
        {
            var history = new History
            {
                TaskID = tasks.ID,
            };

            switch (tasks.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    history.Deadline = tasks.DueDateDaily.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckDailyOntime(tasks);
                    tasks.DueDateDaily = PeriodDaily(tasks);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    history.Deadline = tasks.DueDateWeekly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckWeeklyOntime(tasks);
                    tasks.DueDateWeekly = PeriodWeekly(tasks).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy");
                    break;
                case Data.Enum.PeriodType.Monthly:
                    history.Deadline = tasks.DueDateMonthly.ToParseStringDateTime().ToString("d MMM, yyyy");
                    history.Status = CheckMonthlyOntime(tasks);
                    tasks.DueDateMonthly = PeriodMonthly(tasks);
                    break;
                default:
                    break;
            }
            if (tasks.periodType == Data.Enum.PeriodType.SpecificDate)
            {
                history.Deadline = tasks.SpecificDate.ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                history.Status = CheckSpecificDateOntime(tasks);
            }
            await PushTaskToHistory(history);
            await _context.SaveChangesAsync();
        }
        #endregion
        public async Task<Tuple<bool, bool, string>> Done2(int id, int userid)
        {
            try
            {
                var flag = true;
                var item = await _context.Tasks.FindAsync(id);

                if (item.Status)
                {
                    return Tuple.Create(false, false, "This task was completed!");
                }
                var listUpdateStatus = new List<int>();
                var projectName = string.Empty;
                string pathName = "todolist";
                if (item.ProjectID > 0)
                {
                    var project = await _context.Projects.FindAsync(item.ProjectID);
                    projectName = project.Name;
                    if (item.Level == 1 && item.periodType == Data.Enum.PeriodType.SpecificDate)
                        item.FinishedMainTask = true;
                }

                var user = await _context.Users.FindAsync(userid);
                var rootTask = ToFindParentByChild(_context.Tasks, item.ID);
                var tasks = await GetListTree(rootTask.ParentID, rootTask.ID);

                //Tim tat ca con chau

                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
                var seftAndDescendants = _context.Tasks.Where(x => taskDescendants.Contains(x.ID)).ToList();

                //Lay danh sach cac user theo jobtype
                var listUsers = await GetListUsersAlert(item.JobTypeID, userid, item.ProjectID, item.ID);
                var mainTaskID = 0;
                // Neu task level 1 va khong co con chau thi chuyen qua history va up date lai deadline
                if (seftAndDescendants.Count == 1 && seftAndDescendants.Any(x => x.ID == item.ID) && item.Level == 1)
                {
                    pathName = "history";
                    flag = true;
                } //Neu level > 1 va co 2 con tro len thi chua chuyen qua history, doi trang thai thanh hoan thanh
                else if (item.Level == 1 && seftAndDescendants.Where(x => x.Level > 1).ToList().Count > 1)
                {
                    seftAndDescendants.Where(x => x.ID != id).ToList().ForEach(x =>
                    {
                        if (x.Status == false && x.Level > 1)
                        {
                            flag = false;
                            return;
                        }
                        if (x.Status && x.Level == 1)
                        {
                            mainTaskID = seftAndDescendants.FirstOrDefault(x => x.Level == 1).ID;
                        }
                    });
                    //item.Status = true;
                }
                else if (item.Level > 1 && seftAndDescendants.Where(x => x.Level > 1).ToList().Count >= 2)
                {
                    seftAndDescendants.Where(x => x.ID != id).ToList().ForEach(x =>
                    {
                        if (x.Status == false && x.Level > 1)
                        {
                            flag = false;
                            return;
                        }
                        if (x.Status && x.Level == 1)
                        {
                            mainTaskID = seftAndDescendants.FirstOrDefault(x => x.Level == 1).ID;
                        }
                    });
                    item.Status = true;
                    //await CloneItemTask(item);
                }
                //Neu hoan thanh cac task con thi chuyen qua history
                if (flag || mainTaskID > 0)
                {
                    seftAndDescendants.ForEach(task =>
                    {
                        if (task.ProjectID > 0 || task.periodType == Data.Enum.PeriodType.SpecificDate)
                        {
                            task.Status = true;
                            //task.FinishedMainTask = true;
                        }
                    });
                    pathName = "history";
                    listUpdateStatus.AddRange(await CloneTask(seftAndDescendants));
                }

                item.ModifyDateTime = DateTime.Now.ToString("dd MMM, yyyy hh:mm:ss tt");
                await _context.SaveChangesAsync();
                if (!flag && item.Level == 1)
                {
                    return Tuple.Create(false, false, "Please finish all sub-tasks!");
                }
                if (listUpdateStatus.Count() > 0)
                {
                    var reupdateStatus = await _context.Tasks.Where(x => listUpdateStatus.Contains(x.ID)).ToListAsync();
                    reupdateStatus.ForEach(item =>
                    {
                        if (item.ProjectID == 0)
                        {
                            item.Status = false;
                            item.FinishedMainTask = false;
                        }
                    });
                    await _context.SaveChangesAsync();

                }
                //Task nao theo doi thi moi thong bao
                var listUserfollowed = new List<int>();
                var listUserAlertHub = new List<int>();
                if (item.Level.Equals(1)) //Level = 1 thi chi thong bao khi task do hoan thanh
                    listUserfollowed = (await _context.Follows.Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToListAsync()).ToList();
                else//level > 1 tuc la co con thi thong bao khi con no va no hoan thanh
                    listUserfollowed = (await _context.Follows.Where(x => taskDescendants.Contains(x.TaskID)).Select(x => x.UserID).ToListAsync()).ToList();

                var followed = listUserfollowed.Count > 0 ? true : false;

                //Neu la project thi thong bao den managers, teamMembers,
                //Neu la routine, abnormal thi thong bao den PICs, deputies
                string urlTodolist = $"/{pathName}/{item.JobName.ToUrlEncode()}";
                listUsers.Add(item.CreatedBy);
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = Data.Enum.AlertType.Done,
                    Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.Done),
                    Users = listUsers.Distinct().ToList(),
                    TaskID = item.ID,
                    URL = urlTodolist,
                    UserID = userid
                });

                //Neu ai theo doi thi gui thong bao
                if (followed)
                {
                    string urlResult = $"/follow/{item.JobName.ToUrlEncode()}";
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.Done,
                        Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.Done),
                        Users = listUserfollowed.Distinct().ToList(),
                        TaskID = item.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUserAlertHub.AddRange(listUserfollowed.Where(x => x != userid));

                }
                listUserAlertHub.AddRange(listUsers);

                return Tuple.Create(true, followed, string.Join(",", listUserAlertHub.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Tuple.Create(false, false, "");
            }
        }
        private async Task<Data.Models.Task> UpdatePeriod(Data.Models.Task task)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    update.DueDateDaily = PeriodDaily(task).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Weekly:
                    update.DueDateWeekly = PeriodWeekly(task).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Monthly:
                    update.DueDateMonthly = PeriodMonthly(task).ToStringFormatDateTime();
                    break;
                default:
                    break;
            }
            await _context.SaveChangesAsync();
            return update;
        }
        private async System.Threading.Tasks.Task ClonePIC(int taskidOld, int taskIDNew)
        {
            var pic = _context.Tags.Where(x => x.TaskID == taskidOld).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = taskIDNew, UserID = item.UserID });
            }
            await _context.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }
        private async Task<Data.Models.Task> UpdatePeriodForDone(CloneTaskViewModel task)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    update.DueDateDaily = PeriodDaily(update).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Weekly:
                    update.DueDateWeekly = PeriodWeekly(update).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Monthly:
                    update.DueDateMonthly = PeriodMonthly(update).ToStringFormatDateTime();
                    break;
                default:
                    break;
            }
            await _context.SaveChangesAsync();
            return update;
        }
        private async System.Threading.Tasks.Task ClonePICForDone(CloneTaskViewModel task)
        {
            var pic = _context.Tags.Where(x => x.TaskID == task.IDTemp).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = task.ID, UserID = item.UserID });
            }
            await _context.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }
        private async System.Threading.Tasks.Task CloneSingleTask(Data.Models.Task task)
        {
            int old = task.ID;
            var clone = task;
            clone.ID = 0;
            clone.Status = false;
            clone.FinishedMainTask = false;
            clone.ModifyDateTime = string.Empty;
            await _context.AddAsync(clone);
            await _context.SaveChangesAsync();
            await UpdatePeriod(clone);
            await ClonePIC(old, clone.ID);
        }


        private async System.Threading.Tasks.Task CloneMultiTask(List<Data.Models.Task> tasks)
        {
            var listTemp = new List<CloneTaskViewModel>();
            foreach (var item in tasks)
            {
                var temp = _mapper.Map<CloneTaskViewModel>(item);
                temp.IDTemp = item.ID;
                temp.ParentTemp = item.ParentID;
                item.ID = 0;
                item.Status = false;
                item.FinishedMainTask = false;
                item.ModifyDateTime = "";
                await _context.AddAsync(item);
                await _context.SaveChangesAsync();
                temp.ID = item.ID;
                listTemp.Add(temp);
            }
            var update = _context.Tasks.Where(x => listTemp.Select(a => a.ID).Contains(x.ID)).ToList();
            //listTemp
            /// ID = 3347, ParentID = 0, ParentTemp = 0, IDTemp = 3342
            /// ID = 3348, ParentID = 3342, ParentTemp = 3342, IDTemp = 3344
            /// ID = 3349, ParentID = 3342, ParentTemp = 3342, IDTemp = 3345
            /// List new
            /// ID = 3347, ParentID = 0
            /// ID = 3348, ParentID = 3342
            /// ID = 3348, ParentID = 3342
            foreach (var item in listTemp)
            {
                await UpdatePeriodForDone(item);
                await ClonePICForDone(item);
            }

            update.ForEach(item =>
            {
                if (item.Level > 1)
                {
                    item.ParentID = listTemp.FirstOrDefault(x => x.IDTemp == item.ParentID).ID;
                }
            });
            await _context.SaveChangesAsync();
        }
        private async Task<List<int>> CloneTaskWhenDone(Data.Models.Task task)
        {
            var userLists = new List<int>();
            userLists.Add(task.FromWhoID);
            userLists.AddRange(_context.Tags.Where(x => x.TaskID == task.ID).Select(x => x.UserID).ToList());
            var clone = task;
            clone.ID = 0;
            if (task.Level == 1)
            {
                await CloneSingleTask(clone);
            }
            if (task.Level > 1)
            {
                var rootTask = ToFindParentByChild(_context.Tasks, clone.ID);
                var tasks = await GetListTree(rootTask.ParentID, rootTask.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
                var seftAndDescendants = _context.Tasks.Where(x => taskDescendants.Contains(x.ID)).ToList();
                foreach (var item in seftAndDescendants)
                {
                    await CloneSingleTask(item);
                }
            }
            return userLists;
        }
        private async Task<List<int>> AlertFollowTask(Data.Models.Task item, int userid)
        {
            string projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _context.Projects.FindAsync(item.ProjectID);
                projectName = project.Name;
                if (item.Level == 1 && item.periodType == Data.Enum.PeriodType.SpecificDate)
                    item.FinishedMainTask = true;
            }
            var user = await _context.Users.FindAsync(userid);
            var listUserfollowed = await _context.Follows.Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToListAsync();
            string urlResult = $"/follow/{item.JobName.ToUrlEncode()}";
            if (listUserfollowed.Count() > 0)
            {
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = Data.Enum.AlertType.Done,
                    Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.Done),
                    Users = listUserfollowed.Distinct().Where(x => x!= userid).ToList(),
                    TaskID = item.ID,
                    URL = urlResult,
                    UserID = userid
                });
            }
            return listUserfollowed;
        }
        private async Task<List<int>> AlertTask(Data.Models.Task item, int userid)
        {
            var pathName = "history";
            var projectName = string.Empty;
            var userList = new List<int>();
            var user = await _context.Users.FindAsync(userid);
            if (item.ProjectID > 0)
            {
                var project = await _context.Projects.FindAsync(item.ProjectID);
                projectName = project.Name;
                if (item.Level == 1 && item.periodType == Data.Enum.PeriodType.SpecificDate)
                    item.FinishedMainTask = true;
            }
            string urlTodolist = $"/{pathName}/{item.JobName.ToUrlEncode()}";
            userList.Add(item.FromWhoID);
            userList.AddRange(_context.Tags.Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToList());
            userList.AddRange(_context.Deputies.Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToList());
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = Data.Enum.AlertType.Done,
                Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, Data.Enum.AlertType.Done),
                Users = userList.Distinct().Where(x => x != userid).ToList(),
                TaskID = item.ID,
                URL = urlTodolist,
                UserID = userid
            });
            return userList;
        }
        private async System.Threading.Tasks.Task UpdateDueDateViaPeriod(Data.Models.Task task)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            var check = await CheckUpdateDueDateTodolist(update);
            if (check)
            {
                switch (task.periodType)
                {
                    case Data.Enum.PeriodType.Daily:
                        update.DueDateDaily = PeriodDaily(task).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        break;
                    case Data.Enum.PeriodType.Weekly:
                        update.DueDateWeekly = PeriodWeekly(task).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        break;
                    case Data.Enum.PeriodType.Monthly:
                        update.DueDateMonthly = PeriodMonthly(task).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        break;
                    case Data.Enum.PeriodType.SpecificDate:
                        update.DueDateMonthly = PeriodMonthly(task).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        break;
                    default:
                        break;
                }
                await _context.SaveChangesAsync();
            }
        }
        private async Task<bool> CheckUpdateDueDateTodolist(Data.Models.Task update)
        {
            var flag = false;
            var dueDate = string.Empty;
            var check = await _context.Tasks.Where(x => x.Code.Equals(update.Code)).ToListAsync();
            foreach (var item in check)
            {
                switch (update.periodType)
                {
                    case Data.Enum.PeriodType.Daily:
                        dueDate = PeriodDaily(update).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        if (item.DueDateDaily.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case Data.Enum.PeriodType.Weekly:
                        dueDate = PeriodWeekly(update).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        if (item.DueDateDaily.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case Data.Enum.PeriodType.Monthly:
                        dueDate = PeriodMonthly(update).ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                        if (item.DueDateDaily.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            return flag;
        }
        private bool CheckPeriodOnTime(Data.Models.Task task)
        {
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    return CheckDailyOntime(task);
                case Data.Enum.PeriodType.Weekly:
                    return CheckWeeklyOntime(task);
                case Data.Enum.PeriodType.Monthly:
                    return CheckMonthlyOntime(task);
                case Data.Enum.PeriodType.SpecificDate:
                    return CheckSpecificDateOntime(task);
                default:
                    return false;
            }
        }
        private string UpdateDueDateViaPeriodHisoty(Data.Models.Task task)
        {
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    return task.DueDateDaily.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.Weekly:
                    return task.DueDateWeekly.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.Monthly:
                    return task.DueDateMonthly.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.SpecificDate:
                    return task.SpecificDate.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                default:
                    return "";
            }
        }
        private async Task<Data.Models.Task> CheckPeriodToPushTaskToHistory(Data.Models.Task task)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            var history = new History
            {
                TaskID = update.ID,
                TaskCode = update.Code,
                Status = CheckPeriodOnTime(update),
                Deadline = UpdateDueDateViaPeriodHisoty(update)
            };
            await UpdateDueDateViaPeriod(update);
            await PushTaskToHistory(history);
            await _context.SaveChangesAsync();
            return update;
        }
        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns></returns>
        private int PeriodComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate.Date, comparedate.Date);
            return res;
        }
        // Check Period
        private bool ValidPeriod(Data.Models.Task task)
        {
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var date = task.DueDateDaily.ToParseStringDateTime().Date;
                    var result = PeriodComparator(date);
                    return result < 0 ? true : false;
                case Data.Enum.PeriodType.Weekly:
                    var weekly = task.DueDateWeekly.ToParseStringDateTime().Date.Subtract(TimeSpan.FromDays(3));
                    var resultW = PeriodComparator(weekly);
                    return resultW < 0 ? true : false;
                case Data.Enum.PeriodType.Monthly:
                    var monthly = task.DueDateMonthly.ToParseStringDateTime().Date.Subtract(TimeSpan.FromDays(10));
                    var resultM = PeriodComparator(monthly);
                    return resultM < 0 ? true : false;
                //case Data.Enum.PeriodType.SpecificDate:
                //    return CheckSpecificDateOntime(task);
                default:
                    return false;
            }
        }
        public async Task<Tuple<bool, bool, string>> Done(int id, int userid)
        {
            try
            {
                var listUserAlertHub = new List<int>();

                var item = await _context.Tasks.FindAsync(id);
                var check = ValidPeriod(item);
                if (ValidPeriod(item))
                    return Tuple.Create(false, false, "This task not available!");
                if (item.Status)
                {
                    return Tuple.Create(false, false, "This task was completed!");
                }
                var rootTask = ToFindParentByChild(_context.Tasks, item.ID);
                var tasks = await GetListTree(rootTask.ParentID, rootTask.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
                var seftAndDescendants = _context.Tasks.Where(x => taskDescendants.Contains(x.ID)).ToList();
                // Kiem tra neu task level = 1 va khong co con chau nao thi chuyen qua history sau do cap nhap lai due date
                //var decendants = seftAndDescendants.Where(x => !seftAndDescendants.Select(x => x.ID).Contains(item.ID));
                // Neu task hien tai la main task thi kiem tra xem tat ca con chau da hoan thanh chua neu chua thi return
                if (seftAndDescendants.Where(x => x.Level > 1).Count() > 0 && item.Level == 1)
                {
                    return Tuple.Create(false, false, "Please finish all sub-tasks!");
                }
                item.Status = true;
                item.ModifyDateTime = DateTime.Now.ToString("dd MMM, yyyy hh:mm:ss tt");
                await _context.SaveChangesAsync();

                listUserAlertHub.AddRange(await AlertTask(item, userid));
                listUserAlertHub.AddRange(await AlertFollowTask(item, userid));
                await CheckPeriodToPushTaskToHistory(item);
                if (seftAndDescendants.Count() == 1 && item.Level == 1)
                {
                    //Clone them cai moi voi period moi
                    await CloneSingleTask(item);
                }
                // Neu task hien tai level 1 va co con chau thi kiem tra neu con chua done thi return
                if (seftAndDescendants.Where(x => x.Level > 1).Count() >= 2 && item.Level == 1)
                {
                    int count = 0;
                    var temp = true;

                    //Kiem tra list con chau neu count > 1 tuc la co 2 con chua hoan thanh => return
                    seftAndDescendants.Where(x => x.Level > 1).ToList().ForEach(x =>
                    {
                        if (x.Status == false)
                        {
                            count++;
                            temp = false;
                        }
                    });
                    if (!temp && count > 1)
                        return Tuple.Create(false, false, "Please finish all sub-tasks!");
                }
                // Neu day la main task  va task nay khong co con thi thong bao cho nhung user lien quan va chuyen no qua history
                //if (decendants.Count() == 0 && item.Level == 1)
                //{
                //    await AlertTask(item, userid);
                //    await AlertFollowTask(item, userid);
                //    await CheckPeriodToPushTaskToHistory(item);
                //}

                // Neu khong fai la main thi kiem tra xem co bao nhieu task hoan thanh roi.
                // Neu chi con task minh chua hoan thanh thi chuyen cha qua history
                if (item.Level > 1 && seftAndDescendants.Where(x => x.Level > 1).Count() >= 2)
                {

                    var temp = true;
                    int count = 0;
                    // trong list nay khong co task hien tai neu count = 0 tuc la con moi task hien tai chua hoan thanh
                    // Add task cha cua task hien tai vao history
                    var taskTemp = seftAndDescendants.Where(x => x.Level > 1 && x.ID != id).ToList();
                    taskTemp.ForEach(x =>
                    {
                        if (x.Status == false)
                        {
                            temp = false;
                            count++;
                        }
                    });
                    // dieu kien nay de push task cha va task hien tai vao db
                    if (temp && count == 0)
                    {
                        var parent = await _context.Tasks.FindAsync(item.ParentID);
                        parent.ModifyDateTime = DateTime.Now.ToString("dd MMM, yyyy hh:mm:ss tt");
                        parent.Status = true;
                        parent.FinishedMainTask = true;
                        item.Status = true;
                        item.FinishedMainTask = true;
                        await _context.SaveChangesAsync();
                        await CheckPeriodToPushTaskToHistory(parent);
                    }
                    // Tao them 1 bo moi trong todolist
                    if (!temp && count >= 1)
                    {
                        //Update Status task con hien tai
                        await CloneMultiTask(seftAndDescendants);
                    }
                }
                return Tuple.Create(true, true, string.Join(",", listUserAlertHub.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Tuple.Create(false, false, "");
            }
        }
        public async Task<object> UpdateTask(UpdateTaskViewModel task)
        {
            if (!await _context.Tasks.AnyAsync(x => x.ID == task.ID))
                return false;

            var update = await _context.Tasks.FindAsync(task.ID);
            update.JobName = task.JobName;
            update.FromWhoID = task.FromWhoID;
            update.CreatedBy = task.CreatedBy;
            update.Status = task.Status;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<object> Follow(int userid, int taskid)
        {
            try
            {
                var taskModel = await _context.Tasks.FindAsync(taskid);
                var tasks = await GetListTree(taskModel.ParentID, taskModel.ID);
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();

                var listTasks = await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToListAsync();
                if (_context.Follows.Any(x => x.TaskID == taskid && x.UserID == userid))
                {
                    _context.Remove(_context.Follows.FirstOrDefault(x => x.TaskID == taskid && x.UserID == userid));
                    await _context.SaveChangesAsync();

                    return true;

                }
                var listSubcribes = new List<Follow>();
                listTasks.ForEach(task =>
                {
                    listSubcribes.Add(new Follow { TaskID = task.ID, UserID = userid });
                });
                await _context.AddRangeAsync(listSubcribes);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<object> Undo(int id)
        {
            if (!await _context.Tasks.AnyAsync(x => x.ID == id))
                return false;
            var item = await _context.Tasks.FindAsync(id);
            var his = _context.Histories.ToList();
            var tasks = await GetListTreeForUndo(item.ParentID, item.ID);
            var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
            var arrs = _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToList();
            arrs.ForEach(task =>
            {
                task.Status = false;
                task.FinishedMainTask = false;
            });
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    his = his.Where(x => x.TaskID == id && x.Deadline.Equals(item.DueDateDaily)).ToList();
                    item.DueDateDaily = "";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    his = his.Where(x => x.TaskID == id && x.Deadline.Equals(item.DueDateWeekly)).ToList();
                    item.DueDateWeekly = "";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    his = his.Where(x => x.TaskID == id && x.Deadline.Equals(item.DueDateMonthly)).ToList();
                    item.DueDateMonthly = "";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    his = his.Where(x => x.TaskID == id && x.Deadline.Equals(item.SpecificDate)).ToList();
                    item.SpecificDate = "";
                    break;
            }
            try
            {
                _context.RemoveRange(his);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public bool IsDaily(Data.Models.Task task)
        {
            if (task.periodType.Equals(Data.Enum.PeriodType.Daily))
            {
                return PeriodComparator(task.DueDateDaily.ToParseStringDateTime()) < 0 ? true : false;
            }
            return false;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "")
        {

            var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
            var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

            var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
            var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

            var UserProjectList = managers.Union(members).ToList();
            var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

            var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();
            var listTasks = await _context.Tasks.Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid)
            //.Where(x => ((x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false)) && x.JobTypeID != Data.Enum.JobType.Project || (x.JobTypeID == Data.Enum.JobType.Project && x.Status == false && x.FinishedMainTask == false || x.JobTypeID == Data.Enum.JobType.Project && x.Status == true && x.FinishedMainTask == false))
            .Include(x => x.User).ToListAsync();

            var daily = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0).ToList();
            var weekly = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Weekly)).ToList();
            var mon = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Monthly)).ToList();
            var specificDate = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.SpecificDate)).ToList();
            var allList = daily.Union(weekly).Union(mon).Union(specificDate).ToList();
            var listTasksFillter = Fillter(allList, sort, priority, userid, startDate, endDate, weekdays, monthly, quarterly);
            //Flatten task
            var tasks = GetListTreeViewTask(listTasksFillter, userid);
            var tree = tasks.Where(x => x.PICs.Count > 0)
             .AsHierarchy(x => x.ID, x => x.ParentID)
             .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
             .ToList();

            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = tasks.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent).ToList();
            tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
            return tree;
            throw new NotImplementedException("Server error!!!");
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid)
        {

            try
            {
                var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
                var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

                var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
                var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

                var UserProjectList = managers.Union(members).ToList();
                var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

                var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();
                var listTasks = await _context.Tasks.Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid)
                //.Where(x => ((x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false)) && x.JobTypeID != Data.Enum.JobType.Project || (x.JobTypeID == Data.Enum.JobType.Project && x.Status == false && x.FinishedMainTask == false || x.JobTypeID == Data.Enum.JobType.Project && x.Status == true && x.FinishedMainTask == false))
                .Include(x => x.User).ToListAsync();

                var daily = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0).ToList();
                var weekly = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Weekly)).ToList();
                var mon = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Monthly)).ToList();
                var specificDate = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.SpecificDate)).ToList();
                var allList = daily.Union(weekly).Union(mon).Union(specificDate).ToList();
                if (!beAssigned.IsNullOrEmpty() && beAssigned == "BeAssigned")
                {
                    allList = listTasks.Where(x => pics.Contains(x.ID)).ToList();
                }
                if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                {
                    allList = listTasks.Where(x => x.FromWhoID.Equals(userid)).ToList();
                }
                //Flatten task
                var tasks = GetListTreeViewTask(allList, userid);
                var tree = tasks.Where(x => x.PICs.Count > 0)
                 .AsHierarchy(x => x.ID, x => x.ParentID)
                 .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
                 .ToList();

                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = tasks.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent).ToList();
                tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
                return tree;

            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }

        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid)
        {

            var jobtype = Data.Enum.JobType.Routine;
            var user = await _context.Users.FindAsync(userid);
            if (ocid == 0)
                return new List<HierarchyNode<TreeViewTask>>();
            var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
            var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

            var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
            var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

            var UserProjectList = managers.Union(members).ToList();
            var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

            var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();

            var listTasks = await _context.Tasks
                .Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid || x.CreatedBy == userid)
                .Where(x => x.OCID.Equals(ocid) && x.JobTypeID.Equals(jobtype))
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();

            //sort
            listTasks = SortRoutine(listTasks, sort, priority);
            var daily = listTasks.Where(x => x.Status == false && x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0).ToList();
            var weekly = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Weekly)).ToList();
            var mon = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Monthly)).ToList();
            var specificDate = listTasks.Where(x => x.periodType.Equals(Data.Enum.PeriodType.SpecificDate)).ToList();
            var allList = daily.Union(weekly).Union(mon).Union(specificDate).ToList();
            //Duoi nay la load theo tree view con nao thi cha do
            var tasks = GetListTreeViewTask(allList, userid);
            var tree = tasks
                .AsEnumerable()
                .AsHierarchy(x => x.ID, x => x.ParentID)
                .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = tasks.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
            throw new NotImplementedException();
        }

        public async Task<List<HierarchyNode<TreeViewTask>>> Abnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            //Khai bao enum Jobtype la Abnormal
            var jobtype = Data.Enum.JobType.Abnormal;
            if (ocid == 0)
                return new List<HierarchyNode<TreeViewTask>>();
            var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
            var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

            var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
            var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

            var UserProjectList = managers.Union(members).ToList();
            var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

            var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();
            //Lay tat ca task theo jobtype la abnormal va oc
            var listTasks = await _context.Tasks
                .Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid || x.CreatedBy == userid)
                .Where(x => x.OCID.Equals(ocid) && x.JobTypeID.Equals(jobtype))
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();
            listTasks = FilterAbnormal(listTasks, ocid, priority, userid, startDate, endDate, weekdays);

            var tasks = GetListTreeViewTask(listTasks, userid).ToList();
            var tree = tasks
               .AsEnumerable()
               .AsHierarchy(x => x.ID, x => x.ParentID)
               .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = tasks.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
            throw new NotImplementedException();
        }

        public async Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int projectid = 0)
        {
            var jobtype = Data.Enum.JobType.Project;

            var listTasks = await _context.Tasks
            .Where(x => x.JobTypeID.Equals(jobtype) && x.ProjectID.Equals(projectid))
            .Include(x => x.User)
            .ToListAsync();
            listTasks = FilterTaskDetail(listTasks, priority);
            var tasks = GetListTreeViewTask(listTasks, userid)
                .AsHierarchy(x => x.ID, x => x.ParentID)
                .ToList();

            return tasks;
            throw new NotImplementedException("Server error!!");

        }

        public async Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0)
        {
            var listTasks = await _context.Follows
               .Join(_context.Tasks,
               follow => follow.TaskID,
               task => task.ID,
               (follow, task) => new
               {
                   task,
                   follow
               }).Select(x => new Data.Models.Task
               {
                   ID = x.follow.TaskID,
                   CreatedBy = x.task.CreatedBy,
                   Status = x.task.Status,
                   CreatedDate = x.task.CreatedDate,
                   ParentID = x.task.ParentID,
                   Level = x.task.Level,
                   ProjectID = x.task.ProjectID,
                   JobName = x.task.JobName,
                   OCID = x.task.OCID,
                   FromWhoID = x.task.FromWhoID,
                   Priority = x.task.Priority,
                   FinishedMainTask = x.task.FinishedMainTask,
                   JobTypeID = x.task.JobTypeID,
                   periodType = x.task.periodType,
                   User = x.task.User,
                   DepartmentID = x.task.DepartmentID,
                   DueDateDaily = x.task.DueDateDaily,
                   DueDateMonthly = x.task.DueDateMonthly,
                   DueDateWeekly = x.task.DueDateWeekly,
                   SpecificDate = x.task.SpecificDate,
                   ModifyDateTime = x.task.ModifyDateTime
               }).ToListAsync();

            listTasks = SortFollow(listTasks, sort, priority);
            var tasks = GetListTreeViewTask(listTasks, userid).ToList();
            var tree = tasks
               .AsEnumerable()
               .AsHierarchy(x => x.ID, x => x.ParentID)
               .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = tasks.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
            throw new NotImplementedException();
        }

        public async Task<List<HierarchyNode<TreeViewTask>>> History(int userid, string start, string end)
        {
            var pics = _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();
            var deputies = _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToList();

            var managers = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();
            var members = _context.Managers.Where(x => x.UserID == userid).Select(x => x.ProjectID).ToList();

            var UserProjectList = managers.Union(members).ToList();
            var projectTasksList = await _context.Tasks.Where(x => UserProjectList.Contains(x.ProjectID)).Select(x => x.ID).ToListAsync();

            var allTasksList = pics.Union(deputies).Union(projectTasksList).Distinct().ToList();
            var listTasks = await _context.Histories
                .Join(_context.Tasks,
                his => his.TaskID,
                task => task.ID,
                (his, task) => new
                {
                    task,
                    his
                }).Select(x => new Data.Models.Task
                {
                    ID = x.his.TaskID,
                    CreatedBy = x.task.CreatedBy,
                    Status = x.his.Status,
                    CreatedDate = x.task.CreatedDate,
                    ParentID = x.task.ParentID,
                    Level = x.task.Level,
                    ProjectID = x.task.ProjectID,
                    JobName = x.task.JobName,
                    OCID = x.task.OCID,
                    FromWhoID = x.task.FromWhoID,
                    Priority = x.task.Priority,
                    FinishedMainTask = x.task.FinishedMainTask,
                    JobTypeID = x.task.JobTypeID,
                    periodType = x.task.periodType,
                    User = x.task.User,
                    DepartmentID = x.task.DepartmentID,
                    DueDateDaily = x.task.DueDateDaily,
                    DueDateMonthly = x.task.DueDateMonthly,
                    DueDateWeekly = x.task.DueDateWeekly,
                    SpecificDate = x.task.SpecificDate,
                    ModifyDateTime = x.his.ModifyDateTime
                }).Where(x => allTasksList.Contains(x.ID) || x.FromWhoID == userid).ToListAsync();
            if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var startDate = start.ToParseStringDateTime().Date;
                var endDate = end.ToParseStringDateTime().Date;
                listTasks = listTasks.Where(x => x.CreatedDate.Date >= startDate.Date || x.CreatedDate.Date <= endDate.Date).ToList();
            }
            var tasks = GetListTreeViewTask(listTasks, userid).ToList();
            var tree = tasks
               .AsEnumerable()
               .AsHierarchy(x => x.ID, x => x.ParentID)
               .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = tasks.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
            throw new NotImplementedException();
        }
    }
}
