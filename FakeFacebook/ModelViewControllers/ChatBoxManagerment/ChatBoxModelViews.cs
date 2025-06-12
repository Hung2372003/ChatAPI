namespace FakeFacebook.ModelViewControllers.ChatBox
{
    public class ChatBoxModelViews
    {
        public int GroupChatId { get; set; }
        public string? Content { get; set; }
    }
    public class CreateWindowChat
    {
        public int? UserCode { get;set; }
        public int? GroupChatId { get; set; }

    }
    public class FileChatModelViews
    {
        public int? FileId { get; set; }
        public int? GroupChatId { get; set; }
    }
}
