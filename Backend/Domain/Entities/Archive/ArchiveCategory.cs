using Domain.Enums;

namespace Domain.Entities.Archive;

/// <summary>
/// Hierarchical archive classification categories
/// تصنيفات الأرشيف الهرمية
/// </summary>
public class ArchiveCategory
{
    public int Id { get; set; }

    /// <summary>
    /// Category code (e.g., "CONT-001")
    /// كود التصنيف
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Category name in Arabic
    /// اسم التصنيف بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Category name in English
    /// اسم التصنيف بالإنجليزي
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Archive classification type
    /// نوع التصنيف
    /// </summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>
    /// Parent category for hierarchical structure
    /// التصنيف الأب
    /// </summary>
    public int? ParentCategoryId { get; set; }
    public ArchiveCategory? ParentCategory { get; set; }

    /// <summary>
    /// Sub-categories
    /// التصنيفات الفرعية
    /// </summary>
    public ICollection<ArchiveCategory> SubCategories { get; set; } = new List<ArchiveCategory>();

    /// <summary>
    /// Default retention period
    /// فترة الحفظ الافتراضية
    /// </summary>
    public string RetentionPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Icon for UI display
    /// أيقونة للعرض
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color for UI display
    /// لون للعرض
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Sort order
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Associated correspondences
    /// المراسلات المرتبطة
    /// </summary>
    public ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
