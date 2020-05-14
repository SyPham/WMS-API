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
    public class WorkingManagementHub2 : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly Data.DataContext _context;
        private readonly ITaskService _taskService;
        public WorkingManagementHub2(Data.DataContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
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
        private async Task<bool> AddMessageGroup(int roomid, string message, int userid, List<string> images )
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(x => x.Room.Equals(roomid));
                var managers = await _context.Managers.Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var members = await _context.TeamMembers.Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var listAll = managers.Union(members);
                var listChats = new List<Chat>();
                var listParticipants = new List<Participant>();

                //Neu chua co participan thi them vao
                if (!await _context.Participants.AnyAsync(x => x.RoomID == roomid))
                {
                    foreach (var user in listAll)
                    {
                        listParticipants.Add(new Participant
                        {
                            UserID = user,
                            RoomID = roomid
                        });
                    }
                    await _context.AddRangeAsync(listParticipants);
                }
                var chat = new Chat
                {
                    Message = message,
                    UserID = userid,
                    ProjectID = project.ID,
                    RoomID = roomid
                };
                //add message userid
                await _context.AddAsync(chat);
                await _context.SaveChangesAsync();
                if(images.Count > 0)
                {
                    var imagesList = new List<UploadImage>();
                    foreach (var item in images)
                    {
                        imagesList.Add(new UploadImage
                        {
                            ChatID = chat.ID,
                            Image = item
                        });
                    }
                    await _context.AddRangeAsync(imagesList);
                    await _context.SaveChangesAsync();
                }
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
            var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
           await _taskService.TaskListIsLate(userId);
           await Clients.User(id).SendAsync("ReceiveCheckAlert", user);
        }
        public async System.Threading.Tasks.Task Online(string user, string message)
        {
            // var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
            await Clients.All.SendAsync("ReceiveOnline", user, message);
        }
        public async System.Threading.Tasks.Task SendMessage(string user, string message)
        {
            // var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
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
        public async System.Threading.Tasks.Task SendMessageToGroup(string group, string message, string user, List<string> images)
        {
            ////Luu vo db
            ////Chi gui den nhung nguoi tham gia phong
            ///
            int roomid = group.ToInt();
            int userid = user.ToInt();
            var check = await AddMessageGroup(roomid, message, userid, images);
            if (check)
                await Clients.Group(group).SendAsync("ReceiveMessageGroup", roomid);
        }
        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }

    }
}
