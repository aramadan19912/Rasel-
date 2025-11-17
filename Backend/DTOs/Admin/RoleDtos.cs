using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.DTOs.Admin;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleRequest
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<int> PermissionIds { get; set; } = new();
}

public class AssignPermissionsRequest
{
    [Required]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    public List<int> PermissionIds { get; set; } = new();
}
