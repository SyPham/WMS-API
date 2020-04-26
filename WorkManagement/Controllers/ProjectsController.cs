using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Service.Interface;
using Data.ViewModel.Project;
using WorkManagement.Helpers;
using Service.Helpers;
using Microsoft.AspNetCore.Authorization;
using WorkManagement.Hub;
using Microsoft.AspNetCore.SignalR;

namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IHubContext<WorkingManagementHub> _hubContext;

        public ProjectsController(DataContext context, IProjectService projectService, ITaskService taskService, IHubContext<WorkingManagementHub> hubContext)
        {
            _context = context;
            _projectService = projectService;
            _taskService = taskService;
            _hubContext = hubContext;
        }

        // GET: api/Projects
        [HttpGet("{page}/{pageSize}/{keyword}")]
        [HttpGet("{page}/{pageSize}")]
        public async Task<ActionResult> GetAllPaging(int page, int pageSize, string keyword)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var model = await _projectService.GetAllPaging(userID, page, pageSize, keyword);
            return Ok(new
            {
                data = model,
                total = model.TotalPages,
                page,
                pageSize
            });
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _projectService.GetAll());
        }
        [HttpGet("{page}/{pageSize}/{projectName}")]
        public async Task<ActionResult> GetProjects(int page = 1, int pageSize = 20, string projectName = "")
        {

            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _projectService.GetProjects(userID, page, pageSize, projectName));
        }
        [HttpGet]
        public async Task<ActionResult> GetProjects([FromQuery]PaginationParams param)
        {

            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var lists = await _projectService.GetAllPaging(userID, param.PageNumber, param.PageSize, "");
            Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
            return Ok(lists);
        }
        [HttpGet("{projectName}")]
        public async Task<ActionResult> Search([FromQuery]PaginationParams param, string projectName)
        {

            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var lists = await _projectService.GetAllPaging(userID, param.PageNumber, param.PageSize, projectName);
            Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
            return Ok(lists);
        }
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            return Ok(await _projectService.GetUsers());
        }
        [HttpGet]
        public async Task<ActionResult> GetListProject()
        {
            return Ok(await _projectService.GetListProject());
        }
        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(int id)
        {
            var project = await _projectService.GetByID(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }
        [HttpGet("{projectId}")]
        public async Task<ActionResult> Open(int projectId)
        {
            var project = await _projectService.Open(projectId);
            return Ok(project);
        }
        [HttpGet("{projectId}")]
        public async Task<ActionResult> Clone(int projectId)
        {
            var project = await _projectService.Clone(projectId);
            return Ok(project);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> ProjectDetail(int id)
        {
            var project = await _projectService.ProjectDetail(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserByProjectID(int id)
        {
            var project = await _projectService.GetUserByProjectID(id);
            return Ok(project);
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<IActionResult> Update(Project project)
        {
            return Ok(await _projectService.Update(project));
        }

        // POST: api/Projects
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Project>> Create(Project project)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            project.CreatedBy = userID;
            return Ok(await _projectService.Create(project));
        }
        [HttpPost]
        public async Task<ActionResult> AddManager(AddManagerViewModel project)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            project.UserID = userID;
            var model = await _projectService.AddManager(project);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item2, "message");
            return Ok(model.Item1);
        }
        [HttpPost]
        public async Task<ActionResult> AddMember(AddMemberViewModel project)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            project.UserID = userID;
            var model = await _projectService.AddMember(project);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item2, "message");
            return Ok(model.Item1);
        }
        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Project>> DeleteProject(int id)
        {
            return Ok(await _projectService.Delete(id));
        }

        //[HttpGet("{projectid}/{sort}")]
        //[HttpGet("{projectid}/{priority}/{sort}")]
        //[HttpGet("{projectid}")]
        //public async Task<IActionResult> ProjectDetail(int projectid = 0, string sort = "", string priority = "")
        //{
        //    string token = Request.Headers["Authorization"];
        //    var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
        //    return Ok(await _taskService.ProjectDetail(sort, priority, userID, projectid));
        //}

    }
}
