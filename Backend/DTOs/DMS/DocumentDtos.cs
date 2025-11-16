using OutlookInboxManagement.Domain.Entities.DMS;

namespace OutlookInboxManagement.DTOs.DMS;

// Document DTOs
public class DocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentType DocumentType { get; set; }
    public DocumentCategory Category { get; set; }
    public List<string> Tags { get; set; } = new();

    public int CurrentVersion { get; set; }
    public bool IsLocked { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedAt { get; set; }

    public DocumentAccessLevel AccessLevel { get; set; }
    public int? CorrespondenceId { get; set; }
    public int? FolderId { get; set; }
    public string? FolderPath { get; set; }

    public string OwnerId { get; set; } = string.Empty;
    public string? OwnerName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<DocumentVersionDto> Versions { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CreateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentCategory Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public DocumentAccessLevel AccessLevel { get; set; } = DocumentAccessLevel.Internal;
    public int? CorrespondenceId { get; set; }
    public int? FolderId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateDocumentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DocumentCategory? Category { get; set; }
    public List<string>? Tags { get; set; }
    public DocumentAccessLevel? AccessLevel { get; set; }
    public int? FolderId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

// Document Version DTOs
public class DocumentVersionDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string VersionComment { get; set; } = string.Empty;
    public VersionChangeType ChangeType { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateVersionDto
{
    public string VersionComment { get; set; } = string.Empty;
    public VersionChangeType ChangeType { get; set; }
}

// Document Annotation DTOs
public class DocumentAnnotationDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int? VersionId { get; set; }
    public AnnotationType Type { get; set; }
    public int PageNumber { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Color { get; set; }
    public double? Opacity { get; set; }
    public double? FontSize { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ParentAnnotationId { get; set; }
    public List<DocumentAnnotationDto> Replies { get; set; } = new();
}

public class CreateAnnotationDto
{
    public int? VersionId { get; set; }
    public AnnotationType Type { get; set; }
    public int PageNumber { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Color { get; set; }
    public double? Opacity { get; set; }
    public double? FontSize { get; set; }
    public int? ParentAnnotationId { get; set; }
}

// Document Activity DTOs
public class DocumentActivityDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int? VersionId { get; set; }
    public DocumentActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Folder DTOs
public class DocumentFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public FolderAccessLevel AccessLevel { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public int DocumentCount { get; set; }
    public int SubFolderCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DocumentFolderDto> SubFolders { get; set; } = new();
}

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentFolderId { get; set; }
    public FolderAccessLevel AccessLevel { get; set; } = FolderAccessLevel.Internal;
}

// Search and Filter DTOs
public class DocumentSearchDto
{
    public string? SearchTerm { get; set; }
    public List<DocumentType>? DocumentTypes { get; set; }
    public List<DocumentCategory>? Categories { get; set; }
    public List<string>? Tags { get; set; }
    public int? FolderId { get; set; }
    public int? CorrespondenceId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? OwnerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DocumentSearchResultDto
{
    public List<DocumentDto> Documents { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
