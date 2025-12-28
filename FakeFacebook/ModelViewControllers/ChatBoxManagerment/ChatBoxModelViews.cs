namespace FakeFacebook.ModelViewControllers.ChatBox
{
    public class ChatBoxModelViews
    {
        public int GroupChatId { get; set; }
        public string? Content { get; set; }

        //new - not
        //public int? PostId { get; set; }
    }
    public class CreateWindowChat
    {
        public int? UserCode { get;set; }
        public int? GroupChatId { get; set; }
        public int? MessId { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
     
        public string? RsaKeyPulic { get; set; }
        //public int PageSize { get; set; }

    }
    public class FileChatModelViews
    {
        public int? FileId { get; set; }
        public int? GroupChatId { get; set; }
    }
}
