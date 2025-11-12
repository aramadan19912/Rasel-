namespace Application.DTOs.Archive;

/// <summary>
/// Correspondence attachment DTO
/// </summary>
public class CorrespondenceAttachmentDto
{
    public int Id { get; set; }
    public int CorrespondenceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsMainDocument { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Upload attachment request
/// </summary>
public class UploadAttachmentRequest
{
    public int CorrespondenceId { get; set; }
    public string? Description { get; set; }
    public bool IsMainDocument { get; set; } = false;
}
