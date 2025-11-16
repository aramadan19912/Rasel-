using OutlookInboxManagement.Domain.Common;

namespace OutlookInboxManagement.Domain.Entities.DMS;

public class DocumentAnnotation : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;

    public int? VersionId { get; set; }
    public virtual DocumentVersion? Version { get; set; }

    // Annotation Details
    public AnnotationType Type { get; set; }
    public int PageNumber { get; set; } = 1;

    // Position and Size (in PDF coordinates)
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    // Content
    public string Content { get; set; } = string.Empty;
    public string? Color { get; set; } // Hex color code
    public double? Opacity { get; set; }
    public double? FontSize { get; set; }

    // Author
    public string CreatedBy { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }

    // Reply/Thread
    public int? ParentAnnotationId { get; set; }
    public virtual DocumentAnnotation? ParentAnnotation { get; set; }
    public virtual ICollection<DocumentAnnotation> Replies { get; set; } = new List<DocumentAnnotation>();
}

public enum AnnotationType
{
    Text,
    Highlight,
    Underline,
    Strikethrough,
    FreeHand,
    Rectangle,
    Circle,
    Arrow,
    Stamp,
    Image,
    Comment
}
