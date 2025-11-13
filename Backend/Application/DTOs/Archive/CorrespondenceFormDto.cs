namespace Application.DTOs.Archive;

/// <summary>
/// Correspondence form DTO
/// </summary>
public class CorrespondenceFormDto
{
    public int Id { get; set; }
    public string FormCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string DefaultClassification { get; set; } = string.Empty;

    public List<FormFieldDto> Fields { get; set; } = new();

    public int Version { get; set; }
    public bool IsPublished { get; set; }
    public bool AllowAnonymous { get; set; }
    public bool RequireApproval { get; set; }
    public string? AutoRoutingRules { get; set; }
    public string? NotificationEmails { get; set; }
    public bool IsActive { get; set; }

    public int SubmissionCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Create/Update form request
/// </summary>
public class CreateCorrespondenceFormRequest
{
    public string FormCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int CategoryId { get; set; }
    public string DefaultClassification { get; set; } = string.Empty;
    public bool AllowAnonymous { get; set; } = false;
    public bool RequireApproval { get; set; } = true;
    public string? AutoRoutingRules { get; set; }
    public string? NotificationEmails { get; set; }
    public List<CreateFormFieldRequest> Fields { get; set; } = new();
}

/// <summary>
/// Publish form request
/// </summary>
public class PublishFormRequest
{
    public int FormId { get; set; }
    public bool IsPublished { get; set; } = true;
}

/// <summary>
/// Form summary for list views
/// </summary>
public class FormSummaryDto
{
    public int Id { get; set; }
    public string FormCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsPublished { get; set; }
    public bool IsActive { get; set; }
    public int FieldCount { get; set; }
    public int SubmissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
