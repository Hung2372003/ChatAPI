﻿using FakeFacebook.Commom;
using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace FakeFacebook.Controllers.Post
{
    [ApiController]
    [Route("api/PostManagement")]
    [Authorize]
    public class PostManagementController:ControllerBase
    {
        private readonly FakeFacebookDbContext _context;
        private readonly GitHubUploaderSevice _githubUploader;
        private readonly string? _getImageDataLink;
        private readonly string? _foderSavePostFile;
        public PostManagementController(FakeFacebookDbContext context, IConfiguration configuration, GitHubUploaderSevice githubUploader)
        {
            _context = context;
            _githubUploader = githubUploader;
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _foderSavePostFile = "PostFile";
        }

        [Authorize]
        [HttpGet("GetPost")]
        public IActionResult GetPost()
        {
            var msg = new Message() { Title = "", Error = false, Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var GetPostCheck = from a in _context.Posts.Where(x => x.Status == "PUBLIC" && x.IsDeleted == false )
                                .OrderByDescending(x => x.Id)                                  
                                   join b in _context.UserInformations
                                   on a.CreatedBy equals b.Id
                                   join g in _context.FileInformations
                                   on a.Id equals g.Code into g1
                                   from g in g1.DefaultIfEmpty()
                                   group new { g, a, b }
                                   by new
                                   {
                                       a.Id,
                                       a.Content,
                                       a.CreatedBy,
                                       a.CreatedTime,
                                       a.Status,
                                       a.CommentNumber,
                                       a.LikeNumber,
                                       //Like = (_context.FeelingPosts.FirstOrDefault(x=>x.CreatedBy==StaticUser && x.PostId==a.Id)!.Like == true)?true:false,
                                       b.Name,
                                       Avatar = _getImageDataLink + "/" + b.Avatar,

                                   } into e 
                               select new
                               {
                                   e.Key.Id,
                                   e.Key.Content,
                                   e.Key.CreatedBy,
                                   e.Key.CreatedTime,
                                   e.Key.Status,
                                   e.Key.LikeNumber,
                                   e.Key.CommentNumber,
                                   //e.Key.Like,
                                   e.Key.Avatar,
                                   e.Key.Name,
                                   ListFile = e.Where(x => x.g != null).Select(x => new
                                   {
                                       x.g.Id,
                                       Path = _getImageDataLink + "/" + x.g.Path,
                                       x.g.Type
                                   }).ToList()
                               };

                //var GetPost = GetPostCheck.ToList();
          var GetPost = GetPostCheck
                .Where(post =>
                    _context.FriendDoubles.Any(f =>
                        ((f.UserCode1 == post.CreatedBy && f.UserCode2 == StaticUser) ||
                         (f.UserCode2 == post.CreatedBy && f.UserCode1 == StaticUser))
                         && f.Status == "ALREADY_FRIENDS")
                    || post.CreatedBy == StaticUser
                    )
                .ToList();

            msg.Object = GetPost.OrderByDescending(x => x.Id);
        }
            catch (Exception ex) { 
                msg.Title = ex.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);

        }
        [Authorize]
        [HttpPost("GetPostComment")]
        public IActionResult GetPostComment([FromBody] int id) {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var data = from a in _context.PostComments.Where(x => x.PostCode == id && x.IsDeleted == false).OrderByDescending(x => x.Id).Take(30)
                           join b in _context.UserInformations
                           on a.CreatedBy equals b.Id
                           select new
                           {
                               a.Id,
                               a.CreatedTime,
                               a.CreatedBy,
                               a.Content,
                               b.Name,
                               Avatar = $"{_getImageDataLink}/{b.Avatar}",
                           };
                msg.Object = data.OrderByDescending(x => x.Id).ToList();
            }catch(Exception e)
            {
                msg.Title = e.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);
        }


       [HttpPost("AddNewPost")]
       [Authorize]
        public async Task<IActionResult> AddNewPost([FromForm] PostManagementModelViews data)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var add = new Posts();
                add.Content =data.Content;
                add.CreatedTime= DateTime.UtcNow;
                add.CreatedBy = StaticUser;
                add.Status=data.Status;
                add.IsDeleted = false;
                _context.Posts.Add(add);
                _context.SaveChanges();

                if (data.Files != null)
                {
                    foreach (var file in data.Files)
                    {
                        var addfile =new FileInformation();
                        addfile.Name = file.Name;
                        addfile.Path = $"{_foderSavePostFile}/{file.FileName}";
                        addfile.Type=file.ContentType;
                        addfile.Code = add.Id;
                        addfile.CreatedTime = DateTime.UtcNow;
                        addfile.CreatedBy = StaticUser;
                        addfile.IsDeleted = false;
                        _context.FileInformations.Add(addfile);
                        _context.SaveChanges();

                        string path = $"PostFile/{file?.FileName}";
                        var filePath = await _githubUploader.UploadFileAsync(path, file!, $"Upload {file?.FileName}");
               
                    }
                }
                msg.Object = add;
                msg.Title = "Đăng bài thành công";

            }
            catch (Exception e) 
            {

                msg.Title = "Có lỗi xảy ra: " + e;
                msg.Error = true;
            }
            return new JsonResult(msg);
        }

        [HttpDelete("DeletePost")]
        [Authorize]
        public IActionResult DeletePost(int Id)
        {
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var msg = new Message() { Title = "", Error = false, Object = "" };
            try
            {
                var check =_context.Posts.FirstOrDefault(x=>x.IsDeleted==false && x.Id==Id);
                if (check != null && check.CreatedBy == StaticUser)
                {
                    check.IsDeleted = true;
                    _context.Posts.Update(check);
                    _context.SaveChanges();
                    msg.Title = "Xóa bài đăng thành công";

                }
                else if(check != null && check.CreatedBy != StaticUser )
                {
                    msg.Error = true;
                    msg.Title = "Bạn không có quyền xóa bài đăng này";
                   
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "Không tìm thấy bài đăng hoặc đã bị xóa";
                }
            }
            catch(Exception e) {
                msg.Error=true;
                msg.Title="Có lỗi xảy ra khi xóa:" + e.Message;
            }
            return new JsonResult(msg);
        }
        [HttpPost("AddNewComment")]
        [Authorize]
        public JsonResult AddNewComment(CommentModelViews data)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var post = _context.Posts.FirstOrDefault(x => x.Id == data.PostCode && x.IsDeleted == false);
                if (post != null) {
                    post.CommentNumber = post.CommentNumber + 1;
                }
                var addNew = new PostComment();
                addNew.PostCode=data.PostCode;
                addNew.Content = data.Content;
                addNew.CreatedBy = StaticUser;
                addNew.IsDeleted = false;
                addNew.CreatedTime = DateTime.UtcNow;

                _context.PostComments.Add(addNew);
                _context.SaveChanges();

                var user = _context.UserInformations.FirstOrDefault(x => x.Id == StaticUser);
                if (user != null)
                {
                    var avatar = _context.FileInformations.FirstOrDefault(x => x.Id == user.FileCode)?.Path;
                    msg.Title = "Bình luận Thành công";
                    msg.Object = new
                    {

                        CreatedTime = addNew.CreatedTime,
                        CreatedBy = addNew.CreatedBy,
                        Content = addNew.Content,
                        Name = user.Name,
                        Avatar = $"{_getImageDataLink}/{avatar}",
                    };
                }
                else {
                    msg.Error = true;
                    msg.Title = "Bạn đã đăng xuất";
                }
            }
            catch (Exception ex)
            {
                msg.Error= true;
                msg.Title= ex.Message;
            }
            return new JsonResult(msg);

        }

        [HttpPost("FeelPost")]
        [Authorize]
        public IActionResult FeelPost([FromBody]  FeelPostModelViews data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var msg = new Message() { Title = "", Error = false, Object = "" };
            try
            {
                var check = _context.FeelingPosts.FirstOrDefault(x => x.CreatedBy == StaticUser && x.PostId == data.PostCode);
                var post = _context.Posts.FirstOrDefault(x => x.Id == data.PostCode && x.IsDeleted == false);
                if (check != null && post != null)
                {
                    //if(check.Feeling == data.Feeling)
                    //{
                    //    check.Feeling = null;
                    //}
                    //else
                    //{
                        check.Feeling = data.Feeling;
                    //}
                    _context.Update(check);
                    _context.SaveChanges();
                }
                else
                {                 
                    var add = new FeelingPost();
                    add.PostId = data.PostCode;
                    add.Feeling = data.Feeling;
                    add.CreatedBy = StaticUser;
                    _context.FeelingPosts.Add(add);                     
                    _context.SaveChanges();

                }
                msg.Title = "Thả cảm xúc thành công";
                
            }
            catch (Exception e)
            {
                msg.Title = e.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);
        }

        [HttpGet("GetFeelPost")]
        //[Authorize]
        public IActionResult GetFeelPost(int? postCode)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            try
            {
                var check = _context.FeelingPosts.Where(x => x.PostId == postCode && x.Feeling != null).ToList();
                if (check.Count > 0)
                {
                    var data = from a in check
                               join b in _context.UserInformations
                               on a.CreatedBy equals b.Id
                               select new
                               {
                                   UserCode = a.CreatedBy,
                                   b.Name,
                                   Avatar = _getImageDataLink + "/" + b.Avatar,
                                   a.Feeling,
                               };
                    msg.Object = data.ToList();
                    msg.Title = "ok";

                }
                else
                {
                    msg.Error=true;
                    msg.Title = "không có cảm xúc nào được thả";
                }

            }
            catch (Exception e) { 
                msg.Title = e.Message;
                msg.Error = true;           
            }
            return new JsonResult(msg);
        }

    }
}
