using FakeFacebook.Data;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FakeFacebook.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FakeFacebookDbContext _context;
        private readonly GitHubUploaderSevice _githubUploader;
         private readonly ICloudinaryService _cloudinaryService;
        public TestController(IConfiguration configuration, FakeFacebookDbContext context, GitHubUploaderSevice githubUploader, ICloudinaryService cloudinaryService )
        {
            _configuration = configuration;
            _context = context;
            _githubUploader = githubUploader;
            _cloudinaryService = cloudinaryService;
        }


        [HttpPost("upload-video")]
        public async Task<IActionResult> UploadVideoCloudinary(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var url = await _cloudinaryService.UploadVideoAsync(file,"");
            return Ok(url);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImageCloudinary(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var url = await _cloudinaryService.UploadImageAsync(file, "");
            return Ok(new { Url = url.Url, PublicId = url.PublicId });
            ;
        }

        //[HttpPost("image")]
        public async Task<IActionResult> UploadImageGit(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");
            string path = $"Images/Avatar/{file.FileName}";

            try
            {
                var getDataLink = await _githubUploader.UploadFileAsync(path, file, $"Upload {file.FileName}");
                return Ok(new { message = "Upload thành công", url = getDataLink });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}

