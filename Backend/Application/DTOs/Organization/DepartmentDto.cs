using System;
using System.Collections.Generic;

namespace Application.DTOs.Organization
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Mission { get; set; }

        public int? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }

        public string? HeadOfDepartmentId { get; set; }
        public string? HeadOfDepartmentName { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? OfficeNumber { get; set; }

        public string? CostCenter { get; set; }
        public decimal? AnnualBudget { get; set; }

        public int EmployeeCount { get; set; }
        public int SubDepartmentCount { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string DepartmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Mission { get; set; }

        public int? ParentDepartmentId { get; set; }
        public string? HeadOfDepartmentId { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? OfficeNumber { get; set; }

        public string? CostCenter { get; set; }
        public decimal? AnnualBudget { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Mission { get; set; }

        public int? ParentDepartmentId { get; set; }
        public string? HeadOfDepartmentId { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? OfficeNumber { get; set; }

        public string? CostCenter { get; set; }
        public decimal? AnnualBudget { get; set; }

        public bool? IsActive { get; set; }
    }

    public class DepartmentHierarchyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string? HeadOfDepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public int Level { get; set; }
        public List<DepartmentHierarchyDto> SubDepartments { get; set; } = new();
    }
}
