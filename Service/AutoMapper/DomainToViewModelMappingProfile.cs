using AutoMapper;
using AutoMapper.Configuration;
using Data.Extensions;
using Data.Models;
using Data.ViewModel.Comment;
using Data.ViewModel.Notification;
using Data.ViewModel.OC;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Data.ViewModel.Tutorial;
using Data.ViewModel.User;
using Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        private string CheckStatus(Data.Models.Task task)
        {
            string result = "#N/A";
            var currentDate = DateTime.Now;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    result = currentDate.CompareTo(task.DueDateTime) > 0 ? "Delay" : "On going";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    result = currentDate.CompareTo(task.DueDateTime) > 0 ? "Delay" : "On going";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    result = currentDate.CompareTo(task.DueDateTime) > 0 ? "Delay" : "On going";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    result = currentDate.CompareTo(task.DueDateTime) > 0 ? "Delay" : "On going";
                    break;
                default:
                    break;
            }
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
        private string CheckStatusForHistory(Data.Models.Task task)
        {
            string result = "#N/A";
            var currentDate = DateTime.Now;
            switch (task.periodType)
            {
                case Data.Enum.PeriodType.Daily:
                    result = currentDate.CompareTo(task.DueDateTime) <= 0 ? "On time" : "Late";
                    break;
                case Data.Enum.PeriodType.Weekly:
                    result = currentDate.CompareTo(task.DueDateTime) <= 0 ? "On time" : "Late";
                    break;
                case Data.Enum.PeriodType.Monthly:
                    result = currentDate.CompareTo(task.DueDateTime) <= 0 ? "On time" : "Late";
                    break;
                case Data.Enum.PeriodType.SpecificDate:
                    result = currentDate.CompareTo(task.DueDateTime) <= 0 ? "On time" : "Late";
                    break;
                default:
                    break;
            }
            return result;
        }
        public DomainToViewModelMappingProfile()
        {
            CreateMap<User, UserForRegisterDto>();

            CreateMap<Data.Models.Task, CreateTaskViewModel>()
                .ForMember(x => x.PIC, option => option.Ignore())
                .ForMember(x => x.DueDate, option => option.Ignore())
                .ForMember(x => x.Deputies, option => option.Ignore())
                .ForMember(x => x.FromWhoID, s => s.MapFrom(p => p.FromWhoID))
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.CreatedBy));

            CreateMap<Data.Models.Task, TreeViewTask>()
           .ForMember(d => d.Project, s => s.MapFrom(p => p.Project == null ? new Project() : p.Project))
           .ForMember(d => d.Tutorial, s => s.MapFrom(p => p.Tutorial == null ? new TreeViewTutorial() :
           new TreeViewTutorial
           {
               ID = p.Tutorial.ID,
               Name = p.Tutorial.Name,
               ParentID = p.Tutorial.ParentID,
               Path = p.Tutorial.Path,
               URL = p.Tutorial.URL,
               Level = p.Tutorial.Level,
               ProjectID = p.Tutorial.ProjectID,
               TaskID = p.Tutorial.TaskID
           }))
           .ForMember(d => d.ProjectID, s => s.MapFrom(p => p.ProjectID == null ? 0 : p.ProjectID))
           .ForMember(d => d.PriorityID, s => s.MapFrom(p => p.Priority))
           .ForMember(d => d.From, s => s.MapFrom(p => p.User.Username))
           .ForMember(d => d.Follows, s => s.MapFrom(p => p.Follows))
           .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate))
           .ForMember(d => d.TaskCode, s => s.MapFrom(p => p.Code))
           .ForMember(d => d.ProjectName, s => s.MapFrom(p => p.Project == null ? p.JobTypeID.ToString() : p.Project.Name))
           .ForMember(d => d.VideoLink, s => s.MapFrom(p => p.Tutorial == null ? "#N/A" : p.Tutorial.URL))
           .ForMember(d => d.VideoStatus, s => s.MapFrom(p => p.Tutorial == null ? false : true))
           .ForMember(d => d.Priority, s => s.MapFrom(p => p.Priority == "H" ? "High" : p.Priority == "L" ? "Low" : "Medium"))
           .ForMember(d => d.DueDate, s => s.MapFrom(p => p.DueDateTime))
           .ForMember(d => d.state, s => s.MapFrom(p => DateTime.Now.CompareTo(p.DueDateTime) <= 0 ? "On going" : "Delay"))
           .ForMember(d => d.User, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
           .ForMember(d => d.FromWho, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
           .ForMember(d => d.FromWhere, s => s.MapFrom(p => p.OC == null ? new FromWhere() : new FromWhere { ID = p.OC.ID, Name = p.OC.Name }))

           .ForMember(d => d.DeputiesList, s => s.MapFrom(p => p.Deputies.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username })))
           .ForMember(d => d.Deputies, s => s.MapFrom(p => p.Deputies.Select(x => x.UserID).ToList()))
           .ForMember(d => d.DeputyName, s => s.MapFrom(p => p.Deputies.Any() == true ? string.Join(",", p.Deputies.Select(x => x.User.Username)) : "#N/A"))
           .ForMember(d => d.PIC, s => s.MapFrom(p => p.Tags.Any() == true ? string.Join(",", p.Tags.Select(x => x.User.Username)) : "#N/A"))
           .ForMember(d => d.BeAssigneds, s => s.MapFrom(p => p.Tags.Any() == true ? p.Tags.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username }) : new List<BeAssigned>()))
           .ForMember(d => d.PICs, s => s.MapFrom(p => p.Tags.Any() == true ? p.Tags.Select(x => x.UserID).ToList() : new List<int>()));

            CreateMap<Data.Models.Task, TreeViewTaskForHistory>()
            .ForMember(d => d.Project, s => s.MapFrom(p => p.Project == null ? new Project() : p.Project))
            .ForMember(d => d.Tutorial, s => s.MapFrom(p => p.Tutorial == null ? new TreeViewTutorial() :
            new TreeViewTutorial
            {
                ID = p.Tutorial.ID,
                Name = p.Tutorial.Name,
                ParentID = p.Tutorial.ParentID,
                Path = p.Tutorial.Path,
                URL = p.Tutorial.URL,
                Level = p.Tutorial.Level,
                ProjectID = p.Tutorial.ProjectID,
                TaskID = p.Tutorial.TaskID
            }))
            .ForMember(d => d.PriorityID, s => s.MapFrom(p => p.Priority))
            .ForMember(d => d.From, s => s.MapFrom(p => p.User.Username))
            .ForMember(d => d.User, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
            .ForMember(d => d.FromWho, s => s.MapFrom(p => p.User == null ? new BeAssigned() : new BeAssigned { ID = p.User.ID, Username = p.User.Username }))
            .ForMember(d => d.FromWhere, s => s.MapFrom(p => p.OC == null ? new FromWhere() : new FromWhere { ID = p.OC.ID, Name = p.OC.Name }))
            .ForMember(d => d.state, s => s.MapFrom(p => CheckStatusForHistory(p)))
            .ForMember(d => d.Follows, s => s.MapFrom(p => p.Follows))
            .ForMember(d => d.FinishedDateTime, s => s.MapFrom(p => p.ModifyDateTime.ToParseStringDateTime()))
            .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate))
            .ForMember(d => d.TaskCode, s => s.MapFrom(p => p.Code))
            .ForMember(d => d.ProjectName, s => s.MapFrom(p => p.Project == null ? p.JobTypeID.ToString() : p.Project.Name))
            .ForMember(d => d.VideoLink, s => s.MapFrom(p => p.Tutorial == null ? "#N/A" : p.Tutorial.URL))
            .ForMember(d => d.VideoStatus, s => s.MapFrom(p => p.Tutorial == null ? false : true))
            .ForMember(d => d.Priority, s => s.MapFrom(p => CastPriority(p.Priority)))
            .ForMember(d => d.DueDate, s => s.MapFrom(p => p.DueDateTime))
            .ForMember(d => d.DeputiesList, s => s.MapFrom(p => p.Deputies.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username })))
            .ForMember(d => d.Deputies, s => s.MapFrom(p => p.Deputies.Select(x => x.UserID)))
            .ForMember(d => d.DeputyName, s => s.MapFrom(p => p.Deputies.Any() == true ? string.Join(",", p.Deputies.Select(x => x.User.Username)) : "#N/A"))
            .ForMember(d => d.PIC, s => s.MapFrom(p => p.Tags.Any() == true ? string.Join(",", p.Tags.Select(x => x.User.Username)) : "#N/A"))
            .ForMember(d => d.BeAssigneds, s => s.MapFrom(p => p.Tags.Any() == true ? p.Tags.Select(x => new BeAssigned { ID = x.UserID, Username = x.User.Username }) : new List<BeAssigned>()))
            .ForMember(d => d.PICs, s => s.MapFrom(p => p.Tags.Any() == true ? p.Tags.Select(x => x.UserID) : new List<int>()));

            CreateMap<User, UserViewModel>();
            CreateMap<Tutorial, TreeViewTutorial>();
            CreateMap<OC, CreateOCViewModel>();
            CreateMap<Comment, CommentViewModel>();
            CreateMap<Comment, AddCommentViewModel>().ForMember(d => d.ClientRouter, s => s.Ignore());

            CreateMap<Project, ProjectViewModel>()
               .ForMember(d => d.Members, s => s.MapFrom(p => p.TeamMembers.Select(_ => _.UserID).ToList()))
               .ForMember(d => d.Manager, s => s.MapFrom(p => p.Managers.Select(_ => _.UserID).ToList()))
            .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate.ToString("d MMM, yyyy")));

            CreateMap<Data.Models.Task, CloneTaskViewModel>()
            .ForMember(x => x.IDTemp, option => option.MapFrom(x => x.ID))
            .ForMember(x => x.ParentTemp, option => option.MapFrom(x => x.ParentID));

            CreateMap<NotificationDetail, NotificationViewModel>()
                .ForMember(d => d.Message, s => s.MapFrom(p => p.Notification.Message))
                .ForMember(d => d.Function, s => s.MapFrom(p => p.Notification.Function))
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.Notification.UserID))
                .ForMember(d => d.BeAssigned, s => s.MapFrom(p => p.UserID))
                .ForMember(d => d.URL, s => s.MapFrom(p => p.Notification.URL))
                .ForMember(d => d.Seen, s => s.MapFrom(p => p.Seen))
                .ForMember(d => d.Sender, s => s.MapFrom(p => p.Notification.User != null ? p.Notification.User.Username : "From System"))
                .ForMember(d => d.ImageBase64, s => s.MapFrom(p => "avatar/logo-1.png"))
                .ForMember(d => d.CreatedTime, s => s.MapFrom(p => p.Notification.CreatedTime));
        }
    }
}
