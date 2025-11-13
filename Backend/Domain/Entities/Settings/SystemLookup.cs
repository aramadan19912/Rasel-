namespace Domain.Entities.Settings;

/// <summary>
/// System lookup/dropdown values manager
/// إدارة القوائم المنسدلة
/// </summary>
public class SystemLookup
{
    public int Id { get; set; }

    /// <summary>
    /// Lookup type/category (e.g., "Priority", "Status", "Country")
    /// نوع القائمة
    /// </summary>
    public string LookupType { get; set; } = string.Empty;

    /// <summary>
    /// Lookup code (unique within type)
    /// كود القيمة
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name in Arabic
    /// الاسم بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Display name in English
    /// الاسم بالإنجليزي
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Parent lookup (for hierarchical lookups)
    /// القيمة الأب
    /// </summary>
    public int? ParentLookupId { get; set; }
    public SystemLookup? ParentLookup { get; set; }

    /// <summary>
    /// Child lookups
    /// القيم الفرعية
    /// </summary>
    public ICollection<SystemLookup> ChildLookups { get; set; } = new List<SystemLookup>();

    /// <summary>
    /// Sort order
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Icon/image
    /// أيقونة
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color (hex code)
    /// لون
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Additional data (JSON)
    /// بيانات إضافية
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Is system defined (cannot be deleted)
    /// معرف من النظام
    /// </summary>
    public bool IsSystemDefined { get; set; } = false;

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is default value for this type
    /// قيمة افتراضية
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
