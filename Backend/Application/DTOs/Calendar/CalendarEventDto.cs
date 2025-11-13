namespace Backend.Application.DTOs.Calendar;

public class CalendarEventDto
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public int CalendarId { get; set; }
    public string CalendarName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? LocationUrl { get; set; }
    public bool IsAllDay { get; set; }

    public string OrganizerUserId { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;

    public List<EventAttendeeDto> Attendees { get; set; } = new();
    public List<EventReminderDto> Reminders { get; set; } = new();

    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public List<DateTime> RecurrenceExceptions { get; set; } = new();

    public EventStatus Status { get; set; }
    public EventPriority Priority { get; set; }
    public EventSensitivity Sensitivity { get; set; }
    public EventShowAs ShowAs { get; set; }

    public string? OnlineMeetingUrl { get; set; }
    public string? OnlineMeetingProvider { get; set; }
    public int? VideoConferenceId { get; set; }

    public List<string> Categories { get; set; } = new();
    public string Color { get; set; } = "#0078D4";

    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreateCalendarEventDto
{
    public int CalendarId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? LocationUrl { get; set; }
    public bool IsAllDay { get; set; }

    public List<CreateEventAttendeeDto> Attendees { get; set; } = new();
    public List<int> ReminderMinutes { get; set; } = new() { 15 };

    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }

    public EventPriority Priority { get; set; } = EventPriority.Normal;
    public EventSensitivity Sensitivity { get; set; } = EventSensitivity.Normal;
    public EventShowAs ShowAs { get; set; } = EventShowAs.Busy;

    public string? OnlineMeetingUrl { get; set; }
    public bool CreateVideoConference { get; set; }

    public List<string> Categories { get; set; } = new();
    public string Color { get; set; } = "#0078D4";
}

public class UpdateCalendarEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Location { get; set; }
    public string? LocationUrl { get; set; }
    public bool? IsAllDay { get; set; }

    public EventStatus? Status { get; set; }
    public EventPriority? Priority { get; set; }
    public EventSensitivity? Sensitivity { get; set; }
    public EventShowAs? ShowAs { get; set; }

    public List<string>? Categories { get; set; }
    public string? Color { get; set; }
}

public class EventReminderDto
{
    public int Id { get; set; }
    public int MinutesBefore { get; set; }
    public ReminderType Type { get; set; }
    public bool IsActive { get; set; }
}

public enum ReminderType
{
    Notification = 0,
    Email = 1,
    SMS = 2
}

public enum EventStatus
{
    Tentative = 0,
    Confirmed = 1,
    Cancelled = 2
}

public enum EventPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}

public enum EventSensitivity
{
    Normal = 0,
    Personal = 1,
    Private = 2,
    Confidential = 3
}

public enum EventShowAs
{
    Free = 0,
    Tentative = 1,
    Busy = 2,
    OutOfOffice = 3
}
