namespace Domain.Entities.Archive;

/// <summary>
/// Attachments for correspondence
/// مرفقات المراسلة
/// </summary>
public class CorrespondenceAttachment
{
    public int Id { get; set; }

    /// <summary>
    /// Associated correspondence
    /// المراسلة المرتبطة
    /// </summary>
    public int CorrespondenceId { get; set; }
    public Correspondence Correspondence { get; set; } = null!;

    /// <summary>
    /// File name
    /// اسم الملف
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Original file name
    /// الاسم الأصلي
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// File path/URL
    /// مسار الملف
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// حجم الملف
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type
    /// نوع الملف
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// File extension
    /// امتداد الملف
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Version number
    /// رقم النسخة
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Is main document
    /// هل هو المستند الرئيسي
    /// </summary>
    public bool IsMainDocument { get; set; } = false;

    /// <summary>
    /// Sort order
    /// الترتيب
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
