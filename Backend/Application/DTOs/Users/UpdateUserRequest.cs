using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Users;

public class UpdateUserRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    public string? Avatar { get; set; }
}
