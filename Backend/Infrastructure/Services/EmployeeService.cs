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
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public EmployeeService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<EmployeeDetailDto> GetByIdAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports.Where(dr => !dr.IsDeleted && dr.IsActive))
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {id} not found");

            return MapToDetailDto(employee);
        }

        public async Task<EmployeeDetailDto> GetByEmployeeNumberAsync(string employeeNumber, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports.Where(dr => !dr.IsDeleted && dr.IsActive))
                .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber && !e.IsDeleted);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with number {employeeNumber} not found");

            return MapToDetailDto(employee);
        }

        public async Task<EmployeeDetailDto> GetByUserIdAsync(string targetUserId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports.Where(dr => !dr.IsDeleted && dr.IsActive))
                .FirstOrDefaultAsync(e => e.UserId == targetUserId && !e.IsDeleted);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with user ID {targetUserId} not found");

            return MapToDetailDto(employee);
        }

        public async Task<List<EmployeeDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.create"))
                throw new UnauthorizedAccessException("Permission denied: organization.create");

            // Check if employee number already exists
            if (await _context.Employees.AnyAsync(e => e.EmployeeNumber == dto.EmployeeNumber && !e.IsDeleted))
                throw new InvalidOperationException($"Employee number {dto.EmployeeNumber} already exists");

            // Validate position and department exist
            var positionExists = await _context.Positions.AnyAsync(p => p.Id == dto.PositionId && !p.IsDeleted);
            if (!positionExists)
                throw new InvalidOperationException($"Position with ID {dto.PositionId} not found");

            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId && !d.IsDeleted);
            if (!departmentExists)
                throw new InvalidOperationException($"Department with ID {dto.DepartmentId} not found");

            var employee = new Employee
            {
                EmployeeNumber = dto.EmployeeNumber,
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                PreferredName = dto.PreferredName,
                Email = dto.Email,
                WorkPhone = dto.WorkPhone,
                MobilePhone = dto.MobilePhone,
                PositionId = dto.PositionId,
                DepartmentId = dto.DepartmentId,
                ManagerId = dto.ManagerId,
                HireDate = dto.HireDate,
                EmploymentStatus = dto.EmploymentStatus,
                EmploymentType = dto.EmploymentType,
                OfficeLocation = dto.OfficeLocation,
                WorkSite = dto.WorkSite,
                Cubicle = dto.Cubicle,
                Floor = dto.Floor,
                IsRemote = dto.IsRemote,
                Salary = dto.Salary,
                SalaryCurrency = dto.SalaryCurrency ?? "USD",
                PayFrequency = dto.PayFrequency,
                CommunicationLevel = dto.CommunicationLevel,
                Bio = dto.Bio,
                Skills = dto.Skills,
                BirthDate = dto.BirthDate,
                EmergencyContact = dto.EmergencyContact,
                EmergencyPhone = dto.EmergencyPhone,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(employee.Id, userId);
        }

        public async Task<EmployeeDetailDto> UpdateAsync(int id, UpdateEmployeeDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new KeyNotFoundException($"Employee with ID {id} not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.FirstName)) employee.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) employee.LastName = dto.LastName;
            if (dto.MiddleName != null) employee.MiddleName = dto.MiddleName;
            if (dto.PreferredName != null) employee.PreferredName = dto.PreferredName;
            if (!string.IsNullOrEmpty(dto.Email)) employee.Email = dto.Email;
            if (dto.WorkPhone != null) employee.WorkPhone = dto.WorkPhone;
            if (dto.MobilePhone != null) employee.MobilePhone = dto.MobilePhone;
            if (dto.PositionId.HasValue) employee.PositionId = dto.PositionId.Value;
            if (dto.DepartmentId.HasValue) employee.DepartmentId = dto.DepartmentId.Value;
            if (dto.ManagerId != null) employee.ManagerId = dto.ManagerId;
            if (!string.IsNullOrEmpty(dto.EmploymentStatus)) employee.EmploymentStatus = dto.EmploymentStatus;
            if (!string.IsNullOrEmpty(dto.EmploymentType)) employee.EmploymentType = dto.EmploymentType;
            if (dto.TerminationDate.HasValue) employee.TerminationDate = dto.TerminationDate;
            if (dto.OfficeLocation != null) employee.OfficeLocation = dto.OfficeLocation;
            if (dto.WorkSite != null) employee.WorkSite = dto.WorkSite;
            if (dto.Cubicle != null) employee.Cubicle = dto.Cubicle;
            if (dto.Floor != null) employee.Floor = dto.Floor;
            if (dto.IsRemote.HasValue) employee.IsRemote = dto.IsRemote.Value;
            if (dto.Salary.HasValue) employee.Salary = dto.Salary;
            if (dto.PayFrequency != null) employee.PayFrequency = dto.PayFrequency;
            if (dto.CommunicationLevel.HasValue) employee.CommunicationLevel = dto.CommunicationLevel.Value;
            if (dto.CanReceiveInternalMessages.HasValue) employee.CanReceiveInternalMessages = dto.CanReceiveInternalMessages.Value;
            if (dto.CanReceiveExternalMessages.HasValue) employee.CanReceiveExternalMessages = dto.CanReceiveExternalMessages.Value;
            if (dto.RequireManagerApproval.HasValue) employee.RequireManagerApproval = dto.RequireManagerApproval.Value;
            if (dto.Bio != null) employee.Bio = dto.Bio;
            if (dto.Skills != null) employee.Skills = dto.Skills;
            if (dto.BirthDate.HasValue) employee.BirthDate = dto.BirthDate;
            if (dto.EmergencyContact != null) employee.EmergencyContact = dto.EmergencyContact;
            if (dto.EmergencyPhone != null) employee.EmergencyPhone = dto.EmergencyPhone;
            if (dto.ProfileImageUrl != null) employee.ProfileImageUrl = dto.ProfileImageUrl;
            if (dto.IsActive.HasValue) employee.IsActive = dto.IsActive.Value;

            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.delete"))
                throw new UnauthorizedAccessException("Permission denied: organization.delete");

            var employee = await _context.Employees
                .Include(e => e.DirectReports)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null || employee.IsDeleted)
                return false;

            // Check if employee has active direct reports
            if (employee.DirectReports.Any(dr => !dr.IsDeleted && dr.IsActive))
                throw new InvalidOperationException("Cannot delete employee with active direct reports. Reassign reports first.");

            employee.IsDeleted = true;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        // Hierarchy Operations
        public async Task<EmployeeDto?> GetManagerAsync(int employeeId, string userId)
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

        public async Task<List<EmployeeDto>> GetDirectReportsAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var directReports = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.ManagerId == employeeId.ToString() && !e.IsDeleted && e.IsActive)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return directReports.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetAllReportsAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var allReports = new List<Employee>();
            await GetAllReportsRecursive(employeeId.ToString(), allReports);

            return allReports.Select(MapToDto).ToList();
        }

        private async Task GetAllReportsRecursive(string managerId, List<Employee> allReports)
        {
            var directReports = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.ManagerId == managerId && !e.IsDeleted && e.IsActive)
                .ToListAsync();

            allReports.AddRange(directReports);

            foreach (var report in directReports)
            {
                await GetAllReportsRecursive(report.Id.ToString(), allReports);
            }
        }

        public async Task<List<EmployeeDto>> GetManagerChainAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var chain = new List<Employee>();
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

                chain.Add(manager);
                currentEmployee = manager;
            }

            return chain.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateManagerAsync(int employeeId, string newManagerId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            // Validate new manager exists
            var managerExists = await _context.Employees.AnyAsync(e => e.Id.ToString() == newManagerId && !e.IsDeleted && e.IsActive);
            if (!managerExists)
                throw new InvalidOperationException($"Manager with ID {newManagerId} not found");

            employee.ManagerId = newManagerId;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        // Department & Position Operations
        public async Task<List<EmployeeDto>> GetByDepartmentAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetByPositionAsync(int positionId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.PositionId == positionId && !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<bool> TransferDepartmentAsync(int employeeId, int newDepartmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == newDepartmentId && !d.IsDeleted);
            if (!departmentExists)
                throw new InvalidOperationException($"Department with ID {newDepartmentId} not found");

            employee.DepartmentId = newDepartmentId;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PromoteAsync(int employeeId, int newPositionId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            var positionExists = await _context.Positions.AnyAsync(p => p.Id == newPositionId && !p.IsDeleted);
            if (!positionExists)
                throw new InvalidOperationException($"Position with ID {newPositionId} not found");

            employee.PositionId = newPositionId;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        // Search and Filter
        public async Task<List<EmployeeDto>> SearchAsync(string searchTerm, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted &&
                    (e.FirstName.Contains(searchTerm) ||
                     e.LastName.Contains(searchTerm) ||
                     e.EmployeeNumber.Contains(searchTerm) ||
                     e.Email.Contains(searchTerm)))
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetByEmploymentStatusAsync(string status, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.EmploymentStatus == status)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetByEmploymentTypeAsync(string type, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.EmploymentType == type)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetByLocationAsync(string location, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.OfficeLocation == location)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetByCommunicationLevelAsync(int level, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.CommunicationLevel == level)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetRemoteEmployeesAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsRemote)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        // Communication Permissions
        public async Task<bool> CanCommunicateWithAsync(int employeeId, int targetEmployeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            var targetEmployee = await _context.Employees.FindAsync(targetEmployeeId);

            if (employee == null || targetEmployee == null)
                return false;

            // Same level or higher can communicate
            if (employee.CommunicationLevel <= targetEmployee.CommunicationLevel)
                return true;

            // Check if within 2 levels difference
            if (Math.Abs(employee.CommunicationLevel - targetEmployee.CommunicationLevel) <= 2)
                return true;

            return false;
        }

        public async Task<List<EmployeeDto>> GetCommunicationPeersAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return new List<EmployeeDto>();

            // Get employees at same level or within 1 level
            var peers = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => !e.IsDeleted && e.IsActive &&
                    e.Id != employeeId &&
                    Math.Abs(e.CommunicationLevel - employee.CommunicationLevel) <= 1)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return peers.Select(MapToDto).ToList();
        }

        public async Task<bool> RequiresManagerApprovalAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employee = await _context.Employees.FindAsync(employeeId);
            return employee?.RequireManagerApproval ?? false;
        }

        // Onboarding & Offboarding
        public async Task<EmployeeDetailDto> OnboardEmployeeAsync(int employeeId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            employee.EmploymentStatus = "Active";
            employee.IsActive = true;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(employeeId, userId);
        }

        public async Task<bool> TerminateEmployeeAsync(int employeeId, DateTime terminationDate, string reason, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
                return false;

            employee.EmploymentStatus = "Terminated";
            employee.TerminationDate = terminationDate;
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        // Statistics
        public async Task<EmployeeStatisticsDto> GetStatisticsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var stats = new EmployeeStatisticsDto
            {
                TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted),
                ActiveEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive),
                OnLeaveEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.EmploymentStatus == "OnLeave"),
                RemoteEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.IsRemote),
                NewHiresThisMonth = await _context.Employees.CountAsync(e => !e.IsDeleted && e.HireDate >= startOfMonth),
                NewHiresThisYear = await _context.Employees.CountAsync(e => !e.IsDeleted && e.HireDate >= startOfYear),
                TerminationsThisYear = await _context.Employees.CountAsync(e => !e.IsDeleted && e.TerminationDate.HasValue && e.TerminationDate >= startOfYear),
                TotalManagers = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.DirectReports.Any(dr => !dr.IsDeleted && dr.IsActive))
            };

            // Calculate average tenure
            var activeEmployees = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .Select(e => e.HireDate)
                .ToListAsync();

            if (activeEmployees.Any())
            {
                var totalTenureDays = activeEmployees.Sum(hireDate => (now - hireDate).TotalDays);
                stats.AverageTenureYears = Math.Round(totalTenureDays / activeEmployees.Count / 365.25, 2);
            }

            // Employees by department
            stats.EmployeesByDepartment = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .GroupBy(e => e.Department.Name)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count);

            // Employees by level
            stats.EmployeesByLevel = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .GroupBy(e => e.CommunicationLevel)
                .Select(g => new { Level = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count);

            // Employees by type
            stats.EmployeesByType = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .GroupBy(e => e.EmploymentType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return stats;
        }

        public async Task<EmployeeStatisticsDto> GetDepartmentStatisticsAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var stats = new EmployeeStatisticsDto
            {
                TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.DepartmentId == departmentId),
                ActiveEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.DepartmentId == departmentId),
                OnLeaveEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.EmploymentStatus == "OnLeave" && e.DepartmentId == departmentId),
                RemoteEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.IsRemote && e.DepartmentId == departmentId),
                NewHiresThisMonth = await _context.Employees.CountAsync(e => !e.IsDeleted && e.HireDate >= startOfMonth && e.DepartmentId == departmentId),
                NewHiresThisYear = await _context.Employees.CountAsync(e => !e.IsDeleted && e.HireDate >= startOfYear && e.DepartmentId == departmentId),
                TerminationsThisYear = await _context.Employees.CountAsync(e => !e.IsDeleted && e.TerminationDate.HasValue && e.TerminationDate >= startOfYear && e.DepartmentId == departmentId),
                TotalManagers = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive && e.DepartmentId == departmentId && e.DirectReports.Any(dr => !dr.IsDeleted && dr.IsActive))
            };

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
                DirectReportCount = employee.DirectReports.Count(dr => !dr.IsDeleted && dr.IsActive),
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

        private EmployeeDetailDto MapToDetailDto(Employee employee)
        {
            return new EmployeeDetailDto
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
                DirectReportCount = employee.DirectReports.Count(dr => !dr.IsDeleted && dr.IsActive),
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                EmploymentStatus = employee.EmploymentStatus,
                EmploymentType = employee.EmploymentType,
                OfficeLocation = employee.OfficeLocation,
                WorkSite = employee.WorkSite,
                IsRemote = employee.IsRemote,
                CommunicationLevel = employee.CommunicationLevel,
                ProfileImageUrl = employee.ProfileImageUrl,
                IsActive = employee.IsActive,
                Cubicle = employee.Cubicle,
                Floor = employee.Floor,
                Salary = employee.Salary,
                SalaryCurrency = employee.SalaryCurrency,
                PayFrequency = employee.PayFrequency,
                CanReceiveInternalMessages = employee.CanReceiveInternalMessages,
                CanReceiveExternalMessages = employee.CanReceiveExternalMessages,
                RequireManagerApproval = employee.RequireManagerApproval,
                Bio = employee.Bio,
                Skills = employee.Skills,
                BirthDate = employee.BirthDate,
                EmergencyContact = employee.EmergencyContact,
                EmergencyPhone = employee.EmergencyPhone,
                DirectReports = employee.DirectReports
                    .Where(dr => !dr.IsDeleted && dr.IsActive)
                    .Select(MapToDto)
                    .ToList(),
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
