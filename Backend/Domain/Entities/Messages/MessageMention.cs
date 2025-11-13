namespace Backend.Domain.Entities.Messages;

public class MessageMention
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public string MentionedUserId { get; set; } = string.Empty;
    public ApplicationUser? MentionedUser { get; set; }
    public int Position { get; set; }
    public string MentionText { get; set; } = string.Empty;
}
