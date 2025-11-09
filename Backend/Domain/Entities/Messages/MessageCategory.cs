namespace Backend.Domain.Entities.Messages;

public class MessageCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public string UserId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // Navigation
    public List<Message> Messages { get; set; } = new();
}
