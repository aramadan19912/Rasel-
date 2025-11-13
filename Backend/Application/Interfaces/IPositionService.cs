using Application.DTOs.Organization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPositionService
    {
        // CRUD Operations
        Task<PositionDto> GetByIdAsync(int id, string userId);
        Task<List<PositionDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
        Task<PositionDto> CreateAsync(CreatePositionDto dto, string userId);
        Task<PositionDto> UpdateAsync(int id, UpdatePositionDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Department Operations
        Task<List<PositionDto>> GetByDepartmentAsync(int departmentId, string userId);
        Task<List<PositionDto>> GetManagementPositionsAsync(string userId);

        // Hierarchy Operations
        Task<PositionDto?> GetReportsToPositionAsync(int positionId, string userId);
        Task<List<PositionDto>> GetSubordinatePositionsAsync(int positionId, string userId);

        // Search and Filter
        Task<List<PositionDto>> SearchAsync(string searchTerm, string userId);
        Task<List<PositionDto>> GetByLevelAsync(int level, string userId);
        Task<List<PositionDto>> GetByEmploymentTypeAsync(string employmentType, string userId);
        Task<List<PositionDto>> GetActivePositionsAsync(string userId);

        // Salary Operations
        Task<List<PositionDto>> GetBySalaryRangeAsync(decimal minSalary, decimal maxSalary, string userId);

        // Statistics
        Task<PositionStatisticsDto> GetStatisticsAsync(string userId);
    }

    public class PositionStatisticsDto
    {
        public int TotalPositions { get; set; }
        public int ActivePositions { get; set; }
        public int ManagementPositions { get; set; }
        public int VacantPositions { get; set; }
        public decimal AverageSalary { get; set; }
    }
}
