using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.Service;
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
        public TestController(IConfiguration configuration, FakeFacebookDbContext context, GitHubUploaderSevice githubUploader, IFirebasePushService firebasePushService)
        {
            _configuration = configuration;
            _context = context;
            _githubUploader = githubUploader;
            _firebasePushService = firebasePushService;

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


        //[HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
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
            string pathToJson = Path.Combine(Directory.GetCurrentDirectory(), "serviceAccountKey.json");

            GoogleCredential credential;
            using (var stream = new FileStream(pathToJson, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var message = new
            {
                message = new
                {
                    token = fcmToken,
                    notification = new
                    {
                        title = "Hello from .NET",
                        body = "This is a test push notification 🚀"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var projectId = "ketlook-5edec"; // lấy đúng project_id trong serviceAccountKey.json
            var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            return Ok(result);
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
