using Domain.Enums;

namespace Domain.Entities.Archive;

/// <summary>
/// Dynamic form templates for correspondence
/// قوالب الفورمز الديناميكية
/// </summary>
public class CorrespondenceForm
{
    public int Id { get; set; }

    /// <summary>
    /// Form code (e.g., "FRM-CONT-001")
    /// كود الفورم
    /// </summary>
    public string FormCode { get; set; } = string.Empty;

    /// <summary>
    /// Form name in Arabic
    /// اسم الفورم بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Form name in English
    /// اسم الفورم بالإنجليزي
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Description in Arabic
    /// الوصف بالعربي
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Description in English
    /// الوصف بالإنجليزي
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Associated archive category
    /// التصنيف المرتبط
    /// </summary>
    public int CategoryId { get; set; }
    public ArchiveCategory Category { get; set; } = null!;

    /// <summary>
    /// Default archive classification
    /// التصنيف الافتراضي
    /// </summary>
    public string DefaultClassification { get; set; } = string.Empty;

    /// <summary>
    /// Form fields
    /// حقول الفورم
    /// </summary>
    public ICollection<FormField> Fields { get; set; } = new List<FormField>();

    /// <summary>
    /// Form submissions
    /// التعبئات
    /// </summary>
    public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();

    /// <summary>
    /// Form version
    /// إصدار الفورم
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Is published (active for use)
    /// منشور
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Allow anonymous submissions
    /// السماح بالتعبئة دون تسجيل دخول
    /// </summary>
    public bool AllowAnonymous { get; set; } = false;

    /// <summary>
    /// Require approval after submission
    /// يتطلب موافقة بعد التعبئة
    /// </summary>
    public bool RequireApproval { get; set; } = true;

    /// <summary>
    /// Auto-routing rules (JSON)
    /// قواعد الإحالة التلقائية
    /// </summary>
    public string? AutoRoutingRules { get; set; }

    /// <summary>
    /// Email notifications on submission
    /// إشعارات البريد عند التعبئة
    /// </summary>
    public string? NotificationEmails { get; set; }

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
