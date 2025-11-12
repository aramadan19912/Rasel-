namespace Domain.Entities.Settings;

/// <summary>
/// Print templates for documents
/// قوالب الطباعة
/// </summary>
public class PrintTemplate
{
    public int Id { get; set; }

    /// <summary>
    /// Template code
    /// كود القالب
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// Template name in Arabic
    /// اسم القالب بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Template name in English
    /// اسم القالب بالإنجليزي
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Template type (Correspondence, Report, Certificate, etc.)
    /// نوع القالب
    /// </summary>
    public string TemplateType { get; set; } = string.Empty;

    /// <summary>
    /// HTML template content
    /// محتوى القالب HTML
    /// </summary>
    public string HtmlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// CSS styles
    /// التنسيقات
    /// </summary>
    public string? CssStyles { get; set; }

    /// <summary>
    /// Header HTML
    /// رأس الصفحة
    /// </summary>
    public string? HeaderHtml { get; set; }

    /// <summary>
    /// Footer HTML
    /// تذييل الصفحة
    /// </summary>
    public string? FooterHtml { get; set; }

    /// <summary>
    /// Paper size (A4, Letter, Legal, A3)
    /// حجم الورق
    /// </summary>
    public string PaperSize { get; set; } = "A4";

    /// <summary>
    /// Orientation (Portrait, Landscape)
    /// الاتجاه
    /// </summary>
    public string Orientation { get; set; } = "Portrait";

    /// <summary>
    /// Margins (JSON: {top, right, bottom, left})
    /// الهوامش
    /// </summary>
    public string? Margins { get; set; }

    /// <summary>
    /// Include header
    /// تضمين الرأس
    /// </summary>
    public bool IncludeHeader { get; set; } = true;

    /// <summary>
    /// Include footer
    /// تضمين التذييل
    /// </summary>
    public bool IncludeFooter { get; set; } = true;

    /// <summary>
    /// Include page numbers
    /// تضمين أرقام الصفحات
    /// </summary>
    public bool IncludePageNumbers { get; set; } = true;

    /// <summary>
    /// Page number format
    /// صيغة رقم الصفحة
    /// </summary>
    public string? PageNumberFormat { get; set; }

    /// <summary>
    /// Include watermark
    /// تضمين علامة مائية
    /// </summary>
    public bool IncludeWatermark { get; set; } = false;

    /// <summary>
    /// Watermark text
    /// نص العلامة المائية
    /// </summary>
    public string? WatermarkText { get; set; }

    /// <summary>
    /// Watermark opacity (0-1)
    /// شفافية العلامة المائية
    /// </summary>
    public decimal WatermarkOpacity { get; set; } = 0.3m;

    /// <summary>
    /// Available variables/placeholders (JSON array)
    /// المتغيرات المتاحة
    /// </summary>
    public string? AvailableVariables { get; set; }

    /// <summary>
    /// Sample data for preview (JSON)
    /// بيانات نموذجية للمعاينة
    /// </summary>
    public string? SampleData { get; set; }

    /// <summary>
    /// Is default for this type
    /// افتراضي لهذا النوع
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Is system template (cannot be deleted)
    /// قالب نظام
    /// </summary>
    public bool IsSystemTemplate { get; set; } = false;

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Usage count
    /// عدد مرات الاستخدام
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Last used date
    /// آخر استخدام
    /// </summary>
    public DateTime? LastUsedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
