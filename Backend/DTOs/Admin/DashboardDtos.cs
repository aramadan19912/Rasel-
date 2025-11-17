namespace OutlookInboxManagement.DTOs.Admin;

public class AdminDashboardDto
{
    public SystemStatistics SystemStats { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<SystemAlert> SystemAlerts { get; set; } = new();
}

public class ExecutiveDashboardDto
{
    // User Statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalRoles { get; set; }

    // Correspondence Statistics
    public int TotalCorrespondences { get; set; }
    public int CorrespondencesThisMonth { get; set; }
    public int CorrespondencesThisYear { get; set; }
    public int PendingCorrespondences { get; set; }
    public int OverdueCorrespondences { get; set; }

    // Archive Statistics
    public int TotalArchiveDocuments { get; set; }
    public int ArchiveCategories { get; set; }

    // System Activity
    public int TotalMessages { get; set; }
    public int MessagesToday { get; set; }

    // Calendar Events
    public int UpcomingEvents { get; set; }
    public int EventsToday { get; set; }

    // Contact Statistics
    public int TotalContacts { get; set; }

    // Growth Metrics
    public double UserGrowthRate { get; set; }
    public double CorrespondenceGrowthRate { get; set; }

    public DateTime LastUpdated { get; set; }
}

public class DepartmentDashboardDto
{
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? ManagerName { get; set; }

    // Employee Statistics
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }

    // Correspondence Statistics
    public int IncomingCorrespondences { get; set; }
    public int OutgoingCorrespondences { get; set; }
    public int PendingCorrespondences { get; set; }
    public int CompletedThisMonth { get; set; }

    // Performance Metrics
    public double AverageResponseTime { get; set; }
    public double CompletionRate { get; set; }

    // Messages
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }

    public DateTime LastUpdated { get; set; }
}

public class EmployeeDashboardDto
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionTitle { get; set; }

    // Assigned Tasks
    public int AssignedCorrespondences { get; set; }
    public int PendingCorrespondences { get; set; }
    public int OverdueCorrespondences { get; set; }
    public int CompletedThisWeek { get; set; }
    public int CompletedThisMonth { get; set; }

    // Messages
    public int UnreadMessages { get; set; }
    public int TotalMessages { get; set; }

    // Calendar
    public int UpcomingMeetings { get; set; }
    public int MeetingsToday { get; set; }

    // Performance
    public double AverageCompletionTime { get; set; }
    public double CompletionRate { get; set; }

    public DateTime LastUpdated { get; set; }
}

public class CorrespondenceStatisticsDto
{
    public int TotalCount { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
    public Dictionary<string, int> ByClassification { get; set; } = new();
    public double AverageResponseTime { get; set; }
    public double CompletionRate { get; set; }
    public int OverdueCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
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
