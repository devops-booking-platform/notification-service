using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Common.Hubs;

public class NotificationHub : Hub
{
    public async Task SendMessage(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }
}