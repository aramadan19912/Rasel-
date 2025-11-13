using System;
using System.Collections.Generic;

namespace Domain.Entities.Organization
{
    public class Position
    {
        public int Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Hierarchy Level (1 = Executive, 2 = Senior Management, 3 = Middle Management, etc.)
        public int Level { get; set; }
        public string? Grade { get; set; }

        // Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Reports To (position-based hierarchy)
        public int? ReportsToPositionId { get; set; }
        public Position? ReportsToPosition { get; set; }
        public ICollection<Position> SubordinatePositions { get; set; } = new List<Position>();

        // Responsibilities
        public string? KeyResponsibilities { get; set; }
        public string? RequiredQualifications { get; set; }
        public string? RequiredSkills { get; set; }

        // Compensation
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? SalaryCurrency { get; set; } = "USD";

        // Employment Details
        public string? EmploymentType { get; set; } // Full-time, Part-time, Contract
        public bool IsManagementPosition { get; set; }
        public int? MaxDirectReports { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
