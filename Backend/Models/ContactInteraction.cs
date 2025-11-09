namespace OutlookInboxManagement.Models;

public class ContactInteraction
{
    public int Id { get; set; }

    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

    // Related entities (optional)
    public int? RelatedMessageId { get; set; }
    public int? RelatedEventId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum InteractionType
{
    Email = 0,
    Call = 1,
    Meeting = 2,
    Note = 3,
    Task = 4,
    Other = 5
}
