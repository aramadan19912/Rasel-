using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstNameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastNameAr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstNameEn { get; set; }

    [MaxLength(100)]
    public string? LastNameEn { get; set; }

    [Required]
    [MaxLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    public string? MobileNumber { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PositionAr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PositionEn { get; set; }

    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? NationalId { get; set; }

    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Avatar { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public List<Employee> Subordinates { get; set; } = new();
    public List<Department> ManagedDepartments { get; set; } = new();
}
