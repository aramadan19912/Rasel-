using Microsoft.AspNetCore.SignalR;

namespace OutlookInboxManagement.Hubs;

public class InboxHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task NotifyMessageRead(int messageId)
    {
        await Clients.Others.SendAsync("MessageRead", messageId);
    }

    public async Task NotifyTyping(string userId)
    {
        await Clients.Others.SendAsync("UserTyping", userId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
