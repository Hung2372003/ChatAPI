
using FakeFacebook.Data;
using FakeFacebook.DTOs;
using FakeFacebook.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace FakeFacebook.Controllers.FirebaseManagerment
{
    [ApiController]
    [Route("api/FirebaseManagerment")]
    public class FirebaseManagermentController : ControllerBase
    {
        private readonly FakeFacebookDbContext _context;

        public FirebaseManagermentController(FakeFacebookDbContext context)
        {
            _context = context;
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                });
            }
        }

        [HttpPost("SaveTokenDevices")]
        [Authorize]
        public async Task<IActionResult> SaveToken([FromBody] DeviceTokenDto dto)
        {          
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest("Token is required");

            // Kiểm tra trùng
            var exists = await _context.UserTokens
                .AnyAsync(t => t.Token == dto.Token);

            if (!exists)
            {
                var token = new UserToken { Token = dto.Token, UserId = StaticUser.ToString(), CreatedTime = DateTime.UtcNow };
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
                return new JsonResult(new { Token = token.Token });
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
        public async Task<IActionResult> SendNotification([FromBody] SenNotifMessageDto data)
        {

            var message = new Message()
            {
                Token = data.FcmToken,
                Notification = new Notification()
                {
                    Title = data.Title ?? "Admin Hưng đẹp Zai siêu cấp vũ trụ yêu cầu thêm Title cho thầy",
                    Body = data.Notification
                }
            };
            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                return new JsonResult(new { Title = e.Message });
            }

        }
    }
}
