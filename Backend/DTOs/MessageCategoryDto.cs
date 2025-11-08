namespace OutlookInboxManagement.DTOs;

public class MessageCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public bool IsDefault { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
}
