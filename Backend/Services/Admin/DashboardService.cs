using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.DTOs.Admin;
using Backend.Infrastructure.Data;

namespace OutlookInboxManagement.Services.Admin;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        return new ExecutiveDashboardDto
        {
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
            ActiveUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive),
            TotalEmployees = await _context.Set<Domain.Entities.Employee>().CountAsync(e => !e.IsDeleted),
            TotalDepartments = await _context.Set<Domain.Entities.Department>().CountAsync(d => !d.IsDeleted),
            TotalRoles = await _context.Roles.CountAsync(),

            // Correspondence Statistics
            TotalCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted),
            CorrespondencesThisMonth = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.CreatedAt >= startOfMonth),
            CorrespondencesThisYear = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.CreatedAt >= startOfYear),
            PendingCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.Status == Domain.Enums.CorrespondenceStatus.Pending),
            OverdueCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.DueDate < now && c.Status != Domain.Enums.CorrespondenceStatus.Completed),

            // Archive Statistics
            TotalArchiveDocuments = await _context.Set<Domain.Entities.Archive.ArchiveDocument>()
                .CountAsync(a => !a.IsDeleted),
            ArchiveCategories = await _context.Set<Domain.Entities.Archive.ArchiveCategory>()
                .CountAsync(a => !a.IsDeleted && a.IsActive),

            // System Activity
            TotalMessages = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted),
            MessagesToday = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted && m.SentAt >= now.Date),

            // Calendar Events
            UpcomingEvents = await _context.Set<Domain.Entities.CalendarEvent>()
                .CountAsync(e => !e.IsDeleted && e.Start >= now),
            EventsToday = await _context.Set<Domain.Entities.CalendarEvent>()
                .CountAsync(e => !e.IsDeleted && e.Start.Date == now.Date),

            // Contact Statistics
            TotalContacts = await _context.Set<Domain.Entities.Contact>()
                .CountAsync(c => !c.IsDeleted),

            // Growth Metrics
            UserGrowthRate = await CalculateGrowthRate(startOfMonth, EntityType.Users),
            CorrespondenceGrowthRate = await CalculateGrowthRate(startOfMonth, EntityType.Correspondences),

            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<DepartmentDashboardDto> GetDepartmentDashboardAsync(int departmentId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var department = await _context.Set<Domain.Entities.Department>()
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

        if (department == null)
            throw new KeyNotFoundException($"Department with ID {departmentId} not found");

        var employeeIds = department.Employees.Select(e => e.Id).ToList();

        return new DepartmentDashboardDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.NameAr ?? department.NameEn,
            ManagerName = department.ManagerId.HasValue
                ? await GetEmployeeNameAsync(department.ManagerId.Value)
                : null,

            TotalEmployees = employeeIds.Count,
            ActiveEmployees = await _context.Set<Domain.Entities.Employee>()
                .CountAsync(e => employeeIds.Contains(e.Id) && !e.IsDeleted && e.IsActive),

            // Correspondence Statistics
            IncomingCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToDepartmentId == departmentId),
            OutgoingCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted &&
                    c.FromEmployee != null && employeeIds.Contains(c.FromEmployee.DepartmentId ?? 0)),
            PendingCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToDepartmentId == departmentId &&
                    c.Status == Domain.Enums.CorrespondenceStatus.Pending),
            CompletedThisMonth = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToDepartmentId == departmentId &&
                    c.Status == Domain.Enums.CorrespondenceStatus.Completed &&
                    c.UpdatedAt >= startOfMonth),

            // Performance Metrics
            AverageResponseTime = await CalculateAverageResponseTime(departmentId),
            CompletionRate = await CalculateCompletionRate(departmentId),

            // Messages
            TotalMessages = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted &&
                    m.Sender != null && employeeIds.Contains(int.Parse(m.Sender.Id))),
            UnreadMessages = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted && !m.IsRead &&
                    m.Recipients.Any(r => employeeIds.Contains(int.Parse(r.Id)))),

            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId)
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var employee = await _context.Set<Domain.Entities.Employee>()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        return new EmployeeDashboardDto
        {
            EmployeeId = employeeId,
            EmployeeName = employee.FullNameAr ?? employee.FullNameEn,
            DepartmentName = employee.Department?.NameAr ?? employee.Department?.NameEn,
            PositionTitle = employee.Position?.TitleAr ?? employee.Position?.TitleEn,

            // Assigned Tasks
            AssignedCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                    c.Status != Domain.Enums.CorrespondenceStatus.Completed),
            PendingCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                    c.Status == Domain.Enums.CorrespondenceStatus.Pending),
            OverdueCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                    c.DueDate < now && c.Status != Domain.Enums.CorrespondenceStatus.Completed),
            CompletedThisWeek = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                    c.Status == Domain.Enums.CorrespondenceStatus.Completed &&
                    c.UpdatedAt >= startOfWeek),
            CompletedThisMonth = await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                    c.Status == Domain.Enums.CorrespondenceStatus.Completed &&
                    c.UpdatedAt >= startOfMonth),

            // Messages
            UnreadMessages = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted && !m.IsRead &&
                    m.Recipients.Any(r => r.Id == employee.UserId)),
            TotalMessages = await _context.Set<Domain.Entities.Message>()
                .CountAsync(m => !m.IsDeleted &&
                    m.Recipients.Any(r => r.Id == employee.UserId)),

            // Calendar
            UpcomingMeetings = await _context.Set<Domain.Entities.CalendarEvent>()
                .CountAsync(e => !e.IsDeleted && e.Start >= now &&
                    e.Attendees.Any(a => a.Id == employee.UserId)),
            MeetingsToday = await _context.Set<Domain.Entities.CalendarEvent>()
                .CountAsync(e => !e.IsDeleted && e.Start.Date == now.Date &&
                    e.Attendees.Any(a => a.Id == employee.UserId)),

            // Performance
            AverageCompletionTime = await CalculateEmployeeAverageCompletionTime(employeeId),
            CompletionRate = await CalculateEmployeeCompletionRate(employeeId),

            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 10)
    {
        return await _context.Set<Domain.Entities.AuditLog>()
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress,
                Description = a.Changes
            })
            .ToListAsync();
    }

    public async Task<CorrespondenceStatisticsDto> GetCorrespondenceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        endDate ??= DateTime.UtcNow;
        startDate ??= endDate.Value.AddMonths(-12);

        var correspondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .ToListAsync();

        return new CorrespondenceStatisticsDto
        {
            TotalCount = correspondences.Count,
            ByStatus = correspondences.GroupBy(c => c.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ByPriority = correspondences.GroupBy(c => c.Priority)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ByClassification = correspondences.GroupBy(c => c.Classification)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            AverageResponseTime = correspondences
                .Where(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed)
                .Average(c => (c.UpdatedAt - c.CreatedAt).TotalHours),
            CompletionRate = correspondences.Any()
                ? (double)correspondences.Count(c => c.Status == Domain.Enums.CorrespondenceStatus.Completed) / correspondences.Count * 100
                : 0,
            OverdueCount = correspondences.Count(c => c.DueDate < DateTime.UtcNow &&
                c.Status != Domain.Enums.CorrespondenceStatus.Completed),
            StartDate = startDate.Value,
            EndDate = endDate.Value
        };
    }

    // Helper Methods
    private async Task<double> CalculateGrowthRate(DateTime startDate, EntityType entityType)
    {
        var previousPeriodStart = startDate.AddMonths(-1);

        int currentCount = entityType switch
        {
            EntityType.Users => await _context.Users.CountAsync(u => !u.IsDeleted && u.CreatedAt >= startDate),
            EntityType.Correspondences => await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.CreatedAt >= startDate),
            _ => 0
        };

        int previousCount = entityType switch
        {
            EntityType.Users => await _context.Users.CountAsync(u => !u.IsDeleted &&
                u.CreatedAt >= previousPeriodStart && u.CreatedAt < startDate),
            EntityType.Correspondences => await _context.Set<Domain.Entities.Archive.Correspondence>()
                .CountAsync(c => !c.IsDeleted && c.CreatedAt >= previousPeriodStart && c.CreatedAt < startDate),
            _ => 0
        };

        return previousCount > 0 ? ((double)(currentCount - previousCount) / previousCount * 100) : 0;
    }

    private async Task<double> CalculateAverageResponseTime(int departmentId)
    {
        var completedCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted && c.ToDepartmentId == departmentId &&
                c.Status == Domain.Enums.CorrespondenceStatus.Completed)
            .ToListAsync();

        if (!completedCorrespondences.Any())
            return 0;

        return completedCorrespondences.Average(c => (c.UpdatedAt - c.CreatedAt).TotalHours);
    }

    private async Task<double> CalculateCompletionRate(int departmentId)
    {
        var total = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .CountAsync(c => !c.IsDeleted && c.ToDepartmentId == departmentId);

        if (total == 0)
            return 0;

        var completed = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .CountAsync(c => !c.IsDeleted && c.ToDepartmentId == departmentId &&
                c.Status == Domain.Enums.CorrespondenceStatus.Completed);

        return (double)completed / total * 100;
    }

    private async Task<double> CalculateEmployeeAverageCompletionTime(int employeeId)
    {
        var completedCorrespondences = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .Where(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                c.Status == Domain.Enums.CorrespondenceStatus.Completed)
            .ToListAsync();

        if (!completedCorrespondences.Any())
            return 0;

        return completedCorrespondences.Average(c => (c.UpdatedAt - c.CreatedAt).TotalHours);
    }

    private async Task<double> CalculateEmployeeCompletionRate(int employeeId)
    {
        var total = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId);

        if (total == 0)
            return 0;

        var completed = await _context.Set<Domain.Entities.Archive.Correspondence>()
            .CountAsync(c => !c.IsDeleted && c.ToEmployeeId == employeeId &&
                c.Status == Domain.Enums.CorrespondenceStatus.Completed);

        return (double)completed / total * 100;
    }

    private async Task<string?> GetEmployeeNameAsync(int employeeId)
    {
        var employee = await _context.Set<Domain.Entities.Employee>()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        return employee?.FullNameAr ?? employee?.FullNameEn;
    }
}

public enum EntityType
{
    Users,
    Correspondences,
    Employees,
    Departments
}
