using Application.DTOs.Archive;

namespace Application.Interfaces.Archive;

/// <summary>
/// Correspondence form service interface
/// </summary>
public interface ICorrespondenceFormService
{
    // Form CRUD Operations
    Task<CorrespondenceFormDto?> GetFormByIdAsync(int id, string userId);
    Task<CorrespondenceFormDto?> GetFormByCodeAsync(string code, string userId);
    Task<List<FormSummaryDto>> GetAllFormsAsync(string userId);
    Task<List<FormSummaryDto>> GetPublishedFormsAsync();
    Task<CorrespondenceFormDto> CreateFormAsync(CreateCorrespondenceFormRequest request, string userId);
    Task<CorrespondenceFormDto?> UpdateFormAsync(int id, CreateCorrespondenceFormRequest request, string userId);
    Task<bool> DeleteFormAsync(int id, string userId);

    // Form Publishing
    Task<bool> PublishFormAsync(int id, bool isPublished, string userId);
    Task<CorrespondenceFormDto?> CreateNewVersionAsync(int formId, string userId);

    // Form Fields Operations
    Task<FormFieldDto> AddFieldAsync(int formId, CreateFormFieldRequest request, string userId);
    Task<FormFieldDto?> UpdateFieldAsync(int fieldId, CreateFormFieldRequest request, string userId);
    Task<bool> DeleteFieldAsync(int fieldId, string userId);
    Task<bool> ReorderFieldsAsync(int formId, List<int> fieldIds, string userId);

    // Form Submissions
    Task<FormSubmissionDto> SubmitFormAsync(SubmitFormRequest request, string userId, string? ipAddress = null, string? userAgent = null);
    Task<FormSubmissionDto?> GetSubmissionByIdAsync(int id, string userId);
    Task<List<FormSubmissionSummaryDto>> GetFormSubmissionsAsync(int formId, string userId);
    Task<(List<FormSubmissionSummaryDto> Items, int TotalCount)> SearchSubmissionsAsync(
        FormSubmissionSearchRequest request, string userId);

    // Submission Approval
    Task<bool> ApproveSubmissionAsync(ApproveSubmissionRequest request, string userId);
    Task<bool> RejectSubmissionAsync(int submissionId, string rejectionNotes, string userId);
    Task<List<FormSubmissionSummaryDto>> GetPendingApprovalsAsync(string userId);

    // Form by Category
    Task<List<FormSummaryDto>> GetFormsByCategoryAsync(int categoryId, string userId);

    // Form Statistics
    Task<Dictionary<string, int>> GetFormSubmissionStatsAsync(int formId, string userId);
    Task<int> GetFormSubmissionCountAsync(int formId);

    // Validation
    Task<bool> FormCodeExistsAsync(string code);
    Task<bool> ValidateFormDataAsync(int formId, string submissionData);
}
