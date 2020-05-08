using AutoMapper;
using Data.Models;
using Data.ViewModel;
using Data.ViewModel.Comment;
using Data.ViewModel.Notification;
using Data.ViewModel.OC;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Data.ViewModel.Tutorial;
using Data.ViewModel.User;
using Service.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WorkManagement.Dtos;

namespace WorkManagement.Helpers
{
    public class AutoMapperProfile : Profile
    {
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
        private string CheckDuedate(CreateTaskViewModel createTaskView)
        {

            switch (createTaskView.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    return createTaskView.DueDate;
                case Data.Enum.PeriodType.Weekly:
                    return createTaskView.DueDate;
                case Data.Enum.PeriodType.Monthly:
                    return createTaskView.DueDate;
                case Data.Enum.PeriodType.SpecificDate:
                    return createTaskView.DueDate;
                default:
                    return "";
            }
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
        public AutoMapperProfile()
        {
            CreateMap<User, UserForRegisterDto>();
            CreateMap<UserForRegisterDto, User>()
                .ForMember(x => x.Role, option => option.Ignore())
                .ForMember(x => x.Email, option => option.Ignore())
                .ForMember(x => x.OCID, option => option.Ignore());

            CreateMap<Data.Models.Task, CreateTaskViewModel>()
                .ForMember(x => x.PIC, option => option.Ignore())
                .ForMember(x => x.DueDate, option => option.Ignore())
                .ForMember(x => x.Deputies, option => option.Ignore())
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.FromWhoID));

            CreateMap<CreateTaskViewModel, Data.Models.Task>()
                .ForMember(x => x.Deputies, option => option.Ignore())
                .ForMember(d => d.JobTypeID, s => s.MapFrom(p => CheckJobType(p)))
                .ForMember(d => d.DueDateDaily, s => s.MapFrom(p => CheckDuedate(p)))
                .ForMember(d => d.DueDateWeekly, s => s.MapFrom(p => CheckDuedate(p)))
                .ForMember(d => d.DueDateMonthly, s => s.MapFrom(p => CheckDuedate(p)))
                .ForMember(d => d.SpecificDate, s => s.MapFrom(p => CheckDuedate(p)))
                .ForMember(d => d.Priority, s => s.MapFrom(p => p.Priority.ToUpper()))
                .ForMember(d => d.ProjectID, s => s.MapFrom(p => p.ProjectID == 0 ? null : p.ProjectID))
                .ForMember(d => d.OCID, s => s.MapFrom(p => p.OCID == 0 ? null : p.OCID))
                .ForMember(x => x.OC, option => option.Ignore())
                .ForMember(x => x.Project, option => option.Ignore())
                .ForMember(x => x.Follows, option => option.Ignore())
                .ForMember(x => x.Tags, option => option.Ignore())
                .ForMember(x => x.Tutorial, option => option.Ignore());

