namespace FakeFacebook.DTOs
{
    public class DeviceTokenDto
    {
        public string? Token { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
    public class SenNotifMessageDto
    {
        public string? FcmToken { get; set; }
        public string? Notification { get; set; }
        public string? Title { get; set; }

    }
}
