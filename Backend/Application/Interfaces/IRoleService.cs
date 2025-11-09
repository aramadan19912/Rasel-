using Backend.Application.DTOs.Roles;

namespace Backend.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(string roleId);
    Task<RoleDto?> GetByNameAsync(string roleName);
    Task<List<RoleDto>> GetAllAsync();
    Task<RoleDto> CreateAsync(CreateRoleRequest request);
    Task<RoleDto> UpdateAsync(string roleId, CreateRoleRequest request);
    Task<bool> DeleteAsync(string roleId);
    Task<bool> AssignPermissionAsync(string roleId, int permissionId);
    Task<bool> RemovePermissionAsync(string roleId, int permissionId);
    Task<List<string>> GetRolePermissionsAsync(string roleId);
}
