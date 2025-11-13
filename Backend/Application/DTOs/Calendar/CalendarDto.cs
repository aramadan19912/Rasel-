namespace Backend.Application.DTOs.Calendar;

public class CalendarDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public bool IsDefault { get; set; }
    public bool IsVisible { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
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

public class CalendarShareDto
{
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public string CalendarName { get; set; } = string.Empty;
    public string SharedWithUserId { get; set; } = string.Empty;
    public string SharedWithUserName { get; set; } = string.Empty;
    public CalendarPermission Permission { get; set; }
    public DateTime SharedAt { get; set; }
}

public class ShareCalendarDto
{
    public string SharedWithUserId { get; set; } = string.Empty;
    public CalendarPermission Permission { get; set; }
}

public enum CalendarPermission
{
    ViewOnly = 0,
    ViewDetails = 1,
    Edit = 2,
    FullControl = 3
}
