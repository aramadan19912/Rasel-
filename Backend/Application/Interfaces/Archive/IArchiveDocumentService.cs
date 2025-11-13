using Application.DTOs.Archive;

namespace Application.Interfaces.Archive;

/// <summary>
/// Archive document service interface
/// </summary>
public interface IArchiveDocumentService
{
    // CRUD Operations
    Task<ArchiveDocumentDto?> GetByIdAsync(int id, string userId);
    Task<ArchiveDocumentDto?> GetByArchiveNumberAsync(string archiveNumber, string userId);
    Task<ArchiveDocumentDto?> GetByCorrespondenceIdAsync(int correspondenceId, string userId);
    Task<List<ArchiveDocumentDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 20);

    // PDF Generation
    Task<ArchiveDocumentDto> GeneratePdfFromCorrespondenceAsync(
        GeneratePdfRequest request, string userId);

    // Search & Filter
    Task<(List<ArchiveDocumentDto> Items, int TotalCount)> SearchAsync(
        ArchiveSearchRequest request, string userId);

    // Document Access
    Task<(byte[] FileData, string FileName, string ContentType)> DownloadPdfAsync(int id, string userId);
    Task<bool> RecordAccessAsync(int id, string userId);

    // Retention & Destruction
    Task<List<ArchiveDocumentDto>> GetDocumentsNearingDestructionAsync(int daysThreshold, string userId);
    Task<bool> ApplyLegalHoldAsync(LegalHoldRequest request, string userId);
    Task<bool> DestroyDocumentAsync(DestroyDocumentRequest request, string userId);

    // Backup Operations
    Task<bool> BackupDocumentAsync(int id, string backupLocation, string userId);
    Task<List<ArchiveDocumentDto>> GetDocumentsNeedingBackupAsync(int daysThreshold, string userId);

    // Statistics
    Task<ArchiveStatsDto> GetArchiveStatisticsAsync(string userId);
    Task<Dictionary<string, long>> GetStorageStatsByLocationAsync(string userId);

    // Verification
    Task<bool> VerifyDocumentIntegrityAsync(int id, string userId);
    Task<List<ArchiveDocumentDto>> GetCorruptedDocumentsAsync(string userId);

    // Archive Number Generation
    Task<string> GenerateArchiveNumberAsync(int correspondenceId);
}
