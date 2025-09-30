using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.Service;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FakeFacebook.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FakeFacebookDbContext _context;
        private readonly GitHubUploaderSevice _githubUploader;
        private readonly IFirebasePushService _firebasePushService;
        private readonly ICloudinaryService _cloudinaryService;
        private static bool _isFirebaseInitialized = false;
        public TestController(
            IConfiguration configuration,
            FakeFacebookDbContext context,
            GitHubUploaderSevice githubUploader,
            ICloudinaryService cloudinaryService,
            IFirebasePushService firebasePushService
            )

        {
            _configuration = configuration;
            _context = context;
            _githubUploader = githubUploader;
            _firebasePushService = firebasePushService;
            _cloudinaryService = cloudinaryService;
        }



        //[HttpPost("upload-video")]
        public async Task<IActionResult> UploadVideoCloudinary(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var url = await _cloudinaryService.UploadVideoAsync(file,"");
            return Ok(url);
        }

        //[HttpPost("upload-image")]
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
      
        public class FcmRequest
        {
            public string Token { get; set; } = string.Empty;
        }


    }
}

public class SendRequest
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Data { get; set; }
}
