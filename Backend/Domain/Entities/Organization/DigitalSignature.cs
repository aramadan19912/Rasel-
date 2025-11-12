using Domain.Entities.Organization;

namespace Domain.Entities.Organization;

/// <summary>
/// Digital signature management
/// إدارة التوقيع الرقمي
/// </summary>
public class DigitalSignature
{
    public int Id { get; set; }

    /// <summary>
    /// Employee
    /// الموظف
    /// </summary>
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// Signature type (Digital, Handwritten, Certificate)
    /// نوع التوقيع
    /// </summary>
    public string SignatureType { get; set; } = string.Empty;

    /// <summary>
    /// Signature image path
    /// مسار صورة التوقيع
    /// </summary>
    public string? SignatureImagePath { get; set; }

    /// <summary>
    /// Certificate path (for digital certificates)
    /// مسار الشهادة الرقمية
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Certificate thumbprint
    /// بصمة الشهادة
    /// </summary>
    public string? CertificateThumbprint { get; set; }

    /// <summary>
    /// Certificate issuer
    /// مصدر الشهادة
    /// </summary>
    public string? CertificateIssuer { get; set; }

    /// <summary>
    /// Certificate expiry date
    /// تاريخ انتهاء الشهادة
    /// </summary>
    public DateTime? CertificateExpiryDate { get; set; }

    /// <summary>
    /// PIN/Password (encrypted)
    /// الرمز السري
    /// </summary>
    public string? EncryptedPin { get; set; }

    /// <summary>
    /// Signature position (Top, Bottom, Custom)
    /// موضع التوقيع
    /// </summary>
    public string SignaturePosition { get; set; } = "Bottom";

    /// <summary>
    /// Position coordinates (JSON: {x, y, width, height})
    /// إحداثيات الموضع
    /// </summary>
    public string? PositionCoordinates { get; set; }

    /// <summary>
    /// Include timestamp
    /// تضمين الطابع الزمني
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Include employee name
    /// تضمين اسم الموظف
    /// </summary>
    public bool IncludeEmployeeName { get; set; } = true;

    /// <summary>
    /// Include job title
    /// تضمين المسمى الوظيفي
    /// </summary>
    public bool IncludeJobTitle { get; set; } = true;

    /// <summary>
    /// Is default signature
    /// التوقيع الافتراضي
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Requires two-factor authentication
    /// يتطلب مصادقة ثنائية
    /// </summary>
    public bool Requires2FA { get; set; } = false;

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

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is verified
    /// موثّق
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Verified by
    /// تم التوثيق بواسطة
    /// </summary>
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// Verification date
    /// تاريخ التوثيق
    /// </summary>
    public DateTime? VerificationDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
