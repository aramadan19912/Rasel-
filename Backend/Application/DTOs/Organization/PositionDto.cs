using System;

namespace Application.DTOs.Organization
{
    public class PositionDto
    {
        public int Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int Level { get; set; }
        public string? Grade { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public int? ReportsToPositionId { get; set; }
        public string? ReportsToPositionTitle { get; set; }

        public string? KeyResponsibilities { get; set; }
        public string? RequiredQualifications { get; set; }
        public string? RequiredSkills { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? SalaryCurrency { get; set; }

        public string? EmploymentType { get; set; }
        public bool IsManagementPosition { get; set; }
        public int? MaxDirectReports { get; set; }

        public int CurrentEmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePositionDto
    {
        public string PositionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int Level { get; set; }
        public string? Grade { get; set; }

        public int DepartmentId { get; set; }
        public int? ReportsToPositionId { get; set; }

        public string? KeyResponsibilities { get; set; }
        public string? RequiredQualifications { get; set; }
        public string? RequiredSkills { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? SalaryCurrency { get; set; } = "USD";

        public string? EmploymentType { get; set; }
        public bool IsManagementPosition { get; set; }
        public int? MaxDirectReports { get; set; }
    }

    public class UpdatePositionDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int? Level { get; set; }
        public string? Grade { get; set; }

        public int? DepartmentId { get; set; }
        public int? ReportsToPositionId { get; set; }

        public string? KeyResponsibilities { get; set; }
        public string? RequiredQualifications { get; set; }
        public string? RequiredSkills { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        public bool? IsManagementPosition { get; set; }
        public int? MaxDirectReports { get; set; }

        public bool? IsActive { get; set; }
    }
}
