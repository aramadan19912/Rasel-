namespace Domain.Entities.Organization;

/// <summary>
/// System license information
/// معلومات ترخيص النظام
/// </summary>
public class LicenseInfo
{
    public int Id { get; set; }

    /// <summary>
    /// License number
    /// رقم الترخيص
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// License key
    /// مفتاح الترخيص
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// License type (Trial, Standard, Enterprise, Government)
    /// نوع الترخيص
    /// </summary>
    public string LicenseType { get; set; } = string.Empty;

    /// <summary>
    /// Organization name
    /// اسم المؤسسة
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Issued to (contact person)
    /// صادر لصالح
    /// </summary>
    public string IssuedTo { get; set; } = string.Empty;

    /// <summary>
    /// Issued date
    /// تاريخ الإصدار
    /// </summary>
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Activation date
    /// تاريخ التفعيل
    /// </summary>
    public DateTime? ActivationDate { get; set; }

    /// <summary>
    /// Expiry date
    /// تاريخ الانتهاء
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Is perpetual (never expires)
    /// ترخيص دائم
    /// </summary>
    public bool IsPerpetual { get; set; } = false;

    /// <summary>
    /// Max users allowed
    /// عدد المستخدمين المسموح
    /// </summary>
    public int MaxUsers { get; set; } = 10;

    /// <summary>
    /// Max concurrent sessions
    /// عدد الجلسات المتزامنة
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 10;

    /// <summary>
    /// Max storage (GB)
    /// مساحة التخزين القصوى
    /// </summary>
    public int MaxStorageGB { get; set; } = 100;

    /// <summary>
    /// Enabled modules (JSON array)
    /// الوحدات المفعلة
    /// </summary>
    public string EnabledModules { get; set; } = "[]";

    /// <summary>
    /// Enabled features (JSON array)
    /// المميزات المفعلة
    /// </summary>
    public string EnabledFeatures { get; set; } = "[]";

    /// <summary>
    /// Support level (Basic, Standard, Premium)
    /// مستوى الدعم
    /// </summary>
    public string SupportLevel { get; set; } = "Basic";

    /// <summary>
    /// Support expiry date
    /// تاريخ انتهاء الدعم
    /// </summary>
    public DateTime? SupportExpiryDate { get; set; }

    /// <summary>
    /// Maintenance expiry date
    /// تاريخ انتهاء الصيانة
    /// </summary>
    public DateTime? MaintenanceExpiryDate { get; set; }

    /// <summary>
    /// Hardware ID (for license binding)
    /// معرف الجهاز
    /// </summary>
    public string? HardwareId { get; set; }

    /// <summary>
    /// Server domain (for license binding)
    /// نطاق الخادم
    /// </summary>
    public string? ServerDomain { get; set; }

    /// <summary>
    /// IP addresses allowed (comma-separated)
    /// عناوين IP المسموحة
    /// </summary>
    public string? AllowedIpAddresses { get; set; }

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is trial
    /// نسخة تجريبية
    /// </summary>
    public bool IsTrial { get; set; } = false;

    /// <summary>
    /// Trial days remaining
    /// الأيام المتبقية للتجربة
    /// </summary>
    public int? TrialDaysRemaining { get; set; }

    /// <summary>
    /// License status (Active, Expired, Suspended, Revoked)
    /// حالة الترخيص
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Vendor/Issuer name
    /// الجهة المصدرة
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Vendor contact
    /// جهة الاتصال
    /// </summary>
    public string? VendorContact { get; set; }

    /// <summary>
    /// License agreement URL
    /// رابط اتفاقية الترخيص
    /// </summary>
    public string? LicenseAgreementUrl { get; set; }

    /// <summary>
    /// Notes
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Last validation date
    /// آخر تحقق
    /// </summary>
    public DateTime? LastValidationDate { get; set; }

    /// <summary>
    /// Validation signature
    /// توقيع التحقق
    /// </summary>
    public string? ValidationSignature { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
