namespace FakeFacebook.DTOs
{
    public class DeviceTokenDto
    {
        public string? Token { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
    public class SenNotifMessageDto
    {
        public string? Notification { get; set; }
        public string? Title { get; set; }
        public string? UserId { get; set; }

    }
}
