using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.DTOs.Admin;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string? FirstNameEn { get; set; }
    public string? LastNameEn { get; set; }
    public string FullNameAr => $"{FirstNameAr} {LastNameAr}";
    public string? FullNameEn => FirstNameEn != null && LastNameEn != null ? $"{FirstNameEn} {LastNameEn}" : null;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionAr { get; set; } = string.Empty;
    public string? PositionEn { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? Avatar { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeRequest
{
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
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    [Phone]
    public string? MobileNumber { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PositionAr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PositionEn { get; set; }

    public int? ManagerId { get; set; }

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
}

public class UpdateEmployeeRequest
{
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
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    [Phone]
    public string? MobileNumber { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PositionAr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PositionEn { get; set; }

    public int? ManagerId { get; set; }

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

    public bool IsActive { get; set; }
}

public class EmployeeSearchRequest
{
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

public class EmployeeSearchResponse
{
    public List<EmployeeDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
