namespace Backend.Domain.Entities.Calendar;

public class EventAttachment
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public CalendarEvent? Event { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public byte[]? ContentBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
