using OutlookInboxManagement.Domain.Common;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class DocumentFolder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;

    // Hierarchy
    public int? ParentFolderId { get; set; }
    public virtual DocumentFolder? ParentFolder { get; set; }
    public virtual ICollection<DocumentFolder> SubFolders { get; set; } = new List<DocumentFolder>();

    // Documents
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    // Ownership
    public string OwnerId { get; set; } = string.Empty;

    // Permissions
    public FolderAccessLevel AccessLevel { get; set; }
    public string? AllowedUsers { get; set; } // JSON array
    public string? AllowedRoles { get; set; } // JSON array
}

public enum FolderAccessLevel
{
    Public,
    Internal,
    Restricted,
    Private
}
