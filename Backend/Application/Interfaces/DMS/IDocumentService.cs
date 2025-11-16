using Microsoft.AspNetCore.Http;
using OutlookInboxManagement.DTOs.DMS;

namespace Application.Interfaces.DMS;

public interface IDocumentService
{
    // Document Management
    Task<DocumentDto> GetDocumentByIdAsync(int id);
    Task<DocumentSearchResultDto> SearchDocumentsAsync(DocumentSearchDto searchDto);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, IFormFile file, string userId);
    Task<DocumentDto> UpdateDocumentAsync(int id, UpdateDocumentDto updateDto, string userId);
    Task DeleteDocumentAsync(int id, string userId);
    Task<byte[]> DownloadDocumentAsync(int id, string userId, int? versionId = null);
    Task<string> GetDocumentPreviewUrlAsync(int id, int? versionId = null);

    // Versioning
    Task<List<DocumentVersionDto>> GetVersionHistoryAsync(int documentId);
    Task<DocumentVersionDto> CreateNewVersionAsync(int documentId, CreateVersionDto versionDto, IFormFile file, string userId);
    Task<DocumentDto> RestoreVersionAsync(int documentId, int versionId, string userId);
    Task<byte[]> DownloadVersionAsync(int documentId, int versionId, string userId);

    // Locking
    Task<DocumentDto> LockDocumentAsync(int documentId, string userId);
    Task<DocumentDto> UnlockDocumentAsync(int documentId, string userId);
    Task<bool> IsDocumentLockedAsync(int documentId);

    // Folders
    Task<DocumentFolderDto> GetFolderByIdAsync(int id);
    Task<List<DocumentFolderDto>> GetRootFoldersAsync(string userId);
    Task<List<DocumentFolderDto>> GetSubFoldersAsync(int parentId);
    Task<DocumentFolderDto> CreateFolderAsync(CreateFolderDto createDto, string userId);
    Task<DocumentFolderDto> UpdateFolderAsync(int id, CreateFolderDto updateDto);
    Task DeleteFolderAsync(int id, string userId);
    Task<DocumentDto> MoveDocumentAsync(int documentId, int? targetFolderId, string userId);

    // Annotations
    Task<List<DocumentAnnotationDto>> GetAnnotationsAsync(int documentId, int? versionId = null);
    Task<DocumentAnnotationDto> CreateAnnotationAsync(int documentId, CreateAnnotationDto createDto, string userId);
    Task<DocumentAnnotationDto> UpdateAnnotationAsync(int annotationId, CreateAnnotationDto updateDto);
    Task DeleteAnnotationAsync(int annotationId, string userId);

    // Activity Log
    Task<List<DocumentActivityDto>> GetDocumentActivitiesAsync(int documentId, int limit = 50);
    Task LogActivityAsync(int documentId, DocumentActivityType activityType, string description, string userId, int? versionId = null);

    // Permissions
    Task<bool> CanAccessDocumentAsync(int documentId, string userId);
    Task<bool> CanEditDocumentAsync(int documentId, string userId);
    Task ShareDocumentAsync(int documentId, List<string> userIds, List<string> roleNames, string userId);

    // Metadata
    Task<Dictionary<string, string>> GetMetadataAsync(int documentId);
    Task UpdateMetadataAsync(int documentId, Dictionary<string, string> metadata, string userId);

    // Bulk Operations
    Task<List<DocumentDto>> BulkUploadAsync(List<IFormFile> files, int? folderId, string userId);
    Task BulkDeleteAsync(List<int> documentIds, string userId);
    Task<byte[]> ExportDocumentsAsZipAsync(List<int> documentIds, string userId);
}
