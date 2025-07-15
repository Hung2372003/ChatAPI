
using Microsoft.AspNetCore.SignalR;
namespace FakeFacebook.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
    {
    private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
    public override async Task OnConnectedAsync()
    {
        var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _connections[Context.ConnectionId] = UserCode ?? "";
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _connections.Remove(connectionId);

        //if (UserCode != null)
        //{
        //    var ListUser = GetAllConnectedUsers();
        //    await Clients.All.SendAsync("ListUserOnline", ListUser);
        //}
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string GroupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupId);
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
    }

    public async Task LeaveGroup(string GroupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupId);
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
    }

    public async Task SendMessageToGroup(string GroupId, string? Contents,object? ListFile)
    {
        try
        {
            var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group(GroupId).SendAsync("ReceiveMessage", GroupId, Contents, UserCode, ListFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessageToGroup: {ex.Message}");
            throw; 
        }

    }

    public async Task SendNotificationToGroup(string GroupId, string Notification)
    {
        try
        {
            var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group(GroupId).SendAsync("ReceiveNotification", Notification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessageToGroup: {ex.Message}");
            throw;
        }
    }
    public async Task SendLikeStatus( int PostId ,bool Like)
    {
        try
        {
            var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.All.SendAsync("ReceiveLikeStatus", PostId,Like);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessageToGroup: {ex.Message}");
            throw;  
        }

    }
    public static List<string> GetAllConnectedUsers()
    {
        return _connections.Values.Distinct().ToList();  // Lấy tất cả UserCode từ danh sách kết nối
    }
}
    

