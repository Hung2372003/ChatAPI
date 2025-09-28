using FakeFacebook.Data;
using FakeFacebook.DTOs;
using FakeFacebook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace FakeFacebook.Controllers.FirebaseManagerment
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceTokensController : ControllerBase
    {
        private readonly FakeFacebookDbContext _context;

        public DeviceTokensController(FakeFacebookDbContext context)
        {
            _context = context;
        }

        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tokens = await _context.UserTokens
                .OrderByDescending(t => t.CreatedTime)
                .ToListAsync();
            return Ok(tokens);
        }
    }
}
