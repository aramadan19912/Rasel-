namespace Application.DTOs.Archive;

/// <summary>
/// Form submission DTO
/// </summary>
public class FormSubmissionDto
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string SubmissionData { get; set; } = string.Empty;

    public int? SubmittedByEmployeeId { get; set; }
    public string? SubmittedByEmployeeName { get; set; }
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }

    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsApproved { get; set; }

    public int? ApprovedByEmployeeId { get; set; }
    public string? ApprovedByEmployeeName { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalNotes { get; set; }

    public int? CorrespondenceId { get; set; }
    public string? CorrespondenceReferenceNumber { get; set; }

    public string? IpAddress { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Submit form request
/// </summary>
public class SubmitFormRequest
{
    public int FormId { get; set; }
    public string SubmissionData { get; set; } = string.Empty;
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Approve/Reject submission request
/// </summary>
public class ApproveSubmissionRequest
{
    public int SubmissionId { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovalNotes { get; set; }
    public bool CreateCorrespondence { get; set; } = true;
}

/// <summary>
/// Form submission summary
/// </summary>
public class FormSubmissionSummaryDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public string? SubmitterName { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool HasCorrespondence { get; set; }
}

/// <summary>
/// Form submission search request
/// </summary>
public class FormSubmissionSearchRequest
{
    public int? FormId { get; set; }
    public string? Status { get; set; }
    public bool? IsApproved { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? SubmittedTo { get; set; }
    public int? SubmittedByEmployeeId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
