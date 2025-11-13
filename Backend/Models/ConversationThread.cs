namespace OutlookInboxManagement.Models;

public class ConversationThread
{
    public int Id { get; set; }
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    public string Topic { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public int MessageCount { get; set; }
    public List<string> Participants { get; set; } = new();
    public bool HasAttachments { get; set; }
    public MessageImportance Importance { get; set; } = MessageImportance.Normal;

    // Navigation
    public List<Message> Messages { get; set; } = new();
}
