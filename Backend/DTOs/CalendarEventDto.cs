using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.DTOs;

public class CalendarEventDto
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public int CalendarId { get; set; }
    public string CalendarName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    // Dates
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public string TimeZone { get; set; } = "UTC";

    // Recurrence
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
    public List<DateTime> RecurrenceExceptions { get; set; } = new();

    // Meeting
    public bool IsMeeting { get; set; }
    public bool IsOnlineMeeting { get; set; }
    public string? OnlineMeetingUrl { get; set; }
    public string? OnlineMeetingProvider { get; set; }
    public List<EventAttendeeDto> Attendees { get; set; } = new();

    // Organizer
    public string OrganizerId { get; set; } = string.Empty;
    public string OrganizerEmail { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;

    // Status
    public EventStatus Status { get; set; }
    public EventBusyStatus BusyStatus { get; set; }
    public EventImportance Importance { get; set; }
    public EventSensitivity Sensitivity { get; set; }

    // Response
    public ResponseStatus ResponseStatus { get; set; }
    public DateTime? ResponseTime { get; set; }

    // Categories & Reminders
    public List<string> Categories { get; set; } = new();
    public List<EventReminderDto> Reminders { get; set; } = new();

    // Resources
    public List<EventResourceDto> Resources { get; set; } = new();

    // Attachments
    public List<EventAttachmentDto> Attachments { get; set; } = new();

    // Travel
    public int? TravelTimeMinutes { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsCancelled { get; set; }
}

public class CreateEventDto
{
    public int CalendarId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Location { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public string TimeZone { get; set; } = "UTC";

    // Recurrence
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }

    // Meeting
    public bool IsMeeting { get; set; }
    public bool IsOnlineMeeting { get; set; }
    public string? OnlineMeetingProvider { get; set; }
    public List<CreateAttendeeDto>? Attendees { get; set; }

    // Status
    public EventBusyStatus BusyStatus { get; set; } = EventBusyStatus.Busy;
    public EventImportance Importance { get; set; } = EventImportance.Normal;
    public EventSensitivity Sensitivity { get; set; } = EventSensitivity.Normal;

    // Categories & Reminders
    public List<string>? Categories { get; set; }
    public List<CreateReminderDto>? Reminders { get; set; }

    // Resources
    public List<int>? ResourceIds { get; set; }

    // Travel
    public int? TravelTimeMinutes { get; set; }
}

public class UpdateEventDto
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Location { get; set; }

    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool? IsAllDay { get; set; }
    public string? TimeZone { get; set; }

    // Recurrence
    public bool? IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }

    // Meeting
    public bool? IsOnlineMeeting { get; set; }
    public string? OnlineMeetingProvider { get; set; }

    // Status
    public EventBusyStatus? BusyStatus { get; set; }
    public EventImportance? Importance { get; set; }
    public EventSensitivity? Sensitivity { get; set; }

    // Travel
    public int? TravelTimeMinutes { get; set; }
}

public class EventAttendeeDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AttendeeType Type { get; set; }
    public ResponseStatus ResponseStatus { get; set; }
    public DateTime? ResponseTime { get; set; }
    public string? ResponseComment { get; set; }
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
}

public class CreateAttendeeDto
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public AttendeeType Type { get; set; } = AttendeeType.Required;
}

public class UpdateAttendeeResponseDto
{
    public ResponseStatus ResponseStatus { get; set; }
    public string? ResponseComment { get; set; }
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
}

public class EventReminderDto
{
    public int Id { get; set; }
    public int MinutesBeforeStart { get; set; }
    public ReminderMethod Method { get; set; }
    public bool IsTriggered { get; set; }
    public DateTime? TriggeredAt { get; set; }
}

public class CreateReminderDto
{
    public int MinutesBeforeStart { get; set; } = 15;
    public ReminderMethod Method { get; set; } = ReminderMethod.Notification;
}

public class EventResourceDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public ResourceStatus Status { get; set; }
}

public class EventAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ResourceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }

    // Location Details
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? RoomNumber { get; set; }
    public int? Capacity { get; set; }

    // Equipment
    public List<string> Equipment { get; set; } = new();

    public bool IsAvailable { get; set; }
    public string? Email { get; set; }
}

public class CreateResourceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? RoomNumber { get; set; }
    public int? Capacity { get; set; }
    public List<string>? Equipment { get; set; }
    public string? Email { get; set; }
}

public class CalendarQueryParameters
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? CalendarId { get; set; }
    public List<string>? Categories { get; set; }
    public EventStatus? Status { get; set; }
    public bool IncludeCancelled { get; set; } = false;
}

public class AvailabilityQueryDto
{
    public List<string> AttendeeEmails { get; set; } = new();
    public List<int>? ResourceIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MeetingDurationMinutes { get; set; } = 60;
}

public class AvailabilitySlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<string> AvailableAttendees { get; set; } = new();
    public List<string> UnavailableAttendees { get; set; } = new();
    public List<int> AvailableResources { get; set; } = new();
}
