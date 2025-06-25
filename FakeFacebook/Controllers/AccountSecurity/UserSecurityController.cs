using FakeFacebook.Commom;
using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers;
using FakeFacebook.ModelViewControllers.AccountSecurity;
using FakeFacebook.Service;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
namespace FakeFacebook.Controllers.AccountSecurity
{
    [ApiController]
    [Route("api/Security")]
   
    public class UserSecurityController : ControllerBase
    {
        private readonly string? _key;
        private readonly FakeFacebookDbContext _context;
        private readonly JwtTokenService _jwtService;
        private readonly GoogleAuthService _googleAuthService;
        private readonly string _foderSaveAvatarImage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public UserSecurityController(IConfiguration configuration, 
                                      FakeFacebookDbContext context, 
                                      JwtTokenService jwtService,
                                      GoogleAuthService googleAuthService,
                                      IHttpClientFactory httpClientFactory,
                                      IConfiguration config
                                      ) {
            _config = config;
            _key = configuration["JwtSettings:SecretKey"];
            _context = context;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
            _foderSaveAvatarImage = "Images/Avatar";
            _httpClientFactory = httpClientFactory;

        }

        [HttpPost("UserLogin")]
        public JsonResult UserLogin([FromBody] LoginModelViews loginModel) 
        {
            var msg = new Message() { Id=null,Title = "", Error = false, Object = "" };
            try {
                loginModel.Username = loginModel.Username;
                loginModel.Password = loginModel.Password;
                var CheckUser = _context.UserAccounts.FirstOrDefault(x => x.UserName == loginModel.Username);
                if ( CheckUser!=null && CheckUser.UserPassword == loginModel.Password ) {
                    var token = _jwtService.GenerateJwtToken(CheckUser.UserCode, CheckUser?.Role ?? "User", CheckUser?.Permission ?? "NOT");
                    msg.Title = "Đăng nhập thành công";
                    msg.Object = token;
                    msg.Id = CheckUser?.UserCode;
                    return new JsonResult(msg);
                }
                else if (  CheckUser != null && CheckUser.UserPassword != loginModel.Password)
                {
                    msg.Error = true;
                    msg.Title = "Mật khẩu không chính xác";
                }    
                else {
                    msg.Error = true;
                    msg.Title = "Tài khoản không tồn tại";
                }
            }
            catch(Exception e) {
                msg.Error = true;
                msg.Title = " Có lỗi xảy ra khi đăng nhập: " + e.Message; 
            }
            return new JsonResult(msg);
        }

        [HttpPost("RegisterAcc")]
        public JsonResult RegisterAcc([FromBody] RegisterAccModelViews RegAcc)
        { 
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            try
            {
                var check = _context.UserAccounts.FirstOrDefault(x => x.UserName == RegAcc.UserAccount);
                if (check != null) {
                    msg.Error = true;
                    msg.Title = "Tên tài khoản đã tồn tại";
                    return new JsonResult(msg);
                }
                var AddInfor = new UserInformation();
                AddInfor.IsDeleted = false;
                AddInfor.Name = RegAcc.Name;
                AddInfor.Email =RegAcc.Email;
                AddInfor.PhoneNumber= RegAcc.PhoneNumber;
                AddInfor.Address = RegAcc.Address;
                AddInfor.Birthday=(RegAcc.Birthday!="" && RegAcc.Birthday!= null) ? DateTimeOffset.Parse(RegAcc.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime.Date:null;
                AddInfor.Avatar= $"{_foderSaveAvatarImage}/mostavatar.png";
                AddInfor.CreatedTime=DateTime.Now;
                AddInfor.IsEncryption = true;
                _context.UserInformations.Add(AddInfor);
                _context.SaveChanges();

                var AddAcc = new UserAccount();
                AddAcc.UserCode = AddInfor.Id;
                AddAcc.IsDeleted = false;
                AddAcc.UserName = RegAcc.UserAccount;
                AddAcc.UserPassword = RegAcc.Password;
                AddAcc.CreatedTime = DateTime.Now;
                AddAcc.UpdatedTime= DateTime.Now;
                AddAcc.CreatedBy = AddInfor.Id;
                AddAcc.IsEncryption = true;
                AddAcc.Role = "User";
                AddAcc.Permission = "NOT";
                _context.UserAccounts.Add(AddAcc);
                _context.SaveChanges();

                var token = _jwtService.GenerateJwtToken(AddAcc.UserCode, AddAcc.Role, AddAcc.Permission);
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // dùng HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });
                msg.Id = AddAcc.UserCode;
                msg.Title = "Đăng ký tài khoản thành công";
            }
            catch (Exception e) 
            {
                msg.Title="có lỗi xảy ra khi tạo tài khoản: " + e.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);

        }
        [HttpPost("GoogleExchangeCode")]
        public async Task<IActionResult> GoogleExchangeCode([FromBody] GoogleAuthRequest request)
        {
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            var clientId = _config["GoogleAuth:IDClientCode"]!;
            var clientSecret = _config["GoogleAuth:ClientSecretCode"]!;
            var redirectUri = _config["GoogleAuth:RedirectUri"]!; // Phải khớp 100% với URI gửi lên Google

            var values = new Dictionary<string, string>{
                { "code", request.Code! },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };
            try { 
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values));
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {            
                    return BadRequest(new { error = true, message = "Không thể lấy token từ Google", raw = json });
                }

                var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenData?.id_token);

                msg.Title = "Đăng nhập thành công";
                msg.Object = payload;
            }
            catch (Exception e) {
                msg.Title = e.Message;
                msg.Error = true;
            }
            return Ok(msg);

        }
    }
}
