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
using System.Globalization;
using System.Linq;
using WorkManagement.Dtos;

namespace WorkManagement.Helpers
{
    public class AutoMapperProfile : Profile
    {
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
                .ForMember(d => d.CreatedBy, s => s.MapFrom(p => p.FromWhoID));

            CreateMap<CreateTaskViewModel, Data.Models.Task>();
                
            CreateMap<Data.Models.Task, TreeViewTask>();

            CreateMap<TreeViewTask, Data.Models.Task>();


            CreateMap<User, UserViewModel>();

            CreateMap<UserViewModel, User>();

            CreateMap<Tutorial, TreeViewTutorial>();

            CreateMap<TreeViewTutorial, Tutorial>();

            CreateMap<OC, CreateOCViewModel>();

            CreateMap<CreateOCViewModel, OC>();

            CreateMap<Comment, CommentViewModel>();

            CreateMap<CommentViewModel, Comment>();

            CreateMap<Project, ProjectViewModel>()
                .ForMember(d => d.Members, s => s.MapFrom(p => p.TeamMembers.Select(_=> _.UserID).ToList()))
                .ForMember(d => d.Manager, s => s.MapFrom(p => p.Managers.Select(_ => _.UserID).ToList()))
             .ForMember(d => d.CreatedDate, s => s.MapFrom(p => p.CreatedDate.ToString("MMM d, yyyy")));

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