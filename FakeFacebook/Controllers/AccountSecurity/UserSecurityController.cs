using FakeFacebook.Commom;
using FakeFacebook.Data;
using FakeFacebook.Models;
using FakeFacebook.ModelViewControllers;
using FakeFacebook.ModelViewControllers.AccountSecurity;
using FakeFacebook.Service;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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

        public UserSecurityController(IConfiguration configuration, FakeFacebookDbContext context, JwtTokenService jwtService,GoogleAuthService googleAuthService) {
            _key = configuration["JwtSettings:SecretKey"];
            _context = context;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
            _foderSaveAvatarImage = "Images/Avatar";
        }

        [HttpPost("UserLogin")]
        public JsonResult UserLogin([FromBody] LoginModelViews loginModel) 
        {
            var msg = new Message() { Id=null,Title = "", Error = false, Object = "" };
            try {
                loginModel.UserName =loginModel.UserName;
                loginModel.Password = loginModel.Password;
                var CheckUser = _context.UserAccounts.FirstOrDefault(x => x.UserName == loginModel.UserName);
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
                    msg.Title = "PassFalse";
                }    
                else {
                    msg.Error = true;
                    msg.Title = "UserFalse";
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
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = false, 
                    Secure = false,   
                    SameSite = SameSiteMode.None, 
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });
                msg.Object = token;
                msg.Id = AddAcc.UserCode;            
            }
            catch (Exception e) 
            {
                msg.Title="có lỗi xảy ra khi tạo tài khoản: " + e.Message;
                msg.Error = true;
            }
            return new JsonResult(msg);

        }
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var msg=new Message();
            try
            {
                var payload = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken!);

                // TODO: kiểm tra email, tạo user trong hệ thống nếu cần
                // Tạo JWT token hệ thống bạn
                if (payload != null)
                {
                    msg.Object = payload;
                }
                else {
                    msg.Title = "Token không hợp lệ";
                    msg.Error = true;
                }
            }
            catch (Exception ex)
            {
               msg.Error=true;
               msg.Title= "có lỗi xảy ra:"+ex.Message;
            }
            return Ok(msg);
        }

    }
}
