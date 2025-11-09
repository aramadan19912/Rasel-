namespace Backend.Application.DTOs.Messages;

public class MessageCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public bool IsDefault { get; set; }
}

public class CreateMessageCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
}

public class UpdateMessageCategoryDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
}
