
using Azure.Core;
using FakeFacebook.Data;
using FakeFacebook.DTOs;
using FakeFacebook.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.Security.Claims;

namespace FakeFacebook.Controllers.FirebaseManagerment
{
    [ApiController]
    [Route("api/FirebaseManagerment")]
    public class FirebaseManagermentController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly FakeFacebookDbContext _context;

        public FirebaseManagermentController(FakeFacebookDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions
            //    {
            //        Credential = GoogleCredential.FromFile("serviceAccountKey.json")
            //    });
            //}
        }

        [HttpPost("SaveTokenDevices")]
        [Authorize]
        public async Task<IActionResult> SaveToken([FromBody] DeviceTokenDto dto)
        {          
            var StaticUser = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(dto.Token) || StaticUser == null)
                return BadRequest("Token/Login is required");

           // Kiểm tra trùng
           var check =  _context.UserTokens
               .FirstOrDefault(t => t.UserId == StaticUser && t.Token == dto.Token);

            if (check == null)
            {
                var token = new UserToken { Token = dto.Token, UserId = StaticUser, CreatedTime = DateTime.UtcNow };
                _context.UserTokens.Add(token);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Token saved successfully" });
        }
        [HttpPost("getTokenById")]
        public JsonResult getTokenById([FromBody] DeviceTokenDto dto)
        {
            try
            {
                var token = _context.UserTokens
                  .FirstOrDefault(t => t.UserId == dto.UserId);
                return new JsonResult(new { Token = token?.Token });
            }
            catch(Exception e)
            {
                return new JsonResult(new { Message = e.Message });
            }
          
        }

        [HttpGet("GetAllTokenDevices")]
        public async Task<IActionResult> GetAll()
        {
            var tokens = await _context.UserTokens
                .OrderByDescending(t => t.CreatedTime)
                .ToListAsync();
            return Ok(tokens);
        }
    

        [HttpPost("SenNotifMessage")]
        [Authorize]
        
        public async Task<IActionResult> SendNotificationBatch([FromBody] SenNotifMessageDto data)
        {
            if (FirebaseMessaging.DefaultInstance == null)
            {
                return BadRequest(new { title = "❌ Firebase not initialized. DefaultInstance is null." });
            }

            var StaticUser = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var NameUser = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var Avatar = User.FindFirstValue("Avatar");
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            //var avatarUrl = !string.IsNullOrEmpty(Avatar)
            //                    ? $"{baseUrl}/api/FirebaseManagerment/Senresize?url={Avatar}"
            //                    : string.Empty;
            var avatarUrl = Avatar;
            var tokens = _context.UserTokens
                .Where(t => t.UserId == data.UserId)
                .Select(t => t.Token)
                .ToList();

            if (tokens.Count == 0)
            {
                return new JsonResult(new { Message = "User chưa đăng nhập trên thiết bị nào" });
            }

            try
            {
                var message = new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new Notification
                    {
                        Title = data.Title ?? ( NameUser ?? "Dcu chuyền cái title vào"),
                        Body = data.Notification ?? "Thông báo giả",
                        //ImageUrl = avatarUrl
                    },
                    Data = new Dictionary<string, string>
                        {
                            { "title", data.Title ?? (NameUser ?? "Dcu chuyền cái title vào") },
                            { "body", data.Notification ?? "" },
                            { "image", (Avatar != null) ? avatarUrl : string.Empty },
                            { "notificationId", (StaticUser != null) ? StaticUser : string.Empty }

                        },
                };


                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                // Kết quả trả về chi tiết từng token
                var result = tokens.Select((token, index) => new MessagePushNotification
                {
                    TokenDevice = token,
                    UserId = data.UserId,
                    Message = response.Responses[index].IsSuccess
                        ? response.Responses[index].MessageId
                        : response.Responses[index].Exception?.Message
                }).ToList();

                return new JsonResult(new
                {
                    SuccessCount = response.SuccessCount,
                    FailureCount = response.FailureCount,
                    Results = result
                });
            }
            catch (Exception e)
            {
                return new JsonResult(new { Title = e.Message });
            }
        }      
    }
}
