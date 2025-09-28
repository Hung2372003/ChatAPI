namespace FakeFacebook.Service
{
    public interface IFirebasePushService
    {
        Task RegisterTokenAsync(string userId, string token);
        Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string> data);
    }

}
