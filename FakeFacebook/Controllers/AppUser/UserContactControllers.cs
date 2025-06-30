using FakeFacebook.Data;
using FakeFacebook.ModelViewControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FakeFacebook.Controllers.AppUser
{
    [ApiController]
   
    [Route("api/ContactUser")]
    public class UserContactControllers : ControllerBase
    {
        private readonly FakeFacebookDbContext _context;
        private readonly string? _getImageDataLink;
        private readonly string? _foderSaveAvatarImage;

        public UserContactControllers(FakeFacebookDbContext context, IConfiguration configuration)
        {
            _context = context;
            _getImageDataLink = configuration["Git:GetImageDataLink"];
            _foderSaveAvatarImage = "Images/Avatar";
        }

        [HttpGet("ListFrends")]
        [Authorize]
        public JsonResult GetListFriends() {
            //var check = HttpContext.User.Identity.Name;
            var check = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var friends = from a in _context.FriendDoubles.Where(x => (x.UserCode1 == check 
                                                                || x.UserCode2 == check)
                                                                && x.IsDeleted == false 
                                                                && x.Status == "ALREADY_FRIENDS").Take(20)
                          where (1 == 1)
                          select new {
                              UserCode = a.UserCode2 == check ? a.UserCode1 : a.UserCode2,
                          };

            var friendList = friends.ToList();
            var cout= new List<MutualFriend>();
            foreach (var item in friendList) {
                var x = from a in _context.FriendDoubles.Where(x => (x.UserCode1 == item.UserCode
                                                                || x.UserCode2 == item.UserCode)
                                                                && x.IsDeleted == false
                                                                && x.Status == "ALREADY_FRIENDS")

                        where (1 == 1)
                        select new
                        {
                            UserCode = a.UserCode2 == item.UserCode ? a.UserCode1 : a.UserCode2,
                        };

                var commonElements = x.ToList().Intersect(friendList).ToList();
                cout.Add(new MutualFriend
                {
                    UserCode = item.UserCode,
                    CoutMutualFriend=commonElements.Count

                });

            }
            var ListFriends = from a in friends.ToList()
                              join b in _context.UserInformations
                              on a.UserCode equals b.Id
                              join d in cout
                              on a.UserCode equals d.UserCode
                             select new
                             {
                                 a.UserCode,
                                 b.Name,
                                 MutualFriend=d.CoutMutualFriend,
                                 Path = (b.Avatar==null) ?
                                       $"{_getImageDataLink}/{_foderSaveAvatarImage}/mostavatar.png" :
                                       $"{_getImageDataLink}/{b.Avatar}"

                             };     
            
            return new JsonResult(ListFriends.ToList());   
        }


    }
}
