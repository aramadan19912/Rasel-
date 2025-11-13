using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public interface INotificationService
{
    Task NotifyNewMessageAsync(Message message);
    Task NotifyReplyAsync(Message message);
    Task NotifyMentionAsync(Message message, string userId);
    Task SendReadReceiptAsync(Message message);
    Task SendDeliveryReceiptAsync(Message message);
}
