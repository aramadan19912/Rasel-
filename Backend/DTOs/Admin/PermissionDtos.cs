using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.DTOs.Admin;

public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePermissionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }
}

public class UpdatePermissionRequest
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; }
}

public class PermissionsByModuleDto
{
    public string Module { get; set; } = string.Empty;
    public List<PermissionsByCategoryDto> Categories { get; set; } = new();
}

public class PermissionsByCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}
