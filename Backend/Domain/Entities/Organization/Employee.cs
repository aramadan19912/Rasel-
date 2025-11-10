using System;
using System.Collections.Generic;
using Domain.Entities.Auth;

namespace Domain.Entities.Organization
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;

        // User Association
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Basic Information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? PreferredName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? WorkPhone { get; set; }
        public string? MobilePhone { get; set; }

        // Position & Department
        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Reporting Hierarchy
        public string? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

        // Employment Details
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string EmploymentStatus { get; set; } = "Active"; // Active, OnLeave, Terminated
        public string EmploymentType { get; set; } = "Full-time"; // Full-time, Part-time, Contract, Intern

        // Work Location
        public string? OfficeLocation { get; set; }
        public string? WorkSite { get; set; }
        public string? Cubicle { get; set; }
        public string? Floor { get; set; }
        public bool IsRemote { get; set; }

        // Compensation
        public decimal? Salary { get; set; }
        public string? SalaryCurrency { get; set; } = "USD";
        public string? PayFrequency { get; set; } // Monthly, Biweekly, Weekly

        // Communication Preferences
        public int CommunicationLevel { get; set; } // 1 = Executive, 2 = Manager, 3 = Employee, etc.
        public bool CanReceiveInternalMessages { get; set; } = true;
        public bool CanReceiveExternalMessages { get; set; } = true;
        public bool RequireManagerApproval { get; set; } = false;

        // Additional Information
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
        public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
        public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();

        // Departments managed
        public ICollection<Department> ManagedDepartments { get; set; } = new List<Department>();

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayName => string.IsNullOrWhiteSpace(PreferredName) ? FullName : PreferredName;
    }
}
