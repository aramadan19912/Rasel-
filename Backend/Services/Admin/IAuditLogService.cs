using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IAuditLogService
{
    Task<AuditLogSearchResponse> SearchAuditLogsAsync(AuditLogSearchRequest request);
    Task<AuditLogDto?> GetAuditLogByIdAsync(long logId);
    Task<AuditLogStatistics> GetStatisticsAsync();
    Task LogActionAsync(string userId, string action, string entityType, string? entityId,
        string? oldValues, string? newValues, string? ipAddress, string? userAgent, bool isSuccessful, string? errorMessage = null);
}
