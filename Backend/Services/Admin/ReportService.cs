using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.DTOs.Admin;
using System.Text;

namespace OutlookInboxManagement.Services.Admin;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Correspondence Reports

    public async Task<CorrespondenceReportDto> GetCorrespondenceReportAsync(ReportFilterDto filter)
    {
        var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = filter.EndDate ?? DateTime.UtcNow;

        var query = _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate);

        // Apply filters
        if (filter.Statuses?.Any() == true)
        {
            query = query.Where(c => filter.Statuses.Contains(c.Status.ToString()));
        }

        if (filter.Priorities?.Any() == true)
        {
            query = query.Where(c => filter.Priorities.Contains(c.Priority.ToString()));
        }

        if (filter.DepartmentIds?.Any() == true)
        {
            query = query.Where(c => c.ToDepartmentId.HasValue && filter.DepartmentIds.Contains(c.ToDepartmentId.Value));
        }

        if (filter.EmployeeIds?.Any() == true)
        {
            query = query.Where(c => c.ToEmployeeId.HasValue && filter.EmployeeIds.Contains(c.ToEmployeeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(c => c.SubjectAr.Contains(filter.SearchTerm) || c.SubjectEn.Contains(filter.SearchTerm));
        }

        var correspondences = await query.ToListAsync();

        var report = new CorrespondenceReportDto
        {
            TotalCount = correspondences.Count,
            StartDate = startDate,
            EndDate = endDate,
            OverdueCount = correspondences.Count(c => c.DueDate.HasValue && c.DueDate < DateTime.UtcNow && c.Status != Domain.Enums.CorrespondenceStatus.Completed)
        };

        // Group by status
        report.ByStatus = correspondences
            .GroupBy(c => c.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by priority
        report.ByPriority = correspondences
            .GroupBy(c => c.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by classification
        report.ByClassification = correspondences
            .GroupBy(c => c.Classification.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by department
        var departmentGroups = correspondences
            .Where(c => c.ToDepartmentId.HasValue)
            .GroupBy(c => c.ToDepartmentId!.Value);

        foreach (var group in departmentGroups)
        {
            var dept = await _context.Set<Domain.Entities.Department>()
                .FirstOrDefaultAsync(d => d.Id == group.Key);
            if (dept != null)
            {
                report.ByDepartment[dept.NameEn ?? dept.NameAr ?? $"Department {group.Key}"] = group.Count();
            }
        }

        // Calculate average completion time
        var completedCorrespondences = correspondences
            .Where(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed && c.UpdatedAt.HasValue)
            .ToList();

        if (completedCorrespondences.Any())
        {
            var totalHours = completedCorrespondences
                .Sum(c => (c.UpdatedAt!.Value - c.CreatedAt).TotalHours);
            report.AverageCompletionTime = totalHours / completedCorrespondences.Count;
        }

        // Top correspondences (most recent 10)
        report.TopCorrespondences = await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .Select(c => new CorrespondenceSummary
            {
                Id = c.Id,
                ReferenceNumber = c.ReferenceNumber ?? "",
                Subject = c.SubjectEn ?? c.SubjectAr ?? "",
                Status = c.Status.ToString(),
                Priority = c.Priority.ToString(),
                CreatedAt = c.CreatedAt,
                CompletedAt = c.Status == Domain.Enums.CorrespondenceStatus.Completed ? c.UpdatedAt : null,
                AssignedTo = c.ToEmployeeId.HasValue ? $"Employee {c.ToEmployeeId}" : null
            })
            .ToListAsync();

        return report;
    }

    public async Task<byte[]> ExportCorrespondenceReportAsync(ReportFilterDto filter, ExportFormat format)
    {
        var report = await GetCorrespondenceReportAsync(filter);

        return format switch
        {
            ExportFormat.CSV => GenerateCsvReport(report),
            ExportFormat.Excel => GenerateExcelReport(report),
            ExportFormat.PDF => GeneratePdfReport(report),
            _ => throw new ArgumentException("Invalid export format")
        };
    }

    #endregion

    #region Employee Performance Reports

    public async Task<EmployeePerformanceReportDto> GetEmployeePerformanceReportAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var employee = await _context.Set<Domain.Entities.Employee>()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        var correspondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted && c.ToEmployeeId == employeeId && c.CreatedAt >= start && c.CreatedAt <= end)
            .ToListAsync();

        var completed = correspondences.Count(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed);
        var pending = correspondences.Count(c => c.Status == Domain.Enums.CorrespondenceStatus.Pending || c.Status == Domain.Enums.CorrespondenceStatus.UnderReview);
        var overdue = correspondences.Count(c => c.DueDate.HasValue && c.DueDate < DateTime.UtcNow && c.Status != Domain.Enums.CorrespondenceStatus.Completed);

        var completionRate = correspondences.Count > 0 ? (double)completed / correspondences.Count * 100 : 0;

        var completedCorrespondences = correspondences
            .Where(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed && c.UpdatedAt.HasValue)
            .ToList();

        var averageCompletionTime = 0.0;
        if (completedCorrespondences.Any())
        {
            var totalHours = completedCorrespondences.Sum(c => (c.UpdatedAt!.Value - c.CreatedAt).TotalHours);
            averageCompletionTime = totalHours / completedCorrespondences.Count;
        }

        var report = new EmployeePerformanceReportDto
        {
            EmployeeId = employeeId,
            EmployeeName = employee.FullNameEn ?? employee.FullNameAr ?? "",
            DepartmentName = employee.Department?.NameEn ?? employee.Department?.NameAr ?? "",
            TotalAssigned = correspondences.Count,
            Completed = completed,
            Pending = pending,
            Overdue = overdue,
            CompletionRate = completionRate,
            AverageCompletionTime = averageCompletionTime,
            StartDate = start,
            EndDate = end
        };

        // Group by status
        report.ByStatus = correspondences
            .GroupBy(c => c.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return report;
    }

    public async Task<List<EmployeePerformanceReportDto>> GetAllEmployeesPerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var employees = await _context.Set<Domain.Entities.Employee>()
            .Where(e => !e.IsDeleted)
            .ToListAsync();

        var reports = new List<EmployeePerformanceReportDto>();

        foreach (var employee in employees)
        {
            var report = await GetEmployeePerformanceReportAsync(employee.Id, startDate, endDate);
            reports.Add(report);
        }

        return reports.OrderByDescending(r => r.CompletionRate).ToList();
    }

    #endregion

    #region Department Reports

    public async Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var department = await _context.Set<Domain.Entities.Department>()
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Department with ID {departmentId} not found");

        var employeeIds = department.Employees.Select(e => e.Id).ToList();

        var correspondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted &&
                        (c.ToDepartmentId == departmentId || (c.ToEmployeeId.HasValue && employeeIds.Contains(c.ToEmployeeId.Value))) &&
                        c.CreatedAt >= start && c.CreatedAt <= end)
            .ToListAsync();

        var completed = correspondences.Count(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed);
        var pending = correspondences.Count(c => c.Status == Domain.Enums.CorrespondenceStatus.Pending || c.Status == Domain.Enums.CorrespondenceStatus.UnderReview);
        var overdue = correspondences.Count(c => c.DueDate.HasValue && c.DueDate < DateTime.UtcNow && c.Status != Domain.Enums.CorrespondenceStatus.Completed);

        var completionRate = correspondences.Count > 0 ? (double)completed / correspondences.Count * 100 : 0;

        var completedCorrespondences = correspondences
            .Where(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed && c.UpdatedAt.HasValue)
            .ToList();

        var averageResponseTime = 0.0;
        if (completedCorrespondences.Any())
        {
            var totalHours = completedCorrespondences.Sum(c => (c.UpdatedAt!.Value - c.CreatedAt).TotalHours);
            averageResponseTime = totalHours / completedCorrespondences.Count;
        }

        var report = new DepartmentReportDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.NameEn ?? department.NameAr ?? "",
            ManagerName = department.ManagerId.HasValue ? $"Manager {department.ManagerId}" : null,
            TotalEmployees = employeeIds.Count,
            TotalCorrespondences = correspondences.Count,
            CompletedCorrespondences = completed,
            PendingCorrespondences = pending,
            OverdueCorrespondences = overdue,
            CompletionRate = completionRate,
            AverageResponseTime = averageResponseTime,
            StartDate = start,
            EndDate = end
        };

        // Group by status
        report.CorrespondencesByStatus = correspondences
            .GroupBy(c => c.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Top performers (top 5 employees by completion rate)
        var topPerformerTasks = employeeIds.Take(5).Select(async empId =>
        {
            try
            {
                return await GetEmployeePerformanceReportAsync(empId, start, end);
            }
            catch
            {
                return null;
            }
        });

        var topPerformers = await Task.WhenAll(topPerformerTasks);
        report.TopPerformers = topPerformers
            .Where(p => p != null)
            .OrderByDescending(p => p!.CompletionRate)
            .Take(5)
            .ToList()!;

        return report;
    }

    public async Task<List<DepartmentReportDto>> GetAllDepartmentsReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var departments = await _context.Set<Domain.Entities.Department>()
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        var reports = new List<DepartmentReportDto>();

        foreach (var department in departments)
        {
            try
            {
                var report = await GetDepartmentReportAsync(department.Id, startDate, endDate);
                reports.Add(report);
            }
            catch
            {
                // Skip departments that fail to generate reports
                continue;
            }
        }

        return reports.OrderByDescending(r => r.CompletionRate).ToList();
    }

    #endregion

    #region Archive Reports

    public async Task<ArchiveReportDto> GetArchiveReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var categories = await _context.Set<Domain.Entities.Archive.ArchiveCategory>()
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        var report = new ArchiveReportDto
        {
            TotalDocuments = categories.Sum(c => c.DocumentCount),
            StartDate = start,
            EndDate = end,
            DocumentsAddedThisPeriod = 0, // This would need document creation tracking
            TotalStorageSize = 0 // This would need actual file size tracking
        };

        // Group by classification
        report.ByClassification = categories
            .GroupBy(c => c.Classification.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(c => c.DocumentCount));

        // Group by retention period
        report.ByRetentionPeriod = categories
            .GroupBy(c => c.RetentionPeriod.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(c => c.DocumentCount));

        // Category breakdown
        report.CategoryBreakdown = categories.Select(c => new CategoryStatistics
        {
            CategoryId = c.Id,
            CategoryName = c.NameEn ?? c.NameAr ?? "",
            DocumentCount = c.DocumentCount,
            StorageSize = 0 // Would need actual storage tracking
        }).ToList();

        // Group by category
        report.ByCategory = categories.ToDictionary(
            c => c.NameEn ?? c.NameAr ?? $"Category {c.Id}",
            c => c.DocumentCount
        );

        return report;
    }

    #endregion

    #region Audit Log Reports

    public async Task<AuditLogReportDto> GetAuditLogReportAsync(AuditLogFilterDto filter)
    {
        var start = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = filter.EndDate ?? DateTime.UtcNow;

        var query = _context.Set<Domain.Entities.AuditLog>()
            .Where(a => a.Timestamp >= start && a.Timestamp <= end);

        // Apply filters
        if (filter.Actions?.Any() == true)
        {
            query = query.Where(a => filter.Actions.Contains(a.Action));
        }

        if (filter.EntityTypes?.Any() == true)
        {
            query = query.Where(a => filter.EntityTypes.Contains(a.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            query = query.Where(a => a.UserId == filter.UserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.IpAddress))
        {
            query = query.Where(a => a.IpAddress == filter.IpAddress);
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalCount = await query.CountAsync();

        var report = new AuditLogReportDto
        {
            TotalLogs = totalCount,
            StartDate = start,
            EndDate = end
        };

        // Group by action
        var allLogs = await query.ToListAsync();
        report.ByAction = allLogs
            .GroupBy(a => a.Action)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by entity type
        report.ByEntityType = allLogs
            .GroupBy(a => a.EntityType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by user
        report.ByUser = allLogs
            .GroupBy(a => a.UserId ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        // Recent logs
        report.RecentLogs = logs.Select(a => new ActivityLogDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.UserId, // Would need to join with User table for actual name
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Timestamp = a.Timestamp,
            IpAddress = a.IpAddress,
            Description = a.Details
        }).ToList();

        return report;
    }

    #endregion

    #region Custom Reports

    public async Task<byte[]> GenerateCustomReportAsync(CustomReportRequest request)
    {
        // This is a placeholder for custom report generation
        // In a real implementation, you would parse the request and generate the appropriate report

        var reportContent = new StringBuilder();
        reportContent.AppendLine($"Custom Report: {request.ReportType}");
        reportContent.AppendLine($"Generated at: {DateTime.UtcNow}");
        reportContent.AppendLine();

        foreach (var param in request.Parameters)
        {
            reportContent.AppendLine($"{param.Key}: {param.Value}");
        }

        return Encoding.UTF8.GetBytes(reportContent.ToString());
    }

    #endregion

    #region Export Helpers

    private byte[] GenerateCsvReport(CorrespondenceReportDto report)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Correspondence Report");
        csv.AppendLine($"Period: {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
        csv.AppendLine($"Total Count: {report.TotalCount}");
        csv.AppendLine($"Overdue Count: {report.OverdueCount}");
        csv.AppendLine($"Average Completion Time: {report.AverageCompletionTime:F2} hours");
        csv.AppendLine();

        // By Status
        csv.AppendLine("Status Breakdown:");
        csv.AppendLine("Status,Count");
        foreach (var item in report.ByStatus)
        {
            csv.AppendLine($"{item.Key},{item.Value}");
        }
        csv.AppendLine();

        // By Priority
        csv.AppendLine("Priority Breakdown:");
        csv.AppendLine("Priority,Count");
        foreach (var item in report.ByPriority)
        {
            csv.AppendLine($"{item.Key},{item.Value}");
        }
        csv.AppendLine();

        // Top Correspondences
        csv.AppendLine("Top Correspondences:");
        csv.AppendLine("Reference Number,Subject,Status,Priority,Created At");
        foreach (var item in report.TopCorrespondences)
        {
            csv.AppendLine($"{item.ReferenceNumber},{item.Subject},{item.Status},{item.Priority},{item.CreatedAt:yyyy-MM-dd}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateExcelReport(CorrespondenceReportDto report)
    {
        // Placeholder - in a real implementation, you would use a library like EPPlus or ClosedXML
        // For now, return CSV format
        return GenerateCsvReport(report);
    }

    private byte[] GeneratePdfReport(CorrespondenceReportDto report)
    {
        // Placeholder - in a real implementation, you would use a library like iTextSharp or QuestPDF
        // For now, return CSV format as plain text
        return GenerateCsvReport(report);
    }

    #endregion
}
