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
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                });
            }
        }


        /// <summary>
        /// Client gửi FCM token lên server khi login
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToken req)
        {
            await _firebasePushService.RegisterTokenAsync(req.UserId, req.Token);
            return Ok(new { success = true, message = "Token registered" });
        }

        /// <summary>
        /// Server gửi notification đến user cụ thể
        /// </summary>
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendRequest req)
        {
            await _firebasePushService.SendToUserAsync(req.UserId, req.Title, req.Body, req.Data);
            return Ok(new { success = true, message = "Notification sent" });
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
        [HttpGet("config")]
        public  async Task<IActionResult> GetConfig()
        {
            var configValue = _configuration["MyConfig"];
            string pathToJson = "serviceAccountKey.json";

            GoogleCredential credential;
            using (var stream = new FileStream(pathToJson, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            Console.WriteLine("ACCESS TOKEN:");
            Console.WriteLine(token);
            return Ok(token);
        }

        public class FcmRequest
        {
            public string Token { get; set; } = string.Empty;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] string fcmToken)
        {
            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification()
                {
                    Title = "Hello from .NET 🚀",
                    Body = "This is a test push notification via FirebaseAdmin SDK"
                }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            return Ok(new { MessageId = response });
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
