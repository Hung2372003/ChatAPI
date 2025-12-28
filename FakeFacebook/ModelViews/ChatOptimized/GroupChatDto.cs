using Microsoft.EntityFrameworkCore;


namespace FakeFacebook.ModelViews.ChatOptimized
{
    public class GroupChatDto
    {
        public int GroupChatId { get; set; }
        public string GroupName { get; set; }
        public string? GroupAvartar { get; set; }
        public bool GroupDouble { get; set; }

        public int UserCode { get; set; }
        public string UserName { get; set; }
        public string? UserAvatar { get; set; }
    }



    [Keyless]
    public class GroupChatIdDto
    {
        public int groupChatId { get; set; }
    }
}
