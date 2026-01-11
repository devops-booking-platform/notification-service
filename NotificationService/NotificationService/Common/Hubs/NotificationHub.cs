using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Common.Hubs;

public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"User connected: {userId}");
        return base.OnConnectedAsync();
    }
    
    public async Task SendMessage(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }
}