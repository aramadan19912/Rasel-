using Domain.Entities.Organization;

namespace Domain.Entities.Archive;

/// <summary>
/// Correspondence tracking and history
/// تتبع المراسلات
/// </summary>
public class CorrespondenceTracking
{
    public int Id { get; set; }

    /// <summary>
    /// Correspondence
    /// المراسلة
    /// </summary>
    public int CorrespondenceId { get; set; }
    public Correspondence Correspondence { get; set; } = null!;

    /// <summary>
    /// Action type (Created, Viewed, Edited, Routed, Approved, Rejected, Archived, Printed, Downloaded)
    /// نوع الإجراء
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Action description
    /// وصف الإجراء
    /// </summary>
    public string? ActionDescription { get; set; }

    /// <summary>
    /// Previous status
    /// الحالة السابقة
    /// </summary>
    public string? PreviousStatus { get; set; }

    /// <summary>
    /// New status
    /// الحالة الجديدة
    /// </summary>
    public string? NewStatus { get; set; }

    /// <summary>
    /// Performed by employee
    /// من قام بالإجراء
    /// </summary>
    public int? PerformedByEmployeeId { get; set; }
    public Employee? PerformedByEmployee { get; set; }

    /// <summary>
    /// Performed date/time
    /// تاريخ ووقت الإجراء
    /// </summary>
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address
    /// عنوان IP
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent
    /// متصفح المستخدم
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Changes made (JSON)
    /// التغييرات
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Comments/Notes
    /// تعليقات
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Related routing (if action is routing)
    /// الإحالة المرتبطة
    /// </summary>
    public int? RoutingId { get; set; }
    public CorrespondenceRouting? Routing { get; set; }

    /// <summary>
    /// Duration (for actions like viewing)
    /// المدة
    /// </summary>
    public int? DurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Document circulation (distribution list)
/// تعميم المستند
/// </summary>
public class CorrespondenceCirculation
{
    public int Id { get; set; }

    /// <summary>
    /// Correspondence
    /// المراسلة
    /// </summary>
    public int CorrespondenceId { get; set; }
    public Correspondence Correspondence { get; set; } = null!;

    /// <summary>
    /// Circulation name
    /// اسم التعميم
    /// </summary>
    public string CirculationName { get; set; } = string.Empty;

    /// <summary>
    /// Circulation type (FYI, ActionRequired, Urgent)
    /// نوع التعميم
    /// </summary>
    public string CirculationType { get; set; } = string.Empty;

    /// <summary>
    /// Priority
    /// الأولوية
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Due date
    /// تاريخ الاستحقاق
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Instructions
    /// التعليمات
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Recipients
    /// المستلمون
    /// </summary>
    public ICollection<CirculationRecipient> Recipients { get; set; } = new List<CirculationRecipient>();

    /// <summary>
    /// Require acknowledgment
    /// يتطلب إقرار
    /// </summary>
    public bool RequireAcknowledgment { get; set; } = false;

    /// <summary>
    /// Require response
    /// يتطلب رد
    /// </summary>
    public bool RequireResponse { get; set; } = false;

    /// <summary>
    /// Status (Active, Completed, Cancelled)
    /// الحالة
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Started by
    /// بدأه
    /// </summary>
    public int StartedByEmployeeId { get; set; }
    public Employee StartedByEmployee { get; set; } = null!;

    /// <summary>
    /// Started date
    /// تاريخ البدء
    /// </summary>
    public DateTime StartedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Completed date
    /// تاريخ الإكمال
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Circulation recipient
/// مستلم التعميم
/// </summary>
public class CirculationRecipient
{
    public int Id { get; set; }

    public int CirculationId { get; set; }
    public CorrespondenceCirculation Circulation { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// Recipient type (To, CC, BCC)
    /// نوع المستلم
    /// </summary>
    public string RecipientType { get; set; } = "To";

    /// <summary>
    /// Is acknowledged
    /// تم الإقرار
    /// </summary>
    public bool IsAcknowledged { get; set; } = false;

    /// <summary>
    /// Acknowledged date
    /// تاريخ الإقرار
    /// </summary>
    public DateTime? AcknowledgedDate { get; set; }

    /// <summary>
    /// Has responded
    /// رد
    /// </summary>
    public bool HasResponded { get; set; } = false;

    /// <summary>
    /// Response date
    /// تاريخ الرد
    /// </summary>
    public DateTime? ResponseDate { get; set; }

    /// <summary>
    /// Response text
    /// نص الرد
    /// </summary>
    public string? ResponseText { get; set; }

    /// <summary>
    /// Is viewed
    /// تم العرض
    /// </summary>
    public bool IsViewed { get; set; } = false;

    /// <summary>
    /// Viewed date
    /// تاريخ العرض
    /// </summary>
    public DateTime? ViewedDate { get; set; }

    /// <summary>
    /// Sequence number
    /// الرقم التسلسلي
    /// </summary>
    public int SequenceNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
