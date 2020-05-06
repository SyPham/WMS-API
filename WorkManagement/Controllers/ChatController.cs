using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Models;
using Data.ViewModel.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Helpers;
using Service.Interface;
using WorkManagement.Hub;
using Microsoft.Extensions.Configuration;
namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        private readonly IChatService _chatService;
        private static IWebHostEnvironment _environment;
        public ChatController(IChatService chatService,
            IHubContext<WorkingManagementHub> hubContext,
            IWebHostEnvironment environment
            )
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _environment = environment;
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
            var res = await _chatService.AddMessageGroup(chat.RoomID.ToInt(), chat.Message, chat.UserID);
            await _hubContext.Clients.Group(chat.RoomID.ToString()).SendAsync("ReceiveMessageGroup", chat.RoomID.ToInt());
            return Ok(res);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UploadImage>> Created([FromForm]List<UploadImage> entity)
        {
            if (ModelState.IsValid)
            {
                var list = new List<UploadImage>();
                var file = Request.Form.Files["UploadedFile"];
                var chat = Request.Form["Chat"];
                if (file != null)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\images\\chats\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\images\\chats\\");
                    }
                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        var currentFile = Request.Form.Files[i];
                        using FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\images\\chats\\" + currentFile.FileName);
                        await currentFile.CopyToAsync(fileStream);
                        fileStream.Flush();
                        list.Add(new UploadImage
                        {
                            ChatID = chat.ToInt(),
                            Image = currentFile.FileName
                        });
                    }
                }

               var model = await _chatService.UploadImage(list);
                return Ok(model);

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return Ok(entity);
        }

    }
}