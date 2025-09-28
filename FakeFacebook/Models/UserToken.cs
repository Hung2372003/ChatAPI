namespace FakeFacebook.Models
{
    public class UserToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
