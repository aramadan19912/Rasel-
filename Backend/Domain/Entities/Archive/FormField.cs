using Domain.Enums;

namespace Domain.Entities.Archive;

/// <summary>
/// Dynamic form field definition
/// تعريف حقل الفورم
/// </summary>
public class FormField
{
    public int Id { get; set; }

    /// <summary>
    /// Associated form
    /// الفورم المرتبط
    /// </summary>
    public int FormId { get; set; }
    public CorrespondenceForm Form { get; set; } = null!;

    /// <summary>
    /// Field name/key (used in code)
    /// اسم الحقل
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Field label in Arabic
    /// التسمية بالعربي
    /// </summary>
    public string LabelAr { get; set; } = string.Empty;

    /// <summary>
    /// Field label in English
    /// التسمية بالإنجليزي
    /// </summary>
    public string? LabelEn { get; set; }

    /// <summary>
    /// Field type
    /// نوع الحقل
    /// </summary>
    public string FieldType { get; set; } = FormFieldType.Text.ToString();

    /// <summary>
    /// Placeholder text
    /// نص توضيحي
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Help text/description
    /// نص المساعدة
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Default value
    /// القيمة الافتراضية
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Options for dropdown/radio/checkbox (JSON array)
    /// خيارات القائمة المنسدلة
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Validation rules (JSON)
    /// قواعد التحقق
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// Is required field
    /// حقل مطلوب
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Is read-only
    /// للقراءة فقط
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Is visible
    /// ظاهر
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Min length (for text fields)
    /// الحد الأدنى
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Max length (for text fields)
    /// الحد الأقصى
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Min value (for number fields)
    /// القيمة الدنيا
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Max value (for number fields)
    /// القيمة القصوى
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Pattern/regex for validation
    /// نمط التحقق
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// CSS class for styling
    /// فئة CSS
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Column width (1-12 for grid system)
    /// عرض العمود
    /// </summary>
    public int ColumnWidth { get; set; } = 12;

    /// <summary>
    /// Sort order
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Conditional display rules (JSON)
    /// قواعد العرض الشرطية
    /// </summary>
    public string? ConditionalRules { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
