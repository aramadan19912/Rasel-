namespace OutlookInboxManagement.Models;

public class EventAttendee
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public CalendarEvent? Event { get; set; }

    // Attendee Info
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // Type
    public AttendeeType Type { get; set; } = AttendeeType.Required;

    // Response
    public ResponseStatus ResponseStatus { get; set; } = ResponseStatus.None;
    public DateTime? ResponseTime { get; set; }
    public string? ResponseComment { get; set; }

    // Proposed Time
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }

    // User Reference
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}

public enum AttendeeType
{
    Required = 0,
    Optional = 1,
    Resource = 2,
    Organizer = 3
}

public enum ResponseStatus
{
    None = 0,
    Accepted = 1,
    Declined = 2,
    Tentative = 3,
    NotResponded = 4
}
