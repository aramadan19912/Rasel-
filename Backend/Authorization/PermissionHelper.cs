using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.Models;
using System.Security.Claims;

namespace OutlookInboxManagement.Authorization;

public interface IPermissionHelper
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<Claim>> GetPermissionClaimsAsync(string userId);
}

public class PermissionHelper : IPermissionHelper
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionHelper(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        // Get user roles
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        var roleNames = await _userManager.GetRolesAsync(user);
        if (!roleNames.Any()) return new List<string>();

        // Check if user has SuperAdmin role
        if (roleNames.Contains("SuperAdmin"))
        {
            // Return all active permissions
            return await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ToListAsync();
        }

        // Get role IDs
        var roleIds = await _context.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        // Get permissions for these roles
        var permissions = await _context.Set<RolePermission>()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Where(rp => rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<List<Claim>> GetPermissionClaimsAsync(string userId)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Select(p => new Claim("permission", p)).ToList();
    }
}
