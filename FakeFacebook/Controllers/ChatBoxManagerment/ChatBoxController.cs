using FakeFacebook.Common;
using FakeFacebook.Data;
using FakeFacebook.Hubs;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers.ChatBox;
using FakeFacebook.ModelViewControllers.ChatBoxManagerment;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
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
        private readonly GitHubUploaderService _githubUploader;
        private readonly string? _foderSaveAvatarImage;
        private readonly string? _foderSaveChatFile;
        private readonly string? _foderSaveGroupAvatarImage;

        private readonly IMemoryCache _cache;
     

        private readonly string? _aesKey;
        private bool IsBase64String(string input)
        {
            Span<byte> buffer = stackalloc byte[input.Length];
            return Convert.TryFromBase64String(input, buffer, out _);
        }

        private string TryDecrypt(string content, string aesKeyBase64)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(aesKeyBase64))
                return content;
            try
            {
                if (!IsBase64String(content))
                    return content;
                var buffer = Convert.FromBase64String(content);
                if (buffer.Length < 32)
                    return content;
                return SecurityHelper.DecryptAes(content, aesKeyBase64);
            }
            catch
            {
                return content;
            }
        }

        public ChatBoxController(FakeFacebookDbContext context, IConfiguration configuration, GitHubUploaderService githubUploader, ICloudinaryService cloudinaryService, IMemoryCache cache)

        {
            _context = context;
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _githubUploader = githubUploader;
            _foderSaveAvatarImage = "Images/Avatar";
            _foderSaveChatFile = "ChatBox";
            _foderSaveGroupAvatarImage = "Images/GroupAvatar";
            _cloudinaryService = cloudinaryService;

            _cache = cache;

            _aesKey = configuration["AESKey"];

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
        public IActionResult GetAllGroupChatId()
        {
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
            catch (Exception ex)
            {
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
                var ListGroup = from a in _context.ChatGroups.Where(x => x.GroupDouble == false && x.IsDeleted == false)
                                join b in _context.GroupMembers.Where(x => x.MemberCode == StaticUser && x.IsDeleted == false)
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
        public JsonResult GenernalMessageData([FromBody] CreateWindowChat data)
        {
            var msg = new Message();
            var staticUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                int limit = data.Limit.GetValueOrDefault(20);
                int offset = data.Offset.GetValueOrDefault(0);

                // ===== 1. Sinh AES session key =====
                string sessionAesKey = SecurityHelper.GenerateRandomAesKey();
                string? encryptedAesKey = null;

                if (!string.IsNullOrEmpty(data.RSAPublicKey))
                {
                    encryptedAesKey = SecurityHelper.EncryptWithRsa(
                        sessionAesKey,
                        data.RSAPublicKey
                    );
                }

                int? groupChatId = data.GroupChatId;

                // ===== 2. Tạo group chat nếu là chat đôi =====
                if (data.UserCode.HasValue)
                {
                    groupChatId = GetOrCreateDoubleChat(staticUser, data.UserCode.Value);

                    if (groupChatId == null)
                    {
                        msg.Title = "NotMess";
                        msg.PreventiveObject = new { GroupChatId = groupChatId };
                        return new JsonResult(msg);
                    }
                }

                if (groupChatId == null)
                    return new JsonResult(msg);

                // ===== 3. Update status =====
                UpdateMemberStatus(groupChatId.Value, staticUser);

                // ===== 4. Lấy raw message (chưa giải mã) =====
                var messRaw = GetRawMessages(groupChatId.Value, data.MessId, limit, offset);

                if (!messRaw.Any())
                {
                    msg.Title = "NotMess";
                    msg.PreventiveObject = new { GroupChatId = groupChatId };
                    return new JsonResult(msg);
                }

                // ===== 5. Giải mã DB key → mã hoá lại session key =====
                var messages = messRaw.Select(m => new
                {
                    m.Id,
                    m.CreatedBy,
                    Content = SecurityHelper.EncryptAes(
                        TryDecrypt(m.Content, _aesKey),
                        sessionAesKey
                    ),
                    m.CreatedTime,
                    m.FileCode,
                    m.ListFile
                }).ToList();

                // ===== 6. Response =====
                msg.Title = "MessOk";
                msg.Object = messages;
                msg.PreventiveObject = new
                {
                    GroupChatId = groupChatId,
                    EncryptedAesKey = encryptedAesKey
                };
            }
            catch (Exception ex)
            {
                msg.Error = true;
                msg.Title = $"Có lỗi xảy ra: {ex.Message}";
            }

            return new JsonResult(msg);
        }
        private int GetOrCreateDoubleChat(int user1, int user2)
        {
            var groupId = (
                from g in _context.ChatGroups
                join m1 in _context.GroupMembers on g.Id equals m1.GroupChatId
                join m2 in _context.GroupMembers on g.Id equals m2.GroupChatId
                where g.GroupDouble
                      && !g.IsDeleted
                      && m1.MemberCode == user1
                      && m2.MemberCode == user2
                select g.Id
            ).FirstOrDefault();

            if (groupId != 0)
                return groupId;

            var group = new ChatGroups
            {
                GroupDouble = true,
                CreatedBy = user1,
                CreatedTime = DateTime.UtcNow,
                Quantity = 2,
                IsDeleted = false
            };

            _context.ChatGroups.Add(group);
            _context.SaveChanges();

            _context.GroupMembers.AddRange(
                new GroupMember
                {
                    GroupChatId = group.Id,
                    MemberCode = user1,
                    InvitedBy = user1,
                    InvitedTime = DateTime.UtcNow,
                    IsDeleted = false
                },
                new GroupMember
                {
                    GroupChatId = group.Id,
                    MemberCode = user2,
                    InvitedBy = user1,
                    InvitedTime = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            _context.SaveChanges();
            return group.Id;
        }

        private void UpdateMemberStatus(int groupChatId, int userId)
        {
            var members = _context.GroupMembers
                .Where(x => x.GroupChatId == groupChatId && !x.IsDeleted)
                .ToList();

            foreach (var m in members)
                m.Status = m.MemberCode == userId || m.Status;

            _context.SaveChanges();
        }
        private List<dynamic> GetRawMessages(int groupChatId, int? messId, int limit, int offset)
        {
            return (
                from a in _context.ChatContents
                where a.GroupChatId == groupChatId
                      && (!messId.HasValue || a.Id < messId)
                orderby a.Id descending
                select new
                {
                    a.Id,
                    a.CreatedBy,
                    a.Content,
                    a.CreatedTime,
                    a.FileCode,
                    ListFile = (
                        from f in _context.FileChats
                        where f.FileCode == a.FileCode
                        select new
                        {
                            f.Id,
                            f.Name,
                            Path = $"{_getImageDataLink}/{f.Path}",
                            f.Type,
                            GroupDouble = true
                        }
                    ).ToList()
                }
            )
            .Skip(offset)
            .Take(limit)
            .ToList<dynamic>();
        }


        [HttpPost("AddNewMessage")]
        [Authorize]
        public async Task<IActionResult> AddNewMessage([FromForm] ChatBoxModelViews ojb, [FromForm] List<IFormFile> FileUpload)
        {
            // Quy trình: Giải mã AESKeyEncrypted bằng RSA private key, giải mã Content bằng AES key đó, mã hóa lại bằng AESKey nội bộ
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

                // 1. Lấy private key server từ config
                var privateKeyXml = HttpContext.RequestServices.GetService(typeof(IConfiguration)) is IConfiguration config ? config["RSA:RSAPrivateKeyServer"] : null;
                if (string.IsNullOrEmpty(privateKeyXml))
                {
                    msg.Error = true;
                    msg.Title = "Server RSA private key not configured!";
                    return new JsonResult(msg);
                }

                // 2. Giải mã AESKeyEncrypted bằng RSA
                string aesKeyFromClient = null;
                if (!string.IsNullOrEmpty(ojb.AESKeyEncrypted))
                {
                    using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider())
                    {
                        rsa.FromXmlString(privateKeyXml);
                        var encryptedBytes = Convert.FromBase64String(ojb.AESKeyEncrypted);
                        var decryptedBytes = rsa.Decrypt(encryptedBytes, true);
                        aesKeyFromClient = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "AESKeyEncrypted is missing!";
                    return new JsonResult(msg);
                }

                // 3. Giải mã nội dung tin nhắn bằng AES key vừa giải mã được
                string plainContent = null;
                if (!string.IsNullOrEmpty(ojb.Content) && !string.IsNullOrEmpty(aesKeyFromClient))
                {
                    plainContent = SecurityHelper.DecryptAes(ojb.Content, aesKeyFromClient);
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "Content or AES key is missing!";
                    return new JsonResult(msg);
                }

                // 4. Làm sạch nội dung để chống XSS
                plainContent = SecurityHelper.SanitizeInput(plainContent);

                // 5. Mã hóa lại nội dung bằng AESKey nội bộ trước khi lưu DB
                string encryptedForDb = null;
                if (!string.IsNullOrEmpty(_aesKey))
                {
                    encryptedForDb = SecurityHelper.EncryptAes(plainContent, _aesKey);
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "Server AESKey not configured!";
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
                chatContent.Content = encryptedForDb ?? string.Empty;
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

                string avatarPath = "";
                if (ojb.Avatar != null)
                {
                    avatarPath = await _githubUploader.UploadFileAsync($"{ojb.Avatar.FileName}/{ojb.Avatar.FileName}", ojb.Avatar, $"Upload {ojb.Avatar.FileName}");
                }
                for (int i = 0; i < ojb.ListUser.Count; i++)
                {
                    var addMember = new GroupMember();
                    addMember.GroupChatId = creatGroupChat.Id;
                    addMember.MemberCode = ojb.ListUser[i];
                    addMember.IsDeleted = false;
                    addMember.Status = ojb.ListUser[i] == StaticUser ? addMember.Status = true : addMember.Status = false;
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
                        GroupChatId = creatGroupChat.Id,
                        GroupAvatar = (creatGroupChat.GroupAvartar != null) ? $"{_getImageDataLink}/{creatGroupChat.GroupAvartar}" : null,
                        GroupName = ojb.GroupName,
                        GroupDouble = false,
                    });
                }
            }
            catch (Exception e)
            {
                msg.Title = "Có lỗi sảy ra: " + e.Message;
                msg.Error = true;
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
                               Path = $"{_getImageDataLink}/{b.Path}",
                               b.Type,
                               b.CreatedTime,
                               a.CreatedBy,
                               ContentId = a.Id
                           };

                msg.Object = data.ToList();
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                msg.Error = true;
                msg.Title = e.Message;
            }
            return new JsonResult(msg);
        }






        // API đã tối ưu


        [HttpPost("CreateWindowChat1")]
        [Authorize]
        public async Task<JsonResult> GenernalMessageData1([FromBody] CreateWindowChat data)
        {
            var msg = new Message { Error = false };
            var userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                int groupChatId;

                // 👉 Chat đôi
                if (data.UserCode != null)
                {
                    var output = new SqlParameter("@GROUP_CHAT_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_GET_OR_CREATE_DOUBLE_CHAT @USER_A, @USER_B, @GROUP_CHAT_ID OUTPUT",
                        new SqlParameter("@USER_A", userId),
                        new SqlParameter("@USER_B", data.UserCode),
                        output
                    );
                    groupChatId = (int)output.Value;
                }
                else
                {
                    groupChatId = data.GroupChatId!.Value;
                }

                var offset = data.Offset ?? 0;
                var limit = data.Limit ?? 50;
                var cacheKey = $"CHAT_{groupChatId}_{data.MessId ?? 0}_{offset}_{limit}";

                if (!_cache.TryGetValue(cacheKey, out List<object> messagesWithFiles))
                {
                    // Query DB với OFFSET/LIMIT trực tiếp trong SQL
                    var sql = @"
                SELECT * 
                FROM FN_GET_CHAT_MESSAGE(@GROUP_CHAT_ID, @LAST_ID, @OFFSET, @LIMIT)
                ORDER BY ID DESC";

                    var messages = await _context.ChatMessageDtos
                        .FromSqlRaw(sql,
                            new SqlParameter("@GROUP_CHAT_ID", groupChatId),
                            new SqlParameter("@LAST_ID", data.MessId ?? (object)DBNull.Value),
                            new SqlParameter("@OFFSET", offset),
                            new SqlParameter("@LIMIT", limit)
                        )
                        .AsNoTracking()
                        .ToListAsync();

                    messagesWithFiles = messages.Select(m => new
                    {
                        m.Id,
                        m.CreatedBy,
                        m.Content,
                        m.CreatedTime,
                        ListFile = m.FileId != null ? new[]
                        {
                    new {
                        m.FileId,
                        m.FileName,
                        Path = $"{_getImageDataLink}/{m.FilePath}",
                        m.FileType
                    }
                } : Array.Empty<object>()
                    }).Cast<object>().ToList();

                    _cache.Set(cacheKey, messagesWithFiles, TimeSpan.FromSeconds(30));
                }

                msg.Title = messagesWithFiles.Any() ? "MessOk" : "NotMess";
                msg.Object = messagesWithFiles;
                msg.PreventiveObject = new { GroupChatId = groupChatId };
            }
            catch (Exception ex)
            {
                msg.Error = true;
                msg.Title = ex.Message;
            }

            return new JsonResult(msg);
        }





    }
}
