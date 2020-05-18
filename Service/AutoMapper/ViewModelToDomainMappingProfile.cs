
using AutoMapper;
using Data.Extensions;
using Data.Models;
using Data.ViewModel;
using Data.ViewModel.Comment;
using Data.ViewModel.OC;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Data.ViewModel.Tutorial;
using Data.ViewModel.User;
using Service.Dto;
using System;
using System.Collections.Generic;
using System.Text;


namespace Service.AutoMapper
{
    public class ViewModelToDomainMappingProfile : Profile
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
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<CreateTaskViewModel, Data.Models.Task>()
              .ForMember(x => x.Deputies, option => option.Ignore())
              .ForMember(d => d.JobTypeID, s => s.MapFrom(p => CheckJobType(p)))
              .ForMember(d => d.DueDateTime, s => s.MapFrom(p => p.DueDate))
              .ForMember(d => d.Priority, s => s.MapFrom(p => p.Priority.ToUpper()))
              .ForMember(d => d.ProjectID, s => s.MapFrom(p => p.ProjectID == 0 ? null : p.ProjectID))
              .ForMember(d => d.OCID, s => s.MapFrom(p => p.OCID == 0 ? null : p.OCID))
              .ForMember(x => x.OC, option => option.Ignore())
              .ForMember(x => x.Project, option => option.Ignore())
              .ForMember(x => x.Follows, option => option.Ignore())
              .ForMember(x => x.Tags, option => option.Ignore())
              .ForMember(x => x.User, option => option.Ignore())
              .ForMember(x => x.Tutorial, option => option.Ignore());

            CreateMap<TreeViewTask, Data.Models.Task>();

            CreateMap<UserForRegisterDto, User>()
             .ForMember(x => x.Role, option => option.Ignore())
             .ForMember(x => x.Email, option => option.Ignore())
             .ForMember(x => x.OCID, option => option.Ignore());

            CreateMap<UserViewModel, User>();

            CreateMap<TreeViewTutorial, Tutorial>();

            CreateMap<CreateOCViewModel, OC>();
            CreateMap<CommentViewModel, Comment>();
            CreateMap<AddCommentViewModel, Comment>();
            CreateMap<ProjectViewModel, Project>()
             .ForMember(x => x.Managers, option => option.Ignore())
             .ForMember(x => x.TeamMembers, option => option.Ignore());

            CreateMap<TreeViewOC, TreeModel>();
            CreateMap<TreeModel, TreeViewOC>()
                .ForMember(x => x.Level, option => option.Ignore())
                .ForMember(x => x.Name, option => option.Ignore());

            CreateMap<CloneTaskViewModel, Data.Models.Task>()
          .ForMember(x => x.ID, option => option.Ignore());
            CreateMap<TreeViewTask, HierarchyNode<TreeViewTask>>()
           .ForMember(d => d.Entity, s => s.MapFrom(p => p));

            CreateMap<HierarchyNode<TreeViewTask>, TreeViewTask>();

            CreateMap<HierarchyNode<TreeViewTaskForHistory>, TreeViewTaskForHistory>();
            CreateMap<TreeViewTaskForHistory, HierarchyNode<TreeViewTaskForHistory>>()
                .ForMember(d => d.Entity, s => s.MapFrom(p => p));
        }


    }
}
