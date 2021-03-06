﻿using System;
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
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private static IWebHostEnvironment _environment;
        public CommentsController(ICommentService commentService,
            IWebHostEnvironment environment
            )
        {
            _commentService = commentService;
            _environment = environment;
        }

        [AllowAnonymous]
        [HttpGet("{taskID}/{userID}")]
        public async Task<IActionResult> GetAll(int taskID, int userID)
        {
            return Ok(await _commentService.GetAllTreeView(taskID, userID));
        }
        [AllowAnonymous]
        [HttpGet("{userID}")]
        public async Task<IActionResult> GetAllCommentWithTask(int userID)
        {
            return Ok(await _commentService.GetAllCommentWithTask(userID));
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UploadImage>> Created([FromForm]List<UploadImage> entity)
        {
            if (ModelState.IsValid)
            {
                var list = new List<UploadImage>();
                var file = Request.Form.Files["UploadedFile"];
                var chat = Request.Form["Comment"];
                if (file != null)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\images\\comments\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\images\\comments\\");
                    }
                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        var currentFile = Request.Form.Files[i];
                        using FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\images\\comments\\" + currentFile.FileName);
                        await currentFile.CopyToAsync(fileStream);
                        fileStream.Flush();
                        list.Add(new UploadImage
                        {
                            CommentID = chat.ToInt(),
                            Image = currentFile.FileName
                        });
                    }
                }
                var model = await _commentService.UploadImage(list);
                return Ok(model);
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return Ok(entity);
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddCommentViewModel comment)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var model = await _commentService.Add(comment,userID);
           // await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item2, "message");
            return Ok(model.Item3);
        }
        public bool deletefile(string fname)
        {
            string _imageToBeDeleted = Path.Combine(_environment.WebRootPath + "\\images\\comments\\", fname);
            try
            {
                if ((System.IO.File.Exists(_imageToBeDeleted)))
                {
                    System.IO.File.Delete(_imageToBeDeleted);
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
           
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _commentService.Delete(id);
            if(model.Images.Count > 0)
            {
                foreach (var imageName in model.Images)
                {
                    deletefile(imageName);
                }
            }
            return Ok(model.Status);
        }
        [HttpPut("{id}/{taskid}/{userid}")]
        public async Task<IActionResult> Pin(int id, int taskid, int userid)
        {
            var model = await _commentService.Pin(id, taskid, userid);
            return Ok(model);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Unpin(int id)
        {
            var model = await _commentService.Unpin(id);
            return Ok(model);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] string message)
        {
            var model = await _commentService.Edit(id, message);
            return Ok(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddSub(AddSubViewModel subComment)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            subComment.CurrentUser = userID;
            var model = await _commentService.AddSub(subComment);
           // await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Item2, "message");
            return Ok(model.Item3);
        }
    }
}