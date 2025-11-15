using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetDashboardDataAsync();
    Task<SystemStatistics> GetSystemStatisticsAsync();
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10);
    Task<List<SystemAlert>> GetSystemAlertsAsync();
}
