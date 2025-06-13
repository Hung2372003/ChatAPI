using FakeFacebook.Commom;
using FakeFacebook.Data;
using FakeFacebook.Hubs;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers.ChatBox;
using FakeFacebook.ModelViewControllers.ChatBoxManagerment;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace FakeFacebook.Controllers.ChatBoxManagerment
{
    [ApiController]
    [Route("api/ChatBox")]
    public class ChatBoxController : ControllerBase
    {
        private readonly FakeFacebookDbContext _context;
        private readonly string? _getImageDataLink;
        private readonly GitHubUploaderSevice _githubUploader;
        public ChatBoxController(FakeFacebookDbContext context, IConfiguration configuration, GitHubUploaderSevice githubUploader)
        {
            _context = context;
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _githubUploader = githubUploader;
        }

        // Get------------------------------------------------------
        [HttpGet("GetListUserOnline")]
        public ActionResult<List<string>> GetAllConnectedUsers()
        {
            var users = ChatHub.GetAllConnectedUsers();
            return new JsonResult(users.ToList());  // Trả về danh sách UserId
        }

        [HttpGet("GetGroupChat")]
        [Authorize]

        public JsonResult GetGroupChat()
        {
            var msg = new Message { Id = 0, Error = false, Title = "", Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var ListGroup = from a in _context.ChatGroups.Where(x => x.GroupDouble == false && x.IsDeleted==false)
                                join b in _context.GroupMembers.Where(x => x.MemberCode == StaticUser && x.IsDeleted==false)
                                on a.Id equals b.GroupChatId
                                select new
                                {
                                    GroupChatId = a.Id,
                                    GroupName = a.GroupName,
                                    GroupAvatar = (a.GroupAvartar != null) ? $"{_getImageDataLink}/{a.GroupAvartar}" :
                                                                         $"{_getImageDataLink}/Images/Avatar/mostavatar.png",
                                    ListMember = (from x in _context.GroupMembers.Where(x => x.GroupChatId == a.Id && x.IsDeleted == false)
                                                  join y in _context.UserInformations
                                                  on x.MemberCode equals y.Id
                                                  group new { x, y }
                                                  by new
                                                  {

                                                      UserCode = x.MemberCode,
                                                      Name = _context.UserInformations.FirstOrDefault(a => a.Id == x.MemberCode)!.Name,
                                                      y.FileCode,
                                                      y.Avatar
                                                  } into g
                                                  select new
                                                  {

                                                      g.Key.UserCode,
                                                      g.Key.Name,
                                                      Avatar = (g.Key.Avatar != null) ?
                                                           $"{_getImageDataLink}/{g.Key.Avatar}"
                                                           : $"{_getImageDataLink}/Images/Avatar/mostavatar.png"
                                                  }).ToList(),
                                    GroupDouble = a.GroupDouble,
                                };
                msg.Object = ListGroup.ToList();


            }
            catch (Exception e)
            {
                msg.Title = "Có lỗi xảy ra: " + e.Message;
                msg.Error = true;

            }
            return new JsonResult(msg);

        }

        // Get------------------------------------------------------
        // Post-----------------------------------------------------
        [HttpPost("CreateWindowChat")]
        [Authorize]
        // tạo tin nhắn nếu có
        public JsonResult GenernalMessageData([FromBody] CreateWindowChat data)
        {
            var msg = new Message() {Id=null, Title = "", Error = false, Object = "" ,PreventiveObject=""};
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {

                if (data?.UserCode != null)
                {
                    var CheckGroupChatIdNew = from a in _context.ChatGroups.Where(x => x.GroupDouble == true) 
                                           join b in _context.GroupMembers.Where(x => x.MemberCode == data.UserCode)
                                           on a.Id equals b.GroupChatId
                                           select new
                                           {
                                               a.Id,
                                           };

                    var GroupChatId = (from a in CheckGroupChatIdNew
                                      join b in _context.GroupMembers.Where(x => x.MemberCode == StaticUser)
                                      on a.Id equals b.GroupChatId
                                      select new
                                      {
                                          a.Id,
                                      })?.FirstOrDefault()?.Id;
                    if (GroupChatId==null)
                    {
                        var addGroup = new ChatGroups();
                        addGroup.GroupDouble = true;
                        addGroup.CreatedBy = StaticUser;
                        addGroup.CreatedTime = DateTime.Now;
                        addGroup.Quantity = 2;
                        addGroup.IsDeleted = false;
                        _context.Add(addGroup);
                        _context.SaveChanges();

                        var addMember1 = new GroupMember();
                        addMember1.GroupChatId = addGroup.Id;
                        addMember1.MemberCode = StaticUser;
                        addMember1.IsDeleted = false;
                        addMember1.InvitedTime = DateTime.Now;
                        addMember1.InvitedBy = StaticUser;
                        _context.Add(addMember1);
                     

                        var addMember2 = new GroupMember();
                        addMember2.GroupChatId = addGroup.Id;
                        addMember2.MemberCode = data.UserCode;
                        addMember2.InvitedBy = StaticUser;
                        addMember2.InvitedTime = DateTime.Now;
                        addMember2.IsDeleted = false;
                        _context.Add(addMember2);
                        _context.SaveChanges();

                        msg.Object = new { 
                            GroupChatId=addGroup.Id,
                            GroupDouble=addGroup.GroupDouble
                        };
                        msg.Title = "NotMess";
                        return new JsonResult(msg);

                    }
                    else
                    {
                        var SetStatus = _context.GroupMembers.Where(x => x.GroupChatId == GroupChatId &&x.IsDeleted==false).ToList();

                        foreach (var item in SetStatus)
                        {
                            item.Status = item.MemberCode == StaticUser ? true : item.Status;
                        }
                        _context.SaveChanges();
                        var checkContent = _context.ChatContents.FirstOrDefault(x => x.GroupChatId == GroupChatId)?.Id;
                        if (checkContent == null)
                        {
                            msg.Object = new {
                                GroupChatId = GroupChatId,
                                GroupDouble = true
                            };
                            msg.Title = "NotMess";
                            return new JsonResult(msg);
                        }
                        var mess = from a in _context.ChatContents.Where(x => x.GroupChatId == GroupChatId).OrderByDescending(x => x.Id)
                                   join b in _context.FileChats on a.FileCode equals b.FileCode into b1
                                   from b in b1.DefaultIfEmpty()
                                   group new {a,b}
                                   by new {
                                    a.Id,
                                    a.CreatedBy,
                                    a.Content,
                                    a.CreatedTime,
                                    a.GroupChatId,
                                    a.FileCode,
                                   } into e
                                   select new
                                   {
                                       e.Key.Id,                                     
                                       e.Key.CreatedBy,
                                       e.Key.Content,
                                       e.Key.CreatedTime,
                                       e.Key.FileCode,
                                       ListFile=e.Where(x=>x.b!=null).Select(x => new
                                       {
                                           x.b.Id,
                                           x.b.Name,
                                           Path = $"{_getImageDataLink}/{x.b.Path}",
                                           x.b.Type,
                                       }).ToList(),

                                   };

                        msg.Title = "MessOk";
                        msg.Object = mess.ToList();    
                        
                        //msg.Id = GroupChatId;
                        msg.PreventiveObject = new
                        {
                            GroupChatId = GroupChatId,
                            GroupDouble = _context.ChatGroups.FirstOrDefault(x => x.Id == GroupChatId)?.GroupDouble
                        };
                        return new JsonResult(msg);
                    }
                }
                else if (data?.GroupChatId != null)
                {

                    var checkContent = _context.ChatContents.FirstOrDefault(x => x.GroupChatId == data.GroupChatId)?.Id;
                    if (checkContent == null)
                    {
                        msg.Object = new { 
                            GroupChatId=data.GroupChatId,
                            GroupDouble=_context.ChatGroups.FirstOrDefault(x=>x.Id==data.GroupChatId)?.GroupDouble
                        };
                        msg.Title = "NotMess";
                        return new JsonResult(msg);
                    }

                    var SetStatus = _context.GroupMembers.Where(x => x.GroupChatId == data.GroupChatId && x.IsDeleted == false).ToList();
                    foreach (var item in SetStatus)
                    {
                        item.Status = item.MemberCode == StaticUser ? true : item.Status;
                    }
                    _context.SaveChanges();

                    var mess = from a in _context.ChatContents.Where(x => x.GroupChatId == data.GroupChatId).OrderByDescending(x => x.Id)
                               join b in _context.FileChats on a.FileCode equals b.FileCode into b1
                               from b in b1.DefaultIfEmpty()
                               group new { a, b }
                               by new
                               {
                                   a.Id,
                                   a.CreatedBy,
                                   a.Content,
                                   a.CreatedTime,
                                   a.GroupChatId,
                                   a.FileCode
                               } into e
                               select new
                               {
                                   e.Key.Id,
                                   e.Key.CreatedBy,
                                   e.Key.Content,
                                   e.Key.CreatedTime,
                                   e.Key.FileCode,
                                   ListFile = e.Where(x => x.b != null).Select(x => new
                                   {
                                       x.b.Id,
                                       x.b.Name,
                                       Path= $"{_getImageDataLink}/{x.b.Path}",
                                       x.b.Type,
                                       GroupDouble = true,
                                   }).ToList(),

                               };
                    msg.Title = "MessOk";
                    msg.Object = mess.ToList();
                    //msg.Id = data.GroupChatId;
                    msg.PreventiveObject = new
                    {
                        GroupChatId=data.GroupChatId,
                        GroupDouble=_context.ChatGroups.FirstOrDefault(x => x.Id==data.GroupChatId)?.GroupDouble
                    };
                    return new JsonResult(msg);

                }


            }
            catch (Exception e)
            {
                msg.Title = $"Có lỗi xảy ra: {e.Message}";
                msg.Error = true;
                
            }
            return new JsonResult(msg);
        }
     
        [HttpPost("UpdateMessage")]
        //[RequestSizeLimit(50 * 1024 * 1024)] // 50MB
        [Authorize]
        public async Task<IActionResult> UpdateMessage([FromForm] ChatBoxModelViews ojb, [FromForm] List<IFormFile> FileUpload)
        {
            var msg = new Message { Id = 0, Error = false, Title = "", Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {           
                var data = _context.GroupMembers.Where(x => x.GroupChatId == ojb.GroupChatId).ToList();
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].MemberCode == StaticUser)
                    {
                        data[i].Status = true;
                    }
                    else
                    {
                        data[i].Status = false;
                    }
                    _context.GroupMembers.Update(data[i]);
                }
                _context.SaveChanges();

                var chatContent = new ChatContent();
                chatContent.CreatedBy = StaticUser;
                chatContent.Content = ojb.Content;
                chatContent.GroupChatId = ojb.GroupChatId;
                chatContent.CreatedTime = DateTime.Now;
                _context.ChatContents.Add(chatContent);
                _context.SaveChanges();

                msg.Id = chatContent.Id;
                if (FileUpload != null && FileUpload.Count > 0)
                {
                    chatContent.FileCode = chatContent.Id;
                    foreach (var file in FileUpload)
                    {
                        string getDataLink = await _githubUploader.UploadFileAsync($"Chatbox/{file.FileName}", file, $"Upload {file.FileName}");
                        var SaveFile = new FileChat();
                        SaveFile.FileCode = chatContent.FileCode;
                        SaveFile.Name = file.FileName;
                        SaveFile.CreatedTime = DateTime.Now;
                        SaveFile.Path = "Chatbox/" + file.FileName;
                        SaveFile.Type = file.ContentType;
                        SaveFile.Size = file.Length;
                        SaveFile.IsDeleted = false;
                        SaveFile.ServerCode = Directory.GetCurrentDirectory();
                        _context.FileChats.Add(SaveFile);                        
       
                        if (msg.Object is List<object> newList)
                        {
                            newList.Add(new
                            {
                                Path = getDataLink,
                                Type = SaveFile.Type,
                                Id = SaveFile.Id,
                                MessId = chatContent.Id,
                                Name = SaveFile.Name
                            });
                        }
                    }
                    _context.SaveChanges();
                }
                return new JsonResult(msg);
            }
            catch (Exception ex)
            {
                msg.Title = "lỗi" + ex;
                msg.Error = true;
                return new JsonResult(msg);
            }
        }
        [HttpPost("CreateGroupChat")]
        [Authorize]
        public async Task<JsonResult> CreateGroupChat([FromForm] ChatGroupModelViews ojb)
        {
            var msg = new Message { Id = 0, Error = false, Title = "", Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                ojb.ListUser?.Add(StaticUser);

                var creatGroupChat = new ChatGroups();
                creatGroupChat.GroupAvartar = (ojb.Avatar != null)?("Images/GroupAvatar/" + ojb.Avatar.FileName):null;
                creatGroupChat.GroupDouble=false;
                creatGroupChat.GroupName = ojb.GroupName;
                creatGroupChat.CreatedBy = StaticUser;
                creatGroupChat.CreatedTime = DateTime.Now;
                creatGroupChat.IsDeleted = false;
                creatGroupChat.Quantity = ojb.ListUser?.Count;
                _context.ChatGroups.Add(creatGroupChat);
                _context.SaveChanges();

                string avatarPath="";
                if (ojb.Avatar != null) {
                    avatarPath = await _githubUploader.UploadFileAsync($"Images/GroupAvatar/{ojb.Avatar.FileName}", ojb.Avatar, $"Upload {ojb.Avatar.FileName}");
                }
                for (int i = 0; i < ojb.ListUser?.Count; i++)
                {
                    var addMember = new GroupMember();
                    addMember.GroupChatId=creatGroupChat.Id;
                    addMember.MemberCode = ojb.ListUser[i];
                    addMember.IsDeleted = false;
                    addMember.Status = ojb.ListUser[i]==StaticUser ? addMember.Status=true : addMember.Status=false;
                    addMember.InvitedTime = DateTime.Now;
                    addMember.InvitedBy = StaticUser;
                    _context.Add(addMember);
                    _context.SaveChanges();     
                }
                msg.Title = "Tạo nhóm thành công";
                if (msg.Object is List<object> newList)
                {
                    newList.Add(new
                    {
                        GroupChatId= creatGroupChat.Id,
                        GroupAvatar= (creatGroupChat.GroupAvartar!=null)?$"{_getImageDataLink}/{creatGroupChat.GroupAvartar}":null,
                        GroupName = ojb.GroupName,
                        GroupDouble=false,
                    });
                }
            }
            catch(Exception e)
            {
                msg.Title = "Có lỗi sảy ra: " + e.Message;
                msg.Error=true;
            }
            return new JsonResult(msg);
        }
        // Post-----------------------------------------------------
        [HttpPost("GetFileChat")]
        [Authorize]
        public JsonResult GetFileChat([FromBody] int groupChatId)
        {
            var msg = new Message { Id = 0, Error = false, Title = "", Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                var data = from a in _context.ChatContents.Where(x => x.IsDeleted == false && x.GroupChatId == groupChatId)
                           join b in _context.FileChats.Where(x => x.IsDeleted == false)
                           on a.Id equals b.FileCode
                           select new
                           {
                               b.Id,
                               Path= $"{_getImageDataLink}/{b.Path}",
                               b.Type,
                               b.CreatedTime,
                               a.CreatedBy,
                               ContentId=a.Id
                           };

                msg.Object = data.ToList();
            }
            catch(Exception e)
            {
                msg.Error = true;
                msg.Title = e.Message;
            }
            return new JsonResult(msg);
        }

        [HttpPost("SetStatusMess")]
        public JsonResult SetStatusMess([FromBody] int groupChatId)
        {
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var msg = new Message{ Error = false, Title = "",Object="" };
            try
            {
                var check = _context.GroupMembers.FirstOrDefault(x => x.GroupChatId == groupChatId && x.MemberCode == StaticUser);
                if (check != null)
                {
                    check.Status = true;
                    _context.SaveChanges();
                    msg.Title = "Đã đọc tin nhắn";
                }
                else {
                    msg.Title = "bạn không có trong đoạn chát này";
                }
            }
            catch(Exception e)
            {
                msg.Error= true;
                msg.Title = e.Message;
            }
            return new JsonResult(msg);
        }

    }

  

}
