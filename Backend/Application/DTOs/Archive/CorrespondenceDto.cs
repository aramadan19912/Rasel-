namespace Application.DTOs.Archive;

/// <summary>
/// Correspondence DTO
/// </summary>
public class CorrespondenceDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public string? SubjectEn { get; set; }
    public string ContentAr { get; set; } = string.Empty;
    public string? ContentEn { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ConfidentialityLevel { get; set; } = string.Empty;

    public DateTime CorrespondenceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public int? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? ExternalSenderName { get; set; }
    public string? ExternalSenderOrganization { get; set; }

    public int? ToDepartmentId { get; set; }
    public string? ToDepartmentName { get; set; }
    public int? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }

    public int? FormId { get; set; }
    public string? FormName { get; set; }
    public int? FormSubmissionId { get; set; }

    public int? RelatedCorrespondenceId { get; set; }
    public string? RelatedReferenceNumber { get; set; }

    public List<CorrespondenceAttachmentDto> Attachments { get; set; } = new();
    public List<CorrespondenceRoutingDto> Routings { get; set; } = new();

    public int? ArchivedDocumentId { get; set; }
    public string? ArchiveNumber { get; set; }
    public string? PdfFilePath { get; set; }

    public string? Keywords { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ArchivedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create correspondence request
/// </summary>
public class CreateCorrespondenceRequest
{
    public string SubjectAr { get; set; } = string.Empty;
    public string? SubjectEn { get; set; }
    public string ContentAr { get; set; } = string.Empty;
    public string? ContentEn { get; set; }

    public int CategoryId { get; set; }
    public string Status { get; set; } = "Draft";
    public string Priority { get; set; } = "Normal";
    public string ConfidentialityLevel { get; set; } = "Internal";

    public DateTime? CorrespondenceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public int? FromEmployeeId { get; set; }
    public string? ExternalSenderName { get; set; }
    public string? ExternalSenderOrganization { get; set; }

    public int? ToDepartmentId { get; set; }
    public int? ToEmployeeId { get; set; }

    public int? FormSubmissionId { get; set; }
    public int? RelatedCorrespondenceId { get; set; }

    public string? Keywords { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Update correspondence request
/// </summary>
public class UpdateCorrespondenceRequest : CreateCorrespondenceRequest
{
}

/// <summary>
/// Archive correspondence request
/// </summary>
public class ArchiveCorrespondenceRequest
{
    public int CorrespondenceId { get; set; }
    public bool GeneratePdf { get; set; } = true;
    public bool IncludeAttachments { get; set; } = true;
    public bool ApplyWatermark { get; set; } = false;
    public bool ApplyDigitalSignature { get; set; } = false;
    public string? Notes { get; set; }
}

/// <summary>
/// Correspondence search request
/// </summary>
public class CorrespondenceSearchRequest
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? Classification { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? ToDepartmentId { get; set; }
    public bool? IsArchived { get; set; }
    public string? Tags { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "DESC";
}

/// <summary>
/// Correspondence summary for list views
/// </summary>
public class CorrespondenceSummaryDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CorrespondenceDate { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}
