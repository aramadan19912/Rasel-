using Domain.Enums;
using OutlookInboxManagement.Models;
using Domain.Entities.Organization;

namespace Domain.Entities.Archive;

/// <summary>
/// Main correspondence/document entity
/// المراسلة الرئيسية
/// </summary>
public class Correspondence
{
    public int Id { get; set; }

    /// <summary>
    /// Unique reference number (e.g., "CORR-2024-0001")
    /// الرقم المرجعي الفريد
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Subject in Arabic
    /// الموضوع بالعربي
    /// </summary>
    public string SubjectAr { get; set; } = string.Empty;

    /// <summary>
    /// Subject in English
    /// الموضوع بالإنجليزي
    /// </summary>
    public string? SubjectEn { get; set; }

    /// <summary>
    /// Content/body in Arabic
    /// المحتوى بالعربي
    /// </summary>
    public string ContentAr { get; set; } = string.Empty;

    /// <summary>
    /// Content/body in English
    /// المحتوى بالإنجليزي
    /// </summary>
    public string? ContentEn { get; set; }

    /// <summary>
    /// Archive category
    /// التصنيف
    /// </summary>
    public int CategoryId { get; set; }
    public ArchiveCategory Category { get; set; } = null!;

    /// <summary>
    /// Status
    /// الحالة
    /// </summary>
    public string Status { get; set; } = CorrespondenceStatus.Draft.ToString();

    /// <summary>
    /// Priority
    /// الأولوية
    /// </summary>
    public string Priority { get; set; } = CorrespondencePriority.Normal.ToString();

    /// <summary>
    /// Confidentiality level
    /// مستوى السرية
    /// </summary>
    public string ConfidentialityLevel { get; set; } = Enums.ConfidentialityLevel.Internal.ToString();

    /// <summary>
    /// Correspondence date
    /// تاريخ المراسلة
    /// </summary>
    public DateTime CorrespondenceDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Due date
    /// تاريخ الاستحقاق
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// From (sender) - Employee or external
    /// من (المرسل)
    /// </summary>
    public int? FromEmployeeId { get; set; }
    public Employee? FromEmployee { get; set; }

    /// <summary>
    /// External sender name (if not employee)
    /// اسم المرسل الخارجي
    /// </summary>
    public string? ExternalSenderName { get; set; }

    /// <summary>
    /// External sender organization
    /// جهة المرسل الخارجية
    /// </summary>
    public string? ExternalSenderOrganization { get; set; }

    /// <summary>
    /// To (primary recipient) - Department or Employee
    /// إلى (المستلم الرئيسي)
    /// </summary>
    public int? ToDepartmentId { get; set; }
    public Department? ToDepartment { get; set; }

    public int? ToEmployeeId { get; set; }
    public Employee? ToEmployee { get; set; }

    /// <summary>
    /// Associated form (if created from form)
    /// الفورم المرتبط
    /// </summary>
    public int? FormId { get; set; }
    public CorrespondenceForm? Form { get; set; }

    /// <summary>
    /// Form submission data (if created from form)
    /// بيانات الفورم المعبأ
    /// </summary>
    public int? FormSubmissionId { get; set; }
    public FormSubmission? FormSubmission { get; set; }

    /// <summary>
    /// Related to another correspondence (reply, follow-up)
    /// مرتبط بمراسلة أخرى
    /// </summary>
    public int? RelatedCorrespondenceId { get; set; }
    public Correspondence? RelatedCorrespondence { get; set; }

    /// <summary>
    /// Attachments
    /// المرفقات
    /// </summary>
    public ICollection<CorrespondenceAttachment> Attachments { get; set; } = new List<CorrespondenceAttachment>();

    /// <summary>
    /// Routing history (إحالات)
    /// سجل الإحالات
    /// </summary>
    public ICollection<CorrespondenceRouting> Routings { get; set; } = new List<CorrespondenceRouting>();

    /// <summary>
    /// Archived PDF document
    /// المستند المؤرشف
    /// </summary>
    public int? ArchivedDocumentId { get; set; }
    public ArchiveDocument? ArchivedDocument { get; set; }

    /// <summary>
    /// Keywords for search (comma-separated)
    /// كلمات مفتاحية للبحث
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Tags (comma-separated)
    /// الوسوم
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Notes
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Is archived
    /// مؤرشف
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Archive date
    /// تاريخ الأرشفة
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>
    /// Archived by user
    /// من قام بالأرشفة
    /// </summary>
    public string? ArchivedBy { get; set; }

    /// <summary>
    /// Is deleted (soft delete)
    /// محذوف
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
