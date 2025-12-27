using FakeFacebook.Common;
using FakeFacebook.Data;
using FakeFacebook.DTOs;
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
        public readonly string? _getImageDataLink;
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
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _context = context;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
            _foderSaveAvatarImage = "Images/Avatar";
            _httpClientFactory = httpClientFactory;

        }

        [HttpPost("UserLogin")]
        public JsonResult UserLogin([FromBody] LoginModelViews loginModel) 
        {
            // --- CODE CŨ (chưa bảo mật, chỉ dùng plain text) ---
            /*
            var msg = new Message() { Id=null,Title = "", Error = false, Object = "" };
            try {
                loginModel.Username = loginModel.Username;
                loginModel.Password = loginModel.Password;
                var CheckUser = _context.UserAccounts.FirstOrDefault(x => x.UserName == loginModel.Username);
                if ( CheckUser!=null && CheckUser.UserPassword == loginModel.Password ) {
                    var userIfo = _context.UserInformations.FirstOrDefault(x => x.Id == CheckUser.UserCode);
                    var Name = userIfo?.Name;
                    var Avatar = $"{_getImageDataLink}/{userIfo.Avatar}";
                    var token = _jwtService.GenerateJwtToken(CheckUser.UserCode, CheckUser?.Role ?? "User", CheckUser?.Permission ?? "NOT", Name ?? "", Avatar);
                    Response.Cookies.Append("access_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // dùng HTTPS
                        SameSite = SameSiteMode.None,
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
            */
            // --- CODE MỚI (bảo mật, ưu tiên hash + salt) ---
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            try
            {
                loginModel.Username = loginModel.Username;
                loginModel.Password = loginModel.Password;
                var CheckUser = _context.UserAccounts.FirstOrDefault(x => x.UserName == loginModel.Username);
                if (CheckUser != null)
                {
                    // Nếu đã có hash + salt thì xác thực bằng hash
                    if (CheckUser.PasswordHash != null && CheckUser.PasswordSalt != null)
                    {
                        if (SecurityHelper.VerifyPasswordHash(loginModel.Password, CheckUser.PasswordHash, CheckUser.PasswordSalt))
                        {
                            var userIfo = _context.UserInformations.FirstOrDefault(x => x.Id == CheckUser.UserCode);
                            var Name = userIfo?.Name;
                            var Avatar = $"{_getImageDataLink}/{userIfo.Avatar}";
                            var token = _jwtService.GenerateJwtToken(CheckUser.UserCode, CheckUser?.Role ?? "User", CheckUser?.Permission ?? "NOT", Name ?? "", Avatar);
                            Response.Cookies.Append("access_token", token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true, // dùng HTTPS
                                SameSite = SameSiteMode.None,
                                Expires = DateTimeOffset.UtcNow.AddHours(24)
                            });
                            msg.Title = "Đăng nhập thành công";
                            msg.Object = token;
                            msg.Id = CheckUser?.UserCode;
                            return new JsonResult(msg);
                        }
                        else
                        {
                            msg.Error = true;
                            msg.Title = "Mật khẩu không chính xác";
                        }
                    }
                    else // Nếu chưa có hash (tài khoản cũ), xác thực bằng UserPassword
                    {
                        if (CheckUser.UserPassword == loginModel.Password)
                        {
                            var userIfo = _context.UserInformations.FirstOrDefault(x => x.Id == CheckUser.UserCode);
                            var Name = userIfo?.Name;
                            var Avatar = $"{_getImageDataLink}/{userIfo.Avatar}";
                            var token = _jwtService.GenerateJwtToken(CheckUser.UserCode, CheckUser?.Role ?? "User", CheckUser?.Permission ?? "NOT", Name ?? "", Avatar);
                            Response.Cookies.Append("access_token", token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true, // dùng HTTPS
                                SameSite = SameSiteMode.None,
                                Expires = DateTimeOffset.UtcNow.AddHours(24)
                            });
                            msg.Title = "Đăng nhập thành công (tài khoản cũ)";
                            msg.Object = token;
                            msg.Id = CheckUser?.UserCode;
                            return new JsonResult(msg);
                        }
                        else
                        {
                            msg.Error = true;
                            msg.Title = "Mật khẩu không chính xác";
                        }
                    }
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "Tài khoản không tồn tại";
                }
            }
            catch (Exception e)
            {
                msg.Error = true;
                msg.Title = " Có lỗi xảy ra khi đăng nhập: " + e.Message;
            }
            return new JsonResult(msg);
        }

        [HttpPost("RegisterAcc")]
        public JsonResult RegisterAcc([FromBody] RegisterAccModelViews RegAcc)
        { 
            var msg = new Message() { Id = null, Title = "", Error = false, Object = "" };
            // --- CODE CŨ (chưa bảo mật, chỉ dùng plain text) ---
            /*
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
                AddInfor.CreatedTime=DateTime.UtcNow;
                AddInfor.IsEncryption = true;
                _context.UserInformations.Add(AddInfor);
                _context.SaveChanges();

                var AddAcc = new UserAccount();
                AddAcc.UserCode = AddInfor.Id;
                AddAcc.IsDeleted = false;
                AddAcc.UserName = RegAcc.UserAccount;
                AddAcc.UserPassword = RegAcc.Password;
                AddAcc.CreatedTime = DateTime.UtcNow;
                AddAcc.UpdatedTime= DateTime.UtcNow;
                AddAcc.CreatedBy = AddInfor.Id;
                AddAcc.IsEncryption = true;
                AddAcc.Role = "User";
                AddAcc.Permission = "NOT";
                _context.UserAccounts.Add(AddAcc);
                _context.SaveChanges();

                var Avatar = $"{_getImageDataLink}/{AddInfor.Avatar}";
                var token = _jwtService.GenerateJwtToken(AddAcc.UserCode, AddAcc.Role, AddAcc.Permission,AddInfor.Name ?? "",Avatar);
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
            */
            // --- CODE MỚI (bảo mật, hash + salt) ---
            try
            {
                // Sanitize các trường đầu vào để chống XSS
                RegAcc.Name = SecurityHelper.SanitizeInput(RegAcc.Name);
                RegAcc.Email = SecurityHelper.SanitizeInput(RegAcc.Email);
                RegAcc.PhoneNumber = SecurityHelper.SanitizeInput(RegAcc.PhoneNumber);
                RegAcc.Address = SecurityHelper.SanitizeInput(RegAcc.Address);
                RegAcc.UserAccount = SecurityHelper.SanitizeInput(RegAcc.UserAccount);
                RegAcc.Password = SecurityHelper.SanitizeInput(RegAcc.Password);
                // Nếu có các trường khác do user nhập, cũng nên sanitize tương tự

                var check = _context.UserAccounts.FirstOrDefault(x => x.UserName == RegAcc.UserAccount);
                if (check != null)
                {
                    msg.Error = true;
                    msg.Title = "Tên tài khoản đã tồn tại";
                    return new JsonResult(msg);
                }
                var AddInfor = new UserInformation();
                AddInfor.IsDeleted = false;
                AddInfor.Name = RegAcc.Name;
                AddInfor.Email = RegAcc.Email;
                AddInfor.PhoneNumber = RegAcc.PhoneNumber;
                AddInfor.Address = RegAcc.Address;
                AddInfor.Birthday = (RegAcc.Birthday != "" && RegAcc.Birthday != null) ? DateTimeOffset.Parse(RegAcc.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime.Date : null;
                AddInfor.Avatar = $"{_foderSaveAvatarImage}/mostavatar.png";
                AddInfor.CreatedTime = DateTime.UtcNow;
                AddInfor.IsEncryption = true;
                _context.UserInformations.Add(AddInfor);
                _context.SaveChanges();

                // Hash mật khẩu khi đăng ký (bảo mật)
                byte[] passwordHash, passwordSalt;
                SecurityHelper.CreatePasswordHash(RegAcc.Password, out passwordHash, out passwordSalt);

                var AddAcc = new UserAccount();
                AddAcc.UserCode = AddInfor.Id;
                AddAcc.IsDeleted = false;
                AddAcc.UserName = RegAcc.UserAccount;
                AddAcc.UserPassword = RegAcc.Password; // Vẫn lưu plain text để hệ thống cũ không lỗi (có thể bỏ sau)
                AddAcc.PasswordHash = passwordHash;
                AddAcc.PasswordSalt = passwordSalt;
                AddAcc.CreatedTime = DateTime.UtcNow;
                AddAcc.UpdatedTime = DateTime.UtcNow;
                AddAcc.CreatedBy = AddInfor.Id;
                AddAcc.IsEncryption = true;
                AddAcc.Role = "User";
                AddAcc.Permission = "NOT";
                _context.UserAccounts.Add(AddAcc);
                _context.SaveChanges();

                var Avatar = $"{_getImageDataLink}/{AddInfor.Avatar}";
                var token = _jwtService.GenerateJwtToken(AddAcc.UserCode, AddAcc.Role, AddAcc.Permission, AddInfor.Name ?? "", Avatar);
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
                msg.Title = "có lỗi xảy ra khi tạo tài khoản: " + e.Message;
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
                  
                    var userIfo = _context.UserInformations.FirstOrDefault(x => x.Id == check.UserCode);
                    var Name = userIfo?.Name;
                    var Avatar = $"{_getImageDataLink}/{userIfo?.Avatar}";
                    var token = _jwtService.GenerateJwtToken(check.UserCode, (check.Role != null)?check.Role:"User", (check.Permission != null)?check.Permission:"NOT",Name??"", Avatar);
                   Response.Cookies.Append("access_token", token, new CookieOptions
                    { 
                        HttpOnly = true,
                        Secure = true, // dùng HTTPS
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    });
                    msg.Id = check.UserCode;
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
                // Sanitize các trường đầu vào nhận từ Google
                payload.Name = SecurityHelper.SanitizeInput(payload.Name);
                payload.Email = SecurityHelper.SanitizeInput(payload.Email);
                payload.Subject = SecurityHelper.SanitizeInput(payload.Subject);
                // Nếu có các trường khác do user nhập, cũng nên sanitize tương tự

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
                    AddInfor.CreatedTime = DateTime.UtcNow;
                    AddInfor.IsEncryption = true;
                    _context.UserInformations.Add(AddInfor);
                    _context.SaveChanges();

                    msg.Id = AddInfor.Id;

                    var AddAcc = new UserAccount();
                    AddAcc.UserCode = AddInfor.Id;
                    AddAcc.IsDeleted = false;
                    AddAcc.UserName = payload.Email;
                    AddAcc.CreatedTime = DateTime.UtcNow;
                    AddAcc.CreatedBy = AddInfor.Id;
                    AddAcc.Provider = "Google";
                    AddAcc.ProviderSub = payload.Subject;
                    AddAcc.IsEncryption = true;
                    AddAcc.Role = "User";
                    AddAcc.Permission = "NOT";
                    _context.UserAccounts.Add(AddAcc);
                    _context.SaveChanges();
                    
                    var Avatar = $"{_getImageDataLink}/{AddInfor.Avatar}";
                    var token = _jwtService.GenerateJwtToken(AddAcc.UserCode, AddAcc.Role, AddAcc.Permission,AddInfor.Name, Avatar);
                    Response.Cookies.Append("access_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // dùng HTTPS
                        SameSite = SameSiteMode.None,
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
                    msg.Title = "Tài khoản chưa đươc đăng ký đăng nhập!";
                }
            }
            catch (Exception e){

                msg.Error = true;
                msg.Title = "Có lỗi xảy ra!" +e.Message;
            }
            return Ok(msg);
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("access_token", "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                HttpOnly = true,
                Secure = true, // nên dùng true nếu backend có HTTPS
                SameSite = SameSiteMode.None
            });


            return Ok(new { Message = "Đăng xuất thành công" });
        }

        [HttpPost("LogoutApp")]
        public IActionResult LogouApp([FromBody] DeviceTokenDto obj)
        {
            var msg = new Message() { Error = false, Title = "", Object = "" };
            var StaticUser = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var check = _context.UserTokens.Where(x => x.Token == obj.Token && x.UserId == StaticUser.ToString()).ToList();
                if (check != null)
                {
                    _context.UserTokens.RemoveRange(check);
                    _context.SaveChanges();
                }
                msg.Title = "Đăng xuất thành công";
            }
            catch(Exception e)
            {
                msg.Error = true;
                msg.Title = e.Message;
                return Ok(msg);
            }
         
            return Ok(msg);
        }


    }
}
