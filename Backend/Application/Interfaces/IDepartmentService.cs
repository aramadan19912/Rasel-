using Application.DTOs.Organization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDepartmentService
    {
        // CRUD Operations
        Task<DepartmentDto> GetByIdAsync(int id, string userId);
        Task<List<DepartmentDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, string userId);
        Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Hierarchy Operations
        Task<DepartmentHierarchyDto> GetDepartmentHierarchyAsync(int departmentId, string userId);
        Task<DepartmentHierarchyDto> GetFullDepartmentTreeAsync(string userId);
        Task<List<DepartmentDto>> GetSubDepartmentsAsync(int parentDepartmentId, string userId);
        Task<DepartmentDto?> GetParentDepartmentAsync(int departmentId, string userId);

        // Search and Filter
        Task<List<DepartmentDto>> SearchAsync(string searchTerm, string userId);
        Task<List<DepartmentDto>> GetByLocationAsync(string location, string userId);
        Task<List<DepartmentDto>> GetActiveDepartmentsAsync(string userId);

        // Employee Operations
        Task<List<EmployeeDto>> GetDepartmentEmployeesAsync(int departmentId, string userId);
        Task<int> GetEmployeeCountAsync(int departmentId, string userId);

        // Statistics
        Task<DepartmentStatisticsDto> GetStatisticsAsync(string userId);
    }

    public class DepartmentStatisticsDto
    {
        public int TotalDepartments { get; set; }
        public int ActiveDepartments { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBudget { get; set; }
        public int DepartmentsWithoutHead { get; set; }
    }
}
