using Application.DTOs.Organization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmployeeService
    {
        // CRUD Operations
        Task<EmployeeDetailDto> GetByIdAsync(int id, string userId);
        Task<EmployeeDetailDto> GetByEmployeeNumberAsync(string employeeNumber, string userId);
        Task<EmployeeDetailDto> GetByUserIdAsync(string targetUserId, string userId);
        Task<List<EmployeeDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
        Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string userId);
        Task<EmployeeDetailDto> UpdateAsync(int id, UpdateEmployeeDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Hierarchy Operations
        Task<EmployeeDto?> GetManagerAsync(int employeeId, string userId);
        Task<List<EmployeeDto>> GetDirectReportsAsync(int employeeId, string userId);
        Task<List<EmployeeDto>> GetAllReportsAsync(int employeeId, string userId); // All subordinates recursively
        Task<List<EmployeeDto>> GetManagerChainAsync(int employeeId, string userId); // Up to CEO
        Task<bool> UpdateManagerAsync(int employeeId, string newManagerId, string userId);

        // Department & Position Operations
        Task<List<EmployeeDto>> GetByDepartmentAsync(int departmentId, string userId);
        Task<List<EmployeeDto>> GetByPositionAsync(int positionId, string userId);
        Task<bool> TransferDepartmentAsync(int employeeId, int newDepartmentId, string userId);
        Task<bool> PromoteAsync(int employeeId, int newPositionId, string userId);

        // Search and Filter
        Task<List<EmployeeDto>> SearchAsync(string searchTerm, string userId);
        Task<List<EmployeeDto>> GetByEmploymentStatusAsync(string status, string userId);
        Task<List<EmployeeDto>> GetByEmploymentTypeAsync(string type, string userId);
        Task<List<EmployeeDto>> GetByLocationAsync(string location, string userId);
        Task<List<EmployeeDto>> GetByCommunicationLevelAsync(int level, string userId);
        Task<List<EmployeeDto>> GetRemoteEmployeesAsync(string userId);

        // Communication Permissions
        Task<bool> CanCommunicateWithAsync(int employeeId, int targetEmployeeId, string userId);
        Task<List<EmployeeDto>> GetCommunicationPeersAsync(int employeeId, string userId);
        Task<bool> RequiresManagerApprovalAsync(int employeeId, string userId);

        // Onboarding & Offboarding
        Task<EmployeeDetailDto> OnboardEmployeeAsync(int employeeId, string userId);
        Task<bool> TerminateEmployeeAsync(int employeeId, DateTime terminationDate, string reason, string userId);

        // Statistics
        Task<EmployeeStatisticsDto> GetStatisticsAsync(string userId);
        Task<EmployeeStatisticsDto> GetDepartmentStatisticsAsync(int departmentId, string userId);
    }

    public class EmployeeStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int OnLeaveEmployees { get; set; }
        public int RemoteEmployees { get; set; }
        public int NewHiresThisMonth { get; set; }
        public int NewHiresThisYear { get; set; }
        public int TerminationsThisYear { get; set; }
        public decimal AverageTenureYears { get; set; }
        public int TotalManagers { get; set; }

        public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
        public Dictionary<string, int> EmployeesByLevel { get; set; } = new();
        public Dictionary<string, int> EmployeesByType { get; set; } = new();
    }
}
