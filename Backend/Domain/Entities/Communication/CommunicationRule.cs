using Domain.Entities.Organization;

namespace Domain.Entities.Communication;

/// <summary>
/// Communication rules and protocols
/// قواعد الاتصال
/// </summary>
public class CommunicationRule
{
    public int Id { get; set; }

    /// <summary>
    /// Rule name
    /// اسم القاعدة
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Rule type (Approval, Routing, Restriction, Notification)
    /// نوع القاعدة
    /// </summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// Priority (higher number = higher priority)
    /// الأولوية
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// From employee/department restrictions
    /// قيود المرسل
    /// </summary>
    public int? FromEmployeeId { get; set; }
    public Employee? FromEmployee { get; set; }

    public int? FromDepartmentId { get; set; }
    public Department? FromDepartment { get; set; }

    /// <summary>
    /// To employee/department restrictions
    /// قيود المستقبل
    /// </summary>
    public int? ToEmployeeId { get; set; }
    public Employee? ToEmployee { get; set; }

    public int? ToDepartmentId { get; set; }
    public Department? ToDepartment { get; set; }

    /// <summary>
    /// Communication level restrictions (from-to)
    /// قيود مستوى الاتصال
    /// </summary>
    public int? MinCommunicationLevel { get; set; }
    public int? MaxCommunicationLevel { get; set; }

    /// <summary>
    /// Requires approval
    /// يتطلب موافقة
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Approver employee
    /// الموافق
    /// </summary>
    public int? ApproverId { get; set; }
    public Employee? Approver { get; set; }

    /// <summary>
    /// Auto-route to specific employee
    /// إحالة تلقائية
    /// </summary>
    public int? AutoRouteToEmployeeId { get; set; }
    public Employee? AutoRouteToEmployee { get; set; }

    /// <summary>
    /// Block communication
    /// حظر الاتصال
    /// </summary>
    public bool BlockCommunication { get; set; } = false;

    /// <summary>
    /// Send notification
    /// إرسال إشعار
    /// </summary>
    public bool SendNotification { get; set; } = false;

    /// <summary>
    /// Notification recipients (JSON array of employee IDs)
    /// مستلمو الإشعار
    /// </summary>
    public string? NotificationRecipients { get; set; }

    /// <summary>
    /// Conditions (JSON)
    /// الشروط
    /// </summary>
    public string? Conditions { get; set; }

    /// <summary>
    /// Actions (JSON)
    /// الإجراءات
    /// </summary>
    public string? Actions { get; set; }

    /// <summary>
    /// Apply to message types (Email, Internal, External)
    /// أنواع الرسائل
    /// </summary>
    public string? ApplyToMessageTypes { get; set; }

    /// <summary>
    /// Apply to classifications (JSON array)
    /// التصنيفات المطبقة
    /// </summary>
    public string? ApplyToClassifications { get; set; }

    /// <summary>
    /// Working hours only
    /// خلال ساعات العمل فقط
    /// </summary>
    public bool WorkingHoursOnly { get; set; } = false;

    /// <summary>
    /// Start date
    /// تاريخ البداية
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date
    /// تاريخ النهاية
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Is active
    /// نشطة
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
