namespace Backend.Application.DTOs.Messages;

public class MessageFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public string? ParentFolderName { get; set; }
    public FolderType Type { get; set; }
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string Color { get; set; } = "#0078D4";
    public string Icon { get; set; } = "folder";
    public int DisplayOrder { get; set; }
    public List<MessageFolderDto> SubFolders { get; set; } = new();
}

public class CreateMessageFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public string Color { get; set; } = "#0078D4";
    public string Icon { get; set; } = "folder";
}

public class UpdateMessageFolderDto
{
    public string? DisplayName { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
}

public enum FolderType
{
    Inbox = 1,
    Drafts = 2,
    Sent = 3,
    Deleted = 4,
    Junk = 5,
    Archive = 6,
    Outbox = 7,
    Custom = 99
}
