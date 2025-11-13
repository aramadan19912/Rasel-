namespace OutlookInboxManagement.Models;

public class Calendar
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";

    // Owner
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Settings
    public bool IsDefault { get; set; }
    public bool IsVisible { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Sharing
    public bool IsShared { get; set; }
    public List<CalendarShare> Shares { get; set; } = new();

    // Events
    public List<CalendarEvent> Events { get; set; } = new();

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
}

public class CalendarShare
{
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public Calendar? Calendar { get; set; }

    public string SharedWithUserId { get; set; } = string.Empty;
    public ApplicationUser? SharedWithUser { get; set; }

    public CalendarPermission Permission { get; set; } = CalendarPermission.ViewOnly;
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
}

public enum CalendarPermission
{
    ViewOnly = 0,
    ViewDetails = 1,
    Edit = 2,
    FullControl = 3
}
