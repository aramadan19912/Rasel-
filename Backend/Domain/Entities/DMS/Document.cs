using OutlookInboxManagement.Domain.Common;
using OutlookInboxManagement.Domain.Enums;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;

    // Document Classification
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentType DocumentType { get; set; }
    public DocumentCategory Category { get; set; }
    public string? Tags { get; set; } // Comma-separated tags

    // Versioning
    public int CurrentVersion { get; set; } = 1;
    public bool IsLocked { get; set; } = false;
    public string? LockedBy { get; set; }
    public DateTime? LockedAt { get; set; }

    // Security
    public DocumentAccessLevel AccessLevel { get; set; }
    public string? AllowedUsers { get; set; } // JSON array of user IDs
    public string? AllowedRoles { get; set; } // JSON array of role names

    // Related Entities
    public int? CorrespondenceId { get; set; }
    public virtual Archive.Correspondence? Correspondence { get; set; }

    public int? FolderId { get; set; }
    public virtual DocumentFolder? Folder { get; set; }

    // Owner
    public string OwnerId { get; set; } = string.Empty;

    // Versions
    public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();

    // Metadata
    public virtual ICollection<DocumentMetadata> Metadata { get; set; } = new List<DocumentMetadata>();

    // Annotations (for PDF)
    public virtual ICollection<DocumentAnnotation> Annotations { get; set; } = new List<DocumentAnnotation>();

    // Activity Log
    public virtual ICollection<DocumentActivity> Activities { get; set; } = new List<DocumentActivity>();
}

public enum DocumentType
{
    Word,
    Excel,
    PowerPoint,
    PDF,
    Image,
    Text,
    Other
}

public enum DocumentCategory
{
    Correspondence,
    Contract,
    Invoice,
    Report,
    Form,
    Policy,
    Procedure,
    General
}

public enum DocumentAccessLevel
{
    Public,
    Internal,
    Restricted,
    Confidential,
    Secret
}
