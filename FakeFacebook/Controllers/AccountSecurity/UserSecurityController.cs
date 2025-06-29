using FakeFacebook.Commom;
using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers;
using FakeFacebook.ModelViewControllers.AccountSecurity;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace FakeFacebook.Controllers.AccountSecurity
{
    [ApiController]
    [Route("api/Security")]
   
    public class UserSecurityController : ControllerBase
    {
        private readonly string? _key;
        private readonly FakeFacebookDbContext _context;
        private readonly JwtTokenService _jwtService;
        private GoogleAuthService _googleAuthService;
        private readonly string _foderSaveAvatarImage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public UserSecurityController(IConfiguration configuration, 
                                      FakeFacebookDbContext context, 
                                      JwtTokenService jwtService,
                                      GoogleAuthService googleAuthService,
                                      IHttpClientFactory httpClientFactory
                                      ) {
            _config = configuration;
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
                    Response.Cookies.Append("access_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // dùng HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    });
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
                    Secure = false, // dùng HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });
                msg.Id = AddAcc.UserCode;
                msg.Title = "Đăng ký tài khoản thành công";
                msg.Object = token;
            }
            catch (Exception e) 
            {
                msg.Title="có lỗi xảy ra khi tạo tài khoản: " + e.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);

        }
 
        [HttpPost("GoogleExchangeCodeLogin")]
        public async Task<IActionResult> GoogleExchangeCode([FromBody] GoogleAuthRequest request)
        {
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            try { 
                var payload = await _googleAuthService.GoogleExchangeCode(request);
                var check = _context.UserAccounts.FirstOrDefault(x => x.ProviderSub == payload.Subject);
                if (check == null)
                {
                    msg.Title = "Vui lòng đăng ký với tài khoản google trước!";
                    msg.Error = null;
                    return new JsonResult(msg);
                }
                else
                {
                  
                    var token = _jwtService.GenerateJwtToken(check.UserCode, check.Role, check.Permission);
                   Response.Cookies.Append("access_token", token, new CookieOptions
                    { 
                        HttpOnly = true,
                        Secure = false, // dùng HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    });

                    msg.Title = "Đăng nhập thành công";
                }
            }
            catch (Exception e) {
                msg.Title = e.Message;
                msg.Error = true;
            }
            return Ok(msg);

        }

        [HttpPost("GoogleExchangeCodeRegister")]
        public async Task<IActionResult> GoogleExchangeCodeRegister([FromBody] GoogleAuthRequest request)
        {
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            try
            {
                var payload = await _googleAuthService.GoogleExchangeCode(request);
                var check = _context.UserAccounts.FirstOrDefault(x => x.ProviderSub == payload.Subject);
                if (check != null)
                {
                    msg.Title = "Tài khoản google đã tồn tại!";
                    msg.Error = false;
                    return new JsonResult(msg);
                }
                else
                {
                    var AddInfor = new UserInformation();
                    AddInfor.IsDeleted = false;
                    AddInfor.Name = payload.Name;
                    AddInfor.Email = payload.Email;
                    AddInfor.Avatar = $"{_foderSaveAvatarImage}/mostavatar.png";
                    AddInfor.CreatedTime = DateTime.Now;
                    AddInfor.IsEncryption = true;
                    _context.UserInformations.Add(AddInfor);
                    _context.SaveChanges();

                    var AddAcc = new UserAccount();
                    AddAcc.UserCode = AddInfor.Id;
                    AddAcc.IsDeleted = false;
                    AddAcc.UserName = payload.Email;
                    AddAcc.CreatedTime = DateTime.Now;
                    AddAcc.CreatedBy = AddInfor.Id;
                    AddAcc.Provider = "Google";
                    AddAcc.ProviderSub = payload.Subject;
                    AddAcc.IsEncryption = true;
                    AddAcc.Role = "User";
                    AddAcc.Permission = "NOT";
                    _context.UserAccounts.Add(AddAcc);
                    _context.SaveChanges();
                    var token = _jwtService.GenerateJwtToken(AddAcc.UserCode, AddAcc.Role, AddAcc.Permission);
                    Response.Cookies.Append("access_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // dùng HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    });
                    msg.Title = "Đăng ký thành công";
                }
            }
            catch (Exception e)
            {
                msg.Title = e.Message;
                msg.Error = true;
            }
            return Ok(msg);
        }
        [Authorize]
        [HttpGet("CheckLogin")]
        public IActionResult GetCurrentUser() { 


            var msg = new Message() { Error = false, Title = "", Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var check = _context.UserAccounts.FirstOrDefault(x => x.UserCode == StaticUser);
                if (check == null)
                {
                    msg.Error = true;
                    msg.Title = "Vui lòng đăng nhập!";
                }
            }
            catch (Exception e){

                msg.Error = true;
                msg.Title = "Có lỗi xảy ra!";
            }
            return Ok(msg);
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
            });
            return Ok(new { Message = "Đăng xuất thành công" });
        }


    }
}
