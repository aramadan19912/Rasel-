using Application.DTOs.Organization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrgChartService
    {
        // Org Chart Visualization
        Task<OrgChartDto> GetCompanyOrgChartAsync(string userId);
        Task<OrgChartDto> GetDepartmentOrgChartAsync(int departmentId, string userId);
        Task<EmployeeHierarchyDto> GetEmployeeOrgChartAsync(int employeeId, int depth, string userId);

        // Communication Level Operations
        Task<int> GetEmployeeCommunicationLevelAsync(int employeeId, string userId);
        Task<List<EmployeeDto>> GetEmployeesAtLevelAsync(int level, string userId);
        Task<List<EmployeeDto>> GetEmployeesAboveLevelAsync(int level, string userId);
        Task<List<EmployeeDto>> GetEmployeesBelowLevelAsync(int level, string userId);

        // Communication Routing
        Task<bool> CanEmployeeCommunicateAsync(int fromEmployeeId, int toEmployeeId, string userId);
        Task<List<EmployeeDto>> GetAuthorizedCommunicationPartnersAsync(int employeeId, string userId);
        Task<CommunicationPathDto> GetCommunicationPathAsync(int fromEmployeeId, int toEmployeeId, string userId);

        // Manager Approval Workflow
        Task<bool> RequiresApprovalAsync(int employeeId, string action, string userId);
        Task<EmployeeDto?> GetApproverAsync(int employeeId, string action, string userId);
        Task<List<EmployeeDto>> GetApprovalChainAsync(int employeeId, string userId);

        // Hierarchy Analysis
        Task<int> GetHierarchyDepthAsync(int employeeId, string userId);
        Task<int> GetSpanOfControlAsync(int managerId, string userId); // Direct reports count
        Task<int> GetTotalSubordinatesAsync(int managerId, string userId); // All reports count
        Task<List<EmployeeDto>> GetPeersAsync(int employeeId, string userId); // Same manager, same level

        // Department Communication
        Task<List<EmployeeDto>> GetDepartmentCommunicationListAsync(int departmentId, string userId);
        Task<bool> CanCommunicateWithDepartmentAsync(int employeeId, int departmentId, string userId);

        // Statistics
        Task<OrgChartStatisticsDto> GetOrgChartStatisticsAsync(string userId);
    }

    public class CommunicationPathDto
    {
        public int FromEmployeeId { get; set; }
        public int ToEmployeeId { get; set; }
        public bool DirectCommunicationAllowed { get; set; }
        public List<EmployeeDto> ApprovalPath { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    public class OrgChartStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalLevels { get; set; }
        public int MaxHierarchyDepth { get; set; }
        public double AverageSpanOfControl { get; set; }
        public int EmployeesRequiringApproval { get; set; }
        public Dictionary<int, int> EmployeesPerLevel { get; set; } = new();
    }
}
