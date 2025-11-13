using Microsoft.AspNetCore.Identity;

namespace Backend.Domain.Entities.Identity;

public class UserClaim : IdentityUserClaim<string>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
