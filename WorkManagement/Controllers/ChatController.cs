using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModel.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Helpers;
using Service.Interface;
using WorkManagement.Hub;

namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService, IHubContext<WorkingManagementHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }
        [AllowAnonymous]
        [HttpGet("{room}")]
        public async Task<IActionResult> GetAllMessageByRoomAndProject(int room)
        {
            return Ok(await _chatService.GetAllMessageByRoomAndProject(room));
        }

        [HttpPost]
        public async Task<IActionResult> AddMessageGroup([FromBody]CreateChatParameters chat)
        {
            var res = await _chatService.AddMessageGroup(chat.RoomID.ToInt(), chat.Message, chat.UserID, chat.Images);
            await _hubContext.Clients.Group(chat.RoomID.ToString()).SendAsync("ReceiveMessageGroup", chat.RoomID.ToInt());
            return Ok(res);
        }

    }
}