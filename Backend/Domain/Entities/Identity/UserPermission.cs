namespace Backend.Domain.Entities.Identity;

public class UserPermission
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
