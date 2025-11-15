namespace OutlookInboxManagement.DTOs.Admin;

public class AdminDashboardDto
{
    public SystemStatistics SystemStats { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<SystemAlert> SystemAlerts { get; set; } = new();
}

public class SystemStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalRoles { get; set; }
    public int TotalPermissions { get; set; }
    public int TodayLogins { get; set; }
    public int TodayAuditLogs { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
}

public class RecentActivity
{
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsSuccessful { get; set; }
}

public class SystemAlert
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
