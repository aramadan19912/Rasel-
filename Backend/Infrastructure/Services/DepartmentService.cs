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
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public DepartmentService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<DepartmentDto> GetByIdAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var department = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.SubDepartments)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (department == null)
                throw new KeyNotFoundException($"Department with ID {id} not found");

            return MapToDto(department);
        }

        public async Task<List<DepartmentDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var departments = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.SubDepartments)
                .Include(d => d.Employees)
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return departments.Select(MapToDto).ToList();
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.create"))
                throw new UnauthorizedAccessException("Permission denied: organization.create");

            // Check if department code already exists
            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == dto.DepartmentCode && !d.IsDeleted))
                throw new InvalidOperationException($"Department code {dto.DepartmentCode} already exists");

            var department = new Department
            {
                DepartmentCode = dto.DepartmentCode,
                Name = dto.Name,
                Description = dto.Description,
                Mission = dto.Mission,
                ParentDepartmentId = dto.ParentDepartmentId,
                HeadOfDepartmentId = dto.HeadOfDepartmentId,
                Email = dto.Email,
                Phone = dto.Phone,
                Location = dto.Location,
                OfficeNumber = dto.OfficeNumber,
                CostCenter = dto.CostCenter,
                AnnualBudget = dto.AnnualBudget,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(department.Id, userId);
        }

        public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var department = await _context.Departments.FindAsync(id);
            if (department == null || department.IsDeleted)
                throw new KeyNotFoundException($"Department with ID {id} not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Name)) department.Name = dto.Name;
            if (dto.Description != null) department.Description = dto.Description;
            if (dto.Mission != null) department.Mission = dto.Mission;
            if (dto.ParentDepartmentId.HasValue) department.ParentDepartmentId = dto.ParentDepartmentId;
            if (dto.HeadOfDepartmentId != null) department.HeadOfDepartmentId = dto.HeadOfDepartmentId;
            if (dto.Email != null) department.Email = dto.Email;
            if (dto.Phone != null) department.Phone = dto.Phone;
            if (dto.Location != null) department.Location = dto.Location;
            if (dto.OfficeNumber != null) department.OfficeNumber = dto.OfficeNumber;
            if (dto.CostCenter != null) department.CostCenter = dto.CostCenter;
            if (dto.AnnualBudget.HasValue) department.AnnualBudget = dto.AnnualBudget;
            if (dto.IsActive.HasValue) department.IsActive = dto.IsActive.Value;

            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.delete"))
                throw new UnauthorizedAccessException("Permission denied: organization.delete");

            var department = await _context.Departments
                .Include(d => d.SubDepartments)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null || department.IsDeleted)
                return false;

            // Check if department has sub-departments
            if (department.SubDepartments.Any(sd => !sd.IsDeleted))
                throw new InvalidOperationException("Cannot delete department with active sub-departments");

            // Check if department has employees
            if (department.Employees.Any(e => !e.IsDeleted))
                throw new InvalidOperationException("Cannot delete department with active employees");

            department.IsDeleted = true;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DepartmentHierarchyDto> GetDepartmentHierarchyAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var department = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Include(d => d.SubDepartments)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                throw new KeyNotFoundException($"Department with ID {departmentId} not found");

            return await MapToHierarchyDto(department);
        }

        public async Task<DepartmentHierarchyDto> GetFullDepartmentTreeAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            // Get root department (no parent)
            var rootDepartment = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Include(d => d.SubDepartments)
                .FirstOrDefaultAsync(d => d.ParentDepartmentId == null && !d.IsDeleted);

            if (rootDepartment == null)
                throw new KeyNotFoundException("No root department found");

            return await MapToHierarchyDto(rootDepartment);
        }

        public async Task<List<DepartmentDto>> GetSubDepartmentsAsync(int parentDepartmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var subDepartments = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Where(d => d.ParentDepartmentId == parentDepartmentId && !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return subDepartments.Select(MapToDto).ToList();
        }

        public async Task<DepartmentDto?> GetParentDepartmentAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var department = await _context.Departments
                .Include(d => d.ParentDepartment)
                    .ThenInclude(pd => pd.HeadOfDepartment)
                .Include(d => d.ParentDepartment)
                    .ThenInclude(pd => pd.Employees)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department?.ParentDepartment == null)
                return null;

            return MapToDto(department.ParentDepartment);
        }

        public async Task<List<DepartmentDto>> SearchAsync(string searchTerm, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var departments = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Where(d => !d.IsDeleted &&
                    (d.Name.Contains(searchTerm) ||
                     d.DepartmentCode.Contains(searchTerm) ||
                     (d.Description != null && d.Description.Contains(searchTerm))))
                .OrderBy(d => d.Name)
                .ToListAsync();

            return departments.Select(MapToDto).ToList();
        }

        public async Task<List<DepartmentDto>> GetByLocationAsync(string location, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var departments = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Where(d => !d.IsDeleted && d.Location == location)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return departments.Select(MapToDto).ToList();
        }

        public async Task<List<DepartmentDto>> GetActiveDepartmentsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var departments = await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Where(d => !d.IsDeleted && d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return departments.Select(MapToDto).ToList();
        }

        public async Task<List<EmployeeDto>> GetDepartmentEmployeesAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted && e.IsActive)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapEmployeeToDto).ToList();
        }

        public async Task<int> GetEmployeeCountAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            return await _context.Employees
                .CountAsync(e => e.DepartmentId == departmentId && !e.IsDeleted && e.IsActive);
        }

        public async Task<DepartmentStatisticsDto> GetStatisticsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var totalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted);
            var activeDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted && d.IsActive);
            var totalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted && e.IsActive);
            var totalBudget = await _context.Departments
                .Where(d => !d.IsDeleted && d.AnnualBudget.HasValue)
                .SumAsync(d => d.AnnualBudget.Value);
            var departmentsWithoutHead = await _context.Departments
                .CountAsync(d => !d.IsDeleted && d.IsActive && d.HeadOfDepartmentId == null);

            return new DepartmentStatisticsDto
            {
                TotalDepartments = totalDepartments,
                ActiveDepartments = activeDepartments,
                TotalEmployees = totalEmployees,
                TotalBudget = totalBudget,
                DepartmentsWithoutHead = departmentsWithoutHead
            };
        }

        // Helper Methods
        private DepartmentDto MapToDto(Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                DepartmentCode = department.DepartmentCode,
                Name = department.Name,
                Description = department.Description,
                Mission = department.Mission,
                ParentDepartmentId = department.ParentDepartmentId,
                ParentDepartmentName = department.ParentDepartment?.Name,
                HeadOfDepartmentId = department.HeadOfDepartmentId,
                HeadOfDepartmentName = department.HeadOfDepartment?.FullName,
                Email = department.Email,
                Phone = department.Phone,
                Location = department.Location,
                OfficeNumber = department.OfficeNumber,
                CostCenter = department.CostCenter,
                AnnualBudget = department.AnnualBudget,
                EmployeeCount = department.Employees.Count(e => !e.IsDeleted && e.IsActive),
                SubDepartmentCount = department.SubDepartments.Count(sd => !sd.IsDeleted),
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt
            };
        }

        private async Task<DepartmentHierarchyDto> MapToHierarchyDto(Department department, int level = 0)
        {
            var dto = new DepartmentHierarchyDto
            {
                Id = department.Id,
                Name = department.Name,
                DepartmentCode = department.DepartmentCode,
                HeadOfDepartmentName = department.HeadOfDepartment?.FullName,
                EmployeeCount = department.Employees.Count(e => !e.IsDeleted && e.IsActive),
                Level = level,
                SubDepartments = new List<DepartmentHierarchyDto>()
            };

            // Recursively load sub-departments
            var subDepartments = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Employees)
                .Where(d => d.ParentDepartmentId == department.Id && !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();

            foreach (var subDept in subDepartments)
            {
                dto.SubDepartments.Add(await MapToHierarchyDto(subDept, level + 1));
            }

            return dto;
        }

        private EmployeeDto MapEmployeeToDto(Employee employee)
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
    }
}
