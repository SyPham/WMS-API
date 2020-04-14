
using Data.Models;
using Data.ViewModel.Notification;
using Data.ViewModel.Task;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkManagement.Hub
{
    public class WorkingManagementHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly Data.DataContext _context;
        private readonly string formatDaily = "{0:ddd, MMM d, yyyy}";
        private readonly string formatYearly = "{0:MMM d, yyyy}";
        private readonly string formatSpecificDate = "{0:MMM d, yyyy HH:mm tt}";
        private readonly string formatCreatedDate = "{0:MMM d, yyyy HH:mm tt}";
        public WorkingManagementHub(Data.DataContext context)
        {
            _context = context;
        }
        #region Extension Method
        private string Message(Data.Enum.PeriodType periodType, TreeViewTask item)
        {
            var mes = string.Empty;
            switch (periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateDaily.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DateOfWeekly.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var dateofmonthly = new DateTime(DateTime.Now.Year, item.DueDateMonthly.ToInt(), 1).ToParseDatetimeToStringISO8061();
                    mes = $"You are late for the task name: '{item.JobName}' on {dateofmonthly.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.Quarterly:
                    var dateofquarterly = (item.DueDateQuarterly.Split(",")[1].Trim() + ", " + DateTime.Now.Year).ToParseStringDateTime().ToParseDatetimeToStringISO8061();
                    mes = $"You are late for the task name: '{item.JobName}' on {dateofquarterly.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.Yearly:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.DueDateYearly.ToStringFormatISO(formatDaily)}";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    mes = $"You are late for the task name: '{item.JobName}' on {item.SpecificDate.ToStringFormatISO(formatDaily)}";
                    break;
                default:
                    break;
            }
            return mes;
        }
        private bool BeAlert(int taskId, string alertType)
        {
            return _context.Notifications.Any(x => x.TaskID == taskId && x.Function.Equals(alertType));
        }
        private string PeriodWeekly(Data.Models.Task task)
        {
            string result = string.Empty;
            int year = DateTime.Now.Year;
            var dateOfWeek = task.DateOfWeekly; //30 mar
            var duedateweekly = task.DueDateWeekly;//Mon
            //Tim dayofweek
            var monthWeekly = dateOfWeek.ToParseStringDateTime().Month;
            var allDateInMonth = monthWeekly.AllDatesInMonth(year);
            var listdayOfWeek = allDateInMonth.Where(x => x.DayOfWeek.ToString().Equals(duedateweekly));
            var listdayOfWeekString = listdayOfWeek.Select(x => x.ToString("d MMM, yyyy")).ToArray(); //2,9,16,23,30 mar
            int index = Array.IndexOf(listdayOfWeekString, dateOfWeek);
            if (index < listdayOfWeekString.Count() - 1)
                result = listdayOfWeekString[index + 1];
            else
            {
                if (monthWeekly == 12)
                {
                    year = year + 1;
                    allDateInMonth = 1.AllDatesInMonth(year);
                    listdayOfWeek = allDateInMonth.Where(x => x.DayOfWeek.ToString().Equals(duedateweekly));
                    listdayOfWeekString = listdayOfWeek.Select(x => x.ToString("MMM d, yyyy")).ToArray(); //6,13,20,27 apr
                    result = listdayOfWeekString.FirstOrDefault();
                }
                else
                {
                    monthWeekly = monthWeekly + 1;
                    allDateInMonth = monthWeekly.AllDatesInMonth(year);
                    listdayOfWeek = allDateInMonth.Where(x => x.DayOfWeek.ToString().Equals(duedateweekly));
                    listdayOfWeekString = listdayOfWeek.Select(x => x.ToString("MMM d, yyyy")).ToArray(); //6,13,20,27 apr
                    result = listdayOfWeekString.FirstOrDefault();

                }
            }
            return result;

        }
        private string PeriodMonthly(Data.Models.Task task)
        {
            var duedateMonthly = task.CreatedDateForEachTask;
            var day = task.DueDateMonthly.ToInt();
            var currentYear = duedateMonthly.Year;
            var month = duedateMonthly.Month + 1;
            if (month == 12)
            {
                currentYear = currentYear + 1;
                month = 1;
                currentYear = currentYear + 1;
            }
            var newDueDate = new DateTime(currentYear, month, day).ToParseDatetimeToStringISO8061();
            return newDueDate;
        }
        private List<int> GetListUserRelateToTask(int taskId, bool isProject)
        {
            var task = _context.Tasks.Find(taskId);
            var listManager = _context.Managers.Where(_ => _.ProjectID.Equals(task.ProjectID)).Select(_ => _.UserID).ToList();
            var listMember = _context.TeamMembers.Where(_ => _.ProjectID.Equals(task.ProjectID)).Select(_ => _.UserID).ToList();
            var listPIC = _context.Tags.Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            var listDeputie = _context.Deputies.Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            if (isProject)
                return listPIC;
            else
                return listPIC.Union(listDeputie).Distinct() .ToList();
        }
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
        private string PeriodQuarterly(Data.Models.Task task)
        {
            var quarter = task.DueDateQuarterly.Split(",");
            var duedatequarterly = quarter[0].ToSafetyString().GetLastDateOfNextQuarter();
            return duedatequarterly;
        }
        private string PeriodYearly(Data.Models.Task task)
        {
            var duedateYearly = task.DueDateYearly.ToParseIso8601();
            var newDueDateYealy = new DateTime(duedateYearly.Year + 1, duedateYearly.Month, duedateYearly.Day).ToParseDatetimeToStringISO8061();
            return newDueDateYealy;
        }
        private async Task<string> AlertTasksIsLate(TreeViewTask item, string message, bool isProject)
        {
            var mes = string.Empty;
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
            }

            var history = new History
            {
                TaskID = item.ID,
                Status = false

            };
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    history.Deadline = update.DueDateDaily;
                    break;
                case Data.Enum.PeriodType.Weekly:
                    history.Deadline = update.DateOfWeekly;
                    break;
                case Data.Enum.PeriodType.Monthly:
                    history.Deadline = update.DateOfMonthly;
                    break;
                case Data.Enum.PeriodType.Quarterly:
                    history.Deadline = update.DueDateQuarterly;
                    break;
                case Data.Enum.PeriodType.Yearly:
                    history.Deadline = update.DueDateYearly + ", " + DateTime.Now.Year;
                    break;
                default:
                    break;
            }
            if (notifyParams.Users.Count > 0)
            {
                await PushTaskToHistory(history);
                await Create(notifyParams);
                if (update.periodType != Data.Enum.PeriodType.SpecificDate)
                {
                    mes = $"{update.JobName}<br/>{update.SpecificDate.ToParseStringDateTime().ToStringFormat("{0:f}")}";
                }
            }
            return mes;

        }
        private async Task<bool> Create(CreateNotifyParams entity)
        {
            try
            {
                var item = new Notification
                {
                    TaskID = entity.TaskID,
                    UserID = entity.UserID,
                    Message = entity.Message,
                    URL = entity.URL,
                    Function = entity.AlertType.ToString()
                };
                await _context.Notifications.AddAsync(item);
                await _context.SaveChangesAsync();

                if (entity.Users.Count > 0 || entity.Users != null)
                {
                    var details = new List<NotificationDetail>();
                    foreach (var user in entity.Users)
                    {
                        details.Add(new NotificationDetail
                        {
                            NotificationID = item.ID,
                            UserID = user,
                            Seen = false
                        });
                    }
                    await _context.AddRangeAsync(details);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        private bool CompareDate(DateTime date2)
        {
            var date1 = DateTime.Now;

            int res = DateTime.Compare(date1.Date, date2.Date);

            return res < 0 ? true : false;
        }
        private bool TimeComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate, comparedate);

            return res < 0 ? true : false;
        }
        private async System.Threading.Tasks.Task<string> PeriodType(TreeViewTask item, bool isProject)
        {
            string mes = Message(item.periodType, item);
            string belate = Data.Enum.AlertType.BeLate.ToSafetyString();
            var task = await _context.Tasks.FindAsync(item.ID);
            switch (item.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    var dateDaily = item.DueDateDaily.ToParseIso8601().AddDays(1);
                    if (!CompareDate(dateDaily) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Weekly:
                    var dateWeekly = PeriodWeekly(task).ToParseStringDateTime();
                    if (!CompareDate(dateWeekly) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Monthly:
                    var dateofmonthly = PeriodMonthly(task).ToParseStringDateTime();
                    if (!CompareDate(dateofmonthly) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Quarterly:
                    var dateofquarterly = (PeriodQuarterly(task).Split(",")[1].Trim() + ", " + DateTime.Now.Year).ToParseStringDateTime();
                    if (!CompareDate(dateofquarterly) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.Yearly:
                    var dateyearly = PeriodYearly(task).ToParseStringDateTime();
                    if (!CompareDate(dateyearly) && !BeAlert(item.ID, belate))
                        await AlertTasksIsLate(item, mes, isProject);
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    var specific = item.SpecificDate.ToParseIso8601();
                    if (!TimeComparator(specific) && !BeAlert(item.ID, belate))
                        return await AlertTasksIsLate(item, mes, isProject);
                    break;
                default:
                    break;
            }
            return "";
        }
        private async System.Threading.Tasks.Task<List<string>> ProjectTaskIsLate(List<TreeViewTask> tasks)
        {
            var messges = new List<string>();
            foreach (var item in tasks)
            {
                if (item.ID == 3270)
                {
                    var message = await PeriodType(item, true);
                    if (!message.IsNullOrEmpty())
                        messges.Add(message);
                }
            }
            return messges;
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
        private int FindParentByChild(IEnumerable<Data.Models.Task> rootNodes, int taskID)
        {
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ID;
            else
                return FindParentByChild(rootNodes, parent);
        }
        private string CastPriority(string value)
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
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                                   .Include(x => x.User)
                                   .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                var deputiesList = _context.Deputies.Where(x => x.TaskID == item.ID)
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
                levelItem.DueDateQuarterly = item.DueDateQuarterly.IsNotAvailable();
                levelItem.DueDateYearly = item.DueDateYearly.ToStringFormatISO(formatYearly).IsNotAvailable();
                levelItem.SpecificDate = item.SpecificDate.ToStringFormatISO(formatSpecificDate).IsNotAvailable();
                levelItem.DateOfWeekly = item.DateOfWeekly;
                levelItem.periodType = item.periodType;

                levelItem.User = item.User;
                levelItem.Level = item.Level;
                levelItem.ProjectID = item.ProjectID;
                levelItem.ParentID = item.ParentID;
                levelItem.state = item.Status == false ? "Undone" : "Done";

                levelItem.Priority = CastPriority(item.Priority);
                levelItem.PICs = tagModel.Where(x => x.TaskID == arrTasks).Select(x => x.UserID).ToList();

                levelItem.BeAssigneds = beAssigneds;
                levelItem.Deputies = deputiesList.Select(_ => _.ID).ToList();
                levelItem.FromWhere = ocModel.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault() ?? new FromWhere();
                levelItem.FromWho = userModel.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault() ?? new BeAssigned();
                levelItem.CreatedDateForEachTask = item.CreatedDateForEachTask.ToStringFormat(formatCreatedDate).IsNotAvailable();
                levelItem.JobTypeID = item.JobTypeID;
                levelItem.periodType = item.periodType;

                levelItem.VideoStatus = statusTutorial;
                levelItem.Tutorial = tutorialModel;
                levelItem.VideoLink = statusTutorial ? tutorialModel.URL : "";
                tasks.Add(levelItem);
            }
            return tasks.OrderByDescending(x => x.ID).ToList();
        }

        #endregion
        private async System.Threading.Tasks.Task<List<string>> ProjectTaskIsLate(int userid)
        {
            //Lay tat ca list task chua hoan thanh va task co con chua hoan thnah
            var listTasks = await _context.Tasks
                            .Where(x => x.Status == false)
                            .Include(x => x.User)
                            .OrderBy(x => x.Level).ToListAsync();
            var tasks = GetListTreeViewTask(listTasks, userid);
            try
            {
                return await ProjectTaskIsLate(tasks.Where(x => x.JobTypeID == (int)Data.Enum.JobType.Project).ToList());
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async Task<string> GetUsername(string user)
        {
            try
            {
                var userid = user.ToInt();
                return (await _context.Users.FirstOrDefaultAsync(x => x.ID.Equals(userid))).Username.ToTitleCase();
            }
            catch (Exception ex)
            {
                return "Someone";
                throw;
            }
            throw new NotImplementedException();
        }
        private async Task<string> NameGroup(object obj)
        {
            var id = obj.ToInt();
            var project = await _context.Rooms.FirstOrDefaultAsync(x => x.ID.Equals(id));
            return project.Name;
        }
        private async Task<object> AddMessageGroup(int roomid, string message, int userid)
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(x => x.Room.Equals(roomid));
                var managers = await _context.Managers.Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var members = await _context.TeamMembers.Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var listAll = managers.Union(members);
                var listChats = new List<Data.Models.Chat>();
                var listParticipants = new List<Data.Models.Participant>();

                //Neu chua co participan thi them vao
                if (!await _context.Participants.AnyAsync(x => x.RoomID == roomid))
                {
                    foreach (var user in listAll)
                    {
                        listParticipants.Add(new Data.Models.Participant
                        {
                            UserID = user,
                            RoomID = roomid
                        });
                    }
                    await _context.AddRangeAsync(listParticipants);
                }
                //add message userid
                await _context.AddAsync(new Data.Models.Chat
                {
                    Message = message,
                    UserID = userid,
                    ProjectID = project.ID,
                    RoomID = roomid
                });
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            throw new NotImplementedException();
        }
        public async System.Threading.Tasks.Task CheckAlert(string user)
        {
            int userId = user.ToInt();
            var list = await ProjectTaskIsLate(userId);
            var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"

            if (list.Count > 0)
                await Clients.User(id).SendAsync("ReceiveCheckAlert", user, list);
            else await Clients.User(id).SendAsync("NotCheckAlert", user, "From Server: There is no some alert!!!");
        }
        public async System.Threading.Tasks.Task Online(string user, string message)
        {
            var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"

            await Clients.All.SendAsync("ReceiveOnline", user, message);
        }
        public async System.Threading.Tasks.Task SendMessage(string user, string message)
        {
            var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public System.Threading.Tasks.Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public System.Threading.Tasks.Task SendMessageToUser(string connectionId, string message)
        {
            return Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }

        public override async System.Threading.Tasks.Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public async System.Threading.Tasks.Task JoinGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("ReceiveJoinGroup", user, await GetUsername(user));

        }
        public async System.Threading.Tasks.Task Typing(string group, string user)
        {
            await Clients.Group(group).SendAsync("ReceiveTyping", user, await GetUsername(user));
        }
        public async System.Threading.Tasks.Task StopTyping(string group, string user)
        {
            await Clients.Group(group).SendAsync("ReceiveStopTyping", user);
        }
        public async System.Threading.Tasks.Task SendMessageToGroup(string group, string message, string user)
        {
            ////Luu vo db
            ////Chi gui den nhung nguoi tham gia phong
            ///
            int roomid = group.ToInt();
            int userid = user.ToInt();
            await AddMessageGroup(roomid, message, userid);
            await Clients.Group(group).SendAsync("ReceiveMessageGroup", message);
        }
        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);

        }
       
    }
}
