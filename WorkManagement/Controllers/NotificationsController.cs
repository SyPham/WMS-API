using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Models;
using Data.ViewModel.Comment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Helpers;
using Service.Hub;
using Service.Interface;
using WorkManagement.Helpers;
using WorkManagement.Hub;

namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService
            )
        {
            _notificationService = notificationService;
        }

        [HttpGet("{page}/{pageSize}/{userid}")]
        [HttpGet("{page}/{pageSize}")]
        public async Task<IActionResult> GetAllNotificationCurrentUser(int page, int pageSize, int userid)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userid > 0)
                userID = userid;
            return Ok(await _notificationService.GetAllByUserID(userID, page, pageSize));
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _notificationService.Delete(id);
            return Ok(model);
        }
    
    }
}