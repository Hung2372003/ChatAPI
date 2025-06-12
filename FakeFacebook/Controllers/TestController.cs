using FakeFacebook.Commom;
using FakeFacebook.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FakeFacebook.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FakeFacebookDbContext _context;
        private readonly GitHubUploader _githubUploader;
        public TestController(IConfiguration configuration, FakeFacebookDbContext context, GitHubUploader githubUploader)
        {
            _configuration = configuration;
            _context = context;
            _githubUploader = githubUploader;

        }

       //[HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            string path = $"Images/Avatar/{file.FileName}";

            try
            {
                var getDataLink = await _githubUploader.UploadFileAsync(path, bytes, $"Upload {file.FileName}");
                return Ok(new { message = "Upload thành công", url = getDataLink });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}

