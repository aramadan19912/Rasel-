using OutlookInboxManagement.Domain.Common;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class DocumentVersion : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty; // SHA256 hash for integrity

    // Version Information
    public string VersionComment { get; set; } = string.Empty;
    public VersionChangeType ChangeType { get; set; }

    // Creator
    public string CreatedBy { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }

    // Restore capability
    public bool IsActive { get; set; } = true;
}

public enum VersionChangeType
{
    Created,
    MinorEdit,
    MajorEdit,
    Annotation,
    ImageEdit,
    Restored
}
