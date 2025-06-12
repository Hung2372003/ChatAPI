namespace FakeFacebook.ModelViewControllers
{
    public class NewMessageEachGroupModelViews
    {
        public int GroupChatId { get; set; }
        public string? GroupAvatar { get; set; }
        public string? GroupName { get; set; }
        public List<UserOfNewMessage>? ListUser { get; set; }
        public bool Status { get; set; }
        public NewMessage? NewMessage { get; set; }
    }


    public class UserOfNewMessage
    {
        public int UserCode { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }

    public class NewMessage
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

}