            CreateMap<Data.Models.Task, TreeViewTask>()
                .ForMember(d => d.Project, s => s.MapFrom(p => p.Project == null ? new Project() : p.Project))
                .ForMember(d => d.Tutorial, s => s.MapFrom(p => p.Tutorial == null ? new TreeViewTutorial() : 
                new TreeViewTutorial 
                {
                    ID = p.Tutorial.ID,
                    Name = p.Tutorial.Name,
                    ParentID = p.Tutorial.ParentID,
                    Path =p.Tutorial.Path,
                    URL = p.Tutorial.URL,
                    Level= p.Tutorial.Level,
                    ProjectID = p.Tutorial.ProjectID,
                    TaskID = p.Tutorial.TaskID
                }))
                .ForMember(d => d.PriorityID, s => s.MapFrom(p => p.Priority))
                .ForMember(d => d.From, s => s.MapFrom(p => p.User.Username))
                .ForMember(d => d.User, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
                .ForMember(d => d.FromWho, s => s.MapFrom(p =>p.User == null? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
                .ForMember(d => d.FromWhere, s => s.MapFrom(p => p.OC == null ? new FromWhere() : new FromWhere {ID = p.OC.ID, Name = p.OC.Name }))
                .ForMember(d => d.state, s => s.MapFrom(p => p.Status == false ? "Undone" : "Done"))
                .ForMember(d => d.Follows, s => s.MapFrom(p => p.Follows))
                .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate.ToString("dd MMM, yyyy hh:mm:ss tt")))
                .ForMember(d => d.TaskCode, s => s.MapFrom(p => p.Code))
                .ForMember(d => d.ProjectName, s => s.MapFrom(p => p.Project == null ? "" : p.Project.Name))
                .ForMember(d => d.VideoLink, s => s.MapFrom(p => p.Tutorial == null ? "" : p.Tutorial.URL))
                .ForMember(d => d.VideoStatus, s => s.MapFrom(p => p.Tutorial == null ? false : true))
                .ForMember(d => d.Priority, s => s.MapFrom(p => CastPriority(p.Priority)))
                .ForMember(d => d.DueDateTime, s => s.MapFrom(p => MapDueDatTimeeWithPeriod(p)))
                .ForMember(d => d.DueDate, s => s.MapFrom(p => MapDueDateWithPeriod(p)))
                .ForMember(d => d.DeputiesList, s => s.MapFrom(p => p.Deputies.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username })))
                .ForMember(d => d.Deputies, s => s.MapFrom(p => p.Deputies.Select(x => x.UserID)))
                .ForMember(d => d.DeputyName, s => s.MapFrom(p => string.Join(",", p.Deputies.Select(x => x.User.Username))))
                .ForMember(d => d.PIC, s => s.MapFrom(p => p.Tags != null ? string.Join(",", p.Tags.Select(x => x.User.Username)) : ""))
                .ForMember(d => d.BeAssigneds, s => s.MapFrom(p => p.Tags != null ? p.Tags.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username }) : new List<BeAssigned>()))
                .ForMember(d => d.PICs, s => s.MapFrom(p => p.Tags != null ? p.Tags.Select(x=>x.UserID): new List<int>()));

            CreateMap<TreeViewTask, Data.Models.Task>();

            CreateMap<Follow, TreeViewTask>()
                .ForMember(d => d.DueDateDaily, s => s.MapFrom(p => p.Task.DueDateDaily))
                .ForMember(d => d.DueDateWeekly, s => s.MapFrom(p => p.Task.DueDateWeekly))
                .ForMember(d => d.DueDateMonthly, s => s.MapFrom(p => p.Task.DueDateMonthly))
                .ForMember(d => d.SpecificDate, s => s.MapFrom(p => p.Task.SpecificDate))
                .ForMember(d => d.periodType, s => s.MapFrom(p => p.Task.periodType))
                .ForMember(d => d.ModifyDateTime, s => s.MapFrom(p => p.Task.ModifyDateTime))
                .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.Task.CreatedDate.ToString("dd MMM, yyyy hh:mm:ss tt")))
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.Task.CreatedBy))
                .ForMember(d => d.FromWhoID, s => s.MapFrom(p => p.Task.FromWhoID))
                .ForMember(d => d.PriorityID, s => s.MapFrom(p => p.Task.Priority))
                .ForMember(d => d.From, s => s.MapFrom(p => p.User.Username))
                .ForMember(d => d.User, s => s.MapFrom(p => p.Task.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
                .ForMember(d => d.FromWho, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
                .ForMember(d => d.FromWhere, s => s.MapFrom(p => p.Task.OC == null ? new FromWhere() : new FromWhere { ID = p.Task.OC.ID, Name = p.Task.OC.Name }))
                .ForMember(d => d.state, s => s.MapFrom(p => p.Task.Status == false ? "Undone" : "Done"))
                .ForMember(d => d.Follows, s => s.MapFrom(p => p.Task.Follows))
                .ForMember(d => d.TaskCode, s => s.MapFrom(p => p.Task.Code))
                .ForMember(d => d.ProjectName, s => s.MapFrom(p => p.Task.Project == null ? "" : p.Task.Project.Name))
                .ForMember(d => d.VideoLink, s => s.MapFrom(p => p.Task.Tutorial == null ? "" : p.Task.Tutorial.URL))
                .ForMember(d => d.VideoStatus, s => s.MapFrom(p => p.Task.Tutorial == null ? false : true))
                .ForMember(d => d.Priority, s => s.MapFrom(p => CastPriority(p.Task.Priority)))
                .ForMember(d => d.DueDateTime, s => s.MapFrom(p => MapDueDatTimeeWithPeriod(p.Task)))
                .ForMember(d => d.DueDate, s => s.MapFrom(p => MapDueDateWithPeriod(p.Task)))
                .ForMember(d => d.SpecificDueDate, s => s.MapFrom(p => MapSpecificDueDateWithPeriod(p.Task)))
                .ForMember(d => d.DeputiesList, s => s.MapFrom(p => p.Task.Deputies.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username })))
                .ForMember(d => d.Deputies, s => s.MapFrom(p => p.Task.Deputies.Select(x => x.UserID)))
                .ForMember(d => d.DeputyName, s => s.MapFrom(p => string.Join(",", p.Task.Deputies.Select(x => x.User.Username))))
                .ForMember(d => d.PIC, s => s.MapFrom(p => string.Join(",", p.Task.Tags.Select(x => x.User.Username))))
                .ForMember(d => d.BeAssigneds, s => s.MapFrom(p => p.Task.Tags.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username })))
                .ForMember(d => d.PICs, s => s.MapFrom(p => p.Task.Tags.Select(x => x.UserID)));

            /*  ID = _.a.detail.ID,
                    Message = _.a.notify.Message,
                    Function = _.a.notify.Function,
                    CreatedBy = _.a.notify.UserID,
                    BeAssigned = _.a.detail.UserID,
                    Seen = _.a.detail.Seen,
                    URL = _.a.notify.URL,
                    Sender = _.b.Username,
                    ImageBase64 = _.b.ImageBase64,
                    CreatedTime = _.a.notify.CreatedTime,*/
            CreateMap<NotificationDetail, NotificationViewModel>()
                .ForMember(d => d.Message, s => s.MapFrom(p => p.Notification.Message))
                .ForMember(d => d.Function, s => s.MapFrom(p => p.Notification.Function))
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.Notification.UserID))
                .ForMember(d => d.BeAssigned, s => s.MapFrom(p => p.UserID))
                .ForMember(d => d.Seen, s => s.MapFrom(p => p.Seen))
                .ForMember(d => d.Sender, s => s.MapFrom(p => p.Notification.User != null ? p.Notification.User.ID : 0))
                .ForMember(d => d.ImageBase64, s => s.MapFrom(p => p.Notification.User != null ? p.Notification.User.ImageBase64 : new byte[] { }))
                .ForMember(d => d.CreatedTime, s => s.MapFrom(p => p.Notification.CreatedTime));
            CreateMap<User, UserViewModel>();

            CreateMap<UserViewModel, User>();

            CreateMap<Tutorial, TreeViewTutorial>();

            CreateMap<TreeViewTutorial, Tutorial>();

            CreateMap<OC, CreateOCViewModel>();

            CreateMap<CreateOCViewModel, OC>();

            CreateMap<Comment, CommentViewModel>();

            CreateMap<CommentViewModel, Comment>();

            CreateMap<Comment, AddCommentViewModel>().ForMember(d => d.ClientRouter, s => s.Ignore());

            CreateMap<AddCommentViewModel, Comment>();

            CreateMap<Project, ProjectViewModel>()
                .ForMember(d => d.Members, s => s.MapFrom(p => p.TeamMembers.Select(_=> _.UserID).ToList()))
                .ForMember(d => d.Manager, s => s.MapFrom(p => p.Managers.Select(_ => _.UserID).ToList()))
             .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate.ToString("d MMM, yyyy")));

            CreateMap<ProjectViewModel, Project>()
                .ForMember(x => x.Managers, option => option.Ignore())
                .ForMember(x => x.TeamMembers, option => option.Ignore());


            CreateMap<TreeViewOC, TreeModel>();
            CreateMap<TreeModel, TreeViewOC>()
                .ForMember(x => x.Level, option => option.Ignore())
                .ForMember(x => x.Name, option => option.Ignore());

            CreateMap<Data.Models.Task, CloneTaskViewModel>()
              .ForMember(x => x.IDTemp, option => option.MapFrom(x => x.ID))
              .ForMember(x => x.ParentTemp, option => option.MapFrom(x => x.ParentID));

            CreateMap<CloneTaskViewModel, Data.Models.Task> ()
          .ForMember(x => x.ID, option => option.Ignore());

            CreateMap<HierarchyNode<TreeViewTask>, TreeViewTask>();
            CreateMap<TreeViewTask, HierarchyNode <TreeViewTask>>()
                .ForMember(d => d.Entity, s => s.MapFrom(p => p));
            //CreateMap<UserAccount, UserModel>();
            //CreateMap<RegisterModel, UserAccount>();
            //CreateMap<UpdateModel, UserAccount>();
        }
    }
}
 