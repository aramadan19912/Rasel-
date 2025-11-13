using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Permissions;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Identity;
using Backend.Domain.Enums;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionDto?> GetByIdAsync(int permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        return permission == null ? null : MapToDto(permission);
    }

    public async Task<PermissionDto?> GetByNameAsync(string permissionName)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName);
        return permission == null ? null : MapToDto(permission);
    }

    public async Task<List<PermissionDto>> GetAllAsync()
    {
        var permissions = await _context.Permissions.ToListAsync();
        return permissions.Select(MapToDto).ToList();
    }

    public async Task<List<PermissionDto>> GetByModuleAsync(string module)
    {
        var permissions = await _context.Permissions
            .Where(p => p.Module == module)
            .ToListAsync();
        return permissions.Select(MapToDto).ToList();
    }

    public async Task<bool> SeedPermissionsAsync()
    {
        var permissions = GetSystemPermissions();

        foreach (var perm in permissions)
        {
            var exists = await _context.Permissions.AnyAsync(p => p.Name == perm.Name);
            if (!exists)
            {
                _context.Permissions.Add(perm);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private List<Permission> GetSystemPermissions()
    {
        return new List<Permission>
        {
            // Users
            new() { Name = SystemPermission.UsersCreate, DisplayName = "Create Users", Module = "Users", Resource = "User", Action = "Create" },
            new() { Name = SystemPermission.UsersRead, DisplayName = "Read Users", Module = "Users", Resource = "User", Action = "Read" },
            new() { Name = SystemPermission.UsersUpdate, DisplayName = "Update Users", Module = "Users", Resource = "User", Action = "Update" },
            new() { Name = SystemPermission.UsersDelete, DisplayName = "Delete Users", Module = "Users", Resource = "User", Action = "Delete" },
            new() { Name = SystemPermission.UsersManage, DisplayName = "Manage Users", Module = "Users", Resource = "User", Action = "Manage" },

            // Roles
            new() { Name = SystemPermission.RolesCreate, DisplayName = "Create Roles", Module = "Roles", Resource = "Role", Action = "Create" },
            new() { Name = SystemPermission.RolesRead, DisplayName = "Read Roles", Module = "Roles", Resource = "Role", Action = "Read" },
            new() { Name = SystemPermission.RolesUpdate, DisplayName = "Update Roles", Module = "Roles", Resource = "Role", Action = "Update" },
            new() { Name = SystemPermission.RolesDelete, DisplayName = "Delete Roles", Module = "Roles", Resource = "Role", Action = "Delete" },
            new() { Name = SystemPermission.RolesManage, DisplayName = "Manage Roles", Module = "Roles", Resource = "Role", Action = "Manage" },

            // Messages
            new() { Name = SystemPermission.MessagesCreate, DisplayName = "Create Messages", Module = "Messages", Resource = "Message", Action = "Create" },
            new() { Name = SystemPermission.MessagesRead, DisplayName = "Read Messages", Module = "Messages", Resource = "Message", Action = "Read" },
            new() { Name = SystemPermission.MessagesUpdate, DisplayName = "Update Messages", Module = "Messages", Resource = "Message", Action = "Update" },
            new() { Name = SystemPermission.MessagesDelete, DisplayName = "Delete Messages", Module = "Messages", Resource = "Message", Action = "Delete" },

            // Calendar
            new() { Name = SystemPermission.CalendarCreate, DisplayName = "Create Calendar Events", Module = "Calendar", Resource = "Event", Action = "Create" },
            new() { Name = SystemPermission.CalendarRead, DisplayName = "Read Calendar Events", Module = "Calendar", Resource = "Event", Action = "Read" },
            new() { Name = SystemPermission.CalendarUpdate, DisplayName = "Update Calendar Events", Module = "Calendar", Resource = "Event", Action = "Update" },
            new() { Name = SystemPermission.CalendarDelete, DisplayName = "Delete Calendar Events", Module = "Calendar", Resource = "Event", Action = "Delete" },

            // Contacts
            new() { Name = SystemPermission.ContactsCreate, DisplayName = "Create Contacts", Module = "Contacts", Resource = "Contact", Action = "Create" },
            new() { Name = SystemPermission.ContactsRead, DisplayName = "Read Contacts", Module = "Contacts", Resource = "Contact", Action = "Read" },
            new() { Name = SystemPermission.ContactsUpdate, DisplayName = "Update Contacts", Module = "Contacts", Resource = "Contact", Action = "Update" },
            new() { Name = SystemPermission.ContactsDelete, DisplayName = "Delete Contacts", Module = "Contacts", Resource = "Contact", Action = "Delete" },

            // Conference
            new() { Name = SystemPermission.ConferenceCreate, DisplayName = "Create Conferences", Module = "Conference", Resource = "Conference", Action = "Create" },
            new() { Name = SystemPermission.ConferenceRead, DisplayName = "Read Conferences", Module = "Conference", Resource = "Conference", Action = "Read" },
            new() { Name = SystemPermission.ConferenceUpdate, DisplayName = "Update Conferences", Module = "Conference", Resource = "Conference", Action = "Update" },
            new() { Name = SystemPermission.ConferenceDelete, DisplayName = "Delete Conferences", Module = "Conference", Resource = "Conference", Action = "Delete" },
            new() { Name = SystemPermission.ConferenceHost, DisplayName = "Host Conferences", Module = "Conference", Resource = "Conference", Action = "Host" },
            new() { Name = SystemPermission.ConferenceRecord, DisplayName = "Record Conferences", Module = "Conference", Resource = "Conference", Action = "Record" }
        };
    }

    private PermissionDto MapToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            DisplayName = permission.DisplayName,
            Description = permission.Description,
            Module = permission.Module,
            Resource = permission.Resource,
            Action = permission.Action
        };
    }
}
