using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IChatService
    {
        Task<object> GetAllMessageByRoomAndProject(int roomid);
        Task<object> AddMessageGroup(int roomid,string message);
        Task<object> Remove(int projectid, int roomid);
        Task<int> JoinGroup(int projectid);
        Task<bool> AddMessageGroup(int roomid, string message, int userid, List<string> images);
    }
}
