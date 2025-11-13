using Application.DTOs.Organization;
using Application.Interfaces;
using Domain.Entities.Organization;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrgChartService : IOrgChartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public OrgChartService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // Org Chart Visualization
        public async Task<OrgChartDto> GetCompanyOrgChartAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            // Find CEO or top-level employee (lowest communication level, no manager)
            var ceo = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted && e.IsActive && e.ManagerId == null)
                .OrderBy(e => e.CommunicationLevel)
                .FirstOrDefaultAsync();

            if (ceo == null)
                throw new KeyNotFoundException("No root employee (CEO) found");

            var hierarchy = await BuildEmployeeHierarchy(ceo);
            var totalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive);
            var maxDepth = await CalculateMaxDepth(ceo.Id);

            return new OrgChartDto
            {
                RootEmployee = hierarchy,
                TotalEmployees = totalEmployees,
                MaxDepth = maxDepth
            };
        }

        public async Task<OrgChartDto> GetDepartmentOrgChartAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var department = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department?.HeadOfDepartment == null)
                throw new KeyNotFoundException($"Department head not found for department {departmentId}");

            var hierarchy = await BuildEmployeeHierarchy(department.HeadOfDepartment, departmentId);
            var totalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.DepartmentId == departmentId);
            var maxDepth = await CalculateMaxDepth(department.HeadOfDepartment.Id, departmentId);

            return new OrgChartDto
            {
                RootEmployee = hierarchy,
                TotalEmployees = totalEmployees,
                MaxDepth = maxDepth
            };
        }

        public async Task<EmployeeHierarchyDto> GetEmployeeOrgChartAsync(int employeeId, int depth, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            return await BuildEmployeeHierarchy(employee, null, depth);
        }

        private async Task<EmployeeHierarchyDto> BuildEmployeeHierarchy(Employee employee, int? filterDepartmentId = null, int maxDepth = int.MaxValue, int currentDepth = 0)
        {
            var dto = new EmployeeHierarchyDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FullName = employee.FullName,
                DisplayName = employee.DisplayName,
                PositionTitle = employee.Position.Title,
                DepartmentName = employee.Department.Name,
                Email = employee.Email,
                ProfileImageUrl = employee.ProfileImageUrl,
                CommunicationLevel = employee.CommunicationLevel,
                DirectReports = new List<EmployeeHierarchyDto>()
            };

            if (currentDepth < maxDepth)
            {
                var directReportsQuery = _context.Employees
                    .Include(e => e.Position)
                    .Include(e => e.Department)
                    .Where(e => e.ManagerId == employee.Id.ToString() && !e.IsDeleted && e.IsActive);

                if (filterDepartmentId.HasValue)
                {
                    directReportsQuery = directReportsQuery.Where(e => e.DepartmentId == filterDepartmentId.Value);
                }

                var directReports = await directReportsQuery
                    .OrderBy(e => e.CommunicationLevel)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();

                foreach (var report in directReports)
                {
                    var reportHierarchy = await BuildEmployeeHierarchy(report, filterDepartmentId, maxDepth, currentDepth + 1);
                    dto.DirectReports.Add(reportHierarchy);
                }
            }

            return dto;
        }

        private async Task<int> CalculateMaxDepth(int rootEmployeeId, int? filterDepartmentId = null)
        {
            var maxDepth = 0;
            await CalculateDepthRecursive(rootEmployeeId.ToString(), filterDepartmentId, 0, ref maxDepth);
            return maxDepth;
        }

        private async Task CalculateDepthRecursive(string managerId, int? filterDepartmentId, int currentDepth, ref int maxDepth)
        {
            var directReportsQuery = _context.Employees
                .Where(e => e.ManagerId == managerId && !e.IsDeleted && e.IsActive);

            if (filterDepartmentId.HasValue)
            {
                directReportsQuery = directReportsQuery.Where(e => e.DepartmentId == filterDepartmentId.Value);
            }

            var directReports = await directReportsQuery.ToListAsync();

            if (!directReports.Any())
            {
                maxDepth = Math.Max(maxDepth, currentDepth);
                return;
            }

            foreach (var report in directReports)
            {
                await CalculateDepthRecursive(report.Id.ToString(), filterDepartmentId, currentDepth + 1, ref maxDepth);
            }
        }

        // Communication Level Operations
        public async Task<int> GetEmployeeCommunicationLevelAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            return employee.CommunicationLevel;
        }

        public async Task<List<EmployeeDto>> GetEmployeesAtLevelAsync(int level, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive && e.CommunicationLevel == level)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetEmployeesAboveLevelAsync(int level, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive && e.CommunicationLevel < level)
                .OrderBy(e => e.CommunicationLevel)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetEmployeesBelowLevelAsync(int level, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive && e.CommunicationLevel > level)
                .OrderBy(e => e.CommunicationLevel)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        // Communication Routing
        public async Task<bool> CanEmployeeCommunicateAsync(int fromEmployeeId, int toEmployeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
            var toEmployee = await _context.Employees.FindAsync(toEmployeeId);

            if (fromEmployee == null || toEmployee == null || fromEmployee.IsDeleted || toEmployee.IsDeleted)
                return false;

            // Check communication preferences
            if (!toEmployee.CanReceiveInternalMessages)
                return false;

            // Higher level (lower number) can always communicate with lower level
            if (fromEmployee.CommunicationLevel <= toEmployee.CommunicationLevel)
                return true;

            // Within 2 levels difference is allowed
            if (Math.Abs(fromEmployee.CommunicationLevel - toEmployee.CommunicationLevel) <= 2)
                return true;

            // Check if direct report or manager relationship
            if (fromEmployee.ManagerId == toEmployee.Id.ToString() ||
                toEmployee.ManagerId == fromEmployee.Id.ToString())
                return true;

            return false;
        }

        public async Task<List<EmployeeDto>> GetAuthorizedCommunicationPartnersAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return new List<EmployeeDto>();

            // Get all employees at same or lower level, or within 2 levels
            var authorizedPartners = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive &&
                    e.Id != employeeId &&
                    e.CanReceiveInternalMessages &&
                    (e.CommunicationLevel >= employee.CommunicationLevel ||
                     Math.Abs(e.CommunicationLevel - employee.CommunicationLevel) <= 2))
                .OrderBy(e => e.CommunicationLevel)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return authorizedPartners.Select(MapToDto).ToList();
        }

        public async Task<CommunicationPathDto> GetCommunicationPathAsync(int fromEmployeeId, int toEmployeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var fromEmployee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == fromEmployeeId && !e.IsDeleted);

            var toEmployee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == toEmployeeId && !e.IsDeleted);

            if (fromEmployee == null || toEmployee == null)
                throw new KeyNotFoundException("Employee not found");

            var path = new CommunicationPathDto
            {
                FromEmployeeId = fromEmployeeId,
                ToEmployeeId = toEmployeeId,
                ApprovalPath = new List<EmployeeDto>()
            };

            // Check direct communication
            var canCommunicate = await CanEmployeeCommunicateAsync(fromEmployeeId, toEmployeeId, userId);

            if (canCommunicate)
            {
                path.DirectCommunicationAllowed = true;
                path.Reason = "Direct communication authorized based on organizational hierarchy";
                return path;
            }

            // Build approval path through management chain
            path.DirectCommunicationAllowed = false;
            path.Reason = "Communication requires manager approval";

            // Get manager chain from fromEmployee
            var currentEmployee = fromEmployee;
            var approvalChain = new List<Employee>();

            while (currentEmployee.Manager != null)
            {
                var manager = await _context.Employees
                    .Include(e => e.Position)
                    .Include(e => e.Department)
                    .Include(e => e.Manager)
                    .FirstOrDefaultAsync(e => e.Id.ToString() == currentEmployee.ManagerId && !e.IsDeleted);

                if (manager == null) break;

                approvalChain.Add(manager);

                // Check if this manager can communicate with target
                if (await CanEmployeeCommunicateAsync(manager.Id, toEmployeeId, userId))
                {
                    break;
                }

                currentEmployee = manager;
            }

            path.ApprovalPath = approvalChain.Select(MapToDto).ToList();
            return path;
        }

        // Manager Approval Workflow
        public async Task<bool> RequiresApprovalAsync(int employeeId, string action, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            // Check if employee requires manager approval
            return employee.RequireManagerApproval;
        }

        public async Task<EmployeeDto?> GetApproverAsync(int employeeId, string action, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees
                .Include(e => e.Manager)
                    .ThenInclude(m => m.Position)
                .Include(e => e.Manager)
                    .ThenInclude(m => m.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

            if (employee?.Manager == null)
                return null;

            return MapToDto(employee.Manager);
        }

        public async Task<List<EmployeeDto>> GetApprovalChainAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var approvalChain = new List<Employee>();
            var currentEmployee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

            while (currentEmployee?.Manager != null)
            {
                var manager = await _context.Employees
                    .Include(e => e.Position)
                    .Include(e => e.Department)
                    .Include(e => e.Manager)
                    .FirstOrDefaultAsync(e => e.Id.ToString() == currentEmployee.ManagerId && !e.IsDeleted);

                if (manager == null) break;

                approvalChain.Add(manager);
                currentEmployee = manager;
            }

            return approvalChain.Select(MapToDto).ToList();
        }

        // Hierarchy Analysis
        public async Task<int> GetHierarchyDepthAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var depth = 0;
            var currentEmployee = await _context.Employees.FindAsync(employeeId);

            while (currentEmployee?.ManagerId != null)
            {
                depth++;
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Id.ToString() == currentEmployee.ManagerId && !e.IsDeleted);
                if (manager == null) break;
                currentEmployee = manager;
            }

            return depth;
        }

        public async Task<int> GetSpanOfControlAsync(int managerId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            return await _context.Employees
                .CountAsync(e => e.ManagerId == managerId.ToString() && !e.IsDeleted && e.IsActive);
        }

        public async Task<int> GetTotalSubordinatesAsync(int managerId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var allSubordinates = new List<Employee>();
            await GetAllSubordinatesRecursive(managerId.ToString(), allSubordinates);
            return allSubordinates.Count;
        }

        private async Task GetAllSubordinatesRecursive(string managerId, List<Employee> allSubordinates)
        {
            var directReports = await _context.Employees
                .Where(e => e.ManagerId == managerId && !e.IsDeleted && e.IsActive)
                .ToListAsync();

            allSubordinates.AddRange(directReports);

            foreach (var report in directReports)
            {
                await GetAllSubordinatesRecursive(report.Id.ToString(), allSubordinates);
            }
        }

        public async Task<List<EmployeeDto>> GetPeersAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return new List<EmployeeDto>();

            // Get employees with same manager and same communication level
            var peers = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive &&
                    e.Id != employeeId &&
                    e.ManagerId == employee.ManagerId &&
                    e.CommunicationLevel == employee.CommunicationLevel)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return peers.Select(MapToDto).ToList();
        }

        // Department Communication
        public async Task<List<EmployeeDto>> GetDepartmentCommunicationListAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted && e.IsActive && e.CanReceiveInternalMessages)
                .OrderBy(e => e.CommunicationLevel)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<bool> CanCommunicateWithDepartmentAsync(int employeeId, int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            // Same department = allowed
            if (employee.DepartmentId == departmentId)
                return true;

            // Check if employee is at management level (level 1-4)
            if (employee.CommunicationLevel <= 4)
                return true;

            return false;
        }

        // Statistics
        public async Task<OrgChartStatisticsDto> GetOrgChartStatisticsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var stats = new OrgChartStatisticsDto
            {
                TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive),
                TotalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted && d.IsActive),
                TotalLevels = await _context.Employees
                    .Where(e => !e.IsDeleted && e.IsActive)
                    .Select(e => e.CommunicationLevel)
                    .Distinct()
                    .CountAsync(),
                EmployeesRequiringApproval = await _context.Employees
                    .CountAsync(e => !e.IsDeleted && e.IsActive && e.RequireManagerApproval)
            };

            // Calculate max hierarchy depth (from CEO to lowest level employee)
            var ceo = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive && e.ManagerId == null)
                .OrderBy(e => e.CommunicationLevel)
                .FirstOrDefaultAsync();

            if (ceo != null)
            {
                stats.MaxHierarchyDepth = await CalculateMaxDepth(ceo.Id);
            }

            // Calculate average span of control
            var managers = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .Where(e => e.DirectReports.Any(dr => !dr.IsDeleted && dr.IsActive))
                .Select(e => new { ManagerId = e.Id, DirectReportCount = e.DirectReports.Count(dr => !dr.IsDeleted && dr.IsActive) })
                .ToListAsync();

            if (managers.Any())
            {
                stats.AverageSpanOfControl = Math.Round(managers.Average(m => m.DirectReportCount), 2);
            }

            // Employees per level
            stats.EmployeesPerLevel = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .GroupBy(e => e.CommunicationLevel)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count);

            return stats;
        }

        // Helper Methods
        private EmployeeDto MapToDto(Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                UserId = employee.UserId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,
                PreferredName = employee.PreferredName,
                FullName = employee.FullName,
                DisplayName = employee.DisplayName,
                Email = employee.Email,
                WorkPhone = employee.WorkPhone,
                MobilePhone = employee.MobilePhone,
                PositionId = employee.PositionId,
                PositionTitle = employee.Position.Title,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department.Name,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                DirectReportCount = employee.DirectReports?.Count(dr => !dr.IsDeleted && dr.IsActive) ?? 0,
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                EmploymentStatus = employee.EmploymentStatus,
                EmploymentType = employee.EmploymentType,
                OfficeLocation = employee.OfficeLocation,
                WorkSite = employee.WorkSite,
                IsRemote = employee.IsRemote,
                CommunicationLevel = employee.CommunicationLevel,
                ProfileImageUrl = employee.ProfileImageUrl,
                IsActive = employee.IsActive
            };
        }
    }
}
