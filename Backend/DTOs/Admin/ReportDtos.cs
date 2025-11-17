namespace OutlookInboxManagement.DTOs.Admin;

// Report Filter DTOs
public class ReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Statuses { get; set; }
    public List<string>? Priorities { get; set; }
    public List<int>? DepartmentIds { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public string? SearchTerm { get; set; }
}

public class AuditLogFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Actions { get; set; }
    public List<string>? EntityTypes { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// Correspondence Reports
public class CorrespondenceReportDto
{
    public int TotalCount { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
    public Dictionary<string, int> ByDepartment { get; set; } = new();
    public Dictionary<string, int> ByClassification { get; set; } = new();
    public List<CorrespondenceSummary> TopCorrespondences { get; set; } = new();
    public double AverageCompletionTime { get; set; }
    public int OverdueCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CorrespondenceSummary
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AssignedTo { get; set; }
}

// Employee Performance Reports
public class EmployeePerformanceReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Overdue { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Department Reports
public class DepartmentReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalCorrespondences { get; set; }
    public int CompletedCorrespondences { get; set; }
    public int PendingCorrespondences { get; set; }
    public int OverdueCorrespondences { get; set; }
    public double CompletionRate { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> CorrespondencesByStatus { get; set; } = new();
    public List<EmployeePerformanceReportDto> TopPerformers { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Archive Reports
public class ArchiveReportDto
{
    public int TotalDocuments { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByClassification { get; set; } = new();
    public Dictionary<string, int> ByRetentionPeriod { get; set; } = new();
    public long TotalStorageSize { get; set; }
    public int DocumentsAddedThisPeriod { get; set; }
    public List<CategoryStatistics> CategoryBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CategoryStatistics
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public long StorageSize { get; set; }
}

// Audit Log Reports
public class AuditLogReportDto
{
    public int TotalLogs { get; set; }
    public Dictionary<string, int> ByAction { get; set; } = new();
    public Dictionary<string, int> ByEntityType { get; set; } = new();
    public Dictionary<string, int> ByUser { get; set; } = new();
    public List<ActivityLogDto> RecentLogs { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Custom Report Request
public class CustomReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public ExportFormat Format { get; set; }
}

public enum ExportFormat
{
    PDF,
    Excel,
    CSV
}
