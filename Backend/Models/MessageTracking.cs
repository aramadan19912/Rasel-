namespace OutlookInboxManagement.Models;

public class MessageTracking
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public TrackingStatus Status { get; set; }
    public DateTime StatusDate { get; set; } = DateTime.UtcNow;
    public string? StatusMessage { get; set; }
}

public enum TrackingStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4,
    Bounced = 5
}
