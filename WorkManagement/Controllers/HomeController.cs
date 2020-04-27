using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;
using Service.Interface;
using WorkManagement.Helpers;
using static System.Net.Mime.MediaTypeNames;

namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ITaskService _taskService;

        public HomeController(INotificationService notificationService, ITaskService taskService)
        {

            _notificationService = notificationService;
            _taskService = taskService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Seen(int id)
        {
           
            return Ok(await _notificationService.Seen(id));
        }
        [HttpGet]
        public async Task<IActionResult> TaskListIsLate()
        {

            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            await _taskService.TaskListIsLate(userID);
            return Ok();
        }
        [HttpGet("{page}/{pageSize}/{userid}")]
        [HttpGet("{page}/{pageSize}")]
        public async Task<IActionResult> GetAllNotificationCurrentUser(int page, int pageSize, int userid)
       {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userid > 0)
                userID = userid;
            return Ok(await _notificationService.GetAllByUserID(userID,page,pageSize));
        }
        [HttpGet("{page}/{pageSize}")]
        [HttpGet("{page}/{pageSize}/{userid}")]
        public async Task<IActionResult> GetNotificationByUser(int page, int pageSize, int userid)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userid > 0)
                userID = userid;
            return Ok(await _notificationService.GetNotificationByUser(userID, page, pageSize));
        }
        [AllowAnonymous]
        [HttpPost]
        public void Base64ToImage(string source)
        {
            string base64 = source.Substring(source.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] chartData = Convert.FromBase64String(base64);
            
        }

        [HttpPost]
        public async Task<IActionResult> Image(IFormFile formFile)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            byte[] image = null;
            IFormFile file = Request.Form.Files["UploadedFile"];
            if ((file != null) && (file.Length > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    image = stream.ToArray();
                };
            }
          
            return Ok();
        }
    }
}