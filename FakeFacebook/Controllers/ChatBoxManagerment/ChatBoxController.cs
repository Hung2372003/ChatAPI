
using FakeFacebook.Common;
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
        private readonly ICloudinaryService _cloudinaryService;
        private readonly GitHubUploaderSevice _githubUploader;
        private readonly string? _foderSaveAvatarImage;
        private readonly string? _foderSaveChatFile;
        private readonly string? _foderSaveGroupAvatarImage;
        public ChatBoxController(FakeFacebookDbContext context, IConfiguration configuration, GitHubUploaderSevice githubUploader, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _githubUploader = githubUploader;
            _foderSaveAvatarImage = "Images/Avatar";
            _foderSaveChatFile = "ChatBox";
            _foderSaveGroupAvatarImage = "Images/GroupAvatar";
            _cloudinaryService = cloudinaryService;
        }

        // Get------------------------------------------------------
        [HttpGet("GetListUserOnline")]
        public ActionResult<List<string>> GetAllConnectedUsers()
        {
            var users = ChatHub.GetAllConnectedUsers();
            return new JsonResult(users.ToList());  // Trả về danh sách UserId
        }

        [HttpGet("GetAllGroupChatId")]
        [Authorize]
        public IActionResult GetAllGroupChatId() {
            var msg = new Message { Id = 0, Error = false, Title = "", Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var ListGroup = from a in _context.ChatGroups.Where(x => x.IsDeleted == false)
                                join b in _context.GroupMembers.Where(x => x.MemberCode == StaticUser && x.IsDeleted == false)
                                on a.Id equals b.GroupChatId
                                select new
                                {
                                    GroupChatId = a.Id,
                                };
                msg.Object = ListGroup.ToList();
            }
            catch (Exception ex) { 
                msg.Error = true;
                msg.Title = ex.Message;  
            }
            return new JsonResult(msg);
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
                                                                         $"{_getImageDataLink}/{_foderSaveAvatarImage}/mostavatar.png",
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
                                                           : $"{_getImageDataLink}/{_foderSaveAvatarImage}/mostavatar.png"
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
                    var CheckGroupChatIdNew = from a in _context.ChatGroups.Where(x => x.GroupDouble == true && x.IsDeleted==false) 
                                           join b in _context.GroupMembers.Where(x => x.MemberCode == data.UserCode && x.IsDeleted==false)
                                           on a.Id equals b.GroupChatId
                                           select new
                                           {
                                               a.Id,
                                           };

                    var GroupChatId = (from a in CheckGroupChatIdNew
                                      join b in _context.GroupMembers.Where(x => x.MemberCode == StaticUser && x.IsDeleted == false)
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
                        addGroup.CreatedTime = DateTime.UtcNow;
                        addGroup.Quantity = 2;
                        addGroup.IsDeleted = false;
                        _context.Add(addGroup);
                        _context.SaveChanges();

                        var addMember1 = new GroupMember();
                        addMember1.GroupChatId = addGroup.Id;
                        addMember1.MemberCode = StaticUser;
                        addMember1.IsDeleted = false;
                        addMember1.InvitedTime = DateTime.UtcNow;
                        addMember1.InvitedBy = StaticUser;
                        _context.Add(addMember1);
                     

                        var addMember2 = new GroupMember();
                        addMember2.GroupChatId = addGroup.Id;
                        addMember2.MemberCode = data.UserCode;
                        addMember2.InvitedBy = StaticUser;
                        addMember2.InvitedTime = DateTime.UtcNow;
                        addMember2.IsDeleted = false;
                        _context.Add(addMember2);
                        _context.SaveChanges();

                        msg.PreventiveObject = new { 
                            GroupChatId=addGroup.Id,
                            //GroupDouble=addGroup.GroupDouble
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
                            msg.PreventiveObject = new {
                                GroupChatId = GroupChatId,
                                //GroupDouble = true
                            };
                            msg.Title = "NotMess";
                            return new JsonResult(msg);
                        }
                        var mess = from a in _context.ChatContents.Where(x => x.GroupChatId == GroupChatId && (data.MessId == null || x.Id < data.MessId)).OrderByDescending(x => x.Id).Take(20)
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
                            //GroupDouble = _context.ChatGroups.FirstOrDefault(x => x.Id == GroupChatId)?.GroupDouble
                        };
                        return new JsonResult(msg);
                    }
                }
                else if (data?.GroupChatId != null)
                {

                    var checkContent = _context.ChatContents.FirstOrDefault(x => x.GroupChatId == data.GroupChatId)?.Id;
                    if (checkContent == null)
                    {
                        msg.PreventiveObject = new { 
                            GroupChatId=data.GroupChatId,
                            //GroupDouble=_context.ChatGroups.FirstOrDefault(x=>x.Id==data.GroupChatId)?.GroupDouble
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

                    var mess = from a in _context.ChatContents.Where(x => x.GroupChatId == data.GroupChatId && (data.MessId == null || x.Id < data.MessId)).OrderByDescending(x => x.Id).Take(20)
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
     
        [HttpPost("AddNewMessage")]
        [Authorize]
        public async Task<IActionResult> AddNewMessage([FromForm] ChatBoxModelViews ojb, [FromForm] List<IFormFile> FileUpload)
        {
            // --- BỔ SUNG: Làm sạch dữ liệu đầu vào để chống XSS ---
            // Nếu muốn tắt xử lý này, chỉ cần comment lại các dòng dưới đây
            if (ojb != null)
            {
                if (ojb.Content != null)
                {
                    ojb.Content = SecurityHelper.SanitizeInput(ojb.Content); // chống XSS
                }
                // Nếu có thêm trường string khác cần chống XSS, thêm vào đây
            }
            var msg = new Message { Id = 0, Error = false, Title = "", Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                if (ojb == null)
                {
                    msg.Error = true;
                    msg.Title = "ojb is null";
                    return new JsonResult(msg);
                }
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

                var chatContent = new ChatContent();
                chatContent.CreatedBy = StaticUser;
                chatContent.Content = ojb.Content ?? string.Empty;
                chatContent.GroupChatId = ojb.GroupChatId;
                chatContent.CreatedTime = DateTime.UtcNow;
                chatContent.IsDeleted = false;
                _context.ChatContents.Add(chatContent);

                msg.Id = chatContent.Id;
                if (FileUpload != null && FileUpload.Count > 0)
                {
                    chatContent.FileCode = chatContent.Id;
                    foreach (var file in FileUpload)
                    {
                        string getDataLink = await _githubUploader.UploadFileAsync($"{_foderSaveChatFile}/{file.FileName}", file, $"Upload {file.FileName}");
                        var SaveFile = new FileChat();
                        SaveFile.FileCode = chatContent.FileCode;
                        SaveFile.Name = file.FileName;
                        SaveFile.CreatedTime = DateTime.UtcNow;
                        SaveFile.Path = $"{_foderSaveChatFile}/{file.FileName}";
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
                    //_context.SaveChanges();
                }
                _context.SaveChanges();
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
            // --- BỔ SUNG: Làm sạch dữ liệu đầu vào để chống XSS ---
            // Nếu muốn tắt xử lý này, chỉ cần comment lại các dòng dưới đây
            if (ojb != null)
            {
                if (ojb.GroupName != null)
                {
                    ojb.GroupName = SecurityHelper.SanitizeInput(ojb.GroupName); // chống XSS
                }
                // Nếu có thêm trường string khác cần chống XSS, thêm vào đây
            }
            // Nếu UserCode là string hoặc các trường text khác, hãy sanitize
            // if (data != null) data.UserCode = SecurityHelper.SanitizeInput(data.UserCode); // chống XSS nếu UserCode là string
            // Thêm các trường khác nếu có
            var msg = new Message { Id = 0, Error = false, Title = "", Object = new List<object>() };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                if (ojb == null)
                {
                    msg.Error = true;
                    msg.Title = "ojb is null";
                    return new JsonResult(msg);
                }
                if (ojb.ListUser == null)
                {
                    msg.Error = true;
                    msg.Title = "ListUser is null";
                    return new JsonResult(msg);
                }
                ojb.ListUser.Add(StaticUser);

                var creatGroupChat = new ChatGroups();
                creatGroupChat.GroupAvartar = (ojb.Avatar != null)?($"{_foderSaveGroupAvatarImage}/{ojb.Avatar.FileName}"):null;
                creatGroupChat.GroupDouble=false;
                creatGroupChat.GroupName = ojb.GroupName ?? string.Empty;
                creatGroupChat.CreatedBy = StaticUser;
                creatGroupChat.CreatedTime = DateTime.UtcNow;
                creatGroupChat.IsDeleted = false;
                creatGroupChat.Quantity = ojb.ListUser.Count;
                _context.ChatGroups.Add(creatGroupChat);
                _context.SaveChanges();

                string avatarPath="";
                if (ojb.Avatar != null) {
                    avatarPath = await _githubUploader.UploadFileAsync($"{ojb.Avatar.FileName}/{ojb.Avatar.FileName}", ojb.Avatar, $"Upload {ojb.Avatar.FileName}");
                }
                for (int i = 0; i < ojb.ListUser.Count; i++)
                {
                    var addMember = new GroupMember();
                    addMember.GroupChatId=creatGroupChat.Id;
                    addMember.MemberCode = ojb.ListUser[i];
                    addMember.IsDeleted = false;
                    addMember.Status = ojb.ListUser[i]==StaticUser ? addMember.Status=true : addMember.Status=false;
                    addMember.InvitedTime = DateTime.UtcNow;
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

        [HttpGet("GetFileChat")]
        [Authorize]
        public JsonResult GetFileChat(int groupChatId)
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

        public class GroupRequest
        {
            public int GroupChatId { get; set; }
        }
        [HttpPost("GetAllUserByGroupChatId")]
        public JsonResult GetUserIdByGroup([FromBody] GroupRequest groupChatId)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var get = from a in _context.GroupMembers.Where(x => x.GroupChatId == groupChatId.GroupChatId && x.IsDeleted == false && x.MemberCode != StaticUser)
                          join b in _context.UserInformations.Where(x => x.IsDeleted == false)
                          on a.MemberCode equals b.Id
                          select new
                          {
                              b.Id,
                              b.Name,
                              Avatar = (b.Avatar != null) ? $"{_getImageDataLink}/{b.Avatar}" : $"{_getImageDataLink}/{_foderSaveAvatarImage}/mostavatar.png",
                          };
                msg.Object = get.ToList();
            }
            catch(Exception e)
            {
                msg.Error = true;
                msg.Title = e.Message;
            }   
            return new JsonResult(msg);
        }
    }
}
