namespace Domain.Enums;

/// <summary>
/// Classification types for archived documents
/// تصنيفات المستندات المؤرشفة
/// </summary>
public enum ArchiveClassification
{
    Contract = 1,           // عقود
    Complaint = 2,          // شكاوى
    Meeting = 3,            // اجتماعات
    Memorandum = 4,         // مذكرات
    Decision = 5,           // قرارات
    Report = 6,             // تقارير
    Invoice = 7,            // فواتير
    PurchaseOrder = 8,      // أوامر شراء
    HR_Document = 9,        // مستندات موارد بشرية
    Legal = 10,             // قانونية
    Financial = 11,         // مالية
    Technical = 12,         // فنية
    Administrative = 13,    // إدارية
    Correspondence = 14,    // مراسلات عامة
    Other = 99             // أخرى
}

/// <summary>
/// Correspondence/document status
/// حالة المراسلة
/// </summary>
public enum CorrespondenceStatus
{
    Draft = 1,              // مسودة
    Pending = 2,            // قيد الانتظار
    UnderReview = 3,        // قيد المراجعة
    Approved = 4,           // معتمدة
    Rejected = 5,           // مرفوضة
    InProgress = 6,         // قيد التنفيذ
    Completed = 7,          // مكتملة
    Archived = 8,           // مؤرشفة
    Cancelled = 9          // ملغاة
}

/// <summary>
/// Priority levels for correspondence
/// مستويات الأولوية
/// </summary>
public enum CorrespondencePriority
{
    Low = 1,               // منخفضة
    Normal = 2,            // عادية
    High = 3,              // عالية
    Urgent = 4,            // عاجلة
    Critical = 5           // حرجة
}

/// <summary>
/// Confidentiality levels
/// مستويات السرية
/// </summary>
public enum ConfidentialityLevel
{
    Public = 1,            // عامة
    Internal = 2,          // داخلية
    Confidential = 3,      // سرية
    HighlyConfidential = 4 // سرية للغاية
}

/// <summary>
/// Routing action types
/// أنواع الإحالة
/// </summary>
public enum RoutingAction
{
    ForReview = 1,         // للمراجعة
    ForApproval = 2,       // للاعتماد
    ForAction = 3,         // للتنفيذ
    ForInformation = 4,    // للعلم
    ForComment = 5,        // للتعليق
    ForSignature = 6       // للتوقيع
}

/// <summary>
/// Form field types for dynamic forms
/// أنواع حقول الفورم
/// </summary>
public enum FormFieldType
{
    Text = 1,              // نص
    TextArea = 2,          // نص متعدد الأسطر
    Number = 3,            // رقم
    Date = 4,              // تاريخ
    DateTime = 5,          // تاريخ ووقت
    Checkbox = 6,          // خانة اختيار
    Radio = 7,             // اختيار واحد
    Dropdown = 8,          // قائمة منسدلة
    MultiSelect = 9,       // اختيار متعدد
    FileUpload = 10,       // رفع ملف
    Signature = 11,        // توقيع
    Email = 12,            // بريد إلكتروني
    Phone = 13,            // هاتف
    Currency = 14,         // مبلغ مالي
    Percentage = 15        // نسبة مئوية
}

/// <summary>
/// Document retention periods
/// فترات الحفظ
/// </summary>
public enum RetentionPeriod
{
    OneYear = 1,           // سنة واحدة
    ThreeYears = 3,        // ثلاث سنوات
    FiveYears = 5,         // خمس سنوات
    SevenYears = 7,        // سبع سنوات
    TenYears = 10,         // عشر سنوات
    Permanent = 999        // دائم
}
