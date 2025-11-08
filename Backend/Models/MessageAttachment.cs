namespace OutlookInboxManagement.Models;

public class MessageAttachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
    public byte[]? ContentBytes { get; set; }
}
