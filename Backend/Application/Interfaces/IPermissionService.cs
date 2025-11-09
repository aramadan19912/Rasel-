using Backend.Application.DTOs.Permissions;

namespace Backend.Application.Interfaces;

public interface IPermissionService
{
    Task<PermissionDto?> GetByIdAsync(int permissionId);
    Task<PermissionDto?> GetByNameAsync(string permissionName);
    Task<List<PermissionDto>> GetAllAsync();
    Task<List<PermissionDto>> GetByModuleAsync(string module);
    Task<bool> SeedPermissionsAsync();
}
