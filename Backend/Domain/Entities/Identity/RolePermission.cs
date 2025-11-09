namespace Backend.Domain.Entities.Identity;

public class RolePermission
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
