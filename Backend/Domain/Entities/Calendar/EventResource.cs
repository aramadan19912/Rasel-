namespace Backend.Domain.Entities.Calendar;

public class EventResource
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public CalendarEvent? Event { get; set; }

    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }

    public ResourceStatus Status { get; set; } = ResourceStatus.Tentative;
}

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }

    // Location Details (for rooms)
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? RoomNumber { get; set; }
    public int? Capacity { get; set; }

    // Equipment Details
    public List<string> Equipment { get; set; } = new();

    // Availability
    public bool IsAvailable { get; set; } = true;
    public string? Email { get; set; }

    // Events
    public List<EventResource> EventResources { get; set; } = new();
}

public enum ResourceType
{
    Room = 0,
    Equipment = 1,
    Vehicle = 2,
    Other = 3
}

public enum ResourceStatus
{
    Tentative = 0,
    Accepted = 1,
    Declined = 2
}
