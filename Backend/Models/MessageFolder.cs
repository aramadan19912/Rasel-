namespace OutlookInboxManagement.Models;

public class MessageFolder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public MessageFolder? ParentFolder { get; set; }
    public List<MessageFolder> SubFolders { get; set; } = new();
    public FolderType Type { get; set; }
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public string Color { get; set; } = "#0078D4";
    public string Icon { get; set; } = "folder";
    public int DisplayOrder { get; set; }

    // Navigation
    public List<Message> Messages { get; set; } = new();
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
