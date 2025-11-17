using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllDepartmentsAsync();
    Task<List<DepartmentHierarchyDto>> GetDepartmentHierarchyAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int departmentId);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, string createdBy);
    Task<DepartmentDto> UpdateDepartmentAsync(int departmentId, UpdateDepartmentRequest request, string updatedBy);
    Task DeleteDepartmentAsync(int departmentId);
}
