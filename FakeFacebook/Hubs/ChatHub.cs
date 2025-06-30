
using Microsoft.AspNetCore.SignalR;
namespace FakeFacebook.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using System.Security.Claims;

//[Authorize]
public class ChatHub : Hub
    {
    private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
    // khi kết nối client
    public override async Task OnConnectedAsync()
    {
        var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _connections[Context.ConnectionId] = UserCode ?? "";
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
        await base.OnConnectedAsync();
    }
    // khi ngát kết nối clent
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

    
    //kết nối đến GroupChat
    public async Task JoinGroup(string GroupChatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChatId);
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
        //await Clients.Group(GroupChatId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} đã tham gia nhóm {GroupChatId}");
    }

    // Khi người dùng rời khỏi phòng (group)
    public async Task LeaveGroup(string GroupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupChatId);
        var ListUser = GetAllConnectedUsers();
        await Clients.All.SendAsync("ListUserOnline", ListUser);
        //await Clients.Group(GroupChatId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} đã rời khỏi nhóm {GroupChatId}");
    }

    // Gửi tin nhắn đến groupchat
    public async Task SendMessageToGroup(string GroupChatId, string? Contents,object ListFile)
    {
        try
        {
            var UserCode = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group(GroupChatId).SendAsync("ReceiveMessage", GroupChatId , Contents, UserCode, ListFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessageToGroup: {ex.Message}");
            throw; 
        }

    }

    public async Task SendNotificationToGroup(string Notification, string GroupId)
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
    

