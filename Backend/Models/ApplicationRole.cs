using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.Models;

public class ApplicationRole : IdentityRole
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public List<RolePermission> RolePermissions { get; set; } = new();
}
