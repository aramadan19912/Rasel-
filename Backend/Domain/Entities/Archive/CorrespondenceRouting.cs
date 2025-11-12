using Domain.Entities.Organization;
using Domain.Enums;

namespace Domain.Entities.Archive;

/// <summary>
/// Correspondence routing/referral tracking
/// تتبع إحالة المراسلات
/// </summary>
public class CorrespondenceRouting
{
    public int Id { get; set; }

    /// <summary>
    /// Associated correspondence
    /// المراسلة المرتبطة
    /// </summary>
    public int CorrespondenceId { get; set; }
    public Correspondence Correspondence { get; set; } = null!;

    /// <summary>
    /// From employee
    /// من الموظف
    /// </summary>
    public int FromEmployeeId { get; set; }
    public Employee FromEmployee { get; set; } = null!;

    /// <summary>
    /// To employee
    /// إلى الموظف
    /// </summary>
    public int ToEmployeeId { get; set; }
    public Employee ToEmployee { get; set; } = null!;

    /// <summary>
    /// To department (optional)
    /// إلى القسم
    /// </summary>
    public int? ToDepartmentId { get; set; }
    public Department? ToDepartment { get; set; }

    /// <summary>
    /// Routing action type
    /// نوع الإحالة
    /// </summary>
    public string Action { get; set; } = RoutingAction.ForReview.ToString();

    /// <summary>
    /// Priority
    /// الأولوية
    /// </summary>
    public string Priority { get; set; } = CorrespondencePriority.Normal.ToString();

    /// <summary>
    /// Instructions/notes from sender
    /// تعليمات من المرسل
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Due date for action
    /// تاريخ الاستحقاق
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Routed date
    /// تاريخ الإحالة
    /// </summary>
    public DateTime RoutedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Received date (when recipient opened)
    /// تاريخ الاستلام
    /// </summary>
    public DateTime? ReceivedDate { get; set; }

    /// <summary>
    /// Is read by recipient
    /// تم القراءة
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Response from recipient
    /// الرد من المستلم
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// Response date
    /// تاريخ الرد
    /// </summary>
    public DateTime? ResponseDate { get; set; }

    /// <summary>
    /// Status of this routing
    /// حالة الإحالة
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Returned

    /// <summary>
    /// Completed date
    /// تاريخ الإنجاز
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Parent routing (if this is a sub-routing)
    /// الإحالة الأم
    /// </summary>
    public int? ParentRoutingId { get; set; }
    public CorrespondenceRouting? ParentRouting { get; set; }

    /// <summary>
    /// Sequence number in routing chain
    /// الرقم التسلسلي
    /// </summary>
    public int SequenceNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
