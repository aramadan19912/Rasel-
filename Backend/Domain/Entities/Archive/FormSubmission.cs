using Domain.Entities.Organization;

namespace Domain.Entities.Archive;

/// <summary>
/// Form submission data
/// بيانات تعبئة الفورم
/// </summary>
public class FormSubmission
{
    public int Id { get; set; }

    /// <summary>
    /// Associated form
    /// الفورم المرتبط
    /// </summary>
    public int FormId { get; set; }
    public CorrespondenceForm Form { get; set; } = null!;

    /// <summary>
    /// Submission reference number
    /// الرقم المرجعي للتعبئة
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Submitted data (JSON)
    /// البيانات المعبأة
    /// </summary>
    public string SubmissionData { get; set; } = string.Empty;

    /// <summary>
    /// Submitted by employee
    /// من قام بالتعبئة
    /// </summary>
    public int? SubmittedByEmployeeId { get; set; }
    public Employee? SubmittedByEmployee { get; set; }

    /// <summary>
    /// Submitted by user (if not employee)
    /// المستخدم
    /// </summary>
    public string? SubmittedByUserId { get; set; }

    /// <summary>
    /// Submitter name (for anonymous)
    /// اسم المعبي
    /// </summary>
    public string? SubmitterName { get; set; }

    /// <summary>
    /// Submitter email
    /// بريد المعبي
    /// </summary>
    public string? SubmitterEmail { get; set; }

    /// <summary>
    /// Submission date
    /// تاريخ التعبئة
    /// </summary>
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status (Pending, Approved, Rejected, Processing)
    /// الحالة
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Is approved
    /// معتمد
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Approved by
    /// من اعتمد
    /// </summary>
    public int? ApprovedByEmployeeId { get; set; }
    public Employee? ApprovedByEmployee { get; set; }

    /// <summary>
    /// Approval date
    /// تاريخ الاعتماد
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Approval notes
    /// ملاحظات الاعتماد
    /// </summary>
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Associated correspondence (created from this submission)
    /// المراسلة المنشأة
    /// </summary>
    public int? CorrespondenceId { get; set; }
    public Correspondence? Correspondence { get; set; }

    /// <summary>
    /// IP address of submitter
    /// عنوان IP
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent
    /// متصفح المستخدم
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Notes
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
