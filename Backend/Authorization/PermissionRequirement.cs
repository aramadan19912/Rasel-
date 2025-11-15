using Microsoft.AspNetCore.Authorization;

namespace OutlookInboxManagement.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public bool RequireAll { get; }

    public PermissionRequirement(string[] permissions, bool requireAll = false)
    {
        Permissions = permissions;
        RequireAll = requireAll;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user permissions from claims
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        if (!userPermissions.Any())
        {
            return Task.CompletedTask;
        }

        // Check if user has required permissions
        bool hasPermission = requirement.RequireAll
            ? requirement.Permissions.All(p => userPermissions.Contains(p))
            : requirement.Permissions.Any(p => userPermissions.Contains(p));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
