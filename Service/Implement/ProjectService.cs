using AutoMapper;
using Data;
using Data.Models;
using Data.ViewModel.Notification;
using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Data.ViewModel.User;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class ProjectService : IProjectService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ProjectService(DataContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;

            _notificationService = notificationService;
        }
        public IEnumerable<TreeViewTask> GetListTree(List<Data.Models.Task> listTasks)
        {
            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var levelItem = new TreeViewTask();
                levelItem.ID = item.ID;
                levelItem.Level = item.Level;
                levelItem.ParentID = item.ParentID;
                tasks.Add(levelItem);
            }

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ParentID == 0)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToHashSet();

            return hierarchy;
        }
        public async Task<IEnumerable<TreeViewTask>> GetListTree(int parentID, int id)
        {
            var listTasks = await _context.Tasks
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               //.Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();

            var tasks = new HashSet<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _context.Tags.Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();

                var levelItem = new TreeViewTask();
                levelItem.ID = item.ID;
                //levelItem.PIC = string.Join(" , ", _context.Tags.Where(x => x.TaskID == item.ID).Include(x => x.User).Select(x => x.User.Username).ToArray());
                //levelItem.ProjectName = item.ProjectID == 0 ? "#N/A" : _context.Projects.Find(item.ProjectID).Name;
                //levelItem.ProjectID = item.ProjectID;
                //levelItem.Deadline = String.Format("{0:s}", item.DueDate);
                //levelItem.BeAssigneds = beAssigneds;
                levelItem.Level = item.Level;

                levelItem.ParentID = item.ParentID;
                //levelItem.Priority = CastPriority(item.Priority);
                //levelItem.PriorityID = item.Priority;
                //levelItem.Description = item.Description;
                //levelItem.DueDate = String.Format("{0:MMM d, yyyy}", item.DueDate);
                //levelItem.CreatedDate = String.Format("{0:MMM d, yyyy}", item.CreatedDate);
                //levelItem.User = item.User;
                //levelItem.FromWhere = _context.OCs.Where(x => x.ID == item.OCID).Select(x => new FromWhere { ID = x.ID, Name = x.Name }).FirstOrDefault();

                //levelItem.FromWho = _context.Users.Where(x => x.ID == item.FromWhoID).Select(x => new BeAssigned { ID = x.ID, Username = x.Username }).FirstOrDefault();
                //levelItem.JobName = item.JobName.IsNotAvailable();
                //levelItem.state = item.Status == false ? "Undone" : "Done";
                //levelItem.Remark = item.Remark.IsNotAvailable();

                //levelItem.From = item.OCID > 0 ? _context.OCs.Find(item.OCID).Name : _context.Users.Find(item.FromWhoID).Username.IsNotAvailable();

                tasks.Add(levelItem);
            }

            HashSet<TreeViewTask> hierarchy = new HashSet<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
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
        public HashSet<TreeViewTask> GetChildren(HashSet<TreeViewTask> tasks, int parentid)
        {
            return tasks
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new TreeViewTask()
                    {
                        ID = c.ID,
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
                        PICs = c.PICs,
                        JobTypeID = c.JobTypeID,
                        FromWho = c.FromWho,
                        FromWhere = c.FromWhere,
                        BeAssigneds = c.BeAssigneds,
                        Deputies = c.Deputies,
                        DeputiesList = c.DeputiesList,
                        DueDateDaily = c.DueDateDaily,
                        DueDateWeekly = c.DueDateWeekly,
                        DueDateMonthly = c.DueDateMonthly,
                        SpecificDate = c.SpecificDate,
                        DeputyName = c.DeputyName,
                        periodType = c.periodType,
                        children = GetChildren(tasks, c.ID)
                    })
                    .OrderByDescending(x => x.ID)
                    .ToHashSet();
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
        public async Task<bool> Create(Project entity)
        {
            try
            {
                entity.CreatedByName = (await _context.Users.FindAsync(entity.CreatedBy)).Username ?? "";
                await _context.Projects.AddAsync(entity);
                await _context.SaveChangesAsync();

                var room = new Room
                {
                    Name = entity.Name,
                    ProjectID = entity.ID
                };
                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();

                var update = await _context.Projects.FirstOrDefaultAsync(x => x.ID.Equals(room.ProjectID));
                update.Room = room.ID;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        private async Task<Project> CreateForClone(Project entity)
        {
            try
            {
                entity.ID = 0;
                entity.Name = entity.Name + "(clone)";
                entity.CreatedByName = (await _context.Users.FindAsync(entity.CreatedBy)).Username ?? "";
                await _context.Projects.AddAsync(entity);
                await _context.SaveChangesAsync();

                var room = new Room
                {
                    Name = entity.Name,
                    ProjectID = entity.ID
                };
                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();

                var update = await _context.Projects.FirstOrDefaultAsync(x => x.ID.Equals(room.ProjectID));
                update.Room = room.ID;
                await _context.SaveChangesAsync();

                return entity;
            }
            catch (Exception)
            {
                return entity;

            }
        }
        public async Task<bool> Delete(int id)
        {
            try
            {

                var entity = await _context.Projects.FindAsync(id);

                if (entity == null)
                {
                    return false;
                }
                _context.Rooms.Remove(await _context.Rooms.FirstOrDefaultAsync(_ => _.ProjectID == id));
                _context.Managers.RemoveRange(await _context.Managers.Where(_ => _.ProjectID == id).ToListAsync());
                _context.TeamMembers.RemoveRange(await _context.TeamMembers.Where(_ => _.ProjectID == id).ToListAsync());

                var listTask = await _context.Tasks.Where(_ => _.ProjectID == id).ToListAsync();
                _context.Tags.RemoveRange(await _context.Tags.Where(_ => listTask.Select(x => x.ID).Contains(_.TaskID)).ToListAsync());
                _context.Tasks.RemoveRange(listTask);
                _context.Follows.RemoveRange(await _context.Follows.Where(_ => listTask.Select(x => x.ID).Contains(_.TaskID)).ToListAsync());

                _context.Projects.Remove(entity);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<object> Clone(int projectId)
        {
            try
            {

                var entity = await _context.Projects.FindAsync(projectId);
                var project = await CreateForClone(entity);
                if (entity == null)
                {
                    return false;
                }
                var manager = await _context.Managers.Where(_ => _.ProjectID == projectId).ToListAsync();
                manager.ForEach(item =>
                {
                    item.ProjectID = project.ID;
                });
                var member = await _context.TeamMembers.Where(_ => _.ProjectID == projectId).ToListAsync();
                member.ForEach(item =>
                {
                    item.ProjectID = project.ID;
                });

                await _context.AddRangeAsync(manager);
                await _context.AddRangeAsync(member);

                var listTask = await _context.Tasks.Where(_ => _.ProjectID == projectId).ToListAsync();

                var tasks = GetListTree(listTask);
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
                var tasksForClone = await _context.Tasks.Where(x => arrTasks.Contains(x.ID)).ToListAsync();

                await CloneTask(tasksForClone, project.ID);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async System.Threading.Tasks.Task CloneTask(List<Data.Models.Task> tasks, int projectId)
        {
            int parentID = 0;
            foreach (var item in tasks)
            {
                if (item.JobTypeID == Data.Enum.JobType.Project)
                {

                    if (item.Level == 1)
                    {
                        parentID = 0;
                    }
                    var itemAdd = new Data.Models.Task
                    {
                        JobName = item.JobName,
                        JobTypeID = item.JobTypeID,
                        periodType = item.periodType,
                        OCID = item.OCID,
                        CreatedBy = item.CreatedBy,
                        FromWhoID = item.FromWhoID,
                        Priority = item.Priority,
                        Level = item.Level,
                        ProjectID = projectId,
                        ParentID = parentID

                    };
                    itemAdd.CreatedDate = DateTime.Now.Date.AddDays(1);
                    await _context.AddAsync(itemAdd);
                    await _context.SaveChangesAsync();
                    parentID = itemAdd.ID;
                }

            }
        }
        public async Task<List<Project>> GetAll()
        {
            return await _context.Projects.ToListAsync();
        }
        public async Task<List<ProjectViewModel>> GetListProject()
        {
            return _mapper.Map<List<ProjectViewModel>>(await _context.Projects.ToListAsync());
        }

        public async Task<PagedList<ProjectViewModel>> GetAllPaging(int userid, int page, int pageSize, string keyword)
        {
            var source2 = await _context.Projects
              .Include(x => x.User)
              .Include(x => x.TeamMembers)
              .ThenInclude(x => x.User)
              .Include(x => x.Managers)
              .ThenInclude(x => x.User)
              .OrderByDescending(x => x.ID).ToListAsync();
            var source = await _context.Projects
                .Include(x => x.User)
                .Include(x => x.TeamMembers)
                .ThenInclude(x => x.User)
                .Include(x => x.Managers)
                .ThenInclude(x => x.User)
                .OrderByDescending(x => x.ID)
                  .Where(_ => _.Managers.Select(a => a.UserID).Contains(userid)
                || _.TeamMembers.Select(a => a.UserID).Contains(userid)
                || _.CreatedBy == userid)
                .Select(x => new ProjectViewModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    CreatedByName = x.User.Username,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate.ToString("dd MMM, yyyy"),
                    Room = x.Room,
                    Status = x.Status,
                    Members = x.TeamMembers.Select(a => a.UserID).ToList(),
                    Manager = x.Managers.Select(a => a.UserID).ToList(),
                })
                .ToListAsync();
            if (!keyword.IsNullOrEmpty())
            {
                source = source.Where(x => x.Name.ToLower().Contains(keyword.Trim().ToLower()) || x.CreatedByName.ToLower().Contains(keyword.Trim().ToLower())).ToList();
            }
            return PagedList<ProjectViewModel>.Create(source, page, pageSize);
        }

        public async Task<Project> GetByID(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<bool> Update(Project entity)
        {
            var item = await _context.Projects.FindAsync(entity.ID);
            item.Name = entity.Name;
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

        public async Task<Tuple<bool, string>> AddManager(AddManagerViewModel addManager)
        {
            try
            {
                var listUsers = new List<int>();
                if (addManager.Users.Length > 0)
                {
                    //get old manager list
                    var oldManagers = await _context.Managers.Where(x => x.ProjectID == addManager.ProjectID).Select(x => x.UserID).ToArrayAsync();
                    //new manager list from client
                    var newManagers = addManager.Users;
                    //get value of old managers list without value in new manager list
                    var withOutInOldManagers = newManagers.Except(oldManagers).ToArray();
                    if (withOutInOldManagers.Length > 0)
                    {
                        var managers = new List<Manager>();
                        foreach (var pic in withOutInOldManagers)
                        {
                            managers.Add(new Manager
                            {
                                UserID = pic,
                                ProjectID = addManager.ProjectID
                            });
                        }
                        await _context.Managers.AddRangeAsync(managers);
                        var project = await _context.Projects.FindAsync(addManager.ProjectID);
                        var user = await _context.Users.FindAsync(addManager.UserID);
                        string urlResult = $"/project/detail/{project.ID}";
                        var message = $"The {user.Username.ToTitleCase()} account has assigned you as manager of {project.Name} project";
                        await _notificationService.Create(new CreateNotifyParams
                        {
                            AlertType = Data.Enum.AlertType.Manager,
                            Message = message,
                            Users = withOutInOldManagers.Distinct().ToList(),
                            URL = urlResult,
                            UserID = addManager.UserID
                        });
                        listUsers.AddRange(withOutInOldManagers);
                    }
                    else
                    {
                        //Day la userID se bi xoa
                        var withOutInNewManagers = oldManagers.Where(x => !newManagers.Contains(x)).ToArray();
                        var listDeleteManagers = await _context.Managers.Where(x => withOutInNewManagers.Contains(x.UserID) && x.ProjectID.Equals(addManager.ProjectID)).ToListAsync();
                        _context.Managers.RemoveRange(listDeleteManagers);
                    }
                }
                await _context.SaveChangesAsync();
                return Tuple.Create(true, string.Join(",", listUsers.Distinct().ToArray()));
            }
            catch (Exception)
            {
                return Tuple.Create(false, "");

            }
        }

        public async Task<Tuple<bool, string>> AddMember(AddMemberViewModel addMember)
        {
            try
            {
                var listUsers = new List<int>();

                if (addMember.Users.Length > 0)
                {
                    //get old member list
                    var oldMembers = await _context.TeamMembers.Where(x => x.ProjectID == addMember.ProjectID).Select(x => x.UserID).ToArrayAsync();
                    //new member list from client
                    var newMembers = addMember.Users;
                    //get value of old members list without value in new member list
                    var withOutInOldMembers = newMembers.Except(oldMembers).ToArray();
                    if (withOutInOldMembers.Length > 0)
                    {
                        var members = new List<TeamMember>();
                        foreach (var pic in withOutInOldMembers)
                        {
                            members.Add(new TeamMember
                            {
                                UserID = pic,
                                ProjectID = addMember.ProjectID
                            });
                        }
                        await _context.TeamMembers.AddRangeAsync(members);
                        var project = await _context.Projects.FindAsync(addMember.ProjectID);
                        var user = await _context.Users.FindAsync(addMember.UserID);
                        string urlResult = $"/project-detail/{project.ID}";
                        var message = $"The {user.Username.ToTitleCase()} account has assigned you as member of {project.Name} project";
                        await _notificationService.Create(new CreateNotifyParams
                        {
                            AlertType = Data.Enum.AlertType.Member,
                            Message = message,
                            Users = withOutInOldMembers.Distinct().ToList(),
                            URL = urlResult,
                            UserID = addMember.UserID
                        });
                        listUsers.AddRange(withOutInOldMembers);
                    }
                    else
                    {
                        //Day la userID se bi xoa
                        var withOutInNewMembers = oldMembers.Where(x => !newMembers.Contains(x)).ToArray();
                        var listDeleteMembers = await _context.TeamMembers.Where(x => withOutInNewMembers.Contains(x.UserID) && x.ProjectID.Equals(addMember.ProjectID)).ToListAsync();
                        _context.TeamMembers.RemoveRange(listDeleteMembers);
                    }
                }

                await _context.SaveChangesAsync();
                return Tuple.Create(true, string.Join(",", listUsers.Distinct().ToArray()));
            }
            catch (Exception)
            {
                return Tuple.Create(false, "");
            }
        }

        public async Task<object> GetUsers()
        {
            return await _context.Users.Where(x => x.RoleID != 1).Select(x => new { x.ID, x.Username }).ToListAsync();
        }

        public async Task<object> GetUserByProjectID(int id)
        {
            try
            {
                var item = await _context.Managers.Include(x => x.Project).Include(x => x.User).FirstOrDefaultAsync(x => x.ProjectID == id);
                return new
                {
                    status = true,
                    room = item.Project.Room,
                    title = item.Project.Name,
                    createdBy = item.Project.CreatedBy,
                    selectedManager = await _context.Managers.Include(x => x.User).Where(x => x.ProjectID == id).Select(x => new { ID = x.User.ID, Username = x.User.Username }).ToArrayAsync(),
                    selectedMember = await _context.TeamMembers.Include(x => x.User).Where(x => x.ProjectID == id).Select(x => new { ID = x.User.ID, Username = x.User.Username }).ToArrayAsync(),
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                };
            }

        }

        public async Task<object> GetProjects(int userid, int page, int pageSize, string projectName)
        {
            var members = _context.TeamMembers.Where(_ => _.UserID == userid).Select(x => x.ProjectID).ToArray();

            var model = await _context.Projects
                .Include(x => x.TeamMembers)
                .ThenInclude(x => x.User)
                .Include(x => x.Managers)
                .ThenInclude(x => x.User).Select(x => new
                {
                    x.ID,
                    x.Name,
                    Manager = x.TeamMembers.Select(a => a.User.Username).ToArray(),
                    ManagerID = x.TeamMembers.Select(a => a.User.ID).ToArray(),
                    Members = x.TeamMembers.Select(a => a.User.Username).ToArray(),
                    MemberIDs = x.TeamMembers.Select(a => a.User.ID).ToArray(),
                    x.CreatedBy
                }).ToListAsync();
            model = model.Where(_ => _.ManagerID.Contains(userid) || _.MemberIDs.Contains(userid) || _.CreatedBy == userid).ToList();
            if (!projectName.IsNullOrEmpty())
            {
                projectName = projectName.Trim().ToLower();
                model = model.Where(x => x.Name.ToLower().Contains(projectName)).ToList();
            }
            var totalCount = model.Count();
            model = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new
            {
                project = model,
                data = model,
                total = (int)Math.Ceiling(totalCount / (double)pageSize),
                totalPage = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public Task<object> GetProjects()
        {
            throw new NotImplementedException();
        }
        public async Task<object> ProjectDetail(int projectID)
        {
            var item = await _context.Projects.FindAsync(projectID);
            return item;
        }
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

        public async Task<object> Open(int projectId)
        {
            var model = await _context.Projects.FindAsync(projectId);
            if (model == null)
                return new
                {
                    status = false
                };
            model.Status = !model.Status;
            await _context.SaveChangesAsync();
            return new
            {
                status = true,
                result = model.Status
            };
        }
    }
}
