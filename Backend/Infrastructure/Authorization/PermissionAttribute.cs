using Microsoft.AspNetCore.Authorization;

namespace Backend.Infrastructure.Authorization;

public class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission) : base(permission)
    {
        Policy = permission;
    }
}
