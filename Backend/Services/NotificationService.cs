using Microsoft.AspNetCore.SignalR;
using OutlookInboxManagement.Hubs;
using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<InboxHub> _hubContext;

    public NotificationService(IHubContext<InboxHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessageAsync(Message message)
    {
        // Send SignalR notification to all connected clients
        await _hubContext.Clients.All.SendAsync("NewMessage", new
        {
            message.Id,
            message.Subject,
            message.Sender?.FullName,
            message.ReceivedAt
        });
    }

    public async Task NotifyReplyAsync(Message message)
    {
        await _hubContext.Clients.All.SendAsync("NewReply", new
        {
            message.Id,
            message.Subject,
            message.ParentMessageId
        });
    }

    public async Task NotifyMentionAsync(Message message, string userId)
    {
        await _hubContext.Clients.User(userId).SendAsync("NewMention", new
        {
            message.Id,
            message.Subject,
            message.Sender?.FullName
        });
    }

    public async Task SendReadReceiptAsync(Message message)
    {
        // Implementation for sending read receipts
        await Task.CompletedTask;
    }

    public async Task SendDeliveryReceiptAsync(Message message)
    {
        // Implementation for sending delivery receipts
        await Task.CompletedTask;
    }
}
