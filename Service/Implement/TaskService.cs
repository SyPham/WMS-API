using AutoMapper;
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
using System.Diagnostics;
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
        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns></returns>
        //private bool DateTimeComparator(DateTime comparedate)
        //{
        //    DateTime systemDate = DateTime.Now;
        //    int res = DateTime.Compare(systemDate, comparedate);
        //    return res <= 0 ? true : false;
        //}
        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns></returns>
        //private bool DateComparator(DateTime comparedate)
        //{
        //    DateTime systemDate = DateTime.Now;
        //    int res = DateTime.Compare(systemDate.Date, comparedate.Date);
        //    return res <= 0 ? true : false;
        //}
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
        public void HieararchyWalk(HashSet<TreeViewTask> hierarchy)
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
        public HashSet<TreeViewTask> GetChildren(HashSet<TreeViewTask> tasks, int parentid)
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
                    .ToHashSet();
        }
        public HashSet<TreeViewTask> GetChildrenForMap(HashSet<TreeViewTask> tasks, int parentid)
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
                    .ToHashSet();
        }
        public IEnumerable<TreeViewOC> GetAllDescendants(IEnumerable<TreeViewOC> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
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
        private int FindParentByChild(IEnumerable<Data.ViewModel.Task.Task> rootNodes, int taskID)
        {
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ID;
            else
                return FindParentByChild(rootNodes, parent);
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
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
                    break;
                case Data.Enum.AlertType.ChangeMonthly:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
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
        private string MapSpecificDueDateWithPeriod(Data.Models.Task task)
        {
            string result = string.Empty;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    result = task.DueDateDaily.ToParseStringDateTime().ToString("dd MMM, yyyy hh:tt:ss tt");
                    break;
                case Data.Enum.PeriodType.Weekly:
                    result = task.DueDateWeekly.ToParseStringDateTime().ToString("dd MMM, yyyy hh:tt:ss tt");
                    break;
                case Data.Enum.PeriodType.Monthly:
                    result = task.DueDateMonthly.ToParseStringDateTime().ToString("dd MMM, yyyy hh:tt:ss tt");
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    result = task.SpecificDate.ToParseStringDateTime().ToString("dd MMM, yyyy hh:tt:ss tt");
                    break;
                default:
                    break;
            }
            return result != string.Empty ? result.ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt") : string.Empty;
        }
        private DateTime MapDueDatTimeeWithPeriod(Data.Models.Task task)
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
            return result.ToParseStringDateTime();
        }
        private HashSet<TreeViewTask> MapperTreeViewTask(HashSet<TreeViewTask> tasks)
        {
            var hierarchyTemp = new HashSet<TreeViewTask>();
            var roots = tasks.Where(x => x.ParentID == 0).ToHashSet();
            var childs = tasks.Where(x => x.ParentID != 0).ToHashSet();

            var BeAdded = new HashSet<TreeViewTask>();
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

            var flatBedAdded = BeAdded.Flatten().ToHashSet();
            var except = childs.Where(x => !flatBedAdded.Select(c => c.ID).Contains(x.ID)).Select(x => x.ID).ToHashSet();
            var lastChilds = childs.Where(x => except.Contains(x.ID)).ToHashSet();
            var result = hierarchyTemp.Union(lastChilds).ToHashSet();
            return result;
        }
        private async Task<HashSet<TreeViewTask>> GetListTreeViewTaskAsync(HashSet<Data.Models.Task> listTasks, int userid)
        {
            //khúc này xử lý hả dung r cho nay ma map ra viewmodel
            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Include(x => x.User).Where(x => x.TaskID == item.ID)
                                   .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).AsQueryable();
                var deputiesList = _context.Deputies.Where(x => x.TaskID == item.ID)
                   .Join(_context.Users,
                   de => de.UserID,
                   user => user.ID,
                   (de, user) => new
                   {
                       user.ID,
                       user.Username
                   })
                   .Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).AsQueryable();

                var statusTutorial = await _context.Tutorials.AnyAsync(x => x.TaskID == item.ID);
                var tutorialModel = await _context.Tutorials.FirstOrDefaultAsync(x => x.TaskID == item.ID);
                //var tasksTree = await GetListTree(item.ParentID, item.ID);
                var arrTasks = FindParentByChild(listTasks, item.ID);

                TreeViewTask levelItem = new TreeViewTask
                {
                    Follow = await _context.Follows.AnyAsync(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No",
                    ID = item.ID,
                    PriorityID = item.Priority,
                    ProjectName = item.ProjectID == 0 ? "" : (await _context.Projects.FindAsync(item.ProjectID)).Name,
                    JobName = item.JobName.IsNotAvailable()
                };
                levelItem.From = item.DepartmentID > 0 ? levelItem.From = _context.OCs.FirstOrDefault(u => u.ID == item.DepartmentID).Name : levelItem.From = _context.Users.FirstOrDefault(u => u.ID == item.FromWhoID).Username;
                levelItem.PIC = _context.Tags.Where(x => x.TaskID == item.ID).Select(x => x.User.Username).ToArray().ToJoin().IsNotAvailable();
                levelItem.DeputiesList = deputiesList.ToList();
                levelItem.DeputyName = deputiesList.Select(x => x.Username).ToArray().ToJoin(" , ").IsNotAvailable();
                levelItem.CreatedDate = item.CreatedDate.ToStringFormat(formatCreatedDate).IsNotAvailable();
                levelItem.DueDateDaily = item.DueDateDaily.ToStringFormatISO(formatDaily).IsNotAvailable();
                levelItem.DueDateWeekly = item.DueDateWeekly.IsNotAvailable();
                levelItem.DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth().IsNotAvailable();
                levelItem.SpecificDate = item.SpecificDate.ToStringFormatISO(formatSpecificDate).IsNotAvailable();
                levelItem.periodType = item.periodType;
                levelItem.DueDateTime = MapDueDatTimeeWithPeriod(item);
                levelItem.SpecificDueDate = MapSpecificDueDateWithPeriod(item);
                levelItem.ModifyDateTime = item.ModifyDateTime;
                // levelItem.User = item.User;
                levelItem.TaskCode = item.Code;
                levelItem.BeAssigned = beAssigneds.Select(x => x.ID).Contains(userid);
                levelItem.Level = item.Level;
                levelItem.ProjectID = item.ProjectID ?? 0;
                levelItem.ParentID = item.ParentID;
                levelItem.state = item.Status == false ? "Undone" : "Done";
                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PICs = beAssigneds.Select(x => x.ID).ToList();
                levelItem.BeAssigneds = beAssigneds.ToList();
                levelItem.Deputies = deputiesList.Select(_ => _.ID).ToList();
                levelItem.FromWhere = await _context.OCs.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefaultAsync() ?? new FromWhere();
                levelItem.FromWho = await _context.Users.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefaultAsync() ?? new BeAssigned();
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
            return tasks.OrderByDescending(x => x.ID).ToHashSet();
        }
        public async Task<List<ProjectViewModel>> GetListProject()
        {
            return await _projectService.GetListProject();
        }

        public IEnumerable<TreeViewTask> GetListTreeByParent(HashSet<TreeViewTask> tasks, int parentID, int id)
        {
            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

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
                            .ToHashSet();

            return hierarchy;
        }
        public async Task<IEnumerable<TreeViewTask>> GetListTreeByParent(int parentID, int id, int userid)
        {
            var listTasks = await _context.Tasks
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();
            var tasks = await GetListTreeViewTaskAsync(listTasks.ToHashSet(), userid);

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

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
                            .ToHashSet();
            return hierarchy;
        }

        #endregion

        #region LoadData helper
        private List<int> GetListUserRelateToTask(int taskId, bool isProject)
        {
            var task = _context.Tasks.Find(taskId);
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
                URL = $"/todolist/{item.JobName.ToUrlEncode()}",
                AlertType = Data.Enum.AlertType.BeLate
            };
            if (notifyParams.Users.Count > 0)
            {
                await _notificationService.Create(notifyParams);
            }
        }
        private string Message(Data.Enum.PeriodType periodType, TreeViewTask item)
        {
            var mes = string.Empty;
            switch (periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateDaily}";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateWeekly}";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateMonthly}";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.SpecificDate}";
                    break;
                default:
                    break;
            }
            return mes;
        }
        private async System.Threading.Tasks.Task PeriodType(TreeViewTask item, bool isProject)
        {
            string mes = Message(item.periodType, item);
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var checkDaily = CheckNotification(item.ID);
                    if (!checkDaily)
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var checkWeekly = CheckNotification(item.ID);
                    if (!checkWeekly)
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var checkMonthly = CheckNotification(item.ID);
                    if (!checkMonthly)
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var checkSpecificDate = CheckNotification(item.ID);
                    if (!checkSpecificDate)
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                default:
                    break;
            }
        }
        private async System.Threading.Tasks.Task ProjectTaskIsLate(HashSet<TreeViewTask> tasks)
        {

            foreach (var item in tasks)
            {
                await PeriodType(item, true);
            }
        }
        private async System.Threading.Tasks.Task RoutineTaskIsLate(HashSet<TreeViewTask> tasks)
        {
            foreach (var item in tasks)
            {
                await PeriodType(item, false);
            }
        }
        private async System.Threading.Tasks.Task AbnormalTaskIsLate(HashSet<TreeViewTask> tasks)
        {

            foreach (var item in tasks)
            {
                await PeriodType(item, false);
            }
        }

        private bool CheckCompletedTask(Data.Models.Task task)
        {
            bool result = false;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var res1 = PeriodComparator(task.DueDateDaily.ToParseStringDateTime());
                    result = res1 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var res2 = PeriodComparator(task.DueDateWeekly.ToParseStringDateTime());
                    result = res2 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var res3 = PeriodComparator(task.DueDateMonthly.ToParseStringDateTime());
                    result = res3 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var res4 = PeriodComparator(task.SpecificDate.ToParseStringDateTime());
                    result = res4 <= 0 ? true : false;
                    break;
                default:
                    break;
            }
            return result;
        }

        private bool CheckNotification(int id)
        {
            return _context.Notifications.Any(x => x.TaskID == id && x.Function.Equals(Data.Enum.AlertType.BeLate.ToSafetyString()));
        }

        public async System.Threading.Tasks.Task TaskListIsLate(int userid)
        {
            var pics = await _context.Tags.Where(x => x.UserID == userid).Select(x => x.TaskID).ToListAsync();
            var deputies = await _context.Deputies.Where(x => x.UserID == userid).Select(x => x.TaskID).ToListAsync();
            //Lay tat ca list task chua hoan thanh va task co con chua hoan thnah

            // Tim tat ca task dc giao ma chua hoan thanh trong ngay hien tai
            var listTasks = _context.Tasks
                .Include(x=>x.Tags).ThenInclude(x=>x.User)
                .Include(x => x.Deputies).ThenInclude(x => x.User)
                .Where(x => x.Tags.Select(a => a.User.ID).Contains(x.ID) || x.Deputies.Select(a=>a.User.ID).Contains(x.ID) && x.Status == false)
                            .OrderBy(x => x.Level).AsQueryable();
            var unCompletedTaskList = new HashSet<Data.Models.Task>();
            var currentDate = DateTime.Now.Date;
            //Kiem tra late task
            foreach (var item in listTasks)
            {
                if (item.periodType.Equals(Data.Enum.PeriodType.Daily))
                {
                    var daily = item.DueDateDaily.ToParseStringDateTime().Date;
                    if (currentDate != daily && !CheckCompletedTask(item))
                    {
                        unCompletedTaskList.Add(item);
                    }
                    if (currentDate == daily && !CheckCompletedTask(item))
                    {
                        unCompletedTaskList.Add(item);
                    }
                }
                else
                {
                    if (!CheckCompletedTask(item))
                    {
                        unCompletedTaskList.Add(item);
                    }
                }
            }

            var allTasks = _context.Tasks.AsQueryable();
            // Clone
            foreach (var item in unCompletedTaskList)
            {
                // Tim root cua task hien tai
                var root = ToFindParentByChild(allTasks, item.ID);
                // Tim tat ca cac con cua task root vua tim dc
                var tasksList = AsTreeView(root.ParentID, root.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasksList).Select(x => x.ID).ToArray();
                var seftAndDescendants = await _context.Tasks.Where(x => taskDescendants.Contains(x.ID)).ToListAsync();
                if (seftAndDescendants.Count == 1)
                {
                    // Clone task nay luon
                    await CloneSingleTask(item);
                }
                if (seftAndDescendants.Count > 1)
                {
                    // clone theo cha con luon
                    await CloneMultiTask(seftAndDescendants);
                }
            }
            //Alert
           // var tasks = await GetListTreeViewTaskAsync(unCompletedTaskList, userid);
            var tasks = _mapper.Map<List<TreeViewTask>>(unCompletedTaskList)
                .Where(x => x.PICs.Count > 0).ToHashSet();
            try
            {
                var projects = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Project).ToHashSet();
                var routine = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Routine).ToHashSet();
                var abnormal = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Abnormal).ToHashSet();
                await ProjectTaskIsLate(projects);
                await RoutineTaskIsLate(routine);
                await AbnormalTaskIsLate(abnormal);
            }
            catch (Exception ex)
            {
                throw;
            }

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
        public async Task<HashSet<TreeViewTask>> GetListTree()
        {
            var listTasks = await _context.Tasks
                .Where(x => x.Status == false)
                .Include(x => x.User)
                .OrderBy(x => x.Level).ToListAsync();

            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();

                var levelItem = new TreeViewTask
                {
                    ID = item.ID,
                    PIC = string.Join(" , ", _context.Tags.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray()),
                    ProjectName = item.ProjectID == null ? "" : _context.Projects.Find(item.ProjectID).Name,
                    ProjectID = item.ProjectID ?? 0,
                    BeAssigneds = beAssigneds,
                    Level = item.Level,

                    ParentID = item.ParentID,
                    Priority = CastPriority(item.Priority),
                    PriorityID = item.Priority,
                    CreatedDate = String.Format("{0:d MMM, yyyy}", item.CreatedDate),
                    User = new BeAssigned { ID = item.User.ID, Username = item.User.Username } ,
                    FromWhere = _context.OCs.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault(),

                    FromWho = _context.Users.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault(),
                    JobName = item.JobName.IsNotAvailable(),
                    state = item.Status == false ? "Undone" : "Done",
                    From = item.OCID > 0 ? _context.OCs.Find(item.OCID).Name : _context.Users.Find(item.FromWhoID).Username.IsNotAvailable(),
                    DueDateDaily = item.DueDateDaily.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:D}", item.DueDateDaily.ToParseIso8601()),
                    DueDateWeekly = item.DueDateWeekly,
                    DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth(),
                    SpecificDate = item.SpecificDate.ToParseIso8601().IsNewDateTime() ? "" : String.Format("{0:s}", item.SpecificDate.ToParseIso8601())
                };

                tasks.Add(levelItem);
            }

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

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
                            .ToHashSet();
            return hierarchy;
        }
        public async Task<object> GetDeputies()
        {
            return await _context.Users.Where(x => x.Username != "admin").Select(x => new { x.Username, x.ID }).ToListAsync();
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
        private Data.Enum.JobType CheckJobType(CreateTaskViewModel task)
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
                if (!await _context.Tasks.AnyAsync(x => x.ID == task.ID))
                {
                    var item = _mapper.Map<Data.Models.Task>(task);
                    item.Level = 1;
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
                _context.Deputies.RemoveRange(await _context.Deputies.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Follows.RemoveRange(await _context.Follows.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Tasks.RemoveRange(await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToListAsync());

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
            catch
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

        private async System.Threading.Tasks.Task ClonePIC(int oldTaskid, int newTaskID)
        {
            var pic = _context.Tags.Where(x => x.TaskID == oldTaskid).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = newTaskID, UserID = item.UserID });
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
        private async Task<bool> CheckExistTask(Data.Models.Task task)
        {
            var currentDate = DateTime.Now;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateDaily.Equals(task.DueDateDaily));
                case Data.Enum.PeriodType.Weekly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateWeekly.Equals(task.DueDateWeekly));
                case Data.Enum.PeriodType.Monthly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateMonthly.Equals(task.DueDateMonthly));
                case Data.Enum.PeriodType.SpecificDate:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.SpecificDate.Equals(task.SpecificDate));
                default:
                    return false;
            }
        }
        private async System.Threading.Tasks.Task CloneSingleTask(Data.Models.Task task)
        {
            int old = task.ID;
            var newTask = new Data.Models.Task
            {
                JobName = task.JobName,
                OCID = task.OCID,
                FromWhoID = task.FromWhoID,
                Priority = task.Priority,
                ProjectID = task.ProjectID,
                JobTypeID = task.JobTypeID,
                DueDateDaily = task.DueDateDaily,
                DueDateMonthly = task.DueDateMonthly,
                DueDateWeekly = task.DueDateWeekly,
                SpecificDate = task.SpecificDate,
                Code = task.Code,
                periodType = task.periodType,
                CreatedBy = task.CreatedBy,
                Level = task.Level
            };
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    newTask.DueDateDaily = PeriodDaily(task).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Weekly:
                    newTask.DueDateWeekly = PeriodWeekly(task).ToStringFormatDateTime();
                    break;
                case Data.Enum.PeriodType.Monthly:
                    newTask.DueDateMonthly = PeriodMonthly(task).ToStringFormatDateTime();
                    break;
                default:
                    break;
            }
            //Kiem tra cai task chuan bi clone nay da ton tai chua
            var check = await CheckExistTask(newTask);
            if (!check)
            {
                await _context.AddAsync(newTask);
                await _context.SaveChangesAsync();
                await ClonePIC(old, newTask.ID);
            }

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

        /// <summary>
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        /// </summary>
        /// <param name="comparedate"></param>
        /// <returns>Result</returns>
        private int PeriodComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate, comparedate);
            return res;
        }
        // Check Period
        private bool ValidPeriod(Data.Models.Task task, out string message)
        {
            var currenDate = DateTime.Now.ToString("dd MMM, yyyy");
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    // var date = task.DueDateDaily.ToParseStringDateTime().Date;
                    // var result = PeriodComparator(date);
                    message = "";
                    return true;
                case Data.Enum.PeriodType.Weekly:
                    var weekly = task.DueDateWeekly.ToParseStringDateTime().Date.Subtract(TimeSpan.FromDays(3));
                    var resultW = PeriodComparator(weekly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateWeekly.ToParseStringDateTime().Subtract(TimeSpan.FromDays(3)):dd MMMM, yyyy} to {task.DueDateWeekly.ToParseStringDateTime():dd MMMM, yyyy}";
                    return resultW > 0 ? true : false;
                case Data.Enum.PeriodType.Monthly:
                    var monthly = task.DueDateMonthly.ToParseStringDateTime().Date.Subtract(TimeSpan.FromDays(10));
                    var resultM = PeriodComparator(monthly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateMonthly.ToParseStringDateTime().Subtract(TimeSpan.FromDays(10)):dd MMMM, yyyy} to {task.DueDateMonthly.ToParseStringDateTime():dd MMMM, yyyy}";
                    return resultM > 0 ? true : false;
                case Data.Enum.PeriodType.SpecificDate:
                    message = "";
                    return true;
                default:
                    message = "";
                    return false;
            }
        }
        #region Helper For Done
        private Data.Models.Task ToFindParentByChild(IQueryable<Data.Models.Task> rootNodes, int taskID)
        {
            var parentItem = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID));
            if (parentItem == null)
                return null;
            var parent = parentItem.ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID));
            else
                return ToFindParentByChild(rootNodes, parent);
        }
        public async Task<IEnumerable<TreeViewTask>> GetListTree(int parentID, int id)
        {
            var listTasks = await _context.Tasks
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();
            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                TreeViewTask levelItem = new TreeViewTask
                {
                    ID = item.ID,
                    Level = item.Level,
                    ParentID = item.ParentID
                };
                tasks.Add(levelItem);
            }

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

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
                            .ToHashSet();
            return hierarchy;
        }
        public IEnumerable<TreeViewTask> GetAllTaskDescendants(IEnumerable<TreeViewTask> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllTaskDescendants(x.children));
            return rootNodes.Concat(descendants);
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
                    Users = listUserfollowed.Distinct().Where(x => x != userid).ToList(),
                    TaskID = item.ID,
                    URL = urlResult,
                    UserID = userid
                });
            }
            return listUserfollowed;
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
        private async System.Threading.Tasks.Task CloneMultiTask(List<Data.Models.Task> tasks)
        {
            var listTemp = new List<CloneTaskViewModel>();
            foreach (var item in tasks)
            {
                var check = await CheckExistTask(item);
                if (!check)
                {
                    var temp = _mapper.Map<CloneTaskViewModel>(item);
                    temp.IDTemp = item.ID;
                    temp.ParentTemp = item.ParentID;
                    item.ID = 0;
                    item.Status = false;
                    item.FinishedMainTask = false;
                    item.ModifyDateTime = "";
                    item.CreatedDate = DateTime.Now;
                    await _context.AddAsync(item);
                    await _context.SaveChangesAsync();
                    temp.ID = item.ID;
                    listTemp.Add(temp);
                }
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
        private string PeriodMonthly(Data.Models.Task task)
        {
            return task.DueDateMonthly.ToParseStringDateTime().AddMonths(1).ToString("dd MMM, yyyy hh:mm:ss tt");
        }
        private bool CheckDailyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateDaily.ToParseStringDateTime()) <= 0 ? true : false;
        }
        private bool CheckWeeklyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateWeekly.ToParseStringDateTime()) <= 0 ? true : false;
        }
        private bool CheckMonthlyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateMonthly.ToParseStringDateTime()) <= 0 ? true : false;
        }
        private bool CheckSpecificDateOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.SpecificDate.ToParseStringDateTime()) <= 0 ? true : false;
        }
        #endregion
        public async Task<Tuple<bool, bool, string>> Done(int id, int userid)
        {
            try
            {
                var listUserAlertHub = new List<int>();

                var item = await _context.Tasks.FindAsync(id);
                string mes = string.Empty;
                var check = ValidPeriod(item, out mes);
                if (!check)
                    return Tuple.Create(false, false, mes);
                if (item.Status)
                {
                    return Tuple.Create(false, false, "This task was completed!");
                }
                var listTasks = _context.Tasks.AsQueryable();
                var rootTask = ToFindParentByChild(listTasks, item.ID);
                var tasks = AsTreeView(rootTask.ParentID, rootTask.ID);
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
        public IEnumerable<TreeViewTask> AsTreeView(int parentID, int id)
        {
            var listTasks = _context.Tasks
               .Include(x => x.User)
               .OrderBy(x => x.Level).AsQueryable();
            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var levelItem = new TreeViewTask
                {
                    ID = item.ID,
                    Level = item.Level,
                    ParentID = item.ParentID
                };
                tasks.Add(levelItem);
            }

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                state = c.state,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToHashSet();
            return hierarchy;
        }
        public async Task<object> Undo(int id)
        {
            if (!await _context.Tasks.AnyAsync(x => x.ID == id))
                return false;
            try
            {
                var listTasks =  _context.Tasks.AsQueryable();
                var item = await _context.Tasks.FindAsync(id);
                var rootTask = ToFindParentByChild(listTasks, item.ID);
                var tasks = AsTreeView(rootTask.ParentID, rootTask.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToList();
                var seftAndDescendants = await _context.Tasks.Where(x => taskDescendants.Contains(x.ID)).ToListAsync();
                if (seftAndDescendants.Count == 1)
                {
                    var his = await _context.Histories.FirstOrDefaultAsync(x => x.TaskID == id);
                    _context.Remove(his);
                    item.Status = false;
                    await _context.SaveChangesAsync();
                }
                if (seftAndDescendants.Count > 1)
                {
                    var his = await _context.Histories.Where(x => seftAndDescendants.Select(x => x.ID).Contains(x.TaskID)).ToListAsync();
                    var arrs = await _context.Tasks.Where(x => seftAndDescendants.Select(a => a.ID).Contains(x.ID)).ToListAsync();
                    arrs.ForEach(task =>
                    {
                        task.Status = false;
                        task.FinishedMainTask = false;
                    });
                    _context.RemoveRange(his);
                    await _context.SaveChangesAsync();
                }


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Main LoadData
        public bool IsDaily(Data.Models.Task task)
        {
            if (task.periodType.Equals(Data.Enum.PeriodType.Daily))
            {
                return PeriodComparator(task.DueDateDaily.ToParseStringDateTime()) < 0 ? true : false;
            }
            return false;
        }
        private IQueryable<Data.Models.Task> Fillter(IQueryable<Data.Models.Task> listTasks, string sort, string priority, int userid, string startDate, string endDate, string weekdays, string monthly, string quarterly)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).AsQueryable();
            }

            //Loc theo weekdays
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateWeekly.ToSafetyString().ToLower().Equals(weekdays.ToLower())).AsQueryable();
            }
            //loc theo thang
            if (!monthly.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateMonthly.ToSafetyString().ToLower().Equals(monthly.ToLower())).AsQueryable();
            }

            if (!sort.IsNullOrEmpty())
            {
                sort = sort.ToLower();
                if (sort == Data.Enum.JobType.Project.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Project)).OrderByDescending(x => x.ProjectID).AsQueryable();
                if (sort == Data.Enum.JobType.Routine.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Routine)).OrderByDescending(x => x.CreatedDate).AsQueryable();
                if (sort == Data.Enum.JobType.Abnormal.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(Data.Enum.JobType.Abnormal)).OrderByDescending(x => x.CreatedDate).AsQueryable();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        private IQueryable<Data.Models.Task> FilterAbnormal(IQueryable<Data.Models.Task> listTasks, int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;

                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).AsQueryable();
            }
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateWeekly.Equals(weekdays)).AsQueryable();
            }

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        private IQueryable<Data.Models.Task> FilterTaskDetail(IQueryable<Data.Models.Task> listTasks, string priority)
        {
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        private IQueryable<Data.Models.Follow> SortFollow(IQueryable<Data.Models.Follow> listTasks, string sort, string priority)
        {

            if (!sort.IsNullOrEmpty())
            {
                if (sort == "project")
                    listTasks = listTasks.Where(x => x.Task.ProjectID > 0).AsQueryable();
                if (sort == "routine")
                    listTasks = listTasks.Where(x => x.Task.ProjectID == 0).AsQueryable();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Task.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        private IQueryable<Data.Models.Task> SortRoutine(IQueryable<Data.Models.Task> listTasks, string sort, string priority)
        {

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        private async Task<List<int>> GetProjectTasksByUserID(int userid)
        {
            var managers = await _context.Managers.Where(x => x.UserID == userid)
                    .Join(
                    _context.Tasks,
                    m => m.ProjectID,
                    t => t.ProjectID,
                    (m, t) => new
                    {
                        m,
                        t
                    }
                    ).Select(x => x.t.ID).ToListAsync();
            var members = await _context.TeamMembers.Where(x => x.UserID == userid)
                .Join(
                _context.Tasks,
                m => m.ProjectID,
                t => t.ProjectID,
                (m, t) => new
                {
                    m,
                    t
                }
                ).Select(x => x.t.ID).ToListAsync();
            var projectTasksList = managers.Union(members).ToList();
            return projectTasksList;
        }
        private IQueryable<Data.Models.Task> GetAllTasks()
        {
            var listTasks = _context.Tasks
                                .Include(x => x.User)
                                .Include(x => x.Tags).ThenInclude(x => x.User)
                                .Include(x => x.Follows).ThenInclude(x => x.User)
                                .Include(x => x.Deputies).ThenInclude(x => x.User)
                                .Include(x => x.Project).ThenInclude(x => x.Managers)
                                .Include(x => x.Project).ThenInclude(x => x.TeamMembers)
                                .Include(x => x.OC).Include(x => x.Tutorial)
                                .AsQueryable();
            return listTasks;
        }
        private List<TreeViewTask> GetListTreeViewTask(List<Data.Models.Task> listTasks, int userid)
        {
            //khúc này xử lý hả dung r cho nay ma map ra viewmodel
            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var arrTasks = FindParentByChild(listTasks, item.ID);
                TreeViewTask levelItem = new TreeViewTask
                {
                    Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No",
                    ID = item.ID,
                    PriorityID = item.Priority,
                    ProjectName = item.ProjectID == null ? "" : item.Project.Name,
                    JobName = item.JobName.IsNotAvailable()
                };
                levelItem.From = item.DepartmentID > 0 ? levelItem.From = _context.OCs.FirstOrDefault(u => u.ID == item.DepartmentID).Name : levelItem.From = _context.Users.FirstOrDefault(u => u.ID == item.FromWhoID).Username;
                levelItem.PIC = _context.Tags.Where(x => x.TaskID == item.ID).Select(x => x.User.Username).ToArray().ToJoin().IsNotAvailable();
                levelItem.DeputiesList = item.Deputies.Select(x => new BeAssigned {ID = x.ID ,Username = x.User.Username }).ToList();
                levelItem.DeputyName = item.Deputies.Select(x => x.User.Username).ToArray().ToJoin(" , ").IsNotAvailable();
                levelItem.CreatedDate = item.CreatedDate.ToStringFormat(formatCreatedDate).IsNotAvailable();
                levelItem.DueDateDaily = item.DueDateDaily.ToStringFormatISO(formatDaily).IsNotAvailable();
                levelItem.DueDateWeekly = item.DueDateWeekly.IsNotAvailable();
                levelItem.DueDateMonthly = item.DueDateMonthly.FindShortDatesOfMonth().IsNotAvailable();
                levelItem.SpecificDate = item.SpecificDate.ToStringFormatISO(formatSpecificDate).IsNotAvailable();
                levelItem.periodType = item.periodType;
                levelItem.DueDateTime = MapDueDatTimeeWithPeriod(item);
                levelItem.SpecificDueDate = MapSpecificDueDateWithPeriod(item);
                levelItem.ModifyDateTime = item.ModifyDateTime;
                // levelItem.User = item.User;
                levelItem.TaskCode = item.Code;
                levelItem.BeAssigned = item.Tags.Select(x => x.ID).Contains(userid);
                levelItem.Level = item.Level;
                levelItem.ProjectID = item.ProjectID ?? 0;
                levelItem.ParentID = item.ParentID;
                levelItem.state = item.Status == false ? "Undone" : "Done";
                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PICs = item.Tags.Select(x => x.ID).ToList();
                levelItem.BeAssigneds = item.Tags.Select(x=> new BeAssigned { ID = item.User.ID, Username = item.User.Username }).ToList();
                levelItem.Deputies = item.Deputies.Select(_ => _.ID).ToList();
                levelItem.FromWho = new BeAssigned { ID = item.User.ID, Username = item.User.Username } ?? new BeAssigned();
                levelItem.JobTypeID = item.JobTypeID;
                levelItem.periodType = item.periodType;
                levelItem.FromWhoID = item.FromWhoID;
                levelItem.CreatedBy = item.CreatedBy;
                levelItem.VideoStatus = item.Tutorial == null ? false : true;
                levelItem.VideoLink = item.Tutorial != null ? item.Tutorial.URL : "";
                levelItem.DueDate = MapDueDateWithPeriod(item);
                tasks.Add(levelItem);

            }
            return tasks.OrderByDescending(x => x.ID).ToList();
        }
        public async Task<HashSet<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "")
        {
            try
            {
             
                var listTasks = GetAllTasks().Where(x=>
                        x.Tags.Select(x=>x.UserID).Contains(userid)
                       || x.FromWhoID == userid
                       || x.Project.Managers.Select(x => x.UserID).Contains(userid)
                       || x.Project.TeamMembers.Select(x => x.UserID).Contains(userid)
                       || x.Deputies.Select(x => x.UserID).Contains(userid)
                    ).Distinct();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var listTasksFillter =await Fillter(listTasks, sort, priority, userid, startDate, endDate, weekdays, monthly, quarterly).ToListAsync();
                var all = GetListTreeViewTask(listTasksFillter, userid);
                all.ForEach(item =>
                   {
                       item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

                   });
              
                Console.WriteLine("Todolist-{0}", watch.Elapsed);
                await _context.CheckTasks.AddAsync(new CheckTask { CreatedDate = DateTime.Now, Function = watch.Elapsed.TotalMilliseconds.ToString() });
                await _context.SaveChangesAsync();
                var tree = all.Where(x =>
                 x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0
                || x.PICs.Count > 0)
                .AsHierarchy(x => x.ID, x => x.ParentID)
                .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
                .ToHashSet();

                var flatten = tree.Flatten(x => x.ChildNodes).ToHashSet();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<HashSet<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
                tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToHashSet();
               
                return tree;
            }
            catch (Exception ex)
            {
                return new HashSet<HierarchyNode<TreeViewTask>>();
                throw;
            }

        }
        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid)
        {

            try
            {
                var listTasks = GetAllTasks().Where(x =>
                       x.Tags.Select(x => x.UserID).Contains(userid)
                      || x.FromWhoID == userid
                      || x.Project.Managers.Select(x => x.UserID).Contains(userid)
                      || x.Project.TeamMembers.Select(x => x.UserID).Contains(userid)
                      || x.Deputies.Select(x => x.UserID).Contains(userid)
                   ).Distinct();
                if (!beAssigned.IsNullOrEmpty() && beAssigned == "BeAssigned")
                {
                    listTasks = listTasks.Where(x => x.Tags.Select(x => x.UserID).Contains(userid)).AsQueryable();
                }
                if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                {
                    listTasks = listTasks.Where(x => x.FromWhoID == userid).AsQueryable();
                }
                var sortTaskList = await listTasks.ToListAsync();
                //Flatten task
                var all = _mapper.Map<List<TreeViewTask>>(sortTaskList);

                var tree = all.Where(x =>
                     x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0
                    || x.PICs.Count > 0)
                 .AsHierarchy(x => x.ID, x => x.ParentID)
                 .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
                 .ToList();

                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent).ToList();
                tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
            }

        }

        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(Data.Enum.Status status, int userid)
        {
            try
            {
                var listTasks = GetAllTasks().Where(x =>
                      x.Tags.Select(x => x.UserID).Contains(userid)
                     || x.FromWhoID == userid
                     || x.Project.Managers.Select(x => x.UserID).Contains(userid)
                     || x.Project.TeamMembers.Select(x => x.UserID).Contains(userid)
                     || x.Deputies.Select(x => x.UserID).Contains(userid)
                  ).Distinct();
                if (status != Data.Enum.Status.Unknown)
                {
                    switch (status)
                    {
                        case Data.Enum.Status.Done:
                            listTasks = listTasks.Where(x => x.Status == true).AsQueryable();
                            break;
                        case Data.Enum.Status.Undone:
                            listTasks = listTasks.Where(x => x.Status == false).AsQueryable();
                            break;
                    }
                }
                var sortTaskList = await listTasks.ToListAsync();
                //Flatten task
                var all = _mapper.Map<List<TreeViewTask>>(sortTaskList);

                var tree = all.Where(x => x.PICs.Count > 0)
                .AsHierarchy(x => x.ID, x => x.ParentID)
                .ToList();
                return tree;
            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }

        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid)
        {
            try
            {
                var jobtype = Data.Enum.JobType.Routine;
                if (ocid == 0)
                    return new List<HierarchyNode<TreeViewTask>>();
                var listTasks = GetAllTasks().Where(x => x.JobTypeID.Equals(jobtype) && x.OCID == ocid);
                var listTasksSort = await SortRoutine(listTasks, sort, priority).ToListAsync();
                var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();

                all = all.Where(x =>
                           x.PICs.Contains(userid)
                        || x.CreatedBy == userid
                        || x.Deputies.Contains(userid)
                        ).Distinct().ToList();
                all.ForEach(item =>
                {
                    item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";
                });
                var tree = all.AsHierarchy(x => x.ID, x => x.ParentID).ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
                tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
                throw;
            }
          
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Abnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            var jobtype = Data.Enum.JobType.Abnormal;
            if (ocid == 0)
                return new List<HierarchyNode<TreeViewTask>>();
            var listTasks = GetAllTasks().Where(x => x.JobTypeID.Equals(jobtype) && x.OCID == ocid);
            var listTasksSort = await FilterAbnormal(listTasks, ocid, priority, userid, startDate, endDate, weekdays).ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();
            all = all.Where(x =>
                       x.PICs.Contains(userid)
                    || x.CreatedBy == userid
                    || x.Deputies.Contains(userid)
                    ).Distinct().ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            var tree = all
              .AsEnumerable()
              .AsHierarchy(x => x.ID, x => x.ParentID)
               .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
              .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
            return tree;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int? projectid = null)
        {
            projectid = projectid ?? 0;
            if (!await _context.Projects.AnyAsync(x => x.ID == projectid))  return new List<HierarchyNode<TreeViewTask>>();
            var jobtype = Data.Enum.JobType.Project;
            var listTasks = GetAllTasks().Where(x => x.JobTypeID.Equals(jobtype) && x.ProjectID == projectid);
           var filterTasksList =await FilterTaskDetail(listTasks, priority)
                .ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(filterTasksList).ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            var tree = all.Where(x => x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateDaily.ToParseStringDateTime().Date.CompareTo(DateTime.Now.Date) <= 0)
                        .AsHierarchy(x => x.ID, x => x.ParentID)
                        .Where(x => x.Entity.Level == 1 && x.Entity.state == "Undone")
                        .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).Where(x => x.Entity.state == "Undone").ToList();
            return tree;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0)
        {
            var listTasks = _context.Follows.Include(x => x.Task).AsQueryable();

            var sortTasksList =await SortFollow(listTasks, sort, priority).ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(sortTasksList).ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            var tree = all
               .AsEnumerable()
               .AsHierarchy(x => x.ID, x => x.ParentID)
               .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = sortTasksList.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.Task.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> History(int userid, string start, string end)
        {
            var listTasks =  _context.Histories
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
                    ModifyDateTime = x.his.ModifyDateTime,
                    Code = x.task.Code
                }).AsQueryable();
            if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var startDate = start.ToParseStringDateTime().Date;
                var endDate = end.ToParseStringDateTime().Date;
                listTasks = listTasks.Where(x => x.CreatedDate.Date >= startDate.Date || x.CreatedDate.Date <= endDate.Date).AsQueryable();
            }
            var fillterTasks = await listTasks.ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(fillterTasks);
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            all = all.Where(x =>
                     x.PICs.Contains(userid)
                  || x.FromWhoID == userid
                  || x.Project.Manager.Contains(userid)
                  || x.Project.Members.Contains(userid)
                  || x.Deputies.Contains(userid)
                  ).Distinct().ToList();
            var tree = all
               .AsEnumerable()
               .AsHierarchy(x => x.ID, x => x.ParentID)
               .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
            var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(map).ToList();
            return tree;
            throw new NotImplementedException();
        }
        public async Task<HashSet<TreeViewTask>> GetListTree(string beAssigned, string assigned, int userid)
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
                var tasks = await GetListTreeViewTaskAsync(listTasks.ToHashSet(), userid);

                if (tasks.Count == 1)
                {
                    if (!tasks.FirstOrDefault().HasChildren)
                        return tasks;
                }
                return MapperTreeViewTask(tasks).OrderByDescending(x => x.JobTypeID)
                    .OrderByDescending(x => x.ID)
                    .ToHashSet();
            }
            catch (Exception ex)
            {

                return new HashSet<TreeViewTask>();
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

        public async Task<bool> CheckTask()
        {
            var currentDate = DateTime.Now.Date;
            var model = await _context.CheckTasks.AnyAsync(x => x.CreatedDate.Date == currentDate && x.Function == "CheckTask");
            return model;
            throw new NotImplementedException();
        }
        public async Task<bool> CreateCheckTask()
        {
            var currentDate = DateTime.Now.Date;
            var model = new CheckTask
            {
                CreatedDate = currentDate,
                Function = "CheckTask"
            };
            try
            {
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
                throw;
            }

            throw new NotImplementedException();
        }
    }
}
