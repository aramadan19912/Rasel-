namespace Domain.Entities.Communication;

/// <summary>
/// External organizations/entities
/// الجهات الخارجية
/// </summary>
public class ExternalEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Entity code
    /// كود الجهة
    /// </summary>
    public string EntityCode { get; set; } = string.Empty;

    /// <summary>
    /// Entity name in Arabic
    /// اسم الجهة بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Entity name in English
    /// اسم الجهة بالإنجليزي
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Short name/abbreviation
    /// الاسم المختصر
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// Entity type (Government, Private, NGO, International)
    /// نوع الجهة
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Category (Ministry, Authority, Company, etc.)
    /// التصنيف
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Country
    /// الدولة
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// City
    /// المدينة
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Address
    /// العنوان
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Phone number
    /// رقم الهاتف
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Fax number
    /// رقم الفاكس
    /// </summary>
    public string? FaxNumber { get; set; }

    /// <summary>
    /// Email
    /// البريد الإلكتروني
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Website
    /// الموقع الإلكتروني
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Contact person name
    /// اسم جهة الاتصال
    /// </summary>
    public string? ContactPersonName { get; set; }

    /// <summary>
    /// Contact person position
    /// منصب جهة الاتصال
    /// </summary>
    public string? ContactPersonPosition { get; set; }

    /// <summary>
    /// Contact person phone
    /// هاتف جهة الاتصال
    /// </summary>
    public string? ContactPersonPhone { get; set; }

    /// <summary>
    /// Contact person email
    /// بريد جهة الاتصال
    /// </summary>
    public string? ContactPersonEmail { get; set; }

    /// <summary>
    /// Alternative contacts (JSON array)
    /// جهات اتصال بديلة
    /// </summary>
    public string? AlternativeContacts { get; set; }

    /// <summary>
    /// Relation type (Partner, Vendor, Client, Regulator, etc.)
    /// نوع العلاقة
    /// </summary>
    public string? RelationType { get; set; }

    /// <summary>
    /// Agreement/MOU exists
    /// وجود اتفاقية
    /// </summary>
    public bool HasAgreement { get; set; } = false;

    /// <summary>
    /// Agreement number
    /// رقم الاتفاقية
    /// </summary>
    public string? AgreementNumber { get; set; }

    /// <summary>
    /// Agreement date
    /// تاريخ الاتفاقية
    /// </summary>
    public DateTime? AgreementDate { get; set; }

    /// <summary>
    /// Agreement expiry date
    /// تاريخ انتهاء الاتفاقية
    /// </summary>
    public DateTime? AgreementExpiryDate { get; set; }

    /// <summary>
    /// Logo path
    /// مسار الشعار
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Notes
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tags (comma-separated)
    /// الوسوم
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Correspondence count with this entity
    /// عدد المراسلات
    /// </summary>
    public int CorrespondenceCount { get; set; } = 0;

    /// <summary>
    /// Last correspondence date
    /// آخر مراسلة
    /// </summary>
    public DateTime? LastCorrespondenceDate { get; set; }

    /// <summary>
    /// Is active
    /// نشطة
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is blocked
    /// محظورة
    /// </summary>
    public bool IsBlocked { get; set; } = false;

    /// <summary>
    /// Block reason
    /// سبب الحظر
    /// </summary>
    public string? BlockReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
