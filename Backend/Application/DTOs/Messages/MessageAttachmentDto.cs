namespace Backend.Application.DTOs.Messages;

public class MessageAttachmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}

public class CreateMessageAttachmentDto
{
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}
