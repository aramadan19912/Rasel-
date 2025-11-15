using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string roleId);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, string createdBy);
    Task<RoleDto> UpdateRoleAsync(string roleId, UpdateRoleRequest request, string updatedBy);
    Task DeleteRoleAsync(string roleId);
    Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId);
    Task AssignPermissionsAsync(AssignPermissionsRequest request, string assignedBy);
}
