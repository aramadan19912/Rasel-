using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Roles;

public class CreateRoleRequest
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public List<int> PermissionIds { get; set; } = new();
}
