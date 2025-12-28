namespace FakeFacebook.ModelViews.ChatOptimized
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }

        public int? FileCode { get; set; }

        public int? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
    }
}
