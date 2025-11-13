namespace Domain.Entities.Organization;

/// <summary>
/// Organization/Building information
/// معلومات المؤسسة/الجهة
/// </summary>
public class OrganizationInfo
{
    public int Id { get; set; }

    /// <summary>
    /// Organization code
    /// كود المؤسسة
    /// </summary>
    public string OrganizationCode { get; set; } = string.Empty;

    /// <summary>
    /// Organization name in Arabic
    /// اسم المؤسسة بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Organization name in English
    /// اسم المؤسسة بالإنجليزي
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Short name/abbreviation
    /// الاسم المختصر
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// Organization type (Government, Private, NGO, etc.)
    /// نوع المؤسسة
    /// </summary>
    public string OrganizationType { get; set; } = string.Empty;

    /// <summary>
    /// Legal form (Corporation, LLC, etc.)
    /// الشكل القانوني
    /// </summary>
    public string? LegalForm { get; set; }

    /// <summary>
    /// Registration number
    /// رقم السجل
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Tax number
    /// الرقم الضريبي
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// Commercial registration number
    /// السجل التجاري
    /// </summary>
    public string? CommercialRegistrationNumber { get; set; }

    /// <summary>
    /// Establishment date
    /// تاريخ التأسيس
    /// </summary>
    public DateTime? EstablishmentDate { get; set; }

    /// <summary>
    /// Main address
    /// العنوان الرئيسي
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// المدينة
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province
    /// المنطقة/المحافظة
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Country
    /// الدولة
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Postal code
    /// الرمز البريدي
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Main phone number
    /// رقم الهاتف الرئيسي
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Fax number
    /// رقم الفاكس
    /// </summary>
    public string? FaxNumber { get; set; }

    /// <summary>
    /// Main email
    /// البريد الإلكتروني الرئيسي
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Website URL
    /// الموقع الإلكتروني
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Logo file path
    /// مسار الشعار
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Header logo for documents
    /// شعار رأس الصفحة
    /// </summary>
    public string? HeaderLogoPath { get; set; }

    /// <summary>
    /// Footer logo for documents
    /// شعار تذييل الصفحة
    /// </summary>
    public string? FooterLogoPath { get; set; }

    /// <summary>
    /// Official stamp image
    /// الختم الرسمي
    /// </summary>
    public string? OfficialStampPath { get; set; }

    /// <summary>
    /// Working hours
    /// ساعات العمل
    /// </summary>
    public string? WorkingHours { get; set; }

    /// <summary>
    /// Time zone
    /// المنطقة الزمنية
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Default language
    /// اللغة الافتراضية
    /// </summary>
    public string DefaultLanguage { get; set; } = "ar";

    /// <summary>
    /// Fiscal year start month (1-12)
    /// شهر بداية السنة المالية
    /// </summary>
    public int FiscalYearStartMonth { get; set; } = 1;

    /// <summary>
    /// Currency code
    /// رمز العملة
    /// </summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>
    /// Vision statement
    /// الرؤية
    /// </summary>
    public string? Vision { get; set; }

    /// <summary>
    /// Mission statement
    /// الرسالة
    /// </summary>
    public string? Mission { get; set; }

    /// <summary>
    /// Values
    /// القيم
    /// </summary>
    public string? Values { get; set; }

    /// <summary>
    /// Social media links (JSON)
    /// روابط التواصل الاجتماعي
    /// </summary>
    public string? SocialMediaLinks { get; set; }

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is headquarters
    /// المقر الرئيسي
    /// </summary>
    public bool IsHeadquarters { get; set; } = true;

    /// <summary>
    /// Parent organization
    /// المؤسسة الأم
    /// </summary>
    public int? ParentOrganizationId { get; set; }
    public OrganizationInfo? ParentOrganization { get; set; }

    /// <summary>
    /// Sub-organizations/branches
    /// الفروع/المؤسسات الفرعية
    /// </summary>
    public ICollection<OrganizationInfo> SubOrganizations { get; set; } = new List<OrganizationInfo>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
