using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IEmployeeService
{
    Task<EmployeeSearchResponse> SearchEmployeesAsync(EmployeeSearchRequest request);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request, string createdBy);
    Task<EmployeeDto> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request, string updatedBy);
    Task DeleteEmployeeAsync(int employeeId);
    Task<List<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId);
}
