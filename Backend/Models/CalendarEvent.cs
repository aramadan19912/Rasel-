using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    // Basic Info
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }

    // Dates & Times
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public string? TimeZone { get; set; } = "UTC";

    // User & Calendar
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int? CalendarId { get; set; }
    public Calendar? Calendar { get; set; }

    // Event Type
    public EventType Type { get; set; } = EventType.Event;
    public bool IsMeeting { get; set; }

    // Status
    public EventStatus Status { get; set; } = EventStatus.Tentative;
    public ShowAs ShowAs { get; set; } = ShowAs.Busy;

    // Visibility
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;

    // Priority
    public EventImportance Importance { get; set; } = EventImportance.Normal;

    // Recurrence
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; } // RRULE format
    public int? RecurrenceParentId { get; set; }
    public CalendarEvent? RecurrenceParent { get; set; }
    public List<CalendarEvent> RecurrenceExceptions { get; set; } = new();

    // Attendees & Resources
    public List<EventAttendee> Attendees { get; set; } = new();
    public List<EventResource> Resources { get; set; } = new();

    // Organizer
    public string? OrganizerId { get; set; }
    public ApplicationUser? Organizer { get; set; }

    // Reminders
    public List<EventReminder> Reminders { get; set; } = new();
    public bool HasReminders { get; set; }

    // Attachments
    public List<EventAttachment> Attachments { get; set; } = new();
    public bool HasAttachments { get; set; }

    // Categories
    public string? CategoryColor { get; set; } = "#0078D4";
    public List<string> Categories { get; set; } = new();

    // Online Meeting
    public bool IsOnlineMeeting { get; set; }
    public string? OnlineMeetingUrl { get; set; }
    public string? OnlineMeetingProvider { get; set; } // Teams, Zoom, etc.

    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Response Tracking
    public int AcceptedCount { get; set; }
    public int DeclinedCount { get; set; }
    public int TentativeCount { get; set; }
    public int NoResponseCount { get; set; }

    // Additional Settings
    public bool AllowNewTimeProposals { get; set; } = true;
    public bool RequestResponses { get; set; } = true;
    public string? Notes { get; set; }
    public Dictionary<string, string> CustomProperties { get; set; } = new();

    // Travel Time
    public int? TravelTimeMinutes { get; set; }

    // Conflict Detection
    public bool HasConflicts { get; set; }
}

public enum EventType
{
    Event = 0,
    Meeting = 1,
    Appointment = 2,
    AllDayEvent = 3,
    RecurringEvent = 4,
    Exception = 5
}

public enum EventStatus
{
    Free = 0,
    Tentative = 1,
    Busy = 2,
    OutOfOffice = 3,
    WorkingElsewhere = 4
}

public enum ShowAs
{
    Free = 0,
    Tentative = 1,
    Busy = 2,
    OutOfOffice = 3,
    WorkingElsewhere = 4
}

public enum EventVisibility
{
    Public = 0,
    Private = 1,
    Confidential = 2
}

public enum EventImportance
{
    Low = 0,
    Normal = 1,
    High = 2
}
