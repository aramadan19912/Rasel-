using Domain.Enums;

namespace Domain.Entities.Archive;

/// <summary>
/// Archived PDF document with metadata
/// المستند المؤرشف مع البيانات الوصفية
/// </summary>
public class ArchiveDocument
{
    public int Id { get; set; }

    /// <summary>
    /// Archive reference number
    /// الرقم المرجعي للأرشيف
    /// </summary>
    public string ArchiveNumber { get; set; } = string.Empty;

    /// <summary>
    /// Associated correspondence
    /// المراسلة المرتبطة
    /// </summary>
    public int CorrespondenceId { get; set; }
    public Correspondence Correspondence { get; set; } = null!;

    /// <summary>
    /// PDF file path
    /// مسار ملف PDF
    /// </summary>
    public string PdfFilePath { get; set; } = string.Empty;

    /// <summary>
    /// PDF file name
    /// اسم الملف
    /// </summary>
    public string PdfFileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// حجم الملف
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Number of pages
    /// عدد الصفحات
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// PDF/A compliant (for long-term archiving)
    /// متوافق مع PDF/A
    /// </summary>
    public bool IsPdfA { get; set; } = false;

    /// <summary>
    /// Metadata (JSON) - includes all correspondence data
    /// البيانات الوصفية
    /// </summary>
    public string Metadata { get; set; } = string.Empty;

    /// <summary>
    /// Searchable text content extracted from PDF
    /// النص القابل للبحث
    /// </summary>
    public string? SearchableContent { get; set; }

    /// <summary>
    /// Checksum/hash for integrity verification
    /// قيمة التحقق
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Retention period
    /// فترة الحفظ
    /// </summary>
    public string RetentionPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Destruction date (based on retention period)
    /// تاريخ الإتلاف المقرر
    /// </summary>
    public DateTime? DestructionDate { get; set; }

    /// <summary>
    /// Legal hold (prevent destruction)
    /// حجز قانوني
    /// </summary>
    public bool IsOnLegalHold { get; set; } = false;

    /// <summary>
    /// Legal hold reason
    /// سبب الحجز القانوني
    /// </summary>
    public string? LegalHoldReason { get; set; }

    /// <summary>
    /// Access count
    /// عدد مرات الوصول
    /// </summary>
    public int AccessCount { get; set; } = 0;

    /// <summary>
    /// Last accessed date
    /// آخر وصول
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Watermark applied
    /// العلامة المائية مطبقة
    /// </summary>
    public bool HasWatermark { get; set; } = false;

    /// <summary>
    /// Digital signature applied
    /// التوقيع الرقمي مطبق
    /// </summary>
    public bool HasDigitalSignature { get; set; } = false;

    /// <summary>
    /// Is encrypted
    /// مشفر
    /// </summary>
    public bool IsEncrypted { get; set; } = false;

    /// <summary>
    /// Archive location (physical or digital storage location)
    /// موقع الأرشيف
    /// </summary>
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Backup location
    /// موقع النسخ الاحتياطي
    /// </summary>
    public string? BackupLocation { get; set; }

    /// <summary>
    /// Last backup date
    /// آخر نسخ احتياطي
    /// </summary>
    public DateTime? LastBackupAt { get; set; }

    /// <summary>
    /// Is destroyed
    /// تم إتلافه
    /// </summary>
    public bool IsDestroyed { get; set; } = false;

    /// <summary>
    /// Destruction date
    /// تاريخ الإتلاف الفعلي
    /// </summary>
    public DateTime? DestroyedAt { get; set; }

    /// <summary>
    /// Destroyed by
    /// من قام بالإتلاف
    /// </summary>
    public string? DestroyedBy { get; set; }

    /// <summary>
    /// Destruction notes
    /// ملاحظات الإتلاف
    /// </summary>
    public string? DestructionNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
