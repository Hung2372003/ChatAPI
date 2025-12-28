using FakeFacebook.Common;
using FakeFacebook.Data;
using FakeFacebook.ModelViewControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FakeFacebook.Controllers.ChatBoxManagerment
{
    [ApiController]
    [Route("api/ActionMessage")]
    [Authorize]
    public class ActionMessageController:ControllerBase
    {
        private readonly FakeFacebookDbContext _context;
        private readonly string? _getImageDataLink;
        public ActionMessageController(FakeFacebookDbContext context, IConfiguration configuration)
        {
            _context = context;
            _getImageDataLink = configuration["Git:GetImageDataLink"];

        }
        [HttpGet("GetAllMessageGroups")]
        public JsonResult GetAllMessageGroups()
        {
            var msg = new Message() { Title = "", Error = false, Object = new List<NewMessageEachGroupModelViews> { } };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var GetGroup = _context.GroupMembers.Where(x => x.MemberCode == StaticUser && x.IsDeleted == false).ToList();
                List<NewMessageEachGroupModelViews> ListMessage = new List<NewMessageEachGroupModelViews>();
                for (int i = 0; i < GetGroup.Count; i++)
                {
                    var check = _context.ChatContents.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.GroupChatId == GetGroup[i].GroupChatId && x.IsDeleted == false);
                    if (check!=null)
                    { 
                        check.Content=(check.Content==null)? "Tệp đính kèm" : check.Content;
                        check.Content =(check.CreatedBy==StaticUser)? ("Bạn: "+check.Content) : check.Content;
                        var InforGroup =  _context.ChatGroups.FirstOrDefault(x => x.IsDeleted == false && x.Id == GetGroup[i].GroupChatId);
                        if (InforGroup != null)
                        {
                                var ListUser = (from a in _context.GroupMembers.Where(x => x.IsDeleted == false && x.GroupChatId == InforGroup.Id)
                                                join b in _context.UserInformations.Where(x => x.IsDeleted == false && x.Id != StaticUser)
                                                on a.MemberCode equals b.Id
                                                select new UserOfNewMessage
                                                {
                                                    UserCode = b.Id,
                                                    Name = b.Name,
                                                    Avatar = $"{_getImageDataLink}/{b.Avatar}",
                                                }).ToList();

                                if (ListUser != null)
                                {

                                    ListMessage.Add(new NewMessageEachGroupModelViews
                                    {
                                        GroupChatId = InforGroup.Id,
                                        GroupAvatar = (InforGroup.GroupDouble == false) ?
                                                    $"{_getImageDataLink}/{InforGroup.GroupAvartar}" :
                                                    ListUser[0].Avatar,
                                        GroupName = (InforGroup.GroupDouble == false) ? InforGroup.GroupName : ListUser[0].Name,
                                        ListUser = ListUser,
                                        Status = GetGroup[i].Status,
                                        NewMessage = new NewMessage
                                        {
                                            Id = check.Id,
                                            Content = (ListUser.Any(a => a.UserCode == check.CreatedBy) && check.CreatedBy != StaticUser && InforGroup.GroupDouble == false) ?
                                                      (ListUser.Find(a => a.UserCode == check.CreatedBy)?.Name + ": " + check.Content) : check.Content,
                                            CreatedBy = check.CreatedBy,
                                            CreatedTime = check.CreatedTime,
                                        },

                                    });
                                }
                                else {
                                msg.Title = "không có đoạn trò chuyện nào";
                                }
                        }
                    }
                }
                msg.Object = ListMessage.OrderByDescending(x=>x.NewMessage?.Id).ToList();
                msg.Title = "Lấy Danh sách tin nhắn các nhóm thành công";
            }
            catch(Exception e)
            {
                msg.Error = true;
                msg.Title = "có lỗi xảy ra: " + e.Message;

            }
            return new JsonResult(msg);
        }

        [HttpDelete("DeleteMessage")]
        public IActionResult DeleteMessage(int Id)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var check = _context.ChatContents.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false);
                if(check != null)
                {
                    check.IsDeleted = false;
                    msg.Title = "Thu hồi tin nhắn thành công";

                }
                else
                {
                    msg.Title = "Tín nhắn đã được xóa hoặc không tồn tại";
                }
            }
            catch(Exception e)
            {
                msg.Error = true;
                msg.Title = "Có lỗi xảy ra khi xóa tin nhắn!" + e.Message;
            }
            return Ok(msg);
        }



        [HttpPatch("SetStatusReadMessage")]
        public JsonResult SetStatusReadMessage([FromBody] int GroupChatId)
        {
            var msg = new Message() { Title = "", Error = false, Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var check = _context.GroupMembers.FirstOrDefault(x => x.MemberCode == StaticUser && x.GroupChatId == GroupChatId && x.IsDeleted == false);
                if (check != null)
                {
                    check.Status = true;
                    _context.SaveChanges();
                    msg.Title = "Đã đọc tin nhắn";

                }
                else { 
                    msg.Error= true;
                    msg.Title = "Tin nhắn không tồn tại hoặc đã bị thu hồi";
                }
               

            }catch(Exception e)
            {
                msg.Error = true;
                msg.Title = "Lỗi: " + e.Message;
            }
            return new JsonResult(msg);
        }
        [HttpGet("GetUnreadMessageCount")]
        [Authorize]
        public IActionResult GetUnreadMessageCount()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var count = _context.GroupMembers
                    .Where(x => !x.IsDeleted && x.MemberCode == userId && x.Status == false)
                    .Count();

                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error" + ex });
            }
        }
    }
}
