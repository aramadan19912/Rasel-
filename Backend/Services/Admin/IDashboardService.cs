using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IDashboardService
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync();
    Task<DepartmentDashboardDto> GetDepartmentDashboardAsync(int departmentId);
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId);
    Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 10);
    Task<CorrespondenceStatisticsDto> GetCorrespondenceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
