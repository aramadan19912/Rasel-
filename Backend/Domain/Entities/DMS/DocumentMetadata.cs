using OutlookInboxManagement.Domain.Common;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class DocumentMetadata : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public MetadataType Type { get; set; }
}

public enum MetadataType
{
    String,
    Number,
    Date,
    Boolean,
    JSON
}
