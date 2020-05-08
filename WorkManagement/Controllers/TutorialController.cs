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
using Data.ViewModel.OC;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Data.ViewModel.Tutorial;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Service.Helpers;
using Newtonsoft.Json;
using Data.ViewModel.Project;

namespace WorkManagement.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TutorialController : ControllerBase
    {
        private readonly ITutorialService _tutorialService;
        public readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuaration;
        private readonly IMapper _mapper;

        public TutorialController(ITutorialService tutorialService, IMapper mapper, IWebHostEnvironment environment, IConfiguration configuaration)
        {
            _tutorialService = tutorialService; _environment = environment;
            _configuaration = configuaration;
            _mapper = mapper;

        }
        // GET: api/Projects
        [HttpPost]
        public ActionResult TestJson(string json)
        {
            var s = JsonConvert.DeserializeObject<object>(json);
            return Ok(json);
        }
        // GET: api/Projects
        [HttpGet("{projectId}")]
        public async Task<ActionResult> GetListTree(int projectId)
        {
            return Ok(await _tutorialService.GetListTree(projectId));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Tutorial tutorial)
        {
            return Ok(await _tutorialService.Edit(tutorial));
        }
        // PUT: api/Projects/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<IActionResult> Rename([FromForm]Tutorial tutorial)
        {
            if (ModelState.IsValid)
            {
                IFormFile file = Request.Form.Files["UploadedFile"];
                var name = Request.Form["UploadedFileName"];
                var id = Request.Form["UploadedFileID"];
                var path = Request.Form["UploadedFilePath"];
                if (file != null)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\video\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\video\\");
                    }
                    using (FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\video\\" + file.FileName))
                    {
                        file.CopyTo(fileStream);
                        fileStream.Flush();
                        tutorial.ID = id.ToInt();
                        var item = await _tutorialService.FindItem(tutorial.ID);
                        item.Name = name;
                        item.Path = path;
                        item.URL = _configuaration["AppSettings:applicationUrl"] + $"/video/{file.FileName}";
                        //return "\\image\\" + file.FileName;
                        await _tutorialService.Save();
                    }
                }
                else
                {
                    tutorial.ID = id.ToInt();
                    var item = await _tutorialService.FindItem(tutorial.ID);
                    item.Name = name;
                    item.Path = path;
                    await _tutorialService.Save();

                }
                //_context.Add(entity);
                //await _context.SaveChangesAsync();
                return Ok(tutorial);

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return Ok(tutorial);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm]Tutorial tutorial)
        {
            if (ModelState.IsValid)
            {
                IFormFile file = Request.Form.Files["UploadedFile"];
                var name = Request.Form["UploadedFileName"];
                var parentid = Request.Form["UploadedFileParentID"];
                var path = Request.Form["UploadedFilePath"];
                var projectid = Request.Form["UploadedProjectID"];
                var taskid = Request.Form["UploadedTaskID"];
                if (file != null)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\video\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\video\\");
                    }
                    using FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\video\\" + file.FileName);
                    file.CopyTo(fileStream);
                    fileStream.Flush();

                    var item = new Tutorial
                    {
                        Level = 1,
                        Name = name.ToSafetyString(),
                        ParentID = parentid.ToInt(),
                        URL = $"{Request.Scheme}://{Request.Host.Value}/video/{file.FileName}",
                        Path = path.ToSafetyString()
                    };
                    if (projectid.ToInt() > 0)
                        item.ProjectID = projectid.ToInt();
                    if (taskid.ToInt() > 0)
                        item.TaskID = taskid.ToInt();
                    await _tutorialService.Create(item);
                }
                else
                {
                    var item = tutorial;
                    item.Level = 1;
                    item.Name = name.ToSafetyString();
                    item.Path = path.ToSafetyString();
                    if (projectid.ToInt() > 0)
                    item.ProjectID = projectid.ToInt();

                    item.ParentID = parentid.ToInt();
                    if (taskid.ToInt() > 0)
                        item.TaskID = taskid.ToInt();
                    await _tutorialService.Create(item);
                }
                return Ok(tutorial);

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return Ok(tutorial);

        }
        [HttpPost]
        public async Task<IActionResult> CreateSub([FromBody]Tutorial tutorial)
        {
            if (ModelState.IsValid)
            {
                IFormFile file = Request.Form.Files["UploadedFile"];
                var name = Request.Form["UploadedFileName"];
                var level = Request.Form["UploadedFileLevel"];
                var parentid = Request.Form["UploadedFileParentID"];
                var projectid = Request.Form["UploadedProjectID"];
                var path = Request.Form["UploadedFilePath"];
                if (file != null)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\video\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\video\\");
                    }
                    using (FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\video\\" + file.FileName))
                    {
                        file.CopyTo(fileStream);
                        fileStream.Flush();

                        var item = new Tutorial();
                        item.ParentID = parentid.ToInt();
                        //Level cha tang len 1 va gan parentid cho subtask
                        var taskParent = _tutorialService.FindItem(item.ParentID);
                        item.Name = name.ToSafetyString();
                        item.Level = level.ToInt();
                        item.ParentID = parentid.ToInt();
                        item.ProjectID = projectid.ToInt();
                        item.Path = path.ToSafetyString();
                        item.URL = _configuaration["AppSettings:applicationUrl"] + $"/video/{file.FileName}";
                        await _tutorialService.Create(item);
                    }
                }
                else
                {
                    var item = new Tutorial();
                    item.ParentID = parentid.ToInt();
                    //Level cha tang len 1 va gan parentid cho subtask
                    var taskParent = _tutorialService.FindItem(item.ParentID);
                    item.Name = name.ToSafetyString();
                    item.ProjectID = projectid.ToInt();
                    item.Level = level.ToInt();
                    item.ParentID = parentid.ToInt();
                    item.Path = path.ToSafetyString();
                    await _tutorialService.Create(item);

                }
                return Ok(tutorial);

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return Ok(tutorial);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _tutorialService.Delete(id));
        }
    }
}
