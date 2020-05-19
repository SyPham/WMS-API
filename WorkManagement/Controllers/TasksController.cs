using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModel.Task;
using Microsoft.AspNetCore.Authorization;
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
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        public TasksController(ITaskService taskService, IHubContext<WorkingManagementHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }
        [HttpGet("{sort}")]
        [HttpGet("{priority}/{sort}")]
        [HttpGet("{priority}/{sort}/{start}/{end}")]
        [HttpGet("{priority}/{sort}/{start}/{end}/{weekdays}")]
        [HttpGet("{priority}/{sort}/{start}/{end}/{weekdays}/{monthly}")]
        [HttpGet("{priority}/{sort}/{start}/{end}/{weekdays}/{monthly}/{quarterly}")]
        [HttpGet]
        public async Task<IActionResult> Todolist(string sort = "", string priority = "", string start = "", string end = "", string weekdays = "", string monthly = "", string quarterly = "")
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();

            return Ok(await _taskService.Todolist(sort, priority, userID, start, end, weekdays, monthly, quarterly));
        }
        [HttpGet("{ocid}/{sort}")]
        [HttpGet("{ocid}/{priority}/{sort}")]
        [HttpGet("{ocid}")]
        public async Task<IActionResult> Routine(int ocid, string sort = "", string priority = "")
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.Routine(sort, priority, userID, ocid));
        }
        [HttpGet]
        [HttpGet("{ocid}")]
        [HttpGet("{ocid}/{priority}")]
        [HttpGet("{ocid}/{priority}/{start}")]
        [HttpGet("{ocid}/{priority}/{start}/{end}")]
        [HttpGet("{ocid}/{priority}/{start}/{end}/{weekdays}")]
        public async Task<IActionResult> Abnormal(int ocid = 0, string priority = "", string start = "", string end = "", string weekdays = "")
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.Abnormal(ocid, priority, userID, start, end, weekdays));
        }
        [HttpGet]
        [HttpGet("{start}/{end}")]
        public async Task<IActionResult> History(string start, string end)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.History(userID, start, end));
        }
        [HttpGet]
        [HttpGet("{start}/{end}")]
        public async Task<IActionResult> HistoryFilterByDueDateTime(string start, string end)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.HistoryFilterByDueDateTime(userID, start, end));
        }
        [HttpGet("{projectid}/{sort}")]
        [HttpGet("{projectid}/{priority}/{sort}")]
        [HttpGet("{projectid}")]
        public async Task<IActionResult> ProjectDetail(int? projectid, string sort = "", string priority = "")
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.ProjectDetail(sort, priority, userID, projectid));
        }

        [HttpGet("beAssigned/{assigned}")]
        [HttpGet("assigned/{assigned}")]
        public async Task<IActionResult> SortBy(string assigned)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                return Ok(await _taskService.TodolistSortBy("", assigned, userID));
            else return Ok(await _taskService.TodolistSortBy(assigned, "", userID));
        }
        [HttpGet("{status}")]
        public async Task<IActionResult> SortBy(Data.Enum.Status status)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
                return Ok(await _taskService.TodolistSortBy(status, userID));
        }


        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody]CreateTaskViewModel createTask)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            createTask.CreatedBy = userID;
            createTask.FromWhoID = userID;
            createTask.UserID = userID;
            var model = await _taskService.CreateTask(createTask);
          //  await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item2, model.Item3);
           // await _hubContext.Clients.All.SendAsync("ReceiveAlertMessage", model.Item2, model.Item3);
            return Ok(model.Item1);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubTask([FromBody]CreateTaskViewModel createSubTask)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            createSubTask.CreatedBy = userID;
            createSubTask.CurrentUser = userID;
            return Ok(await _taskService.CreateSubTask(createSubTask));
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTask([FromBody]UpdateTaskViewModel createTask)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            createTask.CurrentUser = userID;
            return Ok(await _taskService.UpdateTask(createTask));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.Delete(id, userID));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Done(int id)
        {
            string token = Request.Headers["Authorization"];
            string url = $"{Request.Headers["Origin"]}/#/follow/";
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var model = await _taskService.Done(id, userID);
            if (model.Item1)
            {
               // await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item3, "message");
                return Ok(new
                {
                    status = model.Item1,
                    message = "The task was finished!!!"
                });
            } else if (!model.Item1)
            {
                return Ok(new {
                    status = model.Item1,
                    message = model.Item3
                });
            } else
            {
                return Ok(new
                {
                    status = model.Item1,
                    message = "Server error!"
                });
            }

        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetListProject()
        {
            return Ok(await _taskService.GetListProject());
        }
        [HttpGet("{projectid}")]
        public async Task<IActionResult> GetListUser(int projectid = 0)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.GetListUser(userID, projectid));
        }
        [HttpGet]
        public async Task<IActionResult> From()
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.From(userID));
        }
        [HttpGet("{taskid}")]
        public async Task<IActionResult> Follow(int taskid)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.Follow(userID, taskid));
        }
        [HttpGet("{taskid}")]
        public async Task<IActionResult> Undo(int taskid)
        {
            return Ok(await _taskService.Undo(taskid));
        }
        [HttpGet]
        public async Task<IActionResult> GetDeputies()
        {
            return Ok(await _taskService.GetDeputies());
        }
    }
}