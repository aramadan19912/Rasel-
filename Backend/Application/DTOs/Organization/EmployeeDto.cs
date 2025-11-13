using System;
using System.Collections.Generic;

namespace Application.DTOs.Organization
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? PreferredName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? WorkPhone { get; set; }
        public string? MobilePhone { get; set; }

        public int PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int DirectReportCount { get; set; }

        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;

        public string? OfficeLocation { get; set; }
        public string? WorkSite { get; set; }
        public bool IsRemote { get; set; }

        public int CommunicationLevel { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeDetailDto : EmployeeDto
    {
        public string? Cubicle { get; set; }
        public string? Floor { get; set; }
        public decimal? Salary { get; set; }
        public string? SalaryCurrency { get; set; }
        public string? PayFrequency { get; set; }

        public bool CanReceiveInternalMessages { get; set; }
        public bool CanReceiveExternalMessages { get; set; }
        public bool RequireManagerApproval { get; set; }

        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }

        public List<EmployeeDto> DirectReports { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? PreferredName { get; set; }

        public string Email { get; set; } = string.Empty;
        public string? WorkPhone { get; set; }
        public string? MobilePhone { get; set; }

        public int PositionId { get; set; }
        public int DepartmentId { get; set; }
        public string? ManagerId { get; set; }

        public DateTime HireDate { get; set; }
        public string EmploymentStatus { get; set; } = "Active";
        public string EmploymentType { get; set; } = "Full-time";

        public string? OfficeLocation { get; set; }
        public string? WorkSite { get; set; }
        public string? Cubicle { get; set; }
        public string? Floor { get; set; }
        public bool IsRemote { get; set; }

        public decimal? Salary { get; set; }
        public string? SalaryCurrency { get; set; } = "USD";
        public string? PayFrequency { get; set; }

        public int CommunicationLevel { get; set; }
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? PreferredName { get; set; }

        public string? Email { get; set; }
        public string? WorkPhone { get; set; }
        public string? MobilePhone { get; set; }

        public int? PositionId { get; set; }
        public int? DepartmentId { get; set; }
        public string? ManagerId { get; set; }

        public string? EmploymentStatus { get; set; }
        public string? EmploymentType { get; set; }
        public DateTime? TerminationDate { get; set; }

        public string? OfficeLocation { get; set; }
        public string? WorkSite { get; set; }
        public string? Cubicle { get; set; }
        public string? Floor { get; set; }
        public bool? IsRemote { get; set; }

        public decimal? Salary { get; set; }
        public string? PayFrequency { get; set; }

        public int? CommunicationLevel { get; set; }
        public bool? CanReceiveInternalMessages { get; set; }
        public bool? CanReceiveExternalMessages { get; set; }
        public bool? RequireManagerApproval { get; set; }

        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? ProfileImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }

    public class EmployeeHierarchyDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public int CommunicationLevel { get; set; }
        public List<EmployeeHierarchyDto> DirectReports { get; set; } = new();
    }

    public class OrgChartDto
    {
        public EmployeeHierarchyDto RootEmployee { get; set; } = null!;
        public int TotalEmployees { get; set; }
        public int MaxDepth { get; set; }
    }
}
