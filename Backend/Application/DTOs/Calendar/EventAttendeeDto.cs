namespace Backend.Application.DTOs.Calendar;

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

public class CreateEventAttendeeDto
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AttendeeType Type { get; set; } = AttendeeType.Required;
}

public class UpdateAttendeeResponseDto
{
    public ResponseStatus ResponseStatus { get; set; }
    public string? ResponseComment { get; set; }
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
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
