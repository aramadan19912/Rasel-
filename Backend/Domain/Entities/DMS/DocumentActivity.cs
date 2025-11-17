using OutlookInboxManagement.Domain.Common;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class DocumentActivity : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;

    public int? VersionId { get; set; }
    public virtual DocumentVersion? Version { get; set; }

    // Activity Details
    public DocumentActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;

    // User Information
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Additional Data
    public string? AdditionalData { get; set; } // JSON for extra info
}

public enum DocumentActivityType
{
    Created,
    Viewed,
    Downloaded,
    Edited,
    VersionCreated,
    Renamed,
    Moved,
    Deleted,
    Restored,
    Shared,
    Locked,
    Unlocked,
    PermissionsChanged,
    Annotated,
    CommentAdded,
    MetadataChanged
}
