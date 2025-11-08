namespace OutlookInboxManagement.Models;

public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public RecipientType Type { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public enum RecipientType
{
    To = 0,
    Cc = 1,
    Bcc = 2
}
