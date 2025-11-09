using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Roles;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Identity;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ApplicationDbContext _context;

    public RoleService(RoleManager<Role> roleManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<RoleDto?> GetByIdAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role == null ? null : await MapToDto(role);
    }

    public async Task<RoleDto?> GetByNameAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        return role == null ? null : await MapToDto(role);
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var roleDtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            roleDtos.Add(await MapToDto(role));
        }
        return roleDtos;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        foreach (var permissionId in request.PermissionIds)
        {
            await AssignPermissionAsync(role.Id, permissionId);
        }

        return await MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(string roleId, CreateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) throw new KeyNotFoundException("Role not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot update system roles");

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        return await MapToDto(role);
    }

    public async Task<bool> DeleteAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return false;

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot delete system roles");

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> AssignPermissionAsync(string roleId, int permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (exists) return true;

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionAsync(string roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null) return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }

    private async Task<RoleDto> MapToDto(Role role)
    {
        var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        var permissions = await GetRolePermissionsAsync(role.Id);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = userCount,
            Permissions = permissions,
            CreatedAt = role.CreatedAt
        };
    }
}
