using System;
using System.Collections.Generic;

namespace Domain.Entities.Organization
{
    public class Department
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Mission { get; set; }

        // Hierarchy
        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }
        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        // Leadership
        public string? HeadOfDepartmentId { get; set; }
        public Employee? HeadOfDepartment { get; set; }

        // Contact Information
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? OfficeNumber { get; set; }

        // Budget & Cost Center
        public string? CostCenter { get; set; }
        public decimal? AnnualBudget { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
