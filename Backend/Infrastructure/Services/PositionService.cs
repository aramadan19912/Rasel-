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
    public class PositionService : IPositionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public PositionService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<PositionDto> GetByIdAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var position = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (position == null)
                throw new KeyNotFoundException($"Position with ID {id} not found");

            return MapToDto(position);
        }

        public async Task<List<PositionDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<PositionDto> CreateAsync(CreatePositionDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.create"))
                throw new UnauthorizedAccessException("Permission denied: organization.create");

            // Check if position code already exists
            if (await _context.Positions.AnyAsync(p => p.PositionCode == dto.PositionCode && !p.IsDeleted))
                throw new InvalidOperationException($"Position code {dto.PositionCode} already exists");

            var position = new Position
            {
                PositionCode = dto.PositionCode,
                Title = dto.Title,
                Description = dto.Description,
                Level = dto.Level,
                Grade = dto.Grade,
                DepartmentId = dto.DepartmentId,
                ReportsToPositionId = dto.ReportsToPositionId,
                KeyResponsibilities = dto.KeyResponsibilities,
                RequiredQualifications = dto.RequiredQualifications,
                RequiredSkills = dto.RequiredSkills,
                MinSalary = dto.MinSalary,
                MaxSalary = dto.MaxSalary,
                SalaryCurrency = dto.SalaryCurrency ?? "USD",
                EmploymentType = dto.EmploymentType,
                IsManagementPosition = dto.IsManagementPosition,
                MaxDirectReports = dto.MaxDirectReports,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(position.Id, userId);
        }

        public async Task<PositionDto> UpdateAsync(int id, UpdatePositionDto dto, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.update"))
                throw new UnauthorizedAccessException("Permission denied: organization.update");

            var position = await _context.Positions.FindAsync(id);
            if (position == null || position.IsDeleted)
                throw new KeyNotFoundException($"Position with ID {id} not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Title)) position.Title = dto.Title;
            if (dto.Description != null) position.Description = dto.Description;
            if (dto.Level.HasValue) position.Level = dto.Level.Value;
            if (dto.Grade != null) position.Grade = dto.Grade;
            if (dto.DepartmentId.HasValue) position.DepartmentId = dto.DepartmentId.Value;
            if (dto.ReportsToPositionId.HasValue) position.ReportsToPositionId = dto.ReportsToPositionId;
            if (dto.KeyResponsibilities != null) position.KeyResponsibilities = dto.KeyResponsibilities;
            if (dto.RequiredQualifications != null) position.RequiredQualifications = dto.RequiredQualifications;
            if (dto.RequiredSkills != null) position.RequiredSkills = dto.RequiredSkills;
            if (dto.MinSalary.HasValue) position.MinSalary = dto.MinSalary;
            if (dto.MaxSalary.HasValue) position.MaxSalary = dto.MaxSalary;
            if (dto.IsManagementPosition.HasValue) position.IsManagementPosition = dto.IsManagementPosition.Value;
            if (dto.MaxDirectReports.HasValue) position.MaxDirectReports = dto.MaxDirectReports;
            if (dto.IsActive.HasValue) position.IsActive = dto.IsActive.Value;

            position.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.delete"))
                throw new UnauthorizedAccessException("Permission denied: organization.delete");

            var position = await _context.Positions
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null || position.IsDeleted)
                return false;

            // Check if position has active employees
            if (position.Employees.Any(e => !e.IsDeleted && e.IsActive))
                throw new InvalidOperationException("Cannot delete position with active employees");

            position.IsDeleted = true;
            position.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PositionDto>> GetByDepartmentAsync(int departmentId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => p.DepartmentId == departmentId && !p.IsDeleted)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> GetManagementPositionsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted && p.IsManagementPosition)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<PositionDto?> GetReportsToPositionAsync(int positionId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var position = await _context.Positions
                .Include(p => p.ReportsToPosition)
                    .ThenInclude(rtp => rtp.Department)
                .Include(p => p.ReportsToPosition)
                    .ThenInclude(rtp => rtp.Employees)
                .FirstOrDefaultAsync(p => p.Id == positionId && !p.IsDeleted);

            if (position?.ReportsToPosition == null)
                return null;

            return MapToDto(position.ReportsToPosition);
        }

        public async Task<List<PositionDto>> GetSubordinatePositionsAsync(int positionId, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => p.ReportsToPositionId == positionId && !p.IsDeleted)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> SearchAsync(string searchTerm, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted &&
                    (p.Title.Contains(searchTerm) ||
                     p.PositionCode.Contains(searchTerm) ||
                     (p.Description != null && p.Description.Contains(searchTerm))))
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> GetByLevelAsync(int level, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted && p.Level == level)
                .OrderBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> GetByEmploymentTypeAsync(string employmentType, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted && p.EmploymentType == employmentType)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> GetActivePositionsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<List<PositionDto>> GetBySalaryRangeAsync(decimal minSalary, decimal maxSalary, string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.ReportsToPosition)
                .Include(p => p.Employees)
                .Where(p => !p.IsDeleted &&
                    p.MinSalary.HasValue &&
                    p.MaxSalary.HasValue &&
                    p.MinSalary.Value >= minSalary &&
                    p.MaxSalary.Value <= maxSalary)
                .OrderBy(p => p.MinSalary)
                .ToListAsync();

            return positions.Select(MapToDto).ToList();
        }

        public async Task<PositionStatisticsDto> GetStatisticsAsync(string userId)
        {
            if (!await _userService.HasPermissionAsync(userId, "organization.read"))
                throw new UnauthorizedAccessException("Permission denied: organization.read");

            var totalPositions = await _context.Positions.CountAsync(p => !p.IsDeleted);
            var activePositions = await _context.Positions.CountAsync(p => !p.IsDeleted && p.IsActive);
            var managementPositions = await _context.Positions.CountAsync(p => !p.IsDeleted && p.IsManagementPosition);

            var vacantPositions = await _context.Positions
                .Where(p => !p.IsDeleted && p.IsActive)
                .CountAsync(p => !p.Employees.Any(e => !e.IsDeleted && e.IsActive));

            var averageSalary = await _context.Positions
                .Where(p => !p.IsDeleted && p.MinSalary.HasValue && p.MaxSalary.HasValue)
                .AverageAsync(p => (p.MinSalary.Value + p.MaxSalary.Value) / 2);

            return new PositionStatisticsDto
            {
                TotalPositions = totalPositions,
                ActivePositions = activePositions,
                ManagementPositions = managementPositions,
                VacantPositions = vacantPositions,
                AverageSalary = averageSalary
            };
        }

        // Helper Methods
        private PositionDto MapToDto(Position position)
        {
            return new PositionDto
            {
                Id = position.Id,
                PositionCode = position.PositionCode,
                Title = position.Title,
                Description = position.Description,
                Level = position.Level,
                Grade = position.Grade,
                DepartmentId = position.DepartmentId,
                DepartmentName = position.Department.Name,
                ReportsToPositionId = position.ReportsToPositionId,
                ReportsToPositionTitle = position.ReportsToPosition?.Title,
                KeyResponsibilities = position.KeyResponsibilities,
                RequiredQualifications = position.RequiredQualifications,
                RequiredSkills = position.RequiredSkills,
                MinSalary = position.MinSalary,
                MaxSalary = position.MaxSalary,
                SalaryCurrency = position.SalaryCurrency,
                EmploymentType = position.EmploymentType,
                IsManagementPosition = position.IsManagementPosition,
                MaxDirectReports = position.MaxDirectReports,
                CurrentEmployeeCount = position.Employees.Count(e => !e.IsDeleted && e.IsActive),
                IsActive = position.IsActive,
                CreatedAt = position.CreatedAt
            };
        }
    }
}
