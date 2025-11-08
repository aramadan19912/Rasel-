using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.DTOs;

public class MessageFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public List<MessageFolderDto> SubFolders { get; set; } = new();
    public FolderType Type { get; set; }
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string Color { get; set; } = "#0078D4";
    public string Icon { get; set; } = "folder";
    public int DisplayOrder { get; set; }
}

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public string Color { get; set; } = "#0078D4";
    public string Icon { get; set; } = "folder";
}

public class UpdateFolderDto
{
    public string? DisplayName { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
}
