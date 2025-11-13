namespace OutlookInboxManagement.Models;

public class MessageReaction
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
}

public enum ReactionType
{
    Like = 1,
    Love = 2,
    Laugh = 3,
    Wow = 4,
    Sad = 5,
    Angry = 6
}
