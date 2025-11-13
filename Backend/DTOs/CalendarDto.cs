namespace OutlookInboxManagement.DTOs;

public class CalendarDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public string UserId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsVisible { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool IsShared { get; set; }
    public List<CalendarShareDto> Shares { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CalendarShareDto
{
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public string SharedWithUserId { get; set; } = string.Empty;
    public string SharedWithUserEmail { get; set; } = string.Empty;
    public string SharedWithUserName { get; set; } = string.Empty;
    public CalendarPermission Permission { get; set; }
    public DateTime SharedAt { get; set; }
}

public class CreateCalendarDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public bool IsDefault { get; set; }
}

public class UpdateCalendarDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool? IsVisible { get; set; }
    public int? DisplayOrder { get; set; }
}

public class ShareCalendarDto
{
    public string SharedWithUserEmail { get; set; } = string.Empty;
    public CalendarPermission Permission { get; set; }
}
