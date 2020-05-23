using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using Data.Extensions;
using Data.Models;
using Data.ViewModel.Notification;
using Data.ViewModel.OC;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Service.ConfigLine;
using Service.Hub;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        private IConfiguration _configuration;
        private MapperConfiguration _configMapper;

        #endregion

        #region Constructor

        public TaskService(DataContext context,
            INotificationService notificationService,
            IMapper mapper, IUserService userService,
            IHubContext<WorkingManagementHub> hubContext,
            IProjectService projectService,
            MapperConfiguration configMapper,
            IConfiguration configuration,
            IOCService ocService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _projectService = projectService;
            _ocService = ocService;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _configMapper = configMapper;
            _configuration = configuration;
        }

        #endregion

        #region Helpers
        private IEnumerable<TreeViewOC> GetAllDescendants(IEnumerable<TreeViewOC> rootNodes)
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
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeWeekly, task.DueDateTime),
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
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeMonthly, task.DueDateTime),
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
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, Data.Enum.AlertType.ChangeDeadline),
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
        private string AlertMessage(string username, string jobName, string project, bool isProject, Data.Enum.AlertType alertType, DateTime deadline = new DateTime())
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
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
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
        private string CheckMessage(Data.Enum.JobType jobtype, string project, string username, string jobName, Data.Enum.AlertType alertType, DateTime deadline = new DateTime())
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
        private async Task<List<int>> AlertTasksIsLate(TreeViewTask item, string message, bool isProject)
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
                return notifyParams.Users;
            }
            return new List<int>();
        }
        private string Message(Data.Enum.PeriodType periodType, TreeViewTask item)
        {
            var mes = string.Empty;
            switch (periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDate}";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDate}";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDate}";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDate}";
                    break;
                default:
                    break;
            }
            return mes;
        }
        private bool CheckNotification(int id)
        {
            return _context.Notifications.Any(x => x.TaskID == id && x.Function.Equals(Data.Enum.AlertType.BeLate.ToSafetyString()));
        }
        private async Task<List<int>> PeriodType(TreeViewTask item, bool isProject)
        {
            string mes = Message(item.periodType, item);
            var useListForHub = new List<int>();
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var checkDaily = CheckNotification(item.ID);
                    if (!checkDaily)
                        useListForHub.AddRange(await AlertTasksIsLate(item, mes, isProject));
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var checkWeekly = CheckNotification(item.ID);
                    if (!checkWeekly)
                        useListForHub.AddRange(await AlertTasksIsLate(item, mes, isProject));
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var checkMonthly = CheckNotification(item.ID);
                    if (!checkMonthly)
                        useListForHub.AddRange(await AlertTasksIsLate(item, mes, isProject));
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var checkSpecificDate = CheckNotification(item.ID);
                    if (!checkSpecificDate)
                        useListForHub.AddRange(await AlertTasksIsLate(item, mes, isProject));
                    break;
                default:
                    break;
            }
            return useListForHub.Distinct().ToList();
        }
        #endregion

        #region Notification
        /// <0 − If CurrentDate is earlier than comparedate
        /// =0 − If CurrentDate is the same as comparedate
        /// >0 − If CurrentDate is later than comparedate
        private bool CheckCompletedTask(Data.Models.Task task)
        {
            bool result = false;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var res1 = PeriodComparator(task.DueDateTime);
                    result = res1 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var res2 = PeriodComparator(task.DueDateTime);
                    result = res2 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var res3 = PeriodComparator(task.DueDateTime);
                    result = res3 <= 0 ? true : false;
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var res4 = PeriodComparator(task.DueDateTime);
                    result = res4 <= 0 ? true : false;
                    break;
                default:
                    break;
            }
            return result;
        }
        private async Task<List<int>> ProjectTaskIsLate(List<TreeViewTask> tasks)
        {
            var useListForHub = new List<int>();

            foreach (var item in tasks)
            {
                useListForHub.AddRange(await PeriodType(item, true));
            }
            return useListForHub.ToList();
        }
        private async Task<List<int>> RoutineTaskIsLate(List<TreeViewTask> tasks)
        {
            var useListForHub = new List<int>();
            foreach (var item in tasks)
            {
                useListForHub.AddRange(await PeriodType(item, false));
            }
            return useListForHub.ToList();
        }
        private async Task<List<int>> AbnormalTaskIsLate(List<TreeViewTask> tasks)
        {
            var useListForHub = new List<int>();
            foreach (var item in tasks)
            {
                useListForHub.AddRange(await PeriodType(item, false));
            }
            return useListForHub.ToList();
        }
        private async Task<bool> CheckExistTaskForMultiTask(Data.Models.Task task)
        {
            var currentDate = DateTime.Now;
            var dueDateTime = MapDueDateTime(task);
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                case Data.Enum.PeriodType.Weekly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                case Data.Enum.PeriodType.Monthly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                default:
                    return false;
            }
        }

        public async Task<Tuple<List<int>, List<int>>> TaskListIsLate()
        {
            var listTasks = _context.Tasks
              .Where(x => x.DueDateTime.Date.CompareTo(new DateTime(2020, 1, 1)) > 0)
              .Include(x => x.Tags).ThenInclude(x => x.User)
              .Include(x => x.Deputies).ThenInclude(x => x.User)
              .Where(x => x.DueDateTime.Date.CompareTo(DateTime.Now.Date) <= 0 && x.Status == false && x.Tags.Count > 0 && x.DueDateTime.Date.CompareTo(DateTime.MinValue) != 0);
            var userListForHub = new List<int>();
            if (listTasks.Count() == 0)
                return Tuple.Create(new List<int>(), new List<int>());
            var unCompletedTaskList = new List<Data.Models.Task>();
            var currentDate = DateTime.Now.Date;
            foreach (var item in listTasks)
            {
                if (!CheckCompletedTask(item))
                {
                    unCompletedTaskList.Add(item);
                }
            }

            var allTasks = _context.Tasks.AsQueryable();
            var listSingleTask = new List<Data.Models.Task>();
            var listMultiTask = new List<List<Data.Models.Task>>();
            // Clone
            foreach (var item in unCompletedTaskList.Where(x => !x.periodType.Equals(Data.Enum.PeriodType.SpecificDate)))
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
                    listSingleTask.Add(seftAndDescendants.First());

                }
                if (seftAndDescendants.Count > 1 && await CheckExistTask(item))
                {
                    listMultiTask.Add(seftAndDescendants.OrderByDescending(x => x.ID).ToList());
                }
            }
            foreach (var item in listSingleTask.Distinct())
            {
                userListForHub.AddRange(await CloneSingleTask(item));

            }
            var tas = listMultiTask.DistinctBy(x => x.First().ID).ToList();
            foreach (var item in tas)
            {
                userListForHub.AddRange(await CloneMultiTask(item));

            }
            //Alert
            var tasks = _mapper.Map<List<TreeViewTask>>(unCompletedTaskList).Where(x => x.PICs.Count > 0).ToList();
            try
            {
                var projects = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Project).ToList();
                var routine = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Routine).ToList();
                var abnormal = tasks.Where(x => x.JobTypeID == Data.Enum.JobType.Abnormal).ToList();
                var listNotify = new List<int>();
                listNotify.AddRange(await ProjectTaskIsLate(projects));
                listNotify.AddRange(await RoutineTaskIsLate(routine));
                listNotify.AddRange(await AbnormalTaskIsLate(abnormal));
                if (userListForHub.Distinct().Count() > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", userListForHub.Distinct()));
                }
                if (listNotify.Distinct().Count() > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listNotify.Distinct()), GetAlertDueDate());
                }
                return Tuple.Create(userListForHub, listNotify);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region Event( Create Task, Sub-Task, Follow, Undo, Delete, Done, Remark, ...)
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
                var listUsers = new List<int>();
                // task.DueDate = task.DueDate.ToStringFormatDateTime();
                // add
                if (!await _context.Tasks.AnyAsync(x => x.ID == task.ID))
                {
                    var item = _mapper.Map<Data.Models.Task>(task);

                    //Level cha tang len 1 va gan parentid cho subtask
                    var taskParent = _context.Tasks.Find(item.ParentID);
                    item.Level = taskParent.Level + 1;
                    item.ParentID = task.ParentID;
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
                        listUsers.AddRange(tags.Select(x => x.UserID));
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
                        listUsers.AddRange(deputies.Select(x => x.UserID));

                    }
                    await _context.SaveChangesAsync();
                    if (listUsers.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                        await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                    }
                    return true;

                }
                else //update
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
                        listUsers.AddRange(tags.Select(x => x.UserID));

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
                            listUsers.AddRange(deputies.Select(x => x.UserID));
                        }
                    }
                    await _context.SaveChangesAsync();
                    if (listUsers.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                        await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private Data.Models.Task CheckDuedate(Data.Models.Task task, CreateTaskViewModel createTaskView)
        {

            switch (createTaskView.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    task.DueDateTime = createTaskView.DueDate.ToParseStringDateTime();
                    break;
                case Data.Enum.PeriodType.Weekly:
                    task.DueDateTime = createTaskView.DueDate.ToParseStringDateTime();
                    break;
                case Data.Enum.PeriodType.Monthly:
                    task.DueDateTime = createTaskView.DueDate.ToParseStringDateTime();
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    task.DueDateTime = createTaskView.DueDate.ToParseStringDateTime();
                    break;
                default:
                    break;
            }
            return task;
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
        private object GetAlertDueDate()
        {
            var date = DateTime.Now.Date;
            var list = _context.Tasks.Where(x => x.periodType == Data.Enum.PeriodType.SpecificDate && x.CreatedDate.Date == date).Select(x => new
            {
                x.CreatedDate,
                x.DueDateTime
            }).ToList();
            return list;
        }
        public async Task<Tuple<bool, string, object>> CreateTask(CreateTaskViewModel task)
        {
            try
            {
                // task.DueDate = task.DueDate;
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
                    if (listUsers.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                        await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
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
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var daily = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Daily, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(daily.Item1);
                            }
                            break;
                        case Data.Enum.PeriodType.Weekly:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var weekly = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Weekly, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(weekly.Item1);
                            }
                            break;
                        case Data.Enum.PeriodType.Monthly:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var mon = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Monthly, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(mon.Item1);
                            }

                            break;
                        case Data.Enum.PeriodType.SpecificDate:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var due = await AlertDeadlineChanging(Data.Enum.AlertDeadline.Deadline, edit, edit.FromWhoID, pics);
                                listUsers.AddRange(due.Item1);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                            }
                            break;
                        default:
                            break;
                    }
                }
                await _context.SaveChangesAsync();
                if (listUsers.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                }
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
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToList();

                _context.Tags.RemoveRange(await _context.Tags.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Deputies.RemoveRange(await _context.Deputies.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Follows.RemoveRange(await _context.Follows.Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _context.Tasks.RemoveRange(await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToListAsync());
                _context.Tutorials.RemoveRange(await _context.Tutorials.Where(x => arrTasks.Contains(x.TaskID ?? 0)).ToListAsync());

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
        //private DateTime PeriodDaily(Data.Models.Task task)
        //{
        //    return task.DueDateTime.AddDays(1);
        //}
        //private DateTime PeriodWeekly(Data.Models.Task task)
        //{
        //    //Hoan thanh task thi tang len 1 ngay
        //    return task.DueDateTime.AddDays(7);
        //}
        //private DateTime PeriodMonthly(Data.Models.Task task)
        //{
        //    return task.DueDateTime.AddMonths(1);
        //}
        #endregion

        private async Task<List<int>> ClonePIC(int oldTaskid, int newTaskID)
        {

            var userlistForHub = new List<int>();
            var pic = _context.Tags.Where(x => x.TaskID == oldTaskid).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = newTaskID, UserID = item.UserID });
            }
            await _context.Tags.AddRangeAsync(list);
            await _context.SaveChangesAsync();

            var deputies = _context.Deputies.Where(x => x.TaskID == oldTaskid).ToList();
            var list2 = new List<Deputy>();
            foreach (var item in deputies)
            {
                list2.Add(new Deputy { TaskID = newTaskID, UserID = item.UserID });
            }
            await _context.Deputies.AddRangeAsync(list2);
            await _context.SaveChangesAsync();

            var follows = _context.Follows.Where(x => x.TaskID == oldTaskid).ToList();
            var list3 = new List<Follow>();
            foreach (var item in follows)
            {
                list3.Add(new Follow { TaskID = newTaskID, UserID = item.UserID });
            }
            await _context.Follows.AddRangeAsync(list3);
            await _context.SaveChangesAsync();

            var tutorials = _context.Tutorials.Where(x => x.TaskID == oldTaskid).ToList();
            var list4 = new List<Tutorial>();
            foreach (var item in tutorials)
            {
                list4.Add(new Tutorial { TaskID = newTaskID, Name = item.Name, Level = item.Level, ParentID = item.ParentID, Path = item.Path, URL = item.URL });
            }
            await _context.Tutorials.AddRangeAsync(list4);
            await _context.SaveChangesAsync();
            userlistForHub.AddRange(pic.Select(x => x.UserID));
            userlistForHub.AddRange(deputies.Select(x => x.UserID));
            userlistForHub.AddRange(follows.Select(x => x.UserID));
            return userlistForHub.Distinct().ToList();
        }
        private async Task<Data.Models.Task> UpdatePeriodForDone(CloneTaskViewModel task)
        {
            var update = await _context.Tasks.FindAsync(task.ID);
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    update.DueDateTime = update.DueDateTime.AddDays(1);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    update.DueDateTime = update.DueDateTime.AddDays(7);
                    break;
                case Data.Enum.PeriodType.Monthly:
                    update.DueDateTime = update.DueDateTime.AddMonths(1);
                    break;
                default:
                    break;
            }
            await _context.SaveChangesAsync();
            return update;
        }
        private async Task<List<int>> CloneRelatedTable(CloneTaskViewModel task)
        {
            var userlistForHub = new List<int>();
            var pic = _context.Tags.Where(x => x.TaskID == task.IDTemp).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = task.ID, UserID = item.UserID });
            }
            await _context.Tags.AddRangeAsync(list);
            await _context.SaveChangesAsync();

            var deputies = _context.Deputies.Where(x => x.TaskID == task.IDTemp).ToList();
            var list2 = new List<Deputy>();
            foreach (var item in deputies)
            {
                list2.Add(new Deputy { TaskID = task.ID, UserID = item.UserID });
            }
            await _context.Deputies.AddRangeAsync(list2);
            await _context.SaveChangesAsync();
            var follows = _context.Follows.Where(x => x.TaskID == task.IDTemp).ToList();
            var list3 = new List<Follow>();
            foreach (var item in follows)
            {
                list3.Add(new Follow { TaskID = task.ID, UserID = item.UserID });
            }
            await _context.Follows.AddRangeAsync(list3);
            await _context.SaveChangesAsync();
            var tutorials = _context.Tutorials.Where(x => x.TaskID == task.IDTemp).ToList();
            var list4 = new List<Tutorial>();
            foreach (var item in tutorials)
            {
                list4.Add(new Tutorial { TaskID = task.ID, Name = item.Name, Level = item.Level, ParentID = item.ParentID, Path = item.Path, URL = item.URL });
            }
            await _context.Tutorials.AddRangeAsync(list4);
            await _context.SaveChangesAsync();
            userlistForHub.AddRange(pic.Select(x => x.UserID));
            userlistForHub.AddRange(deputies.Select(x => x.UserID));
            userlistForHub.AddRange(follows.Select(x => x.UserID));

            return userlistForHub.Distinct().ToList();

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
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                case Data.Enum.PeriodType.Weekly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                case Data.Enum.PeriodType.Monthly:
                    return await _context.Tasks.AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                default:
                    return false;
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
                        update.DueDateTime = task.DueDateTime.AddDays(1);
                        break;
                    case Data.Enum.PeriodType.Weekly:
                        update.DueDateTime = task.DueDateTime.AddDays(7);
                        break;
                    case Data.Enum.PeriodType.Monthly:
                        update.DueDateTime = task.DueDateTime.AddMonths(1);
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
            var dueDate = DateTime.MinValue;
            var check = await _context.Tasks.Where(x => x.Code.Equals(update.Code)).ToListAsync();
            foreach (var item in check)
            {
                switch (update.periodType)
                {
                    case Data.Enum.PeriodType.Daily:
                        dueDate = update.DueDateTime.AddDays(1);
                        if (item.DueDateTime.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case Data.Enum.PeriodType.Weekly:
                        dueDate = update.DueDateTime.AddDays(7);
                        if (item.DueDateTime.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case Data.Enum.PeriodType.Monthly:
                        dueDate = update.DueDateTime.AddMonths(1);
                        if (item.DueDateTime.Equals(dueDate))
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
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.Weekly:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.Monthly:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case Data.Enum.PeriodType.SpecificDate:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
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
                    var weekly = task.DueDateTime.Date.Subtract(TimeSpan.FromDays(3));
                    var resultW = PeriodComparator(weekly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateTime.Subtract(TimeSpan.FromDays(3)):dd MMMM, yyyy} to {task.DueDateTime:dd MMMM, yyyy}";
                    return resultW > 0 ? true : false;
                case Data.Enum.PeriodType.Monthly:
                    var monthly = task.DueDateTime.Date.Subtract(TimeSpan.FromDays(10));
                    var resultM = PeriodComparator(monthly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateTime.Subtract(TimeSpan.FromDays(10)):dd MMMM, yyyy} to {task.DueDateTime:dd MMMM, yyyy}";
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
            var parentItem = rootNodes.Any(x => x.ID.Equals(taskID));
            if (!parentItem)
                return null;
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
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
            var tasks = new List<TreeViewTask>();
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
                                DeputyName = c.DeputyName,
                                Tutorial = c.Tutorial,
                                ModifyDateTime = c.ModifyDateTime,
                                periodType = c.periodType,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();
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
        private async Task<List<int>> CloneSingleTask(Data.Models.Task task)
        {
            var userListForHub = new List<int>();
            using var transaction = _context.Database.BeginTransaction();

            int old = task.ID;
            var newTask = new Data.Models.Task
            {
                JobName = task.JobName,
                OCID = task.OCID,
                FromWhoID = task.FromWhoID,
                Priority = task.Priority,
                ProjectID = task.ProjectID,
                JobTypeID = task.JobTypeID,
                DueDateTime = MapDueDateTime(task),
                Code = task.Code,
                periodType = task.periodType,
                CreatedBy = task.CreatedBy,
                Level = task.Level,

            };
            //Kiem tra cai task chuan bi clone nay da ton tai chua
            var check = await CheckExistTask(newTask);
            if (!check)
            {
                await CreateTaskAsync(newTask);
                userListForHub.Add(newTask.FromWhoID);
                userListForHub.AddRange(await ClonePIC(old, newTask.ID));
            }
            await transaction.CommitAsync();
            return userListForHub.Distinct().ToList();
        }
        DateTime MapDueDateTime(Data.Models.Task item)
        {
            var result = DateTime.MinValue;
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    result = item.DueDateTime.AddDays(1);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    result = item.DueDateTime.AddDays(7);
                    break;
                case Data.Enum.PeriodType.Monthly:
                    result = item.DueDateTime.AddMonths(1);
                    break;
                default:
                    break;
            }
            return result;
        }
        private async Task<Data.Models.Task> CreateTaskAsync(Data.Models.Task task)
        {
            await _context.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }
        async System.Threading.Tasks.Task UpdateTaskForMultiTask(List<CloneTaskViewModel> listTemp)
        {
            var update = _context.Tasks.Where(x => listTemp.Select(a => a.ID).Contains(x.ID)).ToList();

            update.ForEach(item =>
            {
                if (item.Level > 1)
                {
                    item.ParentID = listTemp.FirstOrDefault(x => x.IDTemp == item.ParentID).ID;
                }
            });
            await _context.SaveChangesAsync();

        }
        private async Task<List<int>> CloneMultiTask(List<Data.Models.Task> tasks)
        {
            var listTemp = new List<CloneTaskViewModel>();
            var userListForHub = new List<int>();
            using var transaction = _context.Database.BeginTransaction();

            foreach (var item in tasks)
            {
                var check = await CheckExistTaskForMultiTask(item);
                if (!check)
                {
                    var temp = _mapper.Map<CloneTaskViewModel>(item);
                    temp.IDTemp = item.ID;
                    temp.ParentTemp = item.ParentID;
                    var task = new Data.Models.Task();
                    task.Code = item.Code;
                    task.JobName = item.JobName;
                    task.ParentID = item.ParentID;
                    task.Level = item.Level;
                    task.ProjectID = item.ProjectID;
                    task.CreatedBy = item.CreatedBy;
                    task.OCID = item.OCID;
                    task.FromWhoID = item.FromWhoID;
                    task.Priority = item.Priority;
                    task.periodType = item.periodType;
                    task.DueDateTime = MapDueDateTime(item);
                    task.JobTypeID = item.JobTypeID;
                    task.CreatedDate = DateTime.Now;
                    var taskModel = await CreateTaskAsync(task);
                    temp.ID = taskModel.ID;
                    userListForHub.Add(taskModel.FromWhoID);
                    listTemp.Add(temp);
                }
            }
            foreach (var item in listTemp)
            {
                userListForHub.AddRange(await CloneRelatedTable(item));
            }
            await UpdateTaskForMultiTask(listTemp);
            await transaction.CommitAsync();
            return userListForHub.Distinct().ToList();
        }

        private bool CheckDailyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckWeeklyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckMonthlyOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckSpecificDateOntime(Data.Models.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
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
                if (item.Level > 1 && seftAndDescendants.Where(x => x.Level > 1).Count() >= 1)
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
                if (listUserAlertHub.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUserAlertHub.ToArray()), "message");
                    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUserAlertHub.Distinct()));
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
        private List<TreeViewTask> GetChildren(List<TreeViewTask> tasks, int parentid)
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
                        //DueDateDaily = c.DueDateDaily,
                        //DueDateWeekly = c.DueDateWeekly,
                        //DueDateMonthly = c.DueDateMonthly,
                        //SpecificDate = c.SpecificDate,
                        DeputyName = c.DeputyName,
                        Tutorial = c.Tutorial,
                        ModifyDateTime = c.ModifyDateTime,
                        periodType = c.periodType,
                        children = GetChildren(tasks, c.ID)
                    })
                    .OrderByDescending(x => x.ID)
                    .ToList();
        }
        private IEnumerable<TreeViewTask> AsTreeView(int parentID, int id)
        {
            var listTasks = _context.Tasks
               .Include(x => x.User)
               .OrderBy(x => x.Level).AsQueryable();
            var tasks = new List<TreeViewTask>();
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

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                state = c.state,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();
            return hierarchy;
        }
        public async Task<object> Undo(int id)
        {
            if (!await _context.Tasks.AnyAsync(x => x.ID == id))
                return false;
            try
            {
                var listUsers = new List<int>();
                var listTasks = _context.Tasks.AsQueryable();
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
                var tags = _context.Tags.Where(a => seftAndDescendants.Select(x => x.ID).Contains(a.UserID)).Select(x => x.UserID).ToList();
                var deputies = _context.Deputies.Where(a => seftAndDescendants.Select(x => x.ID).Contains(a.UserID)).Select(x => x.UserID).ToList();
                listUsers.AddRange(tags);
                listUsers.AddRange(deputies);
                await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region LoadData
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
        public async Task<object> GetDeputies()
        {
            return await _context.Users.Where(x => x.Username != "admin").Select(x => new { x.Username, x.ID }).ToListAsync();
        }
        public async Task<List<ProjectViewModel>> GetListProject()
        {
            return await _projectService.GetListProject();
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
                                .Include(x => x.OC)
                                .Include(x => x.Tutorial)
                                .AsQueryable();
            return listTasks;
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
                listTasks = listTasks.Where(x => x.DueDateTime.ToSafetyString().ToLower().Equals(weekdays.ToLower())).AsQueryable();
            }
            //loc theo thang
            if (!monthly.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateTime.ToSafetyString().ToLower().Equals(monthly.ToLower())).AsQueryable();
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
                listTasks = listTasks.Where(x => x.DueDateTime.Equals(weekdays)).AsQueryable();
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
        private IQueryable<Data.Models.Task> SortFollow(IQueryable<Data.Models.Task> listTasks, string sort, string priority)
        {

            if (!sort.IsNullOrEmpty())
            {
                if (sort == "project")
                    listTasks = listTasks.Where(x => x.ProjectID > 0).AsQueryable();
                if (sort == "routine")
                    listTasks = listTasks.Where(x => x.ProjectID == 0).AsQueryable();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
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
        public async Task<object> GetCodeLineAsync(string code, string state)
        {

            var url = "https://notify-bot.line.me/oauth/token";
            var lineNotifyConfig = _configuration.GetSection("LineNotifyConfig").Get<LineNotifyConfig>();
            lineNotifyConfig.code = code;
            using (var client = new HttpClient())
            {
                // tin nhắn sẽ được thông báo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", lineNotifyConfig.grant_type },
                    { "code", code },
                    { "redirect_uri", lineNotifyConfig.redirect_uri },
                    { "client_id", lineNotifyConfig.client_id },
                    { "client_secret", lineNotifyConfig.client_secret },
                });
                // thêm mã token vào header
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                // Thực hiện gửi thông báo
                var result = await client.PostAsync(url, content);
                if (result.IsSuccessStatusCode)
                {
                    string res = result.Content.ReadAsStringAsync().Result;
                    return res;

                }
                return result;
                // Luu token notifi vao db
            }
        }
        private async void PublishhMessage(string message = "Test")
        {
            // mã token truy cập

            var ACCESS_TOKEN = "7WFjkrUVVugK9W6OPrOgAKhdOZIkRPU8pSd5ZOMb8cY";

            using (var client = new HttpClient())
            {
                // tin nhắn sẽ được thông báo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "message", message },
                });

                // thêm mã token vào header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                // Thực hiện gửi thông báo
                var result = await client.PostAsync("https://notify-api.line.me/api/notify", content);
            }
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid)
        {

            try
            {
                var listTasks = GetAllTasks().Where(x =>
                            (x.Tags.Select(x => x.UserID).Contains(userid)
                            || x.Deputies.Select(x => x.UserID).Contains(userid)
                            || x.FromWhoID == userid
                            || x.CreatedBy == userid)
                            && x.Status == false
                    )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();
                if (!beAssigned.IsNullOrEmpty() && beAssigned == "BeAssigned")
                {
                    listTasks = listTasks.Where(x => x.Tags.Select(x => x.UserID).Contains(userid)).AsQueryable();
                }
                if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                {
                    listTasks = listTasks.Where(x => x.FromWhoID == userid || x.CreatedBy == userid).Distinct().AsQueryable();
                }
                var sortTaskList = await listTasks.ToListAsync();
                //Flatten task
                var all = _mapper.Map<List<TreeViewTask>>(sortTaskList);
                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();

                var flatten = tree.Flatten(x => x.ChildNodes).ToHashSet();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<HashSet<HierarchyNode<TreeViewTask>>>(itemWithOutParent).Where(x => x.Entity.periodType.Equals(Data.Enum.PeriodType.Daily) && x.Entity.DueDate.Date.CompareTo(DateTime.Now.Date) <= 0 && x.Entity.DueDate.Date.CompareTo(DateTime.MinValue) != 0).ToList();
                tree = tree.Concat(map).ToList();

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
                            (x.Tags.Select(x => x.UserID).Contains(userid)
                            || x.Deputies.Select(x => x.UserID).Contains(userid)
                            || x.FromWhoID == userid
                            || x.CreatedBy == userid)
                            && x.Status == false
                    )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1 && x.DueDateTime.Date.CompareTo(DateTime.MinValue) != 0).Distinct();
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
                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToHashSet();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<HashSet<HierarchyNode<TreeViewTask>>>(itemWithOutParent).Where(x => x.Entity.periodType.Equals(Data.Enum.PeriodType.Daily) && x.Entity.DueDate.Date.CompareTo(DateTime.Now.Date) <= 0).ToList();
                tree = tree.Concat(map).ToList();

                return tree;
            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }

        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "")
        {
            try
            {
                //A: Setup and stuff you don't want timed
                // PublishhMessage("Good morning!");
                //  await _notificationService.Create(new CreateNotifyParams { Message = "Test", TaskID = 3675, AlertType = Data.Enum.AlertType.BeLate, URL = "/todolist/Demo-daily-is-late" });
                var listTasks = GetAllTasks()
                    .Where(x =>
                               (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                               && x.Status == false && x.Tags.Count > 0
                    )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1 && x.DueDateTime.Date.CompareTo(DateTime.MinValue) != 0).Distinct();
                var listtasksfillter = await Fillter(listTasks, sort, priority, userid, startDate, endDate, weekdays, monthly, quarterly).ToListAsync();

                var all = _mapper.Map<List<Data.Models.Task>, List<TreeViewTask>>(listtasksfillter,
                    opt => opt.AfterMap((src, dest) =>
                    {
                        dest.ForEach(item =>
                         {
                             item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";
                         });
                    }));

                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();

                // convert lai qua list phang
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();

                // loc ra nhung item chua co trong tree
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
                throw;
            }

        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid)
        {
            try
            {
                // var user = _context.Users.Find(userid);
                // var model =await Notification();
                var jobtype = Data.Enum.JobType.Routine;
                if (ocid == 0)
                    return new List<HierarchyNode<TreeViewTask>>();
                var listTasks = GetAllTasks()
                   .Where(x => x.Status == false && x.JobTypeID.Equals(jobtype) && x.OCID == ocid)
                    .Where(x =>
                               (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                    )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();
                var listTasksSort = await SortRoutine(listTasks, sort, priority).ToListAsync();

                var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();
                all.ForEach(item =>
                {
                    item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";
                });
                //all = all.Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDate.Date.CompareTo(DateTime.Now.Date) != 1)
                //                      .ToList();
                var tree = all
              .AsEnumerable()
              .AsHierarchy(x => x.ID, x => x.ParentID)
              .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                tree = tree.Concat(itemWithOutParent).OrderByDescending(x => x.Entity.ID).ToList();
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
            var listTasks = GetAllTasks()
                    .Where(x => x.Status == false && x.JobTypeID.Equals(jobtype) && x.OCID == ocid)
                    .Where(x =>
                              (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                    )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();

            var listTasksSort = await FilterAbnormal(listTasks, ocid, priority, userid, startDate, endDate, weekdays).ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();
            all = all.Where(x =>
                       x.PICs.Contains(userid)
                    || x.CreatedBy == userid
                    || x.FromWhoID == userid
                    || x.Deputies.Contains(userid)
                    ).Distinct().ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            var tree = all
              .AsEnumerable()
              .AsHierarchy(x => x.ID, x => x.ParentID)
              .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
            { Entity = x }).ToList();
            tree = tree.Concat(itemWithOutParent).ToList();
            return tree;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int? projectid = null)
        {
            projectid = projectid ?? 0;
            if (!await _context.Projects.AnyAsync(x => x.ID == projectid)) return new List<HierarchyNode<TreeViewTask>>();
            var jobtype = Data.Enum.JobType.Project;
            var listTasks = GetAllTasks()
                .Where(x => x.JobTypeID.Equals(jobtype) && x.ProjectID == projectid && x.Status == false)
                .Where(x =>
                              (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                   )
                    .Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();

            var filterTasksList = await FilterTaskDetail(listTasks, priority)
                 .ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(filterTasksList).ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            all = all.Where(x => !x.periodType.Equals(Data.Enum.PeriodType.Daily) || x.periodType.Equals(Data.Enum.PeriodType.Daily) && x.DueDate.Date.CompareTo(DateTime.Now.Date) != 1)
                                      .ToList();
            var tree = all.AsHierarchy(x => x.ID, x => x.ParentID).ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
            { Entity = x }).ToList();
            // var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(itemWithOutParent).ToList();
            return tree;
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0)
        {
            try
            {
                var listTasks = GetAllTasks().Where(x => x.Follows.Select(x => x.UserID).Contains(userid));

                var sortTasksList = await SortFollow(listTasks, sort, priority).ToListAsync();
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
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                //  var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }

        }

        public async Task<List<HierarchyNode<TreeViewTaskForHistory>>> History(int userid, string start, string end)
        {
            try
            {


                var listTasks = _context.Histories
                    .Join(_context.Tasks
                    .Include(x => x.OC)
                    .Include(x => x.Tutorial)
                    .Include(x => x.Tags).ThenInclude(x => x.User)
                     .Include(x => x.Deputies).ThenInclude(x => x.User)
                     .Where(x =>
                         x.Tags.Select(x => x.UserID).Contains(userid)
                      || x.Deputies.Select(x => x.UserID).Contains(userid)
                      || x.FromWhoID.Equals(userid)
                      || x.CreatedBy.Equals(userid)
                      ).Distinct()
                    ,
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
                        DueDateTime = x.task.DueDateTime,
                        ModifyDateTime = x.his.ModifyDateTime,
                        Code = x.task.Code,
                        Deputies = x.task.Deputies,
                        Tags = x.task.Tags,
                        Project = x.task.Project,
                        OC = x.task.OC,
                        Tutorial = x.task.Tutorial
                    }).AsQueryable();
                if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
                {
                    var timespan = new TimeSpan(0, 0, 0);
                    var startDate = start.ToParseStringDateTime().Date;
                    var endDate = end.ToParseStringDateTime().Date;
                    listTasks = listTasks.Where(x => x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date).AsQueryable();
                }
                var fillterTasks = await listTasks.ToListAsync();
                var all = _mapper.Map<List<TreeViewTaskForHistory>>(fillterTasks);
                all = all.ToList();
                var tree = all
                   .AsHierarchy(x => x.ID, x => x.ParentID)
                   .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTaskForHistory>
                { Entity = x }).ToList();
                //var map = _mapper.Map<List<HierarchyNode<TreeViewTaskForHistory>>>(itemWithOutParent);
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return null;

                throw;
            }
        }
        public async Task<List<HierarchyNode<TreeViewTaskForHistory>>> HistoryFilterByDueDateTime(int userid, string start, string end)
        {
            try
            {


                var listTasks = await _context.Histories
                    .Join(_context.Tasks
                    .Include(x => x.OC)
                    .Include(x => x.Tutorial)
                    .Include(x => x.Tags).ThenInclude(x => x.User)
                     .Include(x => x.Deputies).ThenInclude(x => x.User)
                     .Where(x =>
                         x.Tags.Select(x => x.UserID).Contains(userid)
                      || x.Deputies.Select(x => x.UserID).Contains(userid)
                      || x.FromWhoID.Equals(userid)
                      || x.CreatedBy.Equals(userid)
                      ).Distinct()
                    ,
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
                        DueDateTime = x.task.DueDateTime,
                        ModifyDateTime = x.his.ModifyDateTime,
                        Code = x.task.Code,
                        Deputies = x.task.Deputies,
                        Tags = x.task.Tags,
                        Project = x.task.Project,
                        OC = x.task.OC,
                        Tutorial = x.task.Tutorial
                    }).ToListAsync();
                if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
                {
                    var timespan = new TimeSpan(0, 0, 0);
                    var startDate = start.ToParseStringDateTime().Date;
                    var endDate = end.ToParseStringDateTime().Date;
                    listTasks = listTasks.Where(x =>
                    x.DueDateTime.Date >= startDate.Date && x.DueDateTime.Date <= endDate.Date
                    ).ToList();
                }
                var all = _mapper.Map<List<TreeViewTaskForHistory>>(listTasks);
                all = all.ToList();
                var tree = all.Where(x => x.PICs.Count > 0)
                   .AsHierarchy(x => x.ID, x => x.ParentID)
                   .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<List<HierarchyNode<TreeViewTaskForHistory>>>(itemWithOutParent);
                tree = tree.Concat(map).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return null;

                throw;
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

        public async Task<object> UpdateDueDateTime()
        {
            //var tasks = _context.Tasks.ToList();
            //tasks.ForEach(item =>
            //{
            //    item.DueDateTime = item.periodType == Data.Enum.PeriodType.Daily ? item.DueDateTime : item.periodType == Data.Enum.PeriodType.Weekly ? item.DueDateTime : item.periodType == Data.Enum.PeriodType.Monthly ? item.DueDateTime : item.DueDateTime;
            //});
            //try
            //{
            //  await _context.SaveChangesAsync();
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
            throw new NotImplementedException();
        }
    }
}
