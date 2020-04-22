using Data;
using Data.Models;
using Data.ViewModel.Comment;
using Data.ViewModel.Notification;
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
    public class CommentService : ICommentService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notificationService;

        public CommentService(DataContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private async Task<Tuple<List<int>, string, string>> AlertComment(int taskid, int userid)
        {
            var task = await _context.Tasks.FindAsync(taskid);
            var user = await _context.Users.FindAsync(userid);
            var pics = await _context.Tags.Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
            string projectName = string.Empty;
            if (task.ProjectID > 0)
                projectName = (await _context.Projects.FindAsync(task.ProjectID)).Name;
            string message = string.Empty;
            string urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
            switch (task.JobTypeID)
            {
                case Data.Enum.JobType.Project:
                    message = $"{user.Username.ToTitleCase()} commented on your task' {task.JobName}' of {projectName}.";
                    break;
                case Data.Enum.JobType.Abnormal:
                case Data.Enum.JobType.Routine:
                    message = $"{user.Username.ToTitleCase()} commented on your task '{task.JobName}'.";
                    break;
                default:
                    break;
            }
            return Tuple.Create(pics, message, urlResult);
        }
        private async Task<Tuple<List<int>, string, string>> AlertReplyComment(int taskid, int userid, string comment)
        {
            var task = await _context.Tasks.FindAsync(taskid);
            var user = await _context.Users.FindAsync(userid);
            var pics = await _context.Tags.Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
            string projectName = string.Empty;
            if (task.ProjectID > 0)
                projectName = (await _context.Projects.FindAsync(task.ProjectID)).Name;
            string message = string.Empty;
            string urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
            switch (task.JobTypeID)
            {
                case Data.Enum.JobType.Project:
                    message = $"{user.Username.ToTitleCase()} replied to the comment: '{comment}'.";
                    break;
                case Data.Enum.JobType.Abnormal:
                case Data.Enum.JobType.Routine:
                    message = $"{user.Username.ToTitleCase()} replied to the comment: '{comment}'.";
                    break;
                default:
                    break;
            }
            return Tuple.Create(pics, message, urlResult);
        }
        public async Task<Tuple<bool, string>> Add(Comment comment, int currentUser)
        {
            try
            {
                comment.Level = 1;
                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
                await _context.AddAsync(new CommentDetail { CommentID = comment.ID, UserID = comment.UserID, Seen = true });
                await _context.SaveChangesAsync();
                var alert = await AlertComment(comment.TaskID, comment.UserID);
                var task = await _context.Tasks.FindAsync(comment.TaskID);
                if (!currentUser.Equals(task.CreatedBy))
                {
                    alert.Item1.Add(task.CreatedBy);
                    var listUsers = alert.Item1.Where(x => x != currentUser).Distinct().ToList();
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.PostComment,
                        Message = alert.Item2,
                        Users = listUsers,
                        TaskID = comment.TaskID,
                        URL = alert.Item3,
                        UserID = comment.UserID
                    });
                    return Tuple.Create(true, string.Join(",", listUsers.ToArray()));
                }
                else
                {
                    return Tuple.Create(true, string.Empty);
                }

            }
            catch (Exception)
            {
                return Tuple.Create(false, string.Empty);
            }
        }
        public async Task<Tuple<bool, string>> AddSub(AddSubViewModel subcomment)
        {
            try
            {
                var item = _context.Comments.Find(subcomment.ParentID);
                var comment = new Comment
                {
                    ParentID = subcomment.ParentID,
                    TaskID = subcomment.TaskID,
                    UserID = subcomment.UserID,
                    Content = subcomment.Content,
                    Level = item.Level + 1
                };

                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
                await _context.AddAsync(new CommentDetail { CommentID = comment.ID, UserID = comment.UserID, Seen = true });
                await _context.SaveChangesAsync();

                var comtParent = await _context.Comments.FindAsync(subcomment.ParentID);
                //Neu tra loi chinh binh luan cua minh thi khong 
                if (subcomment.CurrentUser.Equals(comtParent.UserID))
                    return Tuple.Create(true, string.Empty);
                else
                {
                    var alert = await AlertReplyComment(comment.TaskID, comment.UserID, comtParent.Content);
                    alert.Item1.Add(comtParent.UserID);
                    var listUsers = alert.Item1.Where(x => x != subcomment.CurrentUser).Distinct().ToList();
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = Data.Enum.AlertType.ReplyComment,
                        Message = alert.Item2,
                        Users = listUsers,
                        TaskID = comment.TaskID,
                        URL = alert.Item3,
                        UserID = comment.UserID
                    });
                    return Tuple.Create(true, string.Join(",", listUsers.ToArray()));
                }
            }
            catch (Exception)
            {
                return Tuple.Create(false, string.Empty);

            }
        }
        private async Task<List<CommentTreeView>> GetAll(int taskID, int userID)
        {
            var detail = _context.CommentDetails;
            return await _context.Comments
                .Join(_context.Users,
                comt => comt.UserID,
                user => user.ID,
                (comt, user) => new { comt, user })
                .Where(x => x.comt.TaskID.Equals(taskID))
                .Select(_ => new CommentTreeView
                {
                    ID = _.comt.ID,
                    UserID = _.comt.UserID,
                    Username = _.user.Username,
                    ImageBase64 = _.user.ImageBase64,
                    Content = _.comt.Content,
                    Level = _.comt.Level,
                    ParentID = _.comt.ParentID,
                    CreatedTime = _.comt.CreatedTime,
                    Seen = detail.FirstOrDefault(d => d.CommentID.Equals(_.comt.ID) && d.UserID.Equals(userID)) == null ? false : true
                })
                .ToListAsync();
        }
        private async Task<List<CommentTreeView>> GetAll(int userID)
        {
            var detail = _context.CommentDetails;
            return await _context.Comments
                .Join(_context.Users,
                comt => comt.UserID,
                user => user.ID,
                (comt, user) => new { comt, user })
                .Select(_ => new CommentTreeView
                {
                    ID = _.comt.ID,
                    UserID = _.comt.UserID,
                    Username = _.user.Username,
                    ImageBase64 = _.user.ImageBase64,
                    Content = _.comt.Content,
                    ParentID = _.comt.ParentID,
                    TaskID = _.comt.TaskID,
                    Level = _.comt.Level,
                    CreatedTime = _.comt.CreatedTime,
                    Seen = detail.FirstOrDefault(d => d.CommentID.Equals(_.comt.ID) && d.UserID.Equals(userID)) == null ? false : true
                })
                .ToListAsync();
        }
        public List<CommentTreeView> GetChildren(List<CommentTreeView> comments, int parentid)
        {
            return comments
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new CommentTreeView()
                    {
                        ID = c.ID,
                        UserID = c.UserID,
                        Username = c.Username,
                        Content = c.Content,
                        ImageBase64 = c.ImageBase64,
                        ParentID = c.ParentID,
                        CreatedTime = c.CreatedTime,
                        Seen = c.Seen,
                        Level = c.Level,
                        children = GetChildren(comments, c.ID)
                    })
                    .OrderByDescending(x => x.CreatedTime)
                    .ToList();
        }
        public async Task<IEnumerable<CommentTreeView>> GetAllTreeView(int taskid, int userid)
        {
            var listComments = await GetAll(taskid, userid);
            List<CommentTreeView> hierarchy = new List<CommentTreeView>();
            hierarchy = listComments.Where(c => c.ParentID.Equals(0))
                            .Select(c => new CommentTreeView()
                            {
                                ID = c.ID,
                                UserID = c.UserID,
                                Username = c.Username,
                                Content = c.Content,
                                ImageBase64 = c.ImageBase64,
                                ParentID = c.ParentID,
                                Seen = c.Seen,
                                CreatedTime = c.CreatedTime,
                                TaskID = c.TaskID,
                                Level = c.Level,
                                children = GetChildren(listComments, c.ID)
                            })
                            .ToList();
            return hierarchy.OrderByDescending(x => x.CreatedTime).ToList();
        }
        public async Task<object> Seen(int comtID, int userID)
        {
            try
            {
                var detail = await _context.CommentDetails.FirstOrDefaultAsync(d => d.CommentID.Equals(comtID) && d.UserID.Equals(userID));
                if (detail == null)
                {
                    await _context.AddAsync(new CommentDetail { CommentID = comtID, UserID = userID, Seen = true });
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TaskHasComment>> GetAllCommentWithTask(int userID)
        {

            var listComments = await GetAll(userID);
            List<CommentTreeView> hierarchy = new List<CommentTreeView>();
            hierarchy = listComments.Where(c => c.ParentID.Equals(0))
                            .Select(c => new CommentTreeView()
                            {
                                ID = c.ID,
                                UserID = c.UserID,
                                Username = c.Username,
                                Content = c.Content,
                                ImageBase64 = c.ImageBase64,
                                ParentID = c.ParentID,
                                Seen = c.Seen,
                                TaskID = c.TaskID,
                                CreatedTime = c.CreatedTime,
                                Level = c.Level,
                                children = GetChildren(listComments, c.ID)
                            })
                            .ToList();
            var tasks = _context.Tasks.ToList().Join(hierarchy,
                t => t.ID,
                ct => ct.TaskID,
                (t, ct) => new 
                {
                   t, ct
                }).Select(x=> new TaskHasComment {
                    TaskID = x.t.ID,
                    TaskName = x.t.JobName,
                    CommentTreeViews = x.ct
                }).ToList();
            return tasks;
        }
    }
}
