using Application.DTOs.Archive;

namespace Application.Interfaces.Archive;

/// <summary>
/// Correspondence service interface
/// </summary>
public interface ICorrespondenceService
{
    // CRUD Operations
    Task<CorrespondenceDto?> GetByIdAsync(int id, string userId);
    Task<CorrespondenceDto?> GetByReferenceNumberAsync(string referenceNumber, string userId);
    Task<List<CorrespondenceSummaryDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<CorrespondenceDto> CreateAsync(CreateCorrespondenceRequest request, string userId);
    Task<CorrespondenceDto?> UpdateAsync(int id, UpdateCorrespondenceRequest request, string userId);
    Task<bool> DeleteAsync(int id, string userId);

    // Search & Filter
    Task<(List<CorrespondenceSummaryDto> Items, int TotalCount)> SearchAsync(
        CorrespondenceSearchRequest request, string userId);

    // Category Operations
    Task<List<CorrespondenceSummaryDto>> GetByCategoryAsync(int categoryId, string userId);
    Task<List<CorrespondenceSummaryDto>> GetByClassificationAsync(string classification, string userId);

    // Employee/Department Operations
    Task<List<CorrespondenceSummaryDto>> GetByFromEmployeeAsync(int employeeId, string userId);
    Task<List<CorrespondenceSummaryDto>> GetByToEmployeeAsync(int employeeId, string userId);
    Task<List<CorrespondenceSummaryDto>> GetByDepartmentAsync(int departmentId, string userId);

    // Status Operations
    Task<List<CorrespondenceSummaryDto>> GetByStatusAsync(string status, string userId);
    Task<bool> UpdateStatusAsync(int id, string status, string userId);

    // Attachment Operations
    Task<CorrespondenceAttachmentDto> AddAttachmentAsync(
        int correspondenceId, UploadAttachmentRequest request, Stream fileStream,
        string fileName, string contentType, string userId);
    Task<List<CorrespondenceAttachmentDto>> GetAttachmentsAsync(int correspondenceId, string userId);
    Task<bool> DeleteAttachmentAsync(int attachmentId, string userId);

    // Routing Operations
    Task<CorrespondenceRoutingDto> RouteCorrespondenceAsync(RouteCorrespondenceRequest request, string userId);
    Task<List<CorrespondenceRoutingDto>> GetRoutingHistoryAsync(int correspondenceId, string userId);
    Task<RoutingChainDto> GetRoutingChainAsync(int correspondenceId, string userId);
    Task<bool> RespondToRoutingAsync(RespondToRoutingRequest request, string userId);

    // My Correspondence (Inbox/Outbox)
    Task<List<CorrespondenceSummaryDto>> GetMyInboxAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<List<CorrespondenceSummaryDto>> GetMyOutboxAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<List<CorrespondenceSummaryDto>> GetMyDraftsAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<List<CorrespondenceRoutingDto>> GetMyPendingActionsAsync(string userId);

    // Archive Operations
    Task<bool> ArchiveCorrespondenceAsync(ArchiveCorrespondenceRequest request, string userId);
    Task<List<CorrespondenceSummaryDto>> GetArchivedCorrespondencesAsync(string userId);

    // Related Correspondence
    Task<List<CorrespondenceSummaryDto>> GetRelatedCorrespondencesAsync(int correspondenceId, string userId);

    // Statistics
    Task<Dictionary<string, int>> GetCorrespondenceStatsByStatusAsync(string userId);
    Task<Dictionary<string, int>> GetCorrespondenceStatsByClassificationAsync(string userId);

    // Reference Number Generation
    Task<string> GenerateReferenceNumberAsync(int categoryId);
}
